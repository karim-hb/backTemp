using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Export
{
    public class ExportResponse
    {
        public List<string> header { get; set; }
        public List<List<string>> body { get; set; }
    }
}
