using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumFileType
    {
        [Display(Name = "xlsx")]
        xlsx = 0,
        [Display(Name = "pdf")]
        pdf = 1,
    }
}
