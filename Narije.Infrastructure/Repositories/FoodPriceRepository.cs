using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.FoodPrice;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Castle.Core.Resource;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.ViewModels.Export;
using System.Security.Claims;
using System.Web;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.Text.Json;


namespace Narije.Infrastructure.Repositories
{
    public class FoodPriceRepository : BaseRepository<FoodPrice>, IFoodPriceRepository
    {

        // ------------------
        // Constructor
        // ------------------
        private readonly LogHistoryHelper _logHistoryHelper;

        public FoodPriceRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper, LogHistoryHelper logHistoryHelper) :
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
            var FoodPrice = await _NarijeDBContext.FoodPrices
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<FoodPriceResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: FoodPrice);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "FoodPrice");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var customerId = Int32.Parse(query.Filter.Where(A => A.Key == "customerId").FirstOrDefault().Value);
            var Q = _NarijeDBContext.Foods
                                    .Where(A => A.Active)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            groupId = A.GroupId,
                                            group = A.Group.Title,
                                            galleryId = A.GalleryId,
                                            isFood = A.IsFood,
                                            hasType = A.HasType,
                                            echoPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.EchoPrice).FirstOrDefault(),
                                            specialPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.SpecialPrice).FirstOrDefault(),
                                        });

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);
            /*
            var c = query.Filter.Where(A => A.Key == "customerId").FirstOrDefault();

            var Q = _NarijeDBContext.vFoodPrices
                        .ProjectTo<FoodPriceResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter.Where(A => A.Key != "customerId").ToList()).OrderDynamic(query.Sort);

            if(c != null)
            {
                Q = Q.Where(A => A.customerId == null || A.customerId == Int32.Parse(c.Value));
            }
            */

            var FoodPrices = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: FoodPrices.Data, _Meta: FoodPrices.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(FoodPriceInsertRequest request)
        {
            var FoodPrice = new FoodPrice()
            {
                FoodId = request.foodId,
                CustomerId = request.customerId,
                EchoPrice = request.echoPrice,
                SpecialPrice = request.specialPrice,

            };


            await _NarijeDBContext.FoodPrices.AddAsync(FoodPrice);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            await _logHistoryHelper.AddLogHistoryAsync(
                            "FoodPrice",
                             FoodPrice.Id,          
                             EnumLogHistroyAction.create,
                             EnumLogHistorySource.site,
                              JsonSerializer.Serialize(request),                
                             true                    
                       );

            return await GetAsync(FoodPrice.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(FoodPriceEditRequest request)
        {

            var customer = await _NarijeDBContext.Customers.Where(A => A.Id == request.customerId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
            if (customer is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات شرکت یافت نشد");

            var food = await _NarijeDBContext.Foods.Where(A => A.Id == request.foodId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
            if (food is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات غذا یافت نشد");

            var foodprice = await _NarijeDBContext.FoodPrices.Where(A => A.FoodId == request.foodId && A.CustomerId == request.customerId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

            if (foodprice is null)
            {
                foodprice = new FoodPrice()
                {
                    CustomerId = request.customerId,
                    FoodId = request.foodId,
                    EchoPrice = request.echoPrice,
                    SpecialPrice = request.specialPrice
                };
                await _NarijeDBContext.FoodPrices.AddAsync(foodprice);
            }
            else
            {
                foodprice.EchoPrice = request.echoPrice;
                foodprice.SpecialPrice = request.specialPrice;
                _NarijeDBContext.FoodPrices.Update(foodprice);
            }


       



            await _NarijeDBContext.SaveChangesAsync();

            var result = new
            {
                id = foodprice.Id,
                echoPrice = foodprice.EchoPrice,
                specialPrice = foodprice.SpecialPrice
            };

        
                await _logHistoryHelper.AddLogHistoryAsync(
                    "FoodPrice",
                    request.foodId,
                    EnumLogHistroyAction.update,
                    EnumLogHistorySource.site,
                    JsonSerializer.Serialize(request),
                    true
                );
           
           
            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var FoodPrice = await _NarijeDBContext.FoodPrices
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (FoodPrice is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.FoodPrices.Remove(FoodPrice);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region Export
        public async Task<ApiResponse> ExportAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "FoodPrice", true);
            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);

            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();

            result.header = dbheader.Select(A => A.title).ToList();

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            var c = query.Filter.Where(A => A.Key == "customerId").FirstOrDefault();
            var customerId = Int32.Parse(query.Filter.Where(A => A.Key == "customerId").FirstOrDefault().Value);

            var Q = _NarijeDBContext.Foods
                                   .Where(A => A.Active)
                                       .Select(A => new
                                       {
                                           id = A.Id,
                                           title = A.Title,
                                           groupId = A.GroupId,
                                           group = A.Group.Title,
                                           galleryId = A.GalleryId,
                                           isFood = A.IsFood,
                                           hasType = A.HasType,
                                           foodId = A.Id,
                                           echoPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.EchoPrice).FirstOrDefault(),
                                           specialPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.SpecialPrice).FirstOrDefault(),
                                       });


            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);



            var Param = HttpUtility.ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            var key = Param.AllKeys.Where(A => A.Equals("ids")).FirstOrDefault();
            if (key != null)
            {
                var ids = Param[key.ToString()];
                if (ids != "")
                {
                    int[] nids = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    Q = Q.Where(A => nids.Contains(A.id));
                }
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var data = Q.ToList<object>();

            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(FoodPriceCloneRequest request)
        {
            var FoodPrice = await _NarijeDBContext.FoodPrices
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (FoodPrice is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region InsertFromExel

        public async Task<ApiResponse> ProcessFoodPriceFileAsync(IFormFile file, int customerId)
        {
            // List to store records for batch processing
            var foodPriceUpdates = new List<FoodPrice>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                if (file.FileName.EndsWith(".csv"))
                {
                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        var records = csv.GetRecords<FoodPriceCsvRecord>().ToList();
                        foodPriceUpdates.AddRange(await ProcessRecordsAsync(records, customerId));
                    }
                }
                else if (file.FileName.EndsWith(".xlsx"))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var records = new List<FoodPriceCsvRecord>();
                        var headers = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]
                            .ToDictionary(cell => cell.Text.Trim(), cell => cell.Start.Column);

                       for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            records.Add(new FoodPriceCsvRecord
                            {
                                id = int.TryParse(worksheet.Cells[row, headers["شناسه ردیف"]].Text, out var id) ? id : 0,
                                foodId = int.Parse(worksheet.Cells[row, headers["ایدی غذا"]].Text),
                                echoPrice = int.Parse(worksheet.Cells[row, headers["قیمت اکو"]].Text),
                                specialPrice = int.Parse(worksheet.Cells[row, headers["قیمت ویژه"]].Text),
                            });
                        }
                        foodPriceUpdates.AddRange(await ProcessRecordsAsync(records, customerId));
                    }
                }
            }

            await _NarijeDBContext.FoodPrices.AddRangeAsync(foodPriceUpdates.Where(fp => fp.Id == 0));
            await _NarijeDBContext.SaveChangesAsync();

            return new ApiOkResponse("File processed successfully.");
        }

        private async Task<List<FoodPrice>> ProcessRecordsAsync(IEnumerable<FoodPriceCsvRecord> records, int customerId)
        {
            var foodPriceUpdates = new List<FoodPrice>();

            foreach (var record in records)
            {
                var foodPrice = await _NarijeDBContext.FoodPrices
                    .Where(fp => fp.FoodId == record.foodId && fp.CustomerId == customerId)
                    .FirstOrDefaultAsync();

                if (foodPrice == null)
                {
                    foodPrice = new FoodPrice
                    {
                        FoodId = record.foodId,
                        CustomerId = customerId,
                        EchoPrice = record.echoPrice / 10,
                        SpecialPrice = record.specialPrice / 10,
                    };

                    foodPriceUpdates.Add(foodPrice);
                    await _logHistoryHelper.AddLogHistoryAsync(
                           "FoodPrice",
                              record.foodId,
                               EnumLogHistroyAction.create,
                           EnumLogHistorySource.excel,
                           JsonSerializer.Serialize(record),
                            false
                         );
                }
                else
                {
                    foodPrice.EchoPrice = record.echoPrice /10;
                    foodPrice.SpecialPrice = record.specialPrice /10;
                    _NarijeDBContext.FoodPrices.Update(foodPrice);
                    await _logHistoryHelper.AddLogHistoryAsync(
                      "FoodPrice",
                        record.foodId,
                       EnumLogHistroyAction.update,
                       EnumLogHistorySource.excel,
                       JsonSerializer.Serialize(record),
                          false
                    );

                }
            }

            return foodPriceUpdates;
        }

        #endregion

   

    }
}


