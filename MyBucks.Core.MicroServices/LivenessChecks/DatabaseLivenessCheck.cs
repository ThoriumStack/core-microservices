using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.ConfigurationModels;
using Serilog;

namespace MyBucks.Core.MicroServices.LivenessChecks
{
    public class DatabaseLivenessCheck<TDbContext> : ILivenessCheck where TDbContext : DbContext
    {
        private readonly List<DbSettings> _databaseSettings;

        public DatabaseLivenessCheck(List<DbSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }

        public bool IsLive()
        {

            
            //Console.WriteLine(typeof(TDbContext).Name);
            var settingsKey = typeof(TDbContext).Name.Substring(0, typeof(TDbContext).Name.IndexOf("Db") + 2);

            var dbSetting = _databaseSettings.Find(c => c.Name == settingsKey);
            TDbContext context;
            if (!string.IsNullOrWhiteSpace(dbSetting.ConnectionString))
                context = (TDbContext) Activator.CreateInstance(typeof(TDbContext), dbSetting.ConnectionString);
            else
                context = Activator.CreateInstance<TDbContext>();

            try
            {
                context.Database.ExecuteSqlNonQuery("select 1");
            }
            catch (Exception exception)
            {
                return false;
            }

            return true;


        }
    }

}