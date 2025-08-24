using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Narije.Core.Entities;

namespace Narije.Infrastructure.Contexts
{
    public partial class NarijeDBContext : DbContext
    {
        public NarijeDBContext()
        {
        }

        public NarijeDBContext(DbContextOptions<NarijeDBContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Accessory> Accessory { get; set; }
        public virtual DbSet<AccessPermission> AccessPermissions { get; set; }
        public virtual DbSet<AccessProfile> AccessProfiles { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Credit> Credits { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Core.Entities.Enum> Enums { get; set; }
        public virtual DbSet<Food> Foods { get; set; }
        public virtual DbSet<FoodGroup> FoodGroups { get; set; }
        public virtual DbSet<FoodPrice> FoodPrices { get; set; }
        public virtual DbSet<vCustomerFoodPrice> vCustomerFoodPrices { get; set; }
        public virtual DbSet<vFoodPrice> vFoodPrices { get; set; }
        public virtual DbSet<Gallery> Galleries { get; set; }
        public virtual DbSet<Header> Headers { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<LoginImage> LoginImage { get; set; }
        public virtual DbSet<LogHistory> LogHistory { get; set; }
        public virtual DbSet<Wallet> Wallets { get; set; }
        public virtual DbSet<WalletPayment> WalletPayments { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<Reserve> Reserves { get; set; }
        public virtual DbSet<vReserve> vReserves { get; set; }
        public virtual DbSet<Search> Searches { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Permission> Premissions { get; set; }
        public virtual DbSet<SurveryValue> SurveryValues { get; set; }
        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<vSurvey> vSurveys { get; set; }
        public virtual DbSet<SurveyDetail> SurveyDetails { get; set; }
        public virtual DbSet<SurveyItem> SurveyItems { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<VCustomer> VCustomers { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<Tutorial> Tutorial { get; set; }
        public virtual DbSet<Meal> Meal { get; set; }
        public virtual DbSet<Settlement> Settlement { get; set; }
        public virtual DbSet<Dish> Dish { get; set; }
        public virtual DbSet<Recipt> Recipt { get; set; }

        public virtual DbSet<AccessoryCompany> AccessoryCompany { get; set; }
        public virtual DbSet<CompanyMeal> CompanyMeal { get; set; }
        public virtual DbSet<Branch> Branch { get; set; }
        public virtual DbSet<FoodType> FoodType { get; set; }
        public virtual DbSet<MenuInfo> MenuInfo { get; set; }
        public virtual DbSet<CustomerMenuInfo> CustomerMenuInfo { get; set; }
        public virtual DbSet<MenuLog> MenuLogs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo")
                        .HasAnnotation("Relational:Collation", "Persian_100_CI_AS");

            #region Soft Delete
            modelBuilder.Entity<Gallery>().HasQueryFilter(b => b.Hidden == false);
            #endregion

            #region Edit Model Builder
            modelBuilder.Entity<AccessPermission>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Access)
                    .WithMany(p => p.AccessPermissions)
                    .HasForeignKey(d => d.AccessId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccessPermissions_AccessProfiles");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.AccessPermissions)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccessPermissions_Premissions");
            });

            modelBuilder.Entity<AccessProfile>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("City");

                entity.Property(e => e.Code).HasMaxLength(20);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Cities)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_City_Province");
            });
            modelBuilder.Entity<Credit>(entity =>
            {
                entity.ToTable("Credit");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTime);

                entity.Property(e => e.Value).IsRequired();

                entity.Property(e => e.Riched);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Credits)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Credit_Customers");
            });
            modelBuilder.Entity<MenuInfo>(entity =>
            {
                entity.ToTable("MenuInfo");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.Month);

                entity.Property(e => e.Year);

                entity.Property(e => e.Active)
                      .IsRequired();

                entity.Property(e => e.LastUpdaterUserId)
                      .HasColumnName("LastUpdaterUserId");

                entity.Property(e => e.UpdatedAt);

                entity.HasOne(d => d.LastUpdaterUser)
                      .WithMany(p => p.MenuInfos)
                      .HasForeignKey(d => d.LastUpdaterUserId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("FK_MenuInfo_Users");
            });
            modelBuilder.Entity<MenuLog>(entity =>
            {
                entity.ToTable("MenuLog");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FoodId)
                      .IsRequired();

                entity.Property(e => e.UserId)
                      .IsRequired();

                entity.Property(e => e.MenuInfoId)
                      .IsRequired();

                entity.Property(e => e.DateTime)
                      .IsRequired();

                entity.Property(e => e.EchoPriceBefore)
                      .IsRequired();

                entity.Property(e => e.EchoPriceAfter)
                     .IsRequired();

                entity.Property(e => e.SpecialPriceBefore)
                     .IsRequired();

                entity.Property(e => e.SpecialPriceAfter)
                     .IsRequired();

                entity.HasOne(d => d.Food)
                      .WithMany(p => p.MenuLogs)
                      .HasForeignKey(d => d.FoodId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("FK_MenuLog_Food");

                entity.HasOne(d => d.User)
                      .WithMany(p => p.MenuLogs)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("FK_MenuLog_Users");

                entity.HasOne(d => d.MenuInfo)
                      .WithMany(p => p.MenuLogs)
                      .HasForeignKey(d => d.MenuInfoId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("FK_MenuLog_MenuInfo");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.ContractStartDate).HasColumnType("datetime");

                entity.Property(e => e.ContractEndDate).IsRequired(false);

                entity.Property(e => e.AddCreditToPrevCredit).IsRequired(false);

                entity.Property(e => e.EconomicCode).HasMaxLength(30);

                entity.Property(e => e.Mobile).HasMaxLength(30);

                entity.Property(e => e.NationalId).HasMaxLength(30);

                entity.Property(e => e.PostalCode).HasMaxLength(30);

                entity.Property(e => e.RegNumber).HasMaxLength(30);

                entity.Property(e => e.ReserveTime).HasDefaultValueSql("('10:00:00.0000000')");

                entity.Property(e => e.Tel).HasMaxLength(80);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasOne(d => d.City)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_Customer_City");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_Customer_Province");
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");

                entity.HasOne(d => d.Parent)
               .WithMany(p => p.Childrens)
               .HasForeignKey(d => d.ParentId);

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Core.Entities.Enum>(entity =>
            {
                entity.ToTable("Enum");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Style).HasMaxLength(80);

                entity.Property(e => e.StyleDark).HasMaxLength(80);

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("Wallet");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTime).HasColumnType("datetime");
                entity.Property(e => e.LastCredit).HasColumnType("datetime").IsRequired(false);

                entity.Property(e => e.Op).HasColumnName("OP");

                entity.Property(e => e.Opkey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("OPKEY");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Wallets)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Wallet_Users");
            });

            modelBuilder.Entity<WalletPayment>(entity =>
            {
                entity.ToTable("WalletPayment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ConsumeCode).HasMaxLength(5);

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.Op).HasColumnName("OP");

                entity.Property(e => e.Pan)
                    .HasMaxLength(20)
                    .HasColumnName("PAN");

                entity.Property(e => e.RefNumber).HasMaxLength(80);

                entity.Property(e => e.Result).HasMaxLength(80);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.WalletPayments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletPayment_Users");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.WalletPayments)
                    .HasForeignKey(d => d.WalletId)
                    .HasConstraintName("FK_WalletPayment_Wallet");
            });

            modelBuilder.Entity<Food>(entity =>
            {
                entity.ToTable("Food");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.HasType)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasOne(d => d.Gallery)
                    .WithMany(p => p.Foods)
                    .HasForeignKey(d => d.GalleryId)
                    .HasConstraintName("FK_Food_Gallery");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Foods)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Food_FoodGroup");
            });

            modelBuilder.Entity<FoodGroup>(entity =>
            {
                entity.ToTable("FoodGroup");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasOne(d => d.Gallery)
                    .WithMany(p => p.FoodGroups)
                    .HasForeignKey(d => d.GalleryId)
                    .HasConstraintName("FK_FoodGroup_Gallery");

                entity.HasMany(fg => fg.Foods)
                 .WithOne(f => f.Group)
                 .HasForeignKey(f => f.GroupId)
                 .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK_Food_FoodGroup");
            });

            modelBuilder.Entity<FoodPrice>(entity =>
            {
                entity.ToTable("FoodPrice");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.FoodPrices)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FoodPrice_Customer");

                entity.HasOne(d => d.Food)
                    .WithMany(p => p.FoodPrices)
                    .HasForeignKey(d => d.FoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FoodPrice_Food");
            });

            modelBuilder.Entity<vCustomerFoodPrice>(entity =>
            {
                entity.ToTable("vCustomerFoodPrice");

                entity.Property(e => e.Id).HasColumnName("id");

            });

            modelBuilder.Entity<vFoodPrice>(entity =>
            {
                entity.ToTable("vFoodPrice");

                entity.Property(e => e.Id).HasColumnName("id");

            });

            modelBuilder.Entity<Gallery>(entity =>
            {
                entity.ToTable("Gallery");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alt).HasMaxLength(80);

                entity.Property(e => e.OriginalFileName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.SystemFileName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Header>(entity =>
            {
                entity.ToTable("Header");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ColumnSpan).HasDefaultValueSql("((1))");

                entity.Property(e => e.ColumnType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValueSql("('string')");

                entity.Property(e => e.DefaultFilter).HasMaxLength(50);

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Link).HasMaxLength(100);

                entity.Property(e => e.Style).HasMaxLength(80);

                entity.Property(e => e.StyleDark).HasMaxLength(80);

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoice");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.Serial)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.ToDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoice_Customer");
            });

            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                entity.ToTable("InvoiceDetail");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Food)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(d => d.FoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InvoiceDetail_Food");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InvoiceDetail_Invoice");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu");

                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Menus)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Menu_Customer");

                entity.HasOne(d => d.Food)
                    .WithMany(p => p.Menus)
                    .HasForeignKey(d => d.FoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Menu_Food");
            });

            modelBuilder.Entity<LogHistory>(entity =>
            {
                entity.ToTable("LogHistory");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.EntityName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("EntityName");

                entity.Property(e => e.Changed)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)")
                    .HasColumnName("Changed");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.LogHistory)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LogHistory_Users");
            });


            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("Province");

                entity.Property(e => e.Code).HasMaxLength(10);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Reserve>(entity =>
            {
                entity.ToTable("Reserve");

                entity.HasIndex(e => new { e.UserId, e.CustomerId, e.FoodId, e.State, e.FoodType, e.DateTime, e.ReserveType }, "IX_Reserve")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.FoodType).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Reserves)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reserve_Customer");

                entity.HasOne(d => d.Food)
                    .WithMany(p => p.Reserves)
                    .HasForeignKey(d => d.FoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reserve_Food");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reserves)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reserve_Users");
            });

            modelBuilder.Entity<vReserve>(entity =>
            {
                entity.ToTable("vReserve");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            });

            modelBuilder.Entity<Search>(entity =>
            {
                entity.ToTable("Search");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.Property(e => e.FieldType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasMaxLength(80);
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.ToTable("Setting");

                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.CompanyName).HasMaxLength(80);

                entity.Property(e => e.ContactMobile).HasMaxLength(50);

                entity.Property(e => e.EconomicCode).HasMaxLength(20);

                entity.Property(e => e.NationalId).HasMaxLength(20);

                entity.Property(e => e.PostalCode).HasMaxLength(20);

                entity.Property(e => e.RegNumber).HasMaxLength(10);

                entity.Property(e => e.Tel).HasMaxLength(30);

                entity.HasOne(d => d.City)
                    .WithMany(p => p.Settings)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_Setting_City");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Settings)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_Setting_Province");
            });

            modelBuilder.Entity<SurveryValue>(entity =>
            {
                entity.ToTable("SurveryValue");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.SurveryValues)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveryValue_SurveyItem");
            });

            modelBuilder.Entity<Survey>(entity =>
            {
                entity.ToTable("Survey");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Surveys)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Survey_Customer");

                entity.HasOne(d => d.Food)
                    .WithMany(p => p.Surveys)
                    .HasForeignKey(d => d.FoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Survey_Food");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Surveys)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Survey_Users");
            });

            modelBuilder.Entity<vSurvey>(entity =>
            {
                entity.ToTable("vSurvey");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTime).HasColumnType("datetime");
                entity.Property(e => e.ReserveTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<SurveyDetail>(entity =>
            {
                entity.ToTable("SurveyDetail");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(d => d.Survey)
                    .WithMany(p => p.SurveyDetails)
                    .HasForeignKey(d => d.SurveyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyDetail_Survey");

                entity.HasOne(d => d.SurveyItem)
                    .WithMany(p => p.SurveyDetails)
                    .HasForeignKey(d => d.SurveyItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyDetail_SurveyItem");

                entity.HasOne(d => d.SurveyValue)
                    .WithMany(p => p.SurveyDetails)
                    .HasForeignKey(d => d.SurveyValueId)
                    .HasConstraintName("FK_SurveyDetail_SurveryValue");
            });

            modelBuilder.Entity<SurveyItem>(entity =>
            {
                entity.ToTable("SurveyItem");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.Property(e => e.Value).HasMaxLength(1000);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Fname)
                    .HasMaxLength(80)
                    .HasColumnName("FName");

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Lname)
                    .HasMaxLength(80)
                    .HasColumnName("LName");

                entity.Property(e => e.Description).HasMaxLength(250)
                     .HasColumnName("Description");

                entity.Property(e => e.Mobile).HasMaxLength(20);

                entity.Property(e => e.Password).HasMaxLength(150);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Users_Customer");
            });

            modelBuilder.Entity<VCustomer>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCustomer");

                entity.Property(e => e.Address).HasMaxLength(300);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.ContractStartDate).HasColumnType("datetime");

                entity.Property(e => e.ContractEndDate).IsRequired(false);

                entity.Property(e => e.AddCreditToPrevCredit).IsRequired(false);

                entity.Property(e => e.EconomicCode).HasMaxLength(30);

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Mobile).HasMaxLength(30);

                entity.Property(e => e.NationalId).HasMaxLength(30);


                entity.Property(e => e.PostalCode).HasMaxLength(30);

                entity.Property(e => e.Province).HasMaxLength(50);

                entity.Property(e => e.RegNumber).HasMaxLength(30);

                entity.Property(e => e.Tel).HasMaxLength(80);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);
            });


            modelBuilder.Entity<Recipt>(entity =>
            {
                entity.ToTable("Recipts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id");

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CustomerId)
                    .IsRequired(false);

                entity.Property(e => e.CustomerParentId)
                    .IsRequired(false);

                entity.Property(e => e.ReserveIds)
                    .HasMaxLength(300);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.FileType)
                    .IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Customer)
                    .WithMany()
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.CustomerParent)
                    .WithMany()
                    .HasForeignKey(d => d.CustomerParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            #endregion

            #region Data Seeding
            #endregion

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<NarijeDBContext>
    {
        public NarijeDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NarijeDBContext>();
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS; Database=tmdb; User Id=tm; Password=ameif2pnvsbghdclxqjk; Connection Timeout=120;")
                          .UseLazyLoadingProxies()
                          .LogTo(message => Debug.WriteLine(message), LogLevel.Debug, DbContextLoggerOptions.DefaultWithLocalTime | DbContextLoggerOptions.SingleLine)
                          .EnableSensitiveDataLogging();

            return new NarijeDBContext(optionsBuilder.Options);
        }
    }
}