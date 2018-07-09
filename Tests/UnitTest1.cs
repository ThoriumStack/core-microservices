using System;
using System.Collections.Generic;
using System.Linq;
using MyBucks.Core.MicroServices.ConfigurationModels;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            AddConfiguration<List<WebServiceSettings>>();
        }
        
        public void AddConfiguration<TConfiguration>() where TConfiguration : class
        {
            var oType = typeof(TConfiguration);
            var isList =(oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
            
            var typeName = typeof(TConfiguration).Name;
            if (isList)
            {
                typeName = oType.GenericTypeArguments.First().Name;
            }
            
            
            
        }
    }
}