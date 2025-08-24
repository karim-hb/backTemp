using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class ReserveHelperFoodRequest
    {
        public int Id { get; set; }
        public int EchoPrice { get; set; }
        public int SpecialPrice { get; set; }
        public bool HasType { get; set; }
        public bool isFood { get; set; }
    }
}
