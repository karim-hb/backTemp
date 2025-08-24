using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Setting;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class SettingRepository : BaseRepository<Setting>, ISettingRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public SettingRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Setting = await _NarijeDBContext.Settings
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<SettingResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Setting);
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Setting");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Settings
                        .ProjectTo<SettingResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Settings = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Settings.Data, _Meta: Settings.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(SettingInsertRequest request)
        {
            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "سرویس ویرایش را استفاده نمایید");

            var Setting = new Setting()
            {
                CompanyName = request.companyName,
                EconomicCode = request.economicCode,
                RegNumber = request.regNumber,
                PostalCode = request.postalCode,
                Address = request.address,
                Tel = request.tel,
                NationalId = request.nationalId,
                CompanyGalleryId = request.companyGalleryId,
                ProvinceId = request.provinceId,
                CityId = request.cityId,
                ContactMobile = request.contactMobile,
                SurveyTime = request.surveyTime

            };


            await _NarijeDBContext.Settings.AddAsync(Setting);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Setting.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(SettingEditRequest request)
        {
            var Setting = await _NarijeDBContext.Settings
                                                  .FirstOrDefaultAsync();
            if (Setting is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Setting.CompanyName = request.companyName;
            Setting.EconomicCode = request.economicCode;
            Setting.RegNumber = request.regNumber;
            Setting.PostalCode = request.postalCode;
            Setting.Address = request.address;
            Setting.Tel = request.tel;
            Setting.NationalId = request.nationalId;
            if(request.provinceId != null)
                Setting.ProvinceId = request.provinceId;
            if(request.cityId != null)
                Setting.CityId = request.cityId;
            Setting.ContactMobile = request.contactMobile;
            Setting.SurveyTime = request.surveyTime;
            Setting.PanelMaintenanceMode = request.panelMaintenanceMode ?? false;
            Setting.FrontMaintenanceMode = request.frontMaintenanceMode ?? false ;
            if (request.forceSurvey != null)
                Setting.ForceSurvey = request.forceSurvey??false;

            if(request.companyGalleryId != null)
                Setting.CompanyGalleryId = await GalleryHelper.EditFromGallery(_NarijeDBContext, Setting.CompanyGalleryId, request.companyGalleryId.Value.ToString());
           
            if (request.companyDarkGalleryId != null)
                Setting.CompanyDarkGalleryId = await GalleryHelper.EditFromGallery(_NarijeDBContext, Setting.CompanyDarkGalleryId, request.companyDarkGalleryId.Value.ToString());


            if (request.files != null)
                Setting.CompanyGalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, Setting.CompanyGalleryId, "SETTING", request.files.FirstOrDefault());
           
            if (request.darkLogoFiles != null)
            {
                Setting.CompanyDarkGalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, Setting.CompanyDarkGalleryId, "SETTING", request.darkLogoFiles.FirstOrDefault());
            }

            _NarijeDBContext.Settings.Update(Setting);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Setting.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Setting = await _NarijeDBContext.Settings
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Setting is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.Settings.Remove(Setting);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(SettingCloneRequest request)
        {
            var Setting = await _NarijeDBContext.Settings
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Setting is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


