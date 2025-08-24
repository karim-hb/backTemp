using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumGateway
    {
        [Display(Name = "بانک شهر")]
        Bank = 0,
        [Display(Name = "بانک ملت")]
        Mellat = 1,
        [Display(Name = "کیف پول سایت")]
        Wallet = 2,
        [Display(Name = "زرین پال")]
        ZarinPal = 3,
    }
}
