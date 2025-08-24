using Narije.Core.DTOs.Enum;
using Narije.Core.Entities;
using System.Threading.Tasks;
using System;
using Narije.Core.DTOs.User;

namespace Narije.Api.Helpers
{
    public class PriceHelper
    {

        public async Task<int> GetPriceForMenu(Customer customer, object menu,bool vip)
        {
            int menuPrice = 0;

            if (menu is Menu menuEntity)
            {
                menuPrice = menuEntity.EchoPrice ??
                           (menuEntity.Food.EchoPrice != 0 ? menuEntity.Food.EchoPrice :
                           (menuEntity.SpecialPrice ?? menuEntity.Food.SpecialPrice));

                if (customer.PriceType == (int)EnumPrice.average)
                {
                    return customer.AvragePrice ?? 0;
                }
                else if (customer.PriceType == (int)EnumPrice.fromMenu)
                {
                    return menuPrice;
                }
                else
                {
                    return vip ? menuPrice : customer.AvragePrice ?? 0;
                }
            }
            else if (menu is ReserveResponse reserveResponse)
            {
                menuPrice = reserveResponse.echoPrice != 0 ? reserveResponse.echoPrice :
                             reserveResponse.specialPrice;


                if (customer.PriceType == (int)EnumPrice.average)
                {
                    return customer.AvragePrice ?? 0;
                }
                else if (customer.PriceType == (int)EnumPrice.fromMenu)
                {
                    return menuPrice;
                }
                else
                {
                    return vip? menuPrice : customer.AvragePrice ?? 0;
                }
            }
            else if (menu is Food food)
            {
                menuPrice = food.EchoPrice !=0 ? food.EchoPrice :
                             food.SpecialPrice;

                if (customer.PriceType == (int)EnumPrice.average)
                {
                    return customer.AvragePrice ?? 0;
                }
                else if (customer.PriceType == (int)EnumPrice.fromMenu)
                {
                    return menuPrice;
                }
                else
                {
                    return vip ? menuPrice : customer.AvragePrice ?? 0;
                }
            }
            else
            {
                throw new ArgumentException("Invalid menu type");
            }
        }
    }
}
