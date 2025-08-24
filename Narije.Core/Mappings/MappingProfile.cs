using AutoMapper;
using Narije.Core.DTOs.ViewModels.Accessory;
using Narije.Core.DTOs.ViewModels.AccessPermission;
using Narije.Core.DTOs.ViewModels.AccessProfile;
using Narije.Core.DTOs.ViewModels.Branch;
using Narije.Core.DTOs.ViewModels.City;
using Narije.Core.DTOs.ViewModels.Credit;
using Narije.Core.DTOs.ViewModels.Customer;
using Narije.Core.DTOs.ViewModels.CustomerMenuInfo;
using Narije.Core.DTOs.ViewModels.Dish;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.DTOs.ViewModels.FoodGroup;
using Narije.Core.DTOs.ViewModels.FoodPrice;
using Narije.Core.DTOs.ViewModels.FoodType;
using Narije.Core.DTOs.ViewModels.Gallery;
using Narije.Core.DTOs.ViewModels.Invoice;
using Narije.Core.DTOs.ViewModels.InvoiceDetail;
using Narije.Core.DTOs.ViewModels.Job;
using Narije.Core.DTOs.ViewModels.LogHistroy;
using Narije.Core.DTOs.ViewModels.LoginImage;
using Narije.Core.DTOs.ViewModels.Meal;
using Narije.Core.DTOs.ViewModels.Menu;
using Narije.Core.DTOs.ViewModels.MenuInfo;
using Narije.Core.DTOs.ViewModels.MenuLog;
using Narije.Core.DTOs.ViewModels.Permission;
using Narije.Core.DTOs.ViewModels.Province;
using Narije.Core.DTOs.ViewModels.Recipts;
using Narije.Core.DTOs.ViewModels.Reserve;
using Narije.Core.DTOs.ViewModels.Search;
using Narije.Core.DTOs.ViewModels.Setting;
using Narije.Core.DTOs.ViewModels.Settlement;
using Narije.Core.DTOs.ViewModels.SurveryValue;
using Narije.Core.DTOs.ViewModels.Survey;
using Narije.Core.DTOs.ViewModels.SurveyDetail;
using Narije.Core.DTOs.ViewModels.SurveyItem;
using Narije.Core.DTOs.ViewModels.Tutorial;
using Narije.Core.DTOs.ViewModels.User;
using Narije.Core.DTOs.ViewModels.VCustomer;
using Narije.Core.DTOs.ViewModels.Wallet;
using Narije.Core.DTOs.ViewModels.WalletPayment;
using Narije.Core.Entities;
using Narije.Core.Interfaces.GenericRepository;
using Narije.Core.Seedwork;
using System.Linq;

namespace Narije.Core.Mappings
{
    public class MappingProfile : Profile
    {

        private void CreateMapGeneric<TRequest, TEntity, TId>()
              where TRequest : class, IFileRequest
              where TEntity : class, IBaseGalaryEntity<TId>, new()
        {
            CreateMap<TRequest, TEntity>()
                .ForMember(dest => dest.GalleryId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    var galleryId = context.Mapper.Map<int>(src.galleryId);
                    dest.GalleryId = galleryId;
                });

            CreateMap<TEntity, TRequest>()
                .ForMember(dest => dest.galleryId, opt => opt.MapFrom(src => src.GalleryId));


        }


        private void CreateMapWithoutGalaryGeneric<TRequest, TEntity, TId>()
             where TRequest : class
             where TEntity : class, IBaseEntity<TId>, new()
        {
            CreateMap<TRequest, TEntity>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());


            CreateMap<TEntity, TRequest>();


        }

        public MappingProfile()
        {
            CreateMap<AccessPermission, AccessPermissionResponse>().ReverseMap();
            CreateMap<AccessProfile, AccessProfileResponse>().ReverseMap();
            CreateMap<Permission, PermissionResponse>().ReverseMap();
            CreateMap<City, CityResponse>().ReverseMap();
            CreateMap<Credit, CreditResponse>().ForMember(dest =>
                                dest.customerTitle,
                                opt => opt.MapFrom(model => model.Customer.Title)).ReverseMap();
            CreateMap<Customer, CustomerResponse>().ReverseMap();
            CreateMap<Food, FoodResponse>().ReverseMap();
   
            CreateMap<FoodGroup, FoodGroupResponse>()
                    .ForMember(dest => dest.totalFood, opt => opt.MapFrom(src => src.Foods.Count()))
                    .ReverseMap();
            CreateMap<FoodPrice, FoodPriceResponse>()
                .ForMember(dest =>
                                dest.customer,
                                opt => opt.MapFrom(model => model.Customer.Title))
                .ForMember(dest =>
                                dest.hasType,
                                opt => opt.MapFrom(model => model.Food.HasType))
                .ForMember(dest =>
                                dest.specialPrice,
                                opt => opt.MapFrom(model => model.Food.HasType))
                .ReverseMap();
            CreateMap<vFoodPrice, FoodPriceResponse>().ReverseMap();
            CreateMap<Gallery, GalleryResponse>().ReverseMap();
            CreateMap<Invoice, InvoiceResponse>()
                .ForMember(dest =>
                                dest.customer,
                                opt => opt.MapFrom(model => model.Customer.Title))
                .ReverseMap();
            CreateMap<InvoiceDetail, InvoiceDetailResponse>().ReverseMap();
            CreateMap<MenuInfo, MenuInfoResponse>()
                 .ForMember(dest => dest.lastUpdaterUser, opt => opt.Ignore()) 
                  .ReverseMap();

            CreateMap<MenuLog, MenuLogResponse>()
                .ForMember(dest => dest.userName, opt => opt.MapFrom(model => model.User.Fname + " " + model.User.Lname))
                .ForMember(dest => dest.foodName, opt => opt.MapFrom(model => model.Food.Title))
                .ReverseMap();


            CreateMap<VCustomer, vCustomerSingleResponse>().ReverseMap();

            CreateMap<Menu, MenuResponse>().ForMember(dest =>
                                dest.food,
                                opt => opt.MapFrom(model => model.Food.Title)).ReverseMap();
            CreateMap<Province, ProvinceResponse>().ReverseMap();
            CreateMap<Reserve, ReserveResponse>()
                .ReverseMap();
            CreateMap<vReserve, ReserveResponse>().ReverseMap();
            CreateMap<Search, SearchResponse>().ReverseMap();
            CreateMap<Setting, SettingResponse>().ReverseMap();
            CreateMap<SurveryValue, SurveryValueResponse>().ReverseMap();
            CreateMap<Survey, SurveyResponse>()
                .ForMember(dest =>
                                dest.food,
                                opt => opt.MapFrom(model => model.Food.Title))
                .ForMember(dest =>
                                dest.user,
                                opt => opt.MapFrom(model => model.User.Fname + " " + model.User.Lname))
                .ReverseMap();
            CreateMap<vSurvey, SurveyResponse>().ReverseMap();
            CreateMap<SurveyDetail, SurveyDetailResponse>().ReverseMap();
            CreateMap<SurveyItem, SurveyItemResponse>()
                .ForMember(dest =>
                                dest.values,
                                opt => opt.MapFrom(model => model.SurveryValues.Select(A => new SurveryValueResponse()
                                {
                                    id = A.Id,
                                    title = A.Title,
                                    value = A.Value,
                                    active = A.Active
                                }).ToList()))
                .ReverseMap();
            CreateMap<LoginImage, LoginImageResponse>().ReverseMap();

            CreateMap<User, UserResponse>()
               .ForMember(dest =>
                               dest.showPrice,
                               opt => opt.MapFrom(model => model.Customer == null ? false : model.Customer.ShowPrice))
               .ForMember(dest =>
                               dest.cancelTime,
                               opt => opt.MapFrom(model => model.Customer == null ? null : model.Customer.CancelTime))
               .ForMember(dest =>
                               dest.guestTime,
                               opt => opt.MapFrom(model => model.Customer == null ? null : model.Customer.GuestTime))
               .ForMember(dest =>
                               dest.customerId,
                               opt => opt.MapFrom(model => model.Customer != null ? (int?)model.Customer.Id : null))
               .ForMember(dest =>
                               dest.reserveTime,
                               opt => opt.MapFrom(model => model.Customer == null ? null : model.Customer.ReserveTime))
               .ForMember(dest =>
                               dest.customer,
                               opt => opt.MapFrom(model => model.Customer == null ? "" : model.Customer.Title))
               .ForMember(dest =>
                               dest.payType,
                               opt => opt.MapFrom(model => model.Customer == null ? 0 : model.Customer.PayType))
               .ForMember(dest =>
                               dest.mealType,
                               opt => opt.MapFrom(model => model.Customer == null ? "" : model.Customer.MealType))
                .ForMember(dest =>
                               dest.foodType,
                               opt => opt.MapFrom(model => model.Customer == null ? 0 : model.Customer.FoodType))
                .ForMember(dest =>
                               dest.customerParentId,
                               opt => opt.MapFrom(model => model.Customer == null ? 0 : model.Customer.ParentId))

               .ReverseMap();
            CreateMap<VCustomer, VCustomerResponse>().ReverseMap(); 
            CreateMap<VCustomer, VCustomerReportResponse>().ReverseMap();
            CreateMap<CustomerMenuInfo, CustomerMenuInfoResponse>().ReverseMap();
            CreateMap<LogHistory, LogHistoryResponse>()
                .ForMember(dest => dest.id,
               opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.entityName,
               opt => opt.MapFrom(src => src.EntityName))
                .ForMember(dest => dest.entityId,
               opt => opt.MapFrom(src => src.EntityId))
                .ForMember(dest => dest.dateTime,
               opt => opt.MapFrom(src => src.DateTime))
                 .ForMember(dest => dest.userName,
               opt => opt.MapFrom(src => src.User.Fname + " " + src.User.Lname))
                 .ForMember(dest => dest.userId,
               opt => opt.MapFrom(src => src.UserId))
                 .ForMember(dest => dest.source,
               opt => opt.MapFrom(src => src.Source))
                 .ForMember(dest => dest.action,
               opt => opt.MapFrom(src => src.Action))
                     .ForMember(dest => dest.changed,
               opt => opt.MapFrom(src => src.Changed))
                      .ReverseMap();

            CreateMap<Wallet, WalletResponse>()
                .ForMember(dest =>
                                dest.fName,
                                opt => opt.MapFrom(model => model.User.Fname))
                .ForMember(dest =>
                                dest.lName,
                                opt => opt.MapFrom(model => model.User.Lname))
                .ForMember(dest =>
                                dest.user,
                                opt => opt.MapFrom(model => model.User.Fname + " " + model.User.Lname))
                .ForMember(dest =>
                                dest.refNumber,
                                opt => opt.MapFrom(model => model.WalletPayments.Select(A => A.RefNumber).FirstOrDefault()))
                .ForMember(dest =>
                                dest.pan,
                                opt => opt.MapFrom(model => model.WalletPayments.Select(A => A.Pan).FirstOrDefault()))
                .ForMember(dest =>
                                dest.gateway,
                                opt => opt.MapFrom(model => model.WalletPayments.Select(A => A.Gateway).FirstOrDefault()))
                .ForMember(dest =>
                                dest.userMobile,
                                opt => opt.MapFrom(model => model.User.Mobile))
                .ForMember(dest =>
                                dest.customerId,
                                opt => opt.MapFrom(model => model.User.CustomerId))
                .ReverseMap();
            CreateMap<WalletPayment, WalletPaymentResponse>()
                .ForMember(dest =>
                                dest.user,
                                opt => opt.MapFrom(model => model.User.Fname + " " + model.User.Lname))
                .ForMember(dest =>
                                dest.fName,
                                opt => opt.MapFrom(model => model.User.Fname))
                .ForMember(dest =>
                                dest.customerId,
                                opt => opt.MapFrom(model => model.User.CustomerId))
                .ForMember(dest =>
                                dest.lName,
                                opt => opt.MapFrom(model => model.User.Lname))
                .ForMember(dest =>
                                dest.userMobile,
                                opt => opt.MapFrom(model => model.User.Mobile))
                .ReverseMap();


            CreateMapWithoutGalaryGeneric<JobRequest, Job, int>();
            CreateMapWithoutGalaryGeneric<TutorialRequest, Tutorial, int>();
            CreateMapGeneric<AccessoryRequest, Accessory, int>();
            CreateMapGeneric<MealRequest, Meal, int>();
            CreateMapWithoutGalaryGeneric<SettlementRequest, Settlement, int>();
            CreateMapGeneric<DishRequest, Dish, int>();
            CreateMapGeneric<BranchRequest, Branch, int>();
            CreateMapGeneric<FoodTypeRquest, FoodType, int>();
            CreateMap<Recipt, ReciptResponse>().ReverseMap();

        }
    }
}

