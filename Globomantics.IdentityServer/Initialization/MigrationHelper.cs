﻿using System;
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
                serviceScope?.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope?.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
            }
            catch (Exception ex)
            {
                // If the database is not available yet just wait and try again
                var dbConnection = serviceScope?.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database
                    .GetConnectionString();
                Log.Error($"Failed performing migrations: {dbConnection} {ex}");

                Thread.Sleep(TimeSpan.FromSeconds(15));
                app.ApplyDatabaseSchema();
            }
        }
    }
}
