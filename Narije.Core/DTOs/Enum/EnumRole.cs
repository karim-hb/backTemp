using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumRole
    {
        [Display(Name = "کاربر")]
        user = 0,
        [Display(Name = "ادمین شرکت")]
        customer = 1,
        [Display(Name = "ادمین سایت")]
        supervisor = 2,
        [Display(Name = "مدیر کل")]
        superadmin = 3,
    }
}
