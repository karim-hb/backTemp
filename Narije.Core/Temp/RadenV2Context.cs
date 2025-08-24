using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Raden.Core.Temp
{
    public partial class RadenV2Context : DbContext
    {
        public RadenV2Context()
        {
        }

        public RadenV2Context(DbContextOptions<RadenV2Context> options)
            : base(options)
        {
        }

        public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
        public virtual DbSet<FeedingDateFoodMeal> FeedingDateFoodMeals { get; set; }
        public virtual DbSet<FeedingFood> FeedingFoods { get; set; }
        public virtual DbSet<FeedingMeal> FeedingMeals { get; set; }
        public virtual DbSet<FeedingReserf> FeedingReserves { get; set; }
        public virtual DbSet<FeedingSelf> FeedingSelfs { get; set; }
        public virtual DbSet<HrBank> HrBanks { get; set; }
        public virtual DbSet<HrCity> HrCities { get; set; }
        public virtual DbSet<HrCompany> HrCompanies { get; set; }
        public virtual DbSet<HrCountry> HrCountries { get; set; }
        public virtual DbSet<HrForm> HrForms { get; set; }
        public virtual DbSet<HrFormField> HrFormFields { get; set; }
        public virtual DbSet<HrInsuranceType> HrInsuranceTypes { get; set; }
        public virtual DbSet<HrModule> HrModules { get; set; }
        public virtual DbSet<HrNotification> HrNotifications { get; set; }
        public virtual DbSet<HrNotificationUser> HrNotificationUsers { get; set; }
        public virtual DbSet<HrPermission> HrPermissions { get; set; }
        public virtual DbSet<HrPermissionRole> HrPermissionRoles { get; set; }
        public virtual DbSet<HrPerson> HrPersons { get; set; }
        public virtual DbSet<HrPersonContract> HrPersonContracts { get; set; }
        public virtual DbSet<HrPersonFinancialInfo> HrPersonFinancialInfos { get; set; }
        public virtual DbSet<HrPersonFormField> HrPersonFormFields { get; set; }
        public virtual DbSet<HrPersonInsurance> HrPersonInsurances { get; set; }
        public virtual DbSet<HrPhone> HrPhones { get; set; }
        public virtual DbSet<HrPosition> HrPositions { get; set; }
        public virtual DbSet<HrProvince> HrProvinces { get; set; }
        public virtual DbSet<HrRecruitmentType> HrRecruitmentTypes { get; set; }
        public virtual DbSet<HrRole> HrRoles { get; set; }
        public virtual DbSet<HrRoleUser> HrRoleUsers { get; set; }
        public virtual DbSet<HrRule> HrRules { get; set; }
        public virtual DbSet<HrRulePack> HrRulePacks { get; set; }
        public virtual DbSet<HrRulePackRule> HrRulePackRules { get; set; }
        public virtual DbSet<HrRuleSubModule> HrRuleSubModules { get; set; }
        public virtual DbSet<HrSetting> HrSettings { get; set; }
        public virtual DbSet<HrSettingPack> HrSettingPacks { get; set; }
        public virtual DbSet<HrType> HrTypes { get; set; }
        public virtual DbSet<HrUser> HrUsers { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<RqReceivedRequest> RqReceivedRequests { get; set; }
        public virtual DbSet<RqRequest> RqRequests { get; set; }
        public virtual DbSet<RqRequestRulePack> RqRequestRulePacks { get; set; }
        public virtual DbSet<RqRequestRulePackPosition> RqRequestRulePackPositions { get; set; }
        public virtual DbSet<RqRequestRulePackRule> RqRequestRulePackRules { get; set; }
        public virtual DbSet<RqRequestRulePackUser> RqRequestRulePackUsers { get; set; }
        public virtual DbSet<RqRequestable> RqRequestables { get; set; }
        public virtual DbSet<RqWorkflow> RqWorkflows { get; set; }
        public virtual DbSet<RqWorkflowApprover> RqWorkflowApprovers { get; set; }
        public virtual DbSet<RqWorkflowLevel> RqWorkflowLevels { get; set; }
        public virtual DbSet<RqWorkflowPosition> RqWorkflowPositions { get; set; }
        public virtual DbSet<RqWorkflowRequestType> RqWorkflowRequestTypes { get; set; }
        public virtual DbSet<RqWorkflowUser> RqWorkflowUsers { get; set; }
        public virtual DbSet<Scope> Scopes { get; set; }
        public virtual DbSet<TaBurntRepurchaseTransfer> TaBurntRepurchaseTransfers { get; set; }
        public virtual DbSet<TaCalendar> TaCalendars { get; set; }
        public virtual DbSet<TaCalendarDay> TaCalendarDays { get; set; }
        public virtual DbSet<TaCalendarDayType> TaCalendarDayTypes { get; set; }
        public virtual DbSet<TaClocking> TaClockings { get; set; }
        public virtual DbSet<TaDevice> TaDevices { get; set; }
        public virtual DbSet<TaDeviceClockingMap> TaDeviceClockingMaps { get; set; }
        public virtual DbSet<TaDeviceEnterType> TaDeviceEnterTypes { get; set; }
        public virtual DbSet<TaDevicePosition> TaDevicePositions { get; set; }
        public virtual DbSet<TaDeviceSetting> TaDeviceSettings { get; set; }
        public virtual DbSet<TaFactor> TaFactors { get; set; }
        public virtual DbSet<TaMonthlyRemainingLeaf> TaMonthlyRemainingLeaves { get; set; }
        public virtual DbSet<TaPolicy> TaPolicies { get; set; }
        public virtual DbSet<TaPolicyPosition> TaPolicyPositions { get; set; }
        public virtual DbSet<TaPolicyRule> TaPolicyRules { get; set; }
        public virtual DbSet<TaPolicyUser> TaPolicyUsers { get; set; }
        public virtual DbSet<TaRemainingLeave> TaRemainingLeaves { get; set; }
        public virtual DbSet<TaRemainingLeaveConfig> TaRemainingLeaveConfigs { get; set; }
        public virtual DbSet<TaShift> TaShifts { get; set; }
        public virtual DbSet<TaShiftMask> TaShiftMasks { get; set; }
        public virtual DbSet<TaShiftPerson> TaShiftPeople { get; set; }
        public virtual DbSet<TaShiftWorkingHour> TaShiftWorkingHours { get; set; }
        public virtual DbSet<TaWorkingHour> TaWorkingHours { get; set; }
        public virtual DbSet<TaWrit> TaWrits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=.; Database=RadenV2; User Id=sa; Password=qazwsx!@#6027;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.ToTable("activity_logs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Client)
                    .HasMaxLength(20)
                    .HasColumnName("client");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Method)
                    .HasMaxLength(10)
                    .HasColumnName("method");

                entity.Property(e => e.Parameters).HasColumnName("parameters");

                entity.Property(e => e.Request).HasColumnName("request");

                entity.Property(e => e.Response).HasColumnName("response");

                entity.Property(e => e.Route)
                    .HasMaxLength(50)
                    .HasColumnName("route");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<FeedingDateFoodMeal>(entity =>
            {
                entity.ToTable("feeding_date_food_meal");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.FoodId).HasColumnName("food_id");

                entity.Property(e => e.MealId).HasColumnName("meal_id");

                entity.Property(e => e.SelfId).HasColumnName("self_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Food)
                    .WithMany(p => p.FeedingDateFoodMeals)
                    .HasForeignKey(d => d.FoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_feeding_date_food_meal_feeding_foods");

                entity.HasOne(d => d.Meal)
                    .WithMany(p => p.FeedingDateFoodMeals)
                    .HasForeignKey(d => d.MealId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_feeding_date_food_meal_feeding_meals");

                entity.HasOne(d => d.Self)
                    .WithMany(p => p.FeedingDateFoodMeals)
                    .HasForeignKey(d => d.SelfId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_feeding_date_food_meal_feeding_selfs");
            });

            modelBuilder.Entity<FeedingFood>(entity =>
            {
                entity.ToTable("feeding_foods");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("title");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<FeedingMeal>(entity =>
            {
                entity.ToTable("feeding_meals");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color")
                    .HasDefaultValueSql("('#000000')");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.EndTime).HasColumnName("end_time");

                entity.Property(e => e.StartTime).HasColumnName("start_time");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("title");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<FeedingReserf>(entity =>
            {
                entity.ToTable("feeding_reserves");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ClockingReasonId).HasColumnName("clocking_reason_id");

                entity.Property(e => e.Count).HasColumnName("count");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DateFoodMealId).HasColumnName("date_food_meal_id");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(80)
                    .HasColumnName("description");

                entity.Property(e => e.PersonIdEaten).HasColumnName("person_id_eaten");

                entity.Property(e => e.PersonIdOrdered).HasColumnName("person_id_ordered");

                entity.Property(e => e.PositionIdEaten).HasColumnName("position_id_eaten");

                entity.Property(e => e.PositionIdOrdered).HasColumnName("position_id_ordered");

                entity.Property(e => e.Printed).HasColumnName("printed");

                entity.Property(e => e.Props)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("props");

                entity.Property(e => e.ReserveType).HasColumnName("reserve_type");

                entity.Property(e => e.Status)
                    .HasMaxLength(1)
                    .HasColumnName("status");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.DateFoodMeal)
                    .WithMany(p => p.FeedingReserves)
                    .HasForeignKey(d => d.DateFoodMealId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_feeding_reserves_feeding_date_food_meal");

                entity.HasOne(d => d.PersonIdEatenNavigation)
                    .WithMany(p => p.FeedingReserfPersonIdEatenNavigations)
                    .HasForeignKey(d => d.PersonIdEaten)
                    .HasConstraintName("FK_feeding_reserves_hr_persons1");

                entity.HasOne(d => d.PersonIdOrderedNavigation)
                    .WithMany(p => p.FeedingReserfPersonIdOrderedNavigations)
                    .HasForeignKey(d => d.PersonIdOrdered)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_feeding_reserves_hr_persons");

                entity.HasOne(d => d.PositionIdEatenNavigation)
                    .WithMany(p => p.FeedingReserfPositionIdEatenNavigations)
                    .HasForeignKey(d => d.PositionIdEaten)
                    .HasConstraintName("FK_feeding_reserves_hr_positions1");

                entity.HasOne(d => d.PositionIdOrderedNavigation)
                    .WithMany(p => p.FeedingReserfPositionIdOrderedNavigations)
                    .HasForeignKey(d => d.PositionIdOrdered)
                    .HasConstraintName("FK_feeding_reserves_hr_positions");
            });

            modelBuilder.Entity<FeedingSelf>(entity =>
            {
                entity.ToTable("feeding_selfs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("title");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrBank>(entity =>
            {
                entity.ToTable("hr_banks");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrCity>(entity =>
            {
                entity.ToTable("hr_cities");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrCompany>(entity =>
            {
                entity.ToTable("hr_companies");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.CityId).HasColumnName("city_id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("code");

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.EconomicCode)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("economic_code");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.EmailPassword)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("email_password");

                entity.Property(e => e.Location)
                    .HasMaxLength(255)
                    .HasColumnName("location");

                entity.Property(e => e.Logo)
                    .HasColumnType("text")
                    .HasColumnName("logo");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("name");

                entity.Property(e => e.NationalIdentificationCode)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("national_identification_code");

                entity.Property(e => e.NatureOfBusinessId).HasColumnName("nature_of_business_id");

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("postal_code");

                entity.Property(e => e.RegisterNumber)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("register_number");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Website)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("website");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.HrCompanies)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_hr_companies_hr_cities");
            });

            modelBuilder.Entity<HrCountry>(entity =>
            {
                entity.ToTable("hr_countries");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("code");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrForm>(entity =>
            {
                entity.ToTable("hr_forms");

                entity.HasIndex(e => new { e.FormableType, e.FormableId }, "IX_hr_forms");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasColumnName("enabled")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.FormableId).HasColumnName("formable_id");

                entity.Property(e => e.FormableType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("formable_type");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("name");

                entity.Property(e => e.Props)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("props")
                    .HasDefaultValueSql("('{}')");

                entity.Property(e => e.Template)
                    .HasColumnType("text")
                    .HasColumnName("template");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Formable)
                    .WithMany(p => p.HrForms)
                    .HasForeignKey(d => d.FormableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_forms_hr_types");
            });

            modelBuilder.Entity<HrFormField>(entity =>
            {
                entity.ToTable("hr_form_fields");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DefaultValue)
                    .HasColumnType("text")
                    .HasColumnName("default_value");

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasColumnName("enabled")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.FormId).HasColumnName("form_id");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("label");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Priority)
                    .HasColumnName("priority")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Props)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("props")
                    .HasDefaultValueSql("('{}')");

                entity.Property(e => e.Required).HasColumnName("required");

                entity.Property(e => e.Resource)
                    .HasColumnType("text")
                    .HasColumnName("resource");

                entity.Property(e => e.Type)
                    .HasMaxLength(40)
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Validation)
                    .HasColumnType("text")
                    .HasColumnName("validation");

                entity.Property(e => e.Values)
                    .HasColumnType("text")
                    .HasColumnName("values");

                entity.Property(e => e.Visible)
                    .IsRequired()
                    .HasColumnName("visible")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Form)
                    .WithMany(p => p.HrFormFields)
                    .HasForeignKey(d => d.FormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_form_fields_hr_forms");
            });

            modelBuilder.Entity<HrInsuranceType>(entity =>
            {
                entity.ToTable("hr_insurance_types");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CompanyCode).HasColumnName("company_code");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrModule>(entity =>
            {
                entity.ToTable("hr_modules");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasColumnName("description");

                entity.Property(e => e.ModuleId).HasColumnName("module_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrNotification>(entity =>
            {
                entity.ToTable("hr_notifications");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Data)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("data")
                    .HasDefaultValueSql("('{}')");

                entity.Property(e => e.NotifiableId).HasColumnName("notifiable_id");

                entity.Property(e => e.NotifiableType)
                    .HasMaxLength(100)
                    .HasColumnName("notifiable_type");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.HrNotifications)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_notifications_hr_types");
            });

            modelBuilder.Entity<HrNotificationUser>(entity =>
            {
                entity.ToTable("hr_notification_users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.NotificationId).HasColumnName("notification_id");

                entity.Property(e => e.ReadDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("read_datetime");

                entity.Property(e => e.SenderModuleId).HasColumnName("sender_module_id");

                entity.Property(e => e.SenderStatus).HasColumnName("sender_status");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Notification)
                    .WithMany(p => p.HrNotificationUsers)
                    .HasForeignKey(d => d.NotificationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_notification_users_hr_notifications");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.HrNotificationUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_notification_users_hr_users");
            });

            modelBuilder.Entity<HrPermission>(entity =>
            {
                entity.ToTable("hr_permissions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.ModuleId).HasColumnName("module_id");

                entity.Property(e => e.ModuleStr)
                    .HasMaxLength(100)
                    .HasColumnName("module_str");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Module)
                    .WithMany(p => p.HrPermissions)
                    .HasForeignKey(d => d.ModuleId)
                    .HasConstraintName("FK_hr_permissions_hr_rule_sub_modules");
            });

            modelBuilder.Entity<HrPermissionRole>(entity =>
            {
                entity.ToTable("hr_permission_role");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.PermissionId).HasColumnName("permission_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.HrPermissionRoles)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_permission_role_hr_permissions");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.HrPermissionRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_permission_role_hr_roles");
            });

            modelBuilder.Entity<HrPerson>(entity =>
            {
                entity.ToTable("hr_persons");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(250)
                    .HasColumnName("address");

                entity.Property(e => e.Avatar)
                    .HasColumnType("text")
                    .HasColumnName("avatar");

                entity.Property(e => e.BirthCertificateNumber)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("birth_certificate_number");

                entity.Property(e => e.Birthday)
                    .HasColumnType("datetime")
                    .HasColumnName("birthday");

                entity.Property(e => e.CityId).HasColumnName("city_id");

                entity.Property(e => e.CompanyId).HasColumnName("company_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Education).HasColumnName("education");

                entity.Property(e => e.FatherName)
                    .HasMaxLength(30)
                    .HasColumnName("father_name");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("first_name");

                entity.Property(e => e.IdentificationCode).HasColumnName("identification_code");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("last_name");

                entity.Property(e => e.Married).HasColumnName("married");

                entity.Property(e => e.Military).HasColumnName("military");

                entity.Property(e => e.NationalCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("national_code")
                    .IsFixedLength();

                entity.Property(e => e.Nationality)
                    .HasMaxLength(50)
                    .HasColumnName("nationality");

                entity.Property(e => e.NightShift).HasColumnName("night_shift");

                entity.Property(e => e.PersonnelId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("personnel_id");

                entity.Property(e => e.Props)
                    .HasMaxLength(200)
                    .HasColumnName("props");

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.Sex).HasColumnName("sex");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.HrPeople)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_hr_persons_hr_cities");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.HrPeople)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_hr_persons_hr_provinces");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.HrPeople)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_user_profiles_hr_users");
            });

            modelBuilder.Entity<HrPersonContract>(entity =>
            {
                entity.ToTable("hr_person_contracts");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BeginDate)
                    .HasColumnType("datetime")
                    .HasColumnName("begin_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.ExpireDate)
                    .HasColumnType("datetime")
                    .HasColumnName("expire_date");

                entity.Property(e => e.InsuranceId).HasColumnName("insurance_id");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.RecruitmentTypeId).HasColumnName("recruitment_type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Insurance)
                    .WithMany(p => p.HrPersonContracts)
                    .HasForeignKey(d => d.InsuranceId)
                    .HasConstraintName("FK_hr_person_contracts_hr_person_insurances");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.HrPersonContracts)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_person_contracts_hr_persons");

                entity.HasOne(d => d.RecruitmentType)
                    .WithMany(p => p.HrPersonContracts)
                    .HasForeignKey(d => d.RecruitmentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_person_contracts_hr_recruitment_types");
            });

            modelBuilder.Entity<HrPersonFinancialInfo>(entity =>
            {
                entity.ToTable("hr_person_financial_infos");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountNumber)
                    .HasMaxLength(30)
                    .HasColumnName("account_number");

                entity.Property(e => e.BankId).HasColumnName("bank_id");

                entity.Property(e => e.CardNumber)
                    .HasMaxLength(20)
                    .HasColumnName("card_number");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Bank)
                    .WithMany(p => p.HrPersonFinancialInfos)
                    .HasForeignKey(d => d.BankId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_user_financial_infos_hr_banks");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.HrPersonFinancialInfos)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_person_financial_infos_hr_persons");
            });

            modelBuilder.Entity<HrPersonFormField>(entity =>
            {
                entity.ToTable("hr_person_form_fields");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Defaults)
                    .HasMaxLength(255)
                    .HasColumnName("defaults");

                entity.Property(e => e.FormFieldId).HasColumnName("form_field_id");

                entity.Property(e => e.PersonId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("person_id");
            });

            modelBuilder.Entity<HrPersonInsurance>(entity =>
            {
                entity.ToTable("hr_person_insurances");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("code");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.HrPersonInsurances)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_person_insurances_hr_persons");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.HrPersonInsurances)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_person_insurances_hr_insurance_types");
            });

            modelBuilder.Entity<HrPhone>(entity =>
            {
                entity.ToTable("hr_phones");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("code")
                    .HasDefaultValueSql("('98')");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("number");

                entity.Property(e => e.NumberType)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("number_type")
                    .HasDefaultValueSql("('work')");

                entity.Property(e => e.PhoneableId).HasColumnName("phoneable_id");

                entity.Property(e => e.PhoneableType)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("phoneable_type");

                entity.Property(e => e.Priority)
                    .HasColumnName("priority")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrPosition>(entity =>
            {
                entity.ToTable("hr_positions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.PositionId).HasColumnName("position_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.HrPositions)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_hr_positions_hr_persons1");
            });

            modelBuilder.Entity<HrProvince>(entity =>
            {
                entity.ToTable("hr_provinces");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CountryId).HasColumnName("country_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrRecruitmentType>(entity =>
            {
                entity.ToTable("hr_recruitment_types");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CompanyCode).HasColumnName("company_code");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrRole>(entity =>
            {
                entity.ToTable("hr_roles");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(80)
                    .HasColumnName("description");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(50)
                    .HasColumnName("display_name");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Scope).HasColumnName("scope");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrRoleUser>(entity =>
            {
                entity.ToTable("hr_role_user");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.HrRoleUsers)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_role_user_hr_roles");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.HrRoleUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_role_user_hr_users");
            });

            modelBuilder.Entity<HrRule>(entity =>
            {
                entity.ToTable("hr_rules");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.AddColumn).HasColumnName("add_column");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .HasColumnName("description");

                entity.Property(e => e.Filename)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("filename");

                entity.Property(e => e.IsPrivate).HasColumnName("is_private");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Visible)
                    .IsRequired()
                    .HasColumnName("visible")
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.HrRules)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_rules_hr_types");
            });

            modelBuilder.Entity<HrRulePack>(entity =>
            {
                entity.ToTable("hr_rule_packs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrRulePackRule>(entity =>
            {
                entity.ToTable("hr_rule_pack_rules");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.RuleId).HasColumnName("rule_id");

                entity.Property(e => e.RulePackId).HasColumnName("rule_pack_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .HasMaxLength(255)
                    .HasColumnName("values");

                entity.HasOne(d => d.RulePack)
                    .WithMany(p => p.HrRulePackRules)
                    .HasForeignKey(d => d.RulePackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_rule_pack_rules_hr_rule_packs");
            });

            modelBuilder.Entity<HrRuleSubModule>(entity =>
            {
                entity.ToTable("hr_rule_sub_modules");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<HrSetting>(entity =>
            {
                entity.ToTable("hr_settings");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.SettingPackId).HasColumnName("setting_pack_id");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .HasMaxLength(255)
                    .HasColumnName("values");

                entity.HasOne(d => d.SettingPack)
                    .WithMany(p => p.HrSettings)
                    .HasForeignKey(d => d.SettingPackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_settings_hr_setting_packs");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.HrSettings)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_settings_hr_types");
            });

            modelBuilder.Entity<HrSettingPack>(entity =>
            {
                entity.ToTable("hr_setting_packs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<HrType>(entity =>
            {
                entity.ToTable("hr_types");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CompanyCode).HasColumnName("company_code");

                entity.Property(e => e.ForModuleId).HasColumnName("for_module_id");

                entity.Property(e => e.FullLabel)
                    .HasMaxLength(150)
                    .HasColumnName("full_label");

                entity.Property(e => e.FullName)
                    .HasMaxLength(80)
                    .HasColumnName("full_name");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("label");

                entity.Property(e => e.ModuleId).HasColumnName("module_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(40)
                    .HasColumnName("name");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.HasOne(d => d.Module)
                    .WithMany(p => p.HrTypes)
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hr_types_hr_modules");
            });

            modelBuilder.Entity<HrUser>(entity =>
            {
                entity.ToTable("hr_users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ActiveRole).HasColumnName("active_role");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.RememberToken)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("remember_token");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<RqReceivedRequest>(entity =>
            {
                entity.ToTable("rq_received_requests");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ApproverId).HasColumnName("approver_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.LevelId).HasColumnName("level_id");

                entity.Property(e => e.RequestId).HasColumnName("request_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Approver)
                    .WithMany(p => p.RqReceivedRequests)
                    .HasForeignKey(d => d.ApproverId)
                    .HasConstraintName("FK_rq_received_requests_hr_positions");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RqReceivedRequests)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_received_requests_rq_requests");
            });

            modelBuilder.Entity<RqRequest>(entity =>
            {
                entity.ToTable("rq_requests");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Done).HasColumnName("done");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("key");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.PositionId).HasColumnName("position_id");

                entity.Property(e => e.Props)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("props")
                    .HasDefaultValueSql("('{}')");

                entity.Property(e => e.Sender)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("sender")
                    .HasDefaultValueSql("('{}')");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.SubstituteId).HasColumnName("substitute_id");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("values")
                    .HasDefaultValueSql("('{}')");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.RqRequests)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_requests_hr_persons");

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.RqRequests)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_requests_hr_positions");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.RqRequests)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_requests_hr_types");
            });

            modelBuilder.Entity<RqRequestRulePack>(entity =>
            {
                entity.ToTable("rq_request_rule_pack");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<RqRequestRulePackPosition>(entity =>
            {
                entity.ToTable("rq_request_rule_pack_position");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PositionId).HasColumnName("position_id");

                entity.Property(e => e.RequestRulePackId).HasColumnName("request_rule_pack_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.RqRequestRulePackPositions)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_request_rule_pack_position_hr_positions");

                entity.HasOne(d => d.RequestRulePack)
                    .WithMany(p => p.RqRequestRulePackPositions)
                    .HasForeignKey(d => d.RequestRulePackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_request_rule_pack_position_rq_request_rule_pack");
            });

            modelBuilder.Entity<RqRequestRulePackRule>(entity =>
            {
                entity.ToTable("rq_request_rule_pack_rules");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(80)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.RequestRulePackId).HasColumnName("request_rule_pack_id");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .HasMaxLength(255)
                    .HasColumnName("values");

                entity.HasOne(d => d.RequestRulePack)
                    .WithMany(p => p.RqRequestRulePackRules)
                    .HasForeignKey(d => d.RequestRulePackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_request_rule_pack_rules_rq_request_rule_pack");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.RqRequestRulePackRules)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_request_rule_pack_rules_hr_types");
            });

            modelBuilder.Entity<RqRequestRulePackUser>(entity =>
            {
                entity.ToTable("rq_request_rule_pack_user");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.RequestRulePackId).HasColumnName("request_rule_pack_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.RqRequestRulePackUsers)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_request_rule_pack_user_hr_persons");

                entity.HasOne(d => d.RequestRulePack)
                    .WithMany(p => p.RqRequestRulePackUsers)
                    .HasForeignKey(d => d.RequestRulePackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_request_rule_pack_user_rq_request_rule_pack");
            });

            modelBuilder.Entity<RqRequestable>(entity =>
            {
                entity.ToTable("rq_requestables");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.RequestId).HasColumnName("request_id");

                entity.Property(e => e.RequestableId).HasColumnName("requestable_id");

                entity.Property(e => e.RequestableType)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("requestable_type");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RqRequestables)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_requestables_rq_requests");
            });

            modelBuilder.Entity<RqWorkflow>(entity =>
            {
                entity.ToTable("rq_workflows");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Final).HasColumnName("final");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<RqWorkflowApprover>(entity =>
            {
                entity.ToTable("rq_workflow_approvers");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ApproverId).HasColumnName("approver_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.LevelId).HasColumnName("level_id");

                entity.Property(e => e.PositionName)
                    .HasMaxLength(80)
                    .HasColumnName("position_name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Approver)
                    .WithMany(p => p.RqWorkflowApprovers)
                    .HasForeignKey(d => d.ApproverId)
                    .HasConstraintName("FK_rq_workflow_approvers_hr_positions");

                entity.HasOne(d => d.Level)
                    .WithMany(p => p.RqWorkflowApprovers)
                    .HasForeignKey(d => d.LevelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_approvers_rq_workflow_levels");
            });

            modelBuilder.Entity<RqWorkflowLevel>(entity =>
            {
                entity.ToTable("rq_workflow_levels");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Priority).HasColumnName("priority");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");

                entity.HasOne(d => d.Workflow)
                    .WithMany(p => p.RqWorkflowLevels)
                    .HasForeignKey(d => d.WorkflowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_levels_rq_workflows");
            });

            modelBuilder.Entity<RqWorkflowPosition>(entity =>
            {
                entity.ToTable("rq_workflow_position");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PositionId).HasColumnName("position_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.RqWorkflowPositions)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_position_hr_positions");

                entity.HasOne(d => d.Workflow)
                    .WithMany(p => p.RqWorkflowPositions)
                    .HasForeignKey(d => d.WorkflowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_position_rq_workflows");
            });

            modelBuilder.Entity<RqWorkflowRequestType>(entity =>
            {
                entity.ToTable("rq_workflow_request_type");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.RqWorkflowRequestTypes)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_request_type_hr_types");

                entity.HasOne(d => d.Workflow)
                    .WithMany(p => p.RqWorkflowRequestTypes)
                    .HasForeignKey(d => d.WorkflowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_request_type_rq_workflows");
            });

            modelBuilder.Entity<RqWorkflowUser>(entity =>
            {
                entity.ToTable("rq_workflow_user");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.RqWorkflowUsers)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_user_hr_persons");

                entity.HasOne(d => d.Workflow)
                    .WithMany(p => p.RqWorkflowUsers)
                    .HasForeignKey(d => d.WorkflowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_rq_workflow_user_rq_workflows");
            });

            modelBuilder.Entity<Scope>(entity =>
            {
                entity.ToTable("scopes");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.All).HasColumnName("all");

                entity.Property(e => e.Cases)
                    .HasColumnType("text")
                    .HasColumnName("cases");

                entity.Property(e => e.Children).HasColumnName("children");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.ScopeModel)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("scope_model");

                entity.Property(e => e.Self).HasColumnName("self");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Scopes)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_scopes_hr_roles");
            });

            modelBuilder.Entity<TaBurntRepurchaseTransfer>(entity =>
            {
                entity.ToTable("ta_burnt_repurchase_transfer");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.RejectStatus).HasColumnName("reject_status");

                entity.Property(e => e.Remaining).HasColumnName("remaining");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Year).HasColumnName("year");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaBurntRepurchaseTransfers)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_burnt_repurchase_transfer_hr_persons");
            });

            modelBuilder.Entity<TaCalendar>(entity =>
            {
                entity.ToTable("ta_calendars");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<TaCalendarDay>(entity =>
            {
                entity.ToTable("ta_calendar_days");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CalendarDayTypeId).HasColumnName("calendar_day_type_id");

                entity.Property(e => e.CalendarId).HasColumnName("calendar_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.CalendarDayType)
                    .WithMany(p => p.TaCalendarDays)
                    .HasForeignKey(d => d.CalendarDayTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_calendar_days_ta_calendar_day_types");

                entity.HasOne(d => d.Calendar)
                    .WithMany(p => p.TaCalendarDays)
                    .HasForeignKey(d => d.CalendarId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_calendar_days_ta_calendars");
            });

            modelBuilder.Entity<TaCalendarDayType>(entity =>
            {
                entity.ToTable("ta_calendar_day_types");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("display_name");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<TaClocking>(entity =>
            {
                entity.ToTable("ta_clockings");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Changed).HasColumnName("changed");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Datetime)
                    .HasColumnType("datetime")
                    .HasColumnName("datetime");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasColumnName("description");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.EntryType).HasColumnName("entry_type");

                entity.Property(e => e.IoId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("io_id");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.TaClockings)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_ta_clockings_ta_devices");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaClockings)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_clockings_hr_persons");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.TaClockings)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_clockings_hr_types");
            });

            modelBuilder.Entity<TaDevice>(entity =>
            {
                entity.ToTable("ta_devices");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(80)
                    .IsUnicode(false)
                    .HasColumnName("address");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Port)
                    .HasColumnName("port")
                    .HasDefaultValueSql("((8080))");

                entity.Property(e => e.RefreshCycle).HasColumnName("refresh_cycle");

                entity.Property(e => e.SaveDraft)
                    .HasColumnName("save_draft")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Username)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<TaDeviceClockingMap>(entity =>
            {
                entity.ToTable("ta_device_clocking_map");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ClockingReasonId).HasColumnName("clocking_reason_id");

                entity.Property(e => e.ClockingTypeId).HasColumnName("clocking_type_id");

                entity.Property(e => e.DeviceClockingTypeId).HasColumnName("device_clocking_type_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.EntryType).HasColumnName("entry_type");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.TaDeviceClockingMaps)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_device_clocking_map_ta_devices");
            });

            modelBuilder.Entity<TaDeviceEnterType>(entity =>
            {
                entity.ToTable("ta_device_enter_types");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeviceBioTypeId).HasColumnName("device_bio_type_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.TaDeviceEnterTypes)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_device_enter_types_ta_devices");
            });

            modelBuilder.Entity<TaDevicePosition>(entity =>
            {
                entity.ToTable("ta_device_positions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CityId).HasColumnName("city_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.TaDevicePositions)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_ta_device_positions_ta_devices");
            });

            modelBuilder.Entity<TaDeviceSetting>(entity =>
            {
                entity.ToTable("ta_device_settings");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.Props)
                    .HasMaxLength(255)
                    .HasColumnName("props");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<TaFactor>(entity =>
            {
                entity.ToTable("ta_factors");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.FactorId).HasColumnName("factor_id");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.Priority)
                    .HasColumnName("priority")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.RuleId).HasColumnName("rule_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .HasMaxLength(255)
                    .HasColumnName("values");

                entity.Property(e => e.WorkingHourId).HasColumnName("working_hour_id");

                entity.HasOne(d => d.WorkingHour)
                    .WithMany(p => p.TaFactors)
                    .HasForeignKey(d => d.WorkingHourId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_factors_ta_working_hours");
            });

            modelBuilder.Entity<TaMonthlyRemainingLeaf>(entity =>
            {
                entity.ToTable("ta_monthly_remaining_leaves");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Year).HasColumnName("year");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaMonthlyRemainingLeaves)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_monthly_remaining_leaves_hr_persons");
            });

            modelBuilder.Entity<TaPolicy>(entity =>
            {
                entity.ToTable("ta_policies");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("color");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<TaPolicyPosition>(entity =>
            {
                entity.ToTable("ta_policy_positions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PolicyId).HasColumnName("policy_id");

                entity.Property(e => e.PositionId).HasColumnName("position_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Policy)
                    .WithMany(p => p.TaPolicyPositions)
                    .HasForeignKey(d => d.PolicyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_policy_positions_ta_policies");

                entity.HasOne(d => d.Position)
                    .WithMany(p => p.TaPolicyPositions)
                    .HasForeignKey(d => d.PositionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_policy_positions_hr_positions");
            });

            modelBuilder.Entity<TaPolicyRule>(entity =>
            {
                entity.ToTable("ta_policy_rules");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("name");

                entity.Property(e => e.PolicyId).HasColumnName("policy_id");

                entity.Property(e => e.RuleId).HasColumnName("rule_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .HasMaxLength(255)
                    .HasColumnName("values");

                entity.HasOne(d => d.Policy)
                    .WithMany(p => p.TaPolicyRules)
                    .HasForeignKey(d => d.PolicyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_policy_rules_ta_policies");

                entity.HasOne(d => d.Rule)
                    .WithMany(p => p.TaPolicyRules)
                    .HasForeignKey(d => d.RuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_policy_rules_hr_rules");
            });

            modelBuilder.Entity<TaPolicyUser>(entity =>
            {
                entity.ToTable("ta_policy_users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.PolicyId).HasColumnName("policy_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaPolicyUsers)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_policy_users_hr_persons");

                entity.HasOne(d => d.Policy)
                    .WithMany(p => p.TaPolicyUsers)
                    .HasForeignKey(d => d.PolicyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_policy_users_ta_policies");
            });

            modelBuilder.Entity<TaRemainingLeave>(entity =>
            {
                entity.ToTable("ta_remaining_leave");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.RemainingLeaves).HasColumnName("remaining_leaves");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Year).HasColumnName("year");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaRemainingLeaves)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_remaining_leave_hr_persons");
            });

            modelBuilder.Entity<TaRemainingLeaveConfig>(entity =>
            {
                entity.ToTable("ta_remaining_leave_config");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.IsLocked).HasColumnName("is_locked");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.RejectStatus).HasColumnName("reject_status");

                entity.Property(e => e.Transferable).HasColumnName("transferable");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Usable).HasColumnName("usable");

                entity.Property(e => e.Year).HasColumnName("year");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaRemainingLeaveConfigs)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_remaining_leave_config_hr_persons");
            });

            modelBuilder.Entity<TaShift>(entity =>
            {
                entity.ToTable("ta_shifts");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasColumnName("active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CalendarId).HasColumnName("calendar_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(150)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(150)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Calendar)
                    .WithMany(p => p.TaShifts)
                    .HasForeignKey(d => d.CalendarId)
                    .HasConstraintName("FK_ta_shifts_ta_calendars");
            });

            modelBuilder.Entity<TaShiftMask>(entity =>
            {
                entity.ToTable("ta_shift_mask");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.WorkingHourId).HasColumnName("working_hour_id");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaShiftMasks)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_shift_mask_hr_persons");

                entity.HasOne(d => d.WorkingHour)
                    .WithMany(p => p.TaShiftMasks)
                    .HasForeignKey(d => d.WorkingHourId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_shift_mask_ta_working_hours");
            });

            modelBuilder.Entity<TaShiftPerson>(entity =>
            {
                entity.ToTable("ta_shift_person");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.FromDate)
                    .HasColumnType("date")
                    .HasColumnName("from_date");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.ShiftId).HasColumnName("shift_id");

                entity.Property(e => e.ToDate)
                    .HasColumnType("date")
                    .HasColumnName("to_date");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaShiftPeople)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_shift_person_hr_persons");

                entity.HasOne(d => d.Shift)
                    .WithMany(p => p.TaShiftPeople)
                    .HasForeignKey(d => d.ShiftId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_shift_person_ta_shifts");
            });

            modelBuilder.Entity<TaShiftWorkingHour>(entity =>
            {
                entity.ToTable("ta_shift_working_hours");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.PoliciesId).HasColumnName("policies_id");

                entity.Property(e => e.PoliciesName)
                    .HasMaxLength(50)
                    .HasColumnName("policies_name");

                entity.Property(e => e.SettingsId).HasColumnName("settings_id");

                entity.Property(e => e.SettingsName)
                    .HasMaxLength(50)
                    .HasColumnName("settings_name");

                entity.Property(e => e.ShiftId).HasColumnName("shift_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.WorkingHoursId).HasColumnName("working_hours_id");

                entity.Property(e => e.WorkingHoursName)
                    .HasMaxLength(50)
                    .HasColumnName("working_hours_name");

                entity.HasOne(d => d.Shift)
                    .WithMany(p => p.TaShiftWorkingHours)
                    .HasForeignKey(d => d.ShiftId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_shift_working_hours_ta_shifts");

                entity.HasOne(d => d.WorkingHours)
                    .WithMany(p => p.TaShiftWorkingHours)
                    .HasForeignKey(d => d.WorkingHoursId)
                    .HasConstraintName("FK_ta_shift_working_hours_ta_working_hours");
            });

            modelBuilder.Entity<TaWorkingHour>(entity =>
            {
                entity.ToTable("ta_working_hours");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.AfterEnd).HasColumnName("after_end");

                entity.Property(e => e.BeforeStart).HasColumnName("before_start");

                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("color")
                    .HasDefaultValueSql("('#000')");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DayTo).HasColumnName("day_to");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .HasColumnName("description");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Period).HasColumnName("period");

                entity.Property(e => e.TimeFrom).HasColumnName("time_from");

                entity.Property(e => e.TimeTo)
                    .HasColumnName("time_to")
                    .HasDefaultValueSql("('00:00')");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<TaWrit>(entity =>
            {
                entity.ToTable("ta_writs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("key");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.Property(e => e.PositionId).HasColumnName("position_id");

                entity.Property(e => e.RegistrationDatetime)
                    .HasColumnType("datetime")
                    .HasColumnName("registration_datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Values)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("values")
                    .HasDefaultValueSql("('{}')");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaWrits)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_writs_hr_persons");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.TaWrits)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ta_writs_hr_types");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
