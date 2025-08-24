using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumWalletOp
    {
        [Display(Name = "مصرف")]
        Debit = 0,
        [Display(Name = "شارژ")]
        Credit = 1,
        [Display(Name = "شارژ داخلی")]
        AdminCredit = 2,
        [Display(Name = "شارژ اعتبار ماهانه")]
        SystemCredit = 5,
        [Display(Name = "عودت وجه")]
        Refund = 3,
        [Display(Name = "عودت بابت پس دادن خرید")]
        Revoke = 4,
        [Display(Name = "عودت بابت اتمام اعتبار ماهانه")]
        CreditRevoke = 6,
    }
}
