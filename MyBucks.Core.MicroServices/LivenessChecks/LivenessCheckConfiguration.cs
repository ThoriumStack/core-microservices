using System;
using System.Collections.Generic;
using System.Linq;
using MyBucks.Core.MicroServices.Abstractions;
using SimpleInjector;

namespace MyBucks.Core.MicroServices.LivenessChecks
{
    public class LivenessCheckConfiguration : ILiveChecker
    {
        private readonly Container _cont;
        private readonly List<Type> _checks= new List<Type>();


        public LivenessCheckConfiguration(Container cont)
        {
            _cont = cont;
            
           
        }

        public void AddCheck<TCheckType>() where TCheckType : class, ILivenessCheck
        {
            
            
            _checks.Add(typeof(TCheckType));
        }


        public void Build()
        {
           _cont.Collection.Register<ILivenessCheck>(_checks.ToArray());
        }

        public bool RunChecks()
        {
            var result = _cont.GetAllInstances<ILivenessCheck>().All(c => c.IsLive());

            return result;
        }
    }
}