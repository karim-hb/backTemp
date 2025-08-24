using Narije.Core.Interfaces;
using System.Threading.Tasks;

namespace Narije.Api.Jobs
{
    public class CustomerUpdateJob
    {
        private readonly ICustomerRepository _customerRepository;
        /// <summary>
        /// برسی شرکت ها در صورت انقضا تاریخ قرار داد اون شرکت غیر فعال می شود
        /// </summary>
        /// <param name="context"></param>

        public CustomerUpdateJob(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;


        }

        public async Task Run()
        {



            await _customerRepository.UpdateCustomersAsync();


        }
    }
}
