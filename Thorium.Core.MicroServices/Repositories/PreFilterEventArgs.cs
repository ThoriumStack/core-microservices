using System;
using System.Linq;

namespace Thorium.Core.MicroServices.Repositories
{
    public class PreFilterEventArgs :
        EventArgs
    {
        private IQueryable<object> _filtered;
        
        public PreFilterEventArgs(IQueryable<object> models, Type type)
        {
            Original = models;
            Type = type;

            IsFiltered = false;
        }

        public IQueryable<object> Original { get; private set; }

        public IQueryable<object> Filtered
        {
            get
            {
                if (_filtered == null)
                {
                    return Original;
                }

                return _filtered;
            }
            set
            {
                IsFiltered = true;

                _filtered = value;
            }
        }

        public Type Type { get; private set; }

        public bool IsFiltered { get; private set; }
    }
}