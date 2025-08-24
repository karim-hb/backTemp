using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Narije.Core.DTOs.Enum
{
    public enum EnumLogHistorySource
    {
        [Display(Name = "از طریق سایت")]
        site = 0,
        [Display(Name = "از طریق اکسل")]
         excel = 1,
    
     }
}
