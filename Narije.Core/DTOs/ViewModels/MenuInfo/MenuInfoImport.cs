using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.MenuInfo
{
    public class MenuInfoImport
    {
        public int id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public bool? active { get; set; }
    }
}
