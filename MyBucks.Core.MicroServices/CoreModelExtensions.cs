using System.Collections.Generic;
using System.Linq;
using MyBucks.Core.Model;
using Thorium.FluentDefense;

namespace MyBucks.Core.MicroServices
{
    public static class CoreModelExtensions
    {
        public static T AddValidationMessages<T>(this T baseReply, DefenderBase defender) where T : ReplyBase
        {
            baseReply.ReplyMessage = defender.ErrorMessage;
            baseReply.ReplyStatus = ReplyStatus.InvalidInput;
            return baseReply;
        }
        
        public static T AddValidationMessages<T>(this T baseReply, IEnumerable<DefenderBase> defenders) where T : ReplyBase
        {
            foreach (var defender in defenders)
            {
                baseReply.ReplyMessage += $"{defender.ErrorMessage}\n";    
            }
            
            baseReply.ReplyStatus = ReplyStatus.InvalidInput;
            return baseReply;
        }
        
        public static T AddValidationMessages<T>(this T baseReply, params DefenderBase[] defenders) where T : ReplyBase
        {
            foreach (var defender in defenders)
            {
                baseReply.ReplyMessage += $"{defender.ErrorMessage}\n";    
            }
            
            baseReply.ReplyStatus = ReplyStatus.InvalidInput;
            return baseReply;
        }

        public static bool AnyErrors(this IEnumerable<DefenderBase> defenders)
        {
            return defenders.Any(c => !c.IsValid);
        } 
    }
}