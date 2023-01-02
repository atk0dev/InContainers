using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Globomantics.Core.Authorization;
using Globomantics.Core.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;

namespace Globomantics.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AssemblyName assembly = System.Reflection.Assembly.GetExecutingAssembly().GetType().Assembly.GetName();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, provider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Seq("http://globoseq");
            });

            
            builder.Services.AddScoped<IDbConnection, SqlConnection>(db =>
                new SqlConnection(builder.Configuration.GetConnectionString("GlobomanticsDataConnection")));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie(options =>
                {
                    options.Cookie.Name = "mvccode";
                    options.AccessDeniedPath = "/AccessDenied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.ResponseType = "code";
                    options.UsePkce = true;

                    AddOidcSettingsFromConfig(options, builder.Configuration);

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.ClaimActions.MapJsonKey("MfaEnabled", "MfaEnabled");
                    options.ClaimActions.MapJsonKey("CompanyId", "CompanyId");
                    options.ClaimActions.MapJsonKey("role", "role");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role
                    };
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTicketReceived = e =>
                        {
                            e.Principal = DoClaimsTransformation(e.Principal);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAccessTokenManagement();

            builder.Services.AddHttpClient<IApiClient, ApiClient>()
                .AddUserAccessTokenHandler()
                .AddResiliencePolicies();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, CustomPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, RightRequirementHandler>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("MfaRequired",
                    p =>
                    {
                        p.RequireClaim("CompanyId");
                        p.RequireClaim("MfaEnabled", "True");
                    });
            });

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeaderOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
                {
                    diagCtx.Set("Machine", Environment.MachineName);
                    diagCtx.Set("Assembly", assembly.Name);
                    diagCtx.Set("Version", assembly.Version);
                    diagCtx.Set("ClientIP", httpCtx.Connection.RemoteIpAddress);
                    diagCtx.Set("UserAgent", httpCtx.Request.Headers["User-Agent"]);
                    if (httpCtx.User.Identity.IsAuthenticated)
                    {
                        diagCtx.Set("UserName", httpCtx.User.Identity?.Name);
                    }
                };
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(ep =>
            {
                ep.MapRazorPages().RequireAuthorization();
            });
            
            app.Run();

            Log.CloseAndFlush();
        }

        private static void AddOidcSettingsFromConfig(OpenIdConnectOptions options,
            Microsoft.Extensions.Configuration.ConfigurationManager builderConfiguration)
        {
            options.Authority = builderConfiguration.GetValue<string>("AuthN:Authority");
            options.ClientId = builderConfiguration.GetValue<string>("AuthN:ClientId");
            options.ClientSecret = builderConfiguration.GetValue<string>("AuthN:ClientSecret");
            options.Scope.Clear();
            var allScopes = builderConfiguration.GetValue<string>("AuthN:Scopes");
            foreach (var scope in allScopes.Split(' '))
            {
                options.Scope.Add(scope);
            }
        }

        private static ClaimsPrincipal DoClaimsTransformation(ClaimsPrincipal argPrincipal)
        {
            var claims = argPrincipal.Claims.ToList();
            claims.Add(new Claim("somenewclaim", "something"));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, argPrincipal.Identity.AuthenticationType,
                JwtClaimTypes.Name, JwtClaimTypes.Role));
        }
    }

    
}
