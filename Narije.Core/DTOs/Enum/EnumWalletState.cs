using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TM.Core.DTOs.Enum
{
    public enum EnumWalletState
    {
        [Display(Name = "در انتظار")]
        Pending = 0,
        [Display(Name = "تایید شده")]
        Accepted = 1,
        [Display(Name = "رد شده")]
        Rejected = 2,
    }
}
