using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumLogHistroyAction
    {
        [Display(Name = "ساخت ")]
        create = 0,
        [Display(Name = "آپدیت")]
        update = 1,
        [Display(Name = "حذف")]
        delete = 2,
    }
}
