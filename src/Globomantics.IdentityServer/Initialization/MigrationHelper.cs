using System;
using System.Data.Common;
using System.Threading;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Globomantics.IdentityServer.Initialization
{
    public static class MigrationHelper
    {
        public static void ApplyDatabaseSchema(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            try
            {
                Log.Information($"Begin ApplyDatabaseSchema");
                serviceScope?.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope?.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                Log.Information($"End ApplyDatabaseSchema success");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                // If the database is not available yet just wait and try again
                var dbConnection = serviceScope?.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database
                    .GetConnectionString();
                Log.Information($"Failed performing migrations: {dbConnection}");
                Log.Information($"End ApplyDatabaseSchema fail. Sleep 15 sec and try again");
                Thread.Sleep(TimeSpan.FromSeconds(15));
                app.ApplyDatabaseSchema();
            }
        }
    }
}
