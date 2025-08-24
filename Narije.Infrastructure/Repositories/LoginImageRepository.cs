using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Menu;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using Narije.Core.DTOs.ViewModels.LoginImage;
namespace Narije.Infrastructure.Repositories
{
    public class LoginImageRepository : BaseRepository<LoginImage>, ILoginImageRepository
    {
        // ------------------
        // Constructor
        // ------------------
        public LoginImageRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var loginImage = await _NarijeDBContext.LoginImage
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<LoginImageResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: loginImage);
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit)
        {
            try
            {
                if (page is null || page == 0)
                    page = 1;
                if (limit is null || limit == 0)
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "LoginImage");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.LoginImage
                            .ProjectTo<LoginImageResponse>(_IMapper.ConfigurationProvider);

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var SiteLinks = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

                return new ApiOkResponse(_Message: "SUCCESS", _Data: SiteLinks.Data, _Meta: SiteLinks.Meta, _Header: header);
            }
            catch (Exception ex)
            {

                return new ApiErrorResponse(
                    _Message: "An error occurred while processing your request." + ex.Message
                );
            }
        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(LoginImageInsertRequest request)
        {
            var loginImage = new LoginImage()
            {
                Title = request.title,
                Description = request.description,
                ForMobile = request.forMobile
            };

            loginImage.GalleryId = await GalleryHelper.AddFromGallery(_NarijeDBContext, request.fromGallery);
            if (request.files != null)
            {
                var k = await GalleryHelper.AddToGallery(_NarijeDBContext, "LoginImage", request.files.FirstOrDefault());
                if (k > 0)
                    loginImage.GalleryId = k;
            }


            await _NarijeDBContext.LoginImage.AddAsync(loginImage);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(loginImage.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(LoginImageEditRequest request)
        {
            var LoginImage = await _NarijeDBContext.LoginImage
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (LoginImage is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            LoginImage.Title = request.title;
            LoginImage.Description = request.description;
            LoginImage.ForMobile = request.forMobile;

            LoginImage.GalleryId = await GalleryHelper.EditFromGallery(_NarijeDBContext, LoginImage.GalleryId, request.fromGallery);
            if (request.files != null)
                LoginImage.GalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, LoginImage.GalleryId, "LoginImage", request.files.FirstOrDefault());

            _NarijeDBContext.LoginImage.Update(LoginImage);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(LoginImage.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var LoginImage = await _NarijeDBContext.LoginImage
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (LoginImage is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.LoginImage.Remove(LoginImage);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion
    }
}
