using System;
using Globomantics.Api.Extenstions;
using Globomantics.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Linq;
using Globomantics.Api.DbContexts;
using Globomantics.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using AutoMapper.Configuration;

namespace Globomantics.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, provider, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Seq("http://globoseq");
            });

            //builder.Services.AddControllers();
            builder.Services.AddControllers(options =>
                {
                    options.ReturnHttpNotAcceptable = true;
                }).AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();


            builder.Services.AddHealthChecks();

            builder.Services.AddMvcCore()
                .AddCors()
                .AddApiExplorer();

            builder.Services.AddApiVersioning(options => options.ReportApiVersions = true)
                .AddVersionedApiExplorer(
                    options =>
                    {
                        options.GroupNameFormat = "'v'VVV";
                        options.SubstituteApiVersionInUrl = true;
                    });

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.Authority = builder.Configuration.GetValue<string>("AuthN:Authority");
                    options.Audience = builder.Configuration.GetValue<string>("AuthN:ApiName");
                });

            builder.Services.AddSwaggerDocumentation();  // defined locally


            builder.Services.AddDbContext<CityInfoContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("GlobomanticsDataConnection")));


            builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddApiVersioning(setupAction =>
            {
                setupAction.AssumeDefaultVersionWhenUnspecified = true;
                setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                setupAction.ReportApiVersions = true;
            });



            var app = builder.Build();


            var virtualPath = "/api";

            app.Map(virtualPath, b =>
            {
                b.UseApiExceptionHandler();  // defined locally

                b.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                var corsOrigins = builder.Configuration.GetValue<string>("CORSOrigins").Split(",");
                if (corsOrigins.Any())
                {
                    b.UseCors(c => c
                        .WithOrigins(corsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod());
                }

                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                b.UseSwaggerDocumentation(virtualPath, builder.Configuration, apiVersionDescriptionProvider);
                b.UseAuthentication();
                b.UseGlobomanticsStyleRequestLogging();
                b.UseRouting();
                b.UseAuthorization();
                b.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers().RequireAuthorization();
                    endpoints.MapHealthChecks("/health");
                });
            });

            app.Run();

            Log.CloseAndFlush();
        }
        
    }
}
