﻿using System;
using System.Collections.Generic;
using System.Linq;
using MyBucks.Core.Model;
using Serilog;

namespace MyBucks.Core.MicroServices
{
    public abstract class ServiceBase
    {
        private ILogger _logger;

        protected ServiceBase(ILogger logger)
        {
            _logger = logger;
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
            
            parmList.Add(errorId);
            
            _logger.Fatal(ex, msg, parmList);
            ReplyBase val = new TReply();

            val.ReplyMessage = $"{msg} Error Id: {errorId}";
            
            return (TReply)val;
        }
    }
}