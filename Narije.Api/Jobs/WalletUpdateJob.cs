using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.EntityFrameworkCore;
using Narije.Core.Interfaces;

namespace Narije.Api.Jobs
{
    public class WalletUpdateJob
    {
        private readonly IWalletRepository _walletRepository;
        /// <summary>
        /// افزودن اعتبار به کیف پول کاربر اول هر ماه
        /// </summary>
        /// <param name="context"></param>

      
    }
}
