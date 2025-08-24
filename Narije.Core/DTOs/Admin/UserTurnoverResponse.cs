using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.Core.DTOs.Admin
{
    public class UserTurnoverResponse
    {
        public DateTime datetime { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public int? id { get; set; }
        public int? reserveId { get; set; }
        public string title { get; set; }
        public decimal bankValue { get; set; }
        public decimal walletValue { get; set; }
        public decimal remValue { get; set; }
        public string gateway { get; set; }
        public string state { get; set; }
        public string pan { get; set; }
        public string result { get; set; }
        public string refNumber { get; set; }
        public decimal remain { get; set; }
        public int type { get; set; }

    }
}
