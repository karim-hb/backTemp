using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Enum
{
    public enum EnumInvoicePayType
    {
        [Display(Name = "نقدی")]
        cash = 0,
        [Display(Name = "اعتباری")]
        debit = 1,
    }
}
