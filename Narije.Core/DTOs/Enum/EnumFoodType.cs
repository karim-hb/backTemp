using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumFoodType
    {
        [Display(Name = "اکو")]
        echo = 0,
        [Display(Name = "ویژه")]
        special = 1,
    }
}
