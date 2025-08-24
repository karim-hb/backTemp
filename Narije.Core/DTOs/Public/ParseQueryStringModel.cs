using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class ParseQueryStringModel
    {
        public ParseQueryStringModel()
        {
            FilterGroups = new();
        }

        public List<FilterGroupModel> FilterGroups { get; set; }
    }

    public class FilterGroupModel
    {
        public string Key { get; set; }

        public List<object> Values { get; set; }
    }
}
