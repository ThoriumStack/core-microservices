using System;
using System.Collections.Generic;
using System.Linq;
using MyBucks.Core.MicroServices.Abstractions;
using SimpleInjector;

namespace MyBucks.Core.MicroServices.LivenessChecks
{
    public class LivenessCheckConfiguration
    {
        private readonly Container _cont;


        public LivenessCheckConfiguration(Container cont)
        {
            _cont = cont;
        }

        public void AddCheck<TCheckType>() where TCheckType : class, ILivenessCheck
        {
            _cont.Collection.Append<ILivenessCheck, TCheckType>();
        }

      

        public bool RunChecks()
        {
            var result = _cont.GetAllInstances<ILivenessCheck>().All(c => c.IsLive());

            return result;
        }
    }
}