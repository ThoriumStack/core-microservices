using System.Linq;
using Thorium.Core.Model.DataModel;

namespace Thorium.Core.MicroServices.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<TContextModel> ApplyContext<TContextModel>(this IQueryable<TContextModel> qry, string context) where TContextModel : BaseContextModel
        {
            return qry.Where(c => c.Context == context);
        }
    }
}