using System;
using System.Collections.Generic;
using System.Linq;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.Model;
using MyBucks.Core.Model.Abstractions;
using Serilog;

namespace MyBucks.Core.MicroServices.Services
{
    public abstract class ServiceBase : IServiceBase
    {
        protected readonly ILogger _logger;
        private readonly IRepositoryBase[] _repositories;
        private readonly IServiceBase[] _services;

        protected ServiceBase(ILogger logger, params IRepositoryBase[] repositories) : this (logger, null, repositories)
        { }

        protected ServiceBase(ILogger logger, IServiceBase[] services, params IRepositoryBase[] repositories)
        {
            _logger = logger;
            _repositories = repositories;
            _services = services;
        }

        private string _currentUserId;
        public string CurrentUserId
        {
            get => _currentUserId;
            set
            {
                _currentUserId = value;
                _repositories?.ToList().ForEach(x => x.CurrentUserId = _currentUserId);
                _services?.ToList().ForEach(x => x.CurrentUserId = _currentUserId);
            }
        }

        private string _currentContext;

        public string CurrentContext
        {
            get => _currentContext;
            set
            {
                _currentContext = value;
                _repositories?.ToList().ForEach(x => x.CurrentContext = _currentContext);
                _services?.ToList().ForEach(x => x.CurrentContext = _currentContext);
            }
        }

        
        private int _currentTimeZoneOffset;

        public int CurrentTimeZoneOffset
        {
            get => _currentTimeZoneOffset;
            set
            {
                _currentTimeZoneOffset = value;
                _repositories?.ToList().ForEach(x => x.CurrentTimeZoneOffset = _currentTimeZoneOffset);
                _services?.ToList().ForEach(x => x.CurrentTimeZoneOffset = _currentTimeZoneOffset);
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
                val.ReplyStatus = ReplyStatus.Failed;
                val.ReplyMessage = $"{msg} Error Id: {errorId}";
            }

            return (TReply)val;
        }
    }
}