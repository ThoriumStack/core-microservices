using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Serilog;
using Thorium.Core.DataProvider;
using Thorium.Core.MicroServices.ConfigurationModels;

namespace Thorium.Core.MicroServices.Extensions
{
    public static class DataMigrationBase
    {
        public static void AutoMigrate<TDbContext>(List<DbSettings> databaseSettings, ILogger logger, Action<TDbContext, ILogger> seeder) where TDbContext : ContextBase
        {
            try
            {
                logger.Information($"Migrating database associated with context {typeof(TDbContext).Name}");

                var settingsKey = typeof(TDbContext).Name.Substring(0, typeof(TDbContext).Name.IndexOf("Db") + 2);
                
                var dbSetting = databaseSettings.Find(c => c.Name == settingsKey);
                TDbContext context;
                if (!string.IsNullOrWhiteSpace(dbSetting.ConnectionString))
                    context = (TDbContext) Activator.CreateInstance(typeof(TDbContext), dbSetting.ConnectionString);
                else
                    context = Activator.CreateInstance<TDbContext>();
                
                var retry = Policy.Handle<NpgsqlException>()
                    .WaitAndRetry(new TimeSpan[]
                    {
                        TimeSpan.FromSeconds(3),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(8),
                    });

                retry.Execute(() =>
                {
                    //if the npg sql server container is not created on run docker compose this
                    //migration can't fail for network related exception. The retry options for DbContext only 
                    //apply to transient exceptions.

                    context.Database
                        .Migrate();

                    seeder(context, logger);
                });
                  

                logger.Information($"Migrated database associated with context {typeof(TDbContext).Name}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"An error occurred while migrating the database used on context {typeof(TDbContext).Name}");
            }
        }
    }
}