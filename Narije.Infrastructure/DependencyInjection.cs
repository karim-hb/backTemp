using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Repositories;
using Narije.Infrastructure.Helpers;
using Narije.Core.Interfaces.GenericRepository;



namespace TikmentApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSolutionInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NarijeDBContext>(option => option.UseSqlServer(configuration.GetConnectionString("Conn"))
                    .UseLazyLoadingProxies()
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Debug, DbContextLoggerOptions.DefaultWithLocalTime | DbContextLoggerOptions.SingleLine));

            #region تزریق وابستگی


            services.AddScoped(typeof(IGenericRepository<,,,>), typeof(GenericRepository<,,,>));
            services.AddScoped<IAccessPermissionRepository, AccessPermissionRepository>();
            services.AddScoped<IAccessProfileRepository, AccessProfileRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICreditRepository, CreditRepository>();
            services.AddScoped<IFoodRepository, FoodRepository>();
            services.AddScoped<IFoodGroupRepository, FoodGroupRepository>();
            services.AddScoped<IFoodPriceRepository, FoodPriceRepository>();
            services.AddScoped<IGalleryRepository, GalleryRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IInvoiceDetailRepository, InvoiceDetailRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<ILoginImageRepository, LoginImageRepository>();
            services.AddScoped<IRecipts, ReciptsRepository>();

            services.AddScoped<IProvinceRepository, ProvinceRepository>();
            services.AddScoped<IReserveRepository, ReserveRepository>();
            services.AddScoped<ISearchRepository, SearchRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ILogHistoryRepository, LogHistororyRepository>();
            services.AddScoped<ISurveryValueRepository, SurveryValueRepository>();
            services.AddScoped<ISurveyRepository, SurveyRepository>();
            services.AddScoped<ISurveyDetailRepository, SurveyDetailRepository>();
            services.AddScoped<ISurveyItemRepository, SurveyItemRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletPaymentRepository, WalletPaymentRepository>();
            services.AddScoped<LogHistoryHelper>();
            services.AddScoped<IMenuInfoRepository, MenuInfoRepository>();
            services.AddScoped<ICustomerMenuInfo, CustomerMenuInfoRepository>();
            services.AddScoped<IMenuLogRepository, MenuLogRepository>();
            services.AddScoped<ICustomerWidget, CustomerWidgetRepository>();
            #endregion

            var ServiceProvider = services.BuildServiceProvider();
            var _IHttpContextAccessor = ServiceProvider.GetService<IHttpContextAccessor>();

            return services;
        }
    }
}

