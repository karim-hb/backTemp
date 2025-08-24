using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Food
    {
        public Food()
        {
            InvoiceDetails = new HashSet<InvoiceDetail>();
            FoodPrices = new HashSet<FoodPrice>();
            Menus = new HashSet<Menu>();
            Reserves = new HashSet<Reserve>();
            Surveys = new HashSet<Survey>();
            MenuLogs = new HashSet<MenuLog>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
        public bool Active { get; set; }
        public int? GalleryId { get; set; }
        public bool IsDaily { get; set; }
        public bool HasType { get; set; }
        public bool IsGuest { get; set; }
        public int EchoPrice { get; set; }
        public int SpecialPrice { get; set; }
        public int? Vat { get; set; }
        public int? ProductType { get; set; }
        public string ArpaNumber { get; set; }
        public bool Vip { get; set; }
        public bool IsFood { get; set; }

        public virtual Gallery Gallery { get; set; }
        public virtual FoodGroup Group { get; set; }
       // public virtual FoodType Type { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Reserve> Reserves { get; set; }
        public virtual ICollection<FoodPrice> FoodPrices { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual ICollection<Survey> Surveys { get; set; }
        public virtual ICollection<MenuLog> MenuLogs { get; set; }
    }
}
