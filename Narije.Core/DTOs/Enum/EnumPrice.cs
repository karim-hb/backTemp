using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumPrice
    {

        [Display(Name = "میانگین قیمت")]
        average = 1,
        [Display(Name = "قیمت تفکیکی")]
        fromMenu = 2,
        [Display(Name = "قیمت تلفیقی")]
        justVipFromMenu = 3,
   
    }
}
