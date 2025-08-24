using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumReserveState
    {
        [Display(Name = "عادی")]
        normal = 0,
        [Display(Name = "کنسل شده")]
        canceled = 1,
        [Display(Name = "مهمان")]
        guest = 2,
        [Display(Name = "ناریجه")]
        admin = 3,
        [Display(Name = "پیش بینی")]
        perdict = 4,
    }
}
