using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Gallery;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class GalleryRepository : BaseRepository<Gallery>, IGalleryRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public GalleryRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Gallery = await _NarijeDBContext.Galleries
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<GalleryResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Gallery);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Gallery");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Galleries
                        .ProjectTo<GalleryResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Gallerys = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Gallerys.Data, _Meta: Gallerys.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(GalleryInsertRequest request)
        {
            int id = 0;
            foreach (var file in request.files)
                id = await GalleryHelper.AddToGallery(_NarijeDBContext, "Gallery", file);

            return await GetAsync(id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(GalleryEditRequest request)
        {
            var Gallery = await _NarijeDBContext.Galleries
                                                  .Where(A => A.Id == request.id && A.Hidden == false)
                                                  .FirstOrDefaultAsync();
            if (Gallery is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Gallery.OriginalFileName = request.originalFileName;
            Gallery.Source = request.source;
            Gallery.Alt = request.alt;

            _NarijeDBContext.Galleries.Update(Gallery);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Gallery.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Gallery = await _NarijeDBContext.Galleries
                                              .Where(A => A.Id == id && A.Hidden == false)
                                              .FirstOrDefaultAsync();
            if (Gallery is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");


            var filepath = "/data/" + string.Format("{0}{1}", Gallery.Id, Gallery.SystemFileName);

            try
            {
                System.IO.File.Delete(filepath);
                filepath = "/data/" + string.Format("{0}_favicon{1}", Gallery.Id, Gallery.SystemFileName);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);
                filepath = "/data/" + string.Format("{0}_thumbnail{1}", Gallery.Id, Gallery.SystemFileName);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);
                filepath = "/data/" + string.Format("{0}_small{1}", Gallery.Id, Gallery.SystemFileName);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);
                filepath = "/data/" + string.Format("{0}_medium{1}", Gallery.Id, Gallery.SystemFileName);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);
                filepath = "/data/" + string.Format("{0}_large{1}", Gallery.Id, Gallery.SystemFileName);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Delete(filepath);

            }
            catch
            {
            }


            _NarijeDBContext.Galleries.Remove(Gallery);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(GalleryCloneRequest request)
        {
            var Gallery = await _NarijeDBContext.Galleries
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Gallery is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


