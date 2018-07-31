using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.Model;
using MyBucks.Core.Model.Abstractions;
using Serilog;

namespace MyBucks.Core.MicroServices.Services
{
    public abstract class ServiceBase : IServiceBase
    {
        private readonly ILogger _logger;
        private readonly IRepositoryBase[] _repositories;

        protected ServiceBase(ILogger logger, params IRepositoryBase[] repository)
        {
            _logger = logger;
            _repositories = repository;
        }

        private string _currentUserId;
        public string CurrentUserId
        {
            get => _currentUserId;
            set
            {
                _currentUserId = value;
                foreach (var repositoryBase in _repositories)
                {
                    repositoryBase.CurrentUserId = _currentUserId;
                }
            }
        }

        private string _currentContext;

        public string CurrentContext
        {
            get => _currentContext;
            set
            {
                _currentContext = value;
                foreach (var repositoryBase in _repositories)
                {
                    repositoryBase.CurrentContext = _currentContext;
                };
            }
        }
        
        protected T ReplyFromFatal<T>(Exception ex, string msg) where T : ReplyBase, new()
        {
            return ReplyFromFatal<T>(ex, msg, null);
        }

        /// <summary>
        /// Log and return a friendly error message from a service method.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg">Friendly error message</param>
        /// <param name="parms">Additional parameters to SeriLog</param>
        /// <typeparam name="TReply"></typeparam>
        /// <returns></returns>
        protected TReply ReplyFromFatal<TReply>(Exception ex, string msg, params object[] parms) where TReply : ReplyBase, new()
        {
            var errorId = Guid.NewGuid();

            var parmList = parms?.ToList() ?? new List<object>();
            ReplyBase val = new TReply();
            parmList.Add(errorId);
            if (ex is ArgumentException)
            {
                val.ReplyStatus = ReplyStatus.InvalidInput;
                val.ReplyMessage = ex.Message;
            }
            else
            {
                _logger.Fatal(ex, msg, parmList);
                val.ReplyMessage = $"{msg} Error Id: {errorId}";
            }

            return (TReply)val;
        }
    }
}