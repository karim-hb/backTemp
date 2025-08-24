using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.Entities;
using System.Security.Cryptography;

namespace Narije.Infrastructure.Helpers
{
    public static class SecurityHelper
    {
        public static string WalletGenerateKey(Wallet wallet)
        {
            var str = $"{wallet.PreValue}-{wallet.Value}-{wallet.RemValue}-{wallet.UserId}-{wallet.Op}";

            str = BCrypt.Net.BCrypt.HashPassword(str);

            return str;
        }
        public static bool WalletCheckKey(Wallet wallet)
        {
            var str = $"{wallet.PreValue}-{wallet.Value}-{wallet.RemValue}-{wallet.UserId}-{wallet.Op}";

            return BCrypt.Net.BCrypt.Verify(str, wallet.Opkey);

        }
    }
}
