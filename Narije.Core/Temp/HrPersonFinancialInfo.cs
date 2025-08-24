using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPersonFinancialInfo
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public int PersonId { get; set; }
        public string AccountNumber { get; set; }
        public string CardNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrBank Bank { get; set; }
        public virtual HrPerson Person { get; set; }
    }
}
