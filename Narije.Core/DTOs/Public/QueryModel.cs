using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class QueryModel
    {
        public List<FilterModel> Filter { get; set; }
        public List<SortModel> Sort { get; set; }
        public string Search { get; set; }
    }
}
