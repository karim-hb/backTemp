using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using Narije.Core.DTOs.Enum;
using System.Text.Json;

namespace Narije.Infrastructure.Repositories
{
    public class FoodRepository : BaseRepository<Food>, IFoodRepository
    {

        // ------------------
        // Constructor
        // ------------------
        private readonly LogHistoryHelper _logHistoryHelper;
        public FoodRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper, LogHistoryHelper logHistoryHelper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
            _logHistoryHelper = logHistoryHelper;
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Food = await _NarijeDBContext.Foods
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<FoodResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Food);
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
                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Food");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.Foods
                            .ProjectTo<FoodResponse>(_IMapper.ConfigurationProvider);

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var Foods = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

                return new ApiOkResponse(_Message: "SUCCESS", _Data: Foods.Data, _Meta: Foods.Meta, _Header: header);
            }

            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(FoodInsertRequest request)
        {


            var existingFood = await _NarijeDBContext.Foods
                                            .Where(f => f.ArpaNumber == request.arpaNumber)
                                            .FirstOrDefaultAsync();

            if (existingFood != null)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: $"غذای دیگری با کد کالا {request.arpaNumber} موجود است"
                );
            }

            var Food = new Food()
            {
                Title = request.title,
                Description = request.description,
                GroupId = request.groupId,
                Active = request.active,
                IsDaily = request.isDaily,
                HasType = request.hasType.Value,
                IsGuest = request.isGuest,
                EchoPrice = request.echoPrice,
                SpecialPrice = request.specialPrice,
                Vat = request.vat,
                ProductType = request.productType,
                ArpaNumber = request.arpaNumber,
                Vip = request.vip ?? false,
                IsFood = request.isFood,

            };
            Food.GalleryId = await GalleryHelper.AddFromGallery(_NarijeDBContext, request.fromGallery);
            if (request.files != null)
            {
                var k = await GalleryHelper.AddToGallery(_NarijeDBContext, "Food", request.files.FirstOrDefault());
                if (k > 0)
                    Food.GalleryId = k;
            }


            await _NarijeDBContext.Foods.AddAsync(Food);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            await _logHistoryHelper.AddLogHistoryAsync(
                "Food",
                   Food.Id,
                   EnumLogHistroyAction.create,
                  EnumLogHistorySource.site,
                 JsonSerializer.Serialize(request),
                 true
                );

            return await GetAsync(Food.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(FoodEditRequest request)
        {
            try
            {
                var Food = await _NarijeDBContext.Foods
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
                if (Food is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");
                var changes = LogHistoryHelper.GetEntityChanges(request, Food);
                var existingFood = await _NarijeDBContext.Foods
                                        .Where(f => f.ArpaNumber == request.arpaNumber)
                                        .FirstOrDefaultAsync();

                if (existingFood != null && existingFood.Id != request.id)
                {
                    return new ApiErrorResponse(
                        _Code: StatusCodes.Status400BadRequest,
                        _Message: $"غذای دیگری با کد کالا {request.arpaNumber} موجود است"
                    );
                }
                bool isVipChanging = (request.vip ?? false) != Food.Vip; 
                bool isIsFoodChanging = request.isFood != Food.IsFood;

                if (isVipChanging || isIsFoodChanging)
                {
                    bool hasActiveReserves = await _NarijeDBContext.Reserves
                        .AnyAsync(r =>
                            r.FoodId == Food.Id && 
                            r.Num > 0 &&
                            r.DateTime > DateTime.Now
                        );

                    if (hasActiveReserves)
                    {
                        return new ApiErrorResponse(
                            StatusCodes.Status400BadRequest,
                            "به دلیل وجود رزرو فعال، امکان تغییر وضعیت پرو یا نوع غذا وجود ندارد"
                        );
                    }
                }

                Food.Title = request.title;
                Food.Description = request.description;
                Food.GroupId = request.groupId;
                Food.Active = request.active;
                Food.IsDaily = request.isDaily ?? false;
                Food.ProductType = request.productType;
                if (request.hasType != null)
                    Food.HasType = request.hasType.Value;
                Food.IsGuest = request.isGuest;
                Food.EchoPrice = request.echoPrice;
                Food.SpecialPrice = request.specialPrice;
                Food.ArpaNumber = request.arpaNumber;
                Food.Vip = request.vip ?? false;
                Food.IsFood = request.isFood;
                if (request.vat != null)
                    Food.Vat = request.vat;


                Food.GalleryId = await GalleryHelper.EditFromGallery(_NarijeDBContext, Food.GalleryId, request.fromGallery);
                if (request.files != null)
                    Food.GalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, Food.GalleryId, "Food", request.files.FirstOrDefault());

                _NarijeDBContext.Foods.Update(Food);

                if (changes.Count > 0)
                {
                    await _logHistoryHelper.AddLogHistoryAsync(
                        "Food",
                        Food.Id,
                        EnumLogHistroyAction.update,
                        EnumLogHistorySource.site,
                        JsonSerializer.Serialize(changes),
                        false
                    );
                }

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                return await GetAsync(Food.Id);
            }

            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Food = await _NarijeDBContext.Foods
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Food is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            try
            {
                _NarijeDBContext.Foods.Remove(Food);

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

            }
            catch (Exception ex)
            {
                if ((ex.InnerException != null) && (((Microsoft.Data.SqlClient.SqlException)ex.InnerException).Number == 547))
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات در حال استفاده قابل حذف نیست");
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات حذف نشد! دوباره سعی کنید");
            }

        }
        #endregion

        #region EditActiveAsync
        // ------------------
        //  EditActiveAsync
        // ------------------
        public async Task<ApiResponse> EditActiveAsync(int id)
        {
            var Data = await _NarijeDBContext.Foods
                                                  .Where(A => A.Id == id)
                                                  .FirstOrDefaultAsync();
            if (Data is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Data.Active = !Data.Active;

            _NarijeDBContext.Foods.Update(Data);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Data.Id);
        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(FoodCloneRequest request)
        {
            var Food = await _NarijeDBContext.Foods
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Food is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region InsertFromExcel

        public async Task<ApiResponse> ProcessFoodFileAsync(IFormFile file)
        {
            var foodUpdates = new List<FoodEditRequest>();
            var newFoods = new List<Food>();

            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    if (file.FileName.EndsWith(".xlsx"))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var headerIndex = new Dictionary<string, int>();

                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                var header = worksheet.Cells[1, col].Text.Trim();
                                headerIndex[header] = col;
                            }

                            string[] requiredHeaders = {"ایدی کالا","کد کالا","نام غذا","ایدی گروه کالا","قیمت پایه","درصد مالیات","پرو","این کالا غذا هست",
                            "آنالیز غذا","ایدی پایه محصول","فعال/غیرفعال"};

                            foreach (var header in requiredHeaders)
                            {
                                if (!headerIndex.ContainsKey(header))
                                {
                                    return new ApiErrorResponse(
                                        _Code: StatusCodes.Status405MethodNotAllowed,
                                        _Message: "هدر فایل اکسل اشتباه است"
                                    );
                                }
                            }

                            bool hasImageColumn = headerIndex.ContainsKey("تصویر");

                            var records = new List<FoodEditRequest>();
                            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                            {
                                if (string.IsNullOrWhiteSpace(worksheet.Cells[row, headerIndex["ایدی کالا"]].Text))
                                    continue;

                                var idText = worksheet.Cells[row, headerIndex["ایدی کالا"]].Text;
                                int id = string.IsNullOrWhiteSpace(idText) ? 0 : int.Parse(idText);

                                var arpaNumber = worksheet.Cells[row, headerIndex["کد کالا"]].Text;


                                var foodRequest = new FoodEditRequest
                                {
                                    id = id,
                                    title = worksheet.Cells[row, headerIndex["نام غذا"]].Text,
                                    description = worksheet.Cells[row, headerIndex["آنالیز غذا"]].Text,
                                    groupId = int.TryParse(worksheet.Cells[row, headerIndex["ایدی گروه کالا"]].Text, out var groupId) ? groupId : 0,
                                    productType = int.TryParse(worksheet.Cells[row, headerIndex["ایدی پایه محصول"]].Text, out var foodType) ? foodType : 0,
                                    active = ParsePersianBool(worksheet.Cells[row, headerIndex["فعال/غیرفعال"]].Text),
                                    vip = ParsePersianBool(worksheet.Cells[row, headerIndex["پرو"]].Text),
                                    isFood = ParsePersianBool(worksheet.Cells[row, headerIndex["این کالا غذا هست"]].Text),
                                    echoPrice = int.TryParse(worksheet.Cells[row, headerIndex["قیمت پایه"]].Text, out var echoPrice) ? echoPrice : 0,
                                    specialPrice = int.TryParse(worksheet.Cells[row, headerIndex["قیمت پایه"]].Text, out var specialPrice) ? specialPrice : 0,
                                    arpaNumber = arpaNumber,
                                    vat = int.TryParse(worksheet.Cells[row, headerIndex["درصد مالیات"]].Text, out var vat) ? vat : (int?)null,
                                };

                                if (hasImageColumn && worksheet.Cells[row, headerIndex["تصویر"]].Value != null)
                                {
                                    foodRequest.imageData = worksheet.Cells[row, headerIndex["تصویر"]].Value;
                                }

                                records.Add(foodRequest);
                            }

                            foodUpdates.AddRange(await ProcessFoodRecordsAsync(records, newFoods));
                        }
                    }
                }

                await _NarijeDBContext.SaveChangesAsync();
                await transaction.CommitAsync();

                foreach (var newfood in newFoods)
                {
                    await _logHistoryHelper.AddLogHistoryAsync(
                        "Food",
                        newfood.Id,
                        EnumLogHistroyAction.create,
                        EnumLogHistorySource.excel,
                        JsonSerializer.Serialize(newfood),
                        true
                    );
                }

                return new ApiOkResponse("File processed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: $"خطا در پردازش فایل: {ex.Message}"
                );
            }
        }

        private bool ParsePersianBool(string value)
        {
            return value?.Trim() == "بلی";
        }

        private async Task<List<FoodEditRequest>> ProcessFoodRecordsAsync(IEnumerable<FoodEditRequest> records, List<Food> newFoods)
        {
            var foodUpdates = new List<FoodEditRequest>();

            foreach (var record in records)
            {
                Food existingFood = null;
                if (record.id > 0 || record.arpaNumber != null)
                {
                    existingFood = await _NarijeDBContext.Foods
                        .Where(f => f.Id == record.id || f.ArpaNumber == record.arpaNumber)
                        .FirstOrDefaultAsync();
                }

                if (existingFood == null)
                {
                    var newFood = new Food
                    {
                        Title = record.title,
                        Description = record.description,
                        GroupId = record.groupId,
                        Active = record.active,
                        EchoPrice = record.echoPrice ,
                        SpecialPrice = record.specialPrice,
                        Vat = record.vat,
                        ArpaNumber = record.arpaNumber,
                        ProductType = record.productType,
                        Vip = record.vip ?? false,
                        IsFood = record.isFood,
                    };
                    _NarijeDBContext.Foods.Add(newFood);
                    newFoods.Add(newFood);
                }
                else
                {
                    existingFood.Title = record.title;
                    existingFood.Description = record.description;
                    existingFood.Active = record.active;
                    existingFood.EchoPrice = record.echoPrice ;
                    existingFood.SpecialPrice = record.specialPrice;
                   // existingFood.Vip = record.vip ?? false;
                   // existingFood.IsFood  = record.isFood;
                    if (record.vat != null)
                        existingFood.Vat = record.vat;


                    var changes = LogHistoryHelper.GetEntityChanges(record, existingFood);
                    await _logHistoryHelper.AddLogHistoryAsync(
                          "Food",
                          existingFood.Id,
                          EnumLogHistroyAction.update,
                          EnumLogHistorySource.excel,
                          JsonSerializer.Serialize(changes),
                          true
                      );
                    _NarijeDBContext.Foods.Update(existingFood);
                }
            }

            return foodUpdates;
        }
      
        #endregion
    }
}


