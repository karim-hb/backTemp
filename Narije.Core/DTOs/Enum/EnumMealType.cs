using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumMealType
    {
        [Display(Name = "نهار")]
        lunch = 0,
        [Display(Name = "شام")]
        dinner = 1,
        [Display(Name = "صبحانه")]
         dinnerAndLunch = 2,
    }
}
