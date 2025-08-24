using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Survey;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.Claims;
using Narije.Core.DTOs.ViewModels.SurveyDetail;
using Newtonsoft.Json;
using Log = Serilog.Log;
using Narije.Core.DTOs.ViewModels.Export;
using Narije.Core.DTOs.ViewModels.FoodPrice;
using System.Web;
using Microsoft.EntityFrameworkCore.Storage;
using Narije.Core.DTOs.Enum;
using System.Runtime.InteropServices;

namespace Narije.Infrastructure.Repositories
{
    public class SurveyRepository : BaseRepository<Survey>, ISurveyRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public SurveyRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Survey = await _NarijeDBContext.Surveys
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<SurveyResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();
            Survey.positive = await _NarijeDBContext.SurveyDetails
                                            .Where(A => A.SurveyId == Survey.id && A.SurveyItem.ItemType == 0)
                                            .Select(A => new SurveyDetailResponse()
                                            {
                                                surveyItemId = A.SurveyItemId,
                                                surveyValueId = A.SurveyValueId,
                                                value = A.Value,
                                                id = id
                                            }).ToListAsync();
            Survey.negative = await _NarijeDBContext.SurveyDetails
                                            .Where(A => A.SurveyId == Survey.id && A.SurveyItem.ItemType == 1)
                                            .Select(A => new SurveyDetailResponse()
                                            {
                                                surveyItemId = A.SurveyItemId,
                                                surveyValueId = A.SurveyValueId,
                                                value = A.Value,
                                                id = id
                                            }).ToListAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Survey);
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit, bool byUser = false)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Survey");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.vSurveys
                        .ProjectTo<SurveyResponse>(_IMapper.ConfigurationProvider);

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            switch (user.Role)
            {
                case (int)EnumRole.user:
                    Q = Q.Where(A => A.userId == user.Id);
                    break;
                case (int)EnumRole.customer:
                    Q = Q.Where(A => A.customerId == user.CustomerId);
                    break;
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Surveys = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Surveys.Data, _Meta: Surveys.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(SurveyInsertRequest request)
        {
            List<SurveyDetail> detail = new();
            var survey = new Survey();

            try
            {
                var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((Identity is null) || (Identity.Claims.Count() == 0))
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

                var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

                if (user is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

                var reserve = await _NarijeDBContext.Reserves
                                        .Where(A => A.Id == request.reserveId && A.UserId == user.Id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (reserve is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات رزرو یافت نشد");

                DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

                var exists = await _NarijeDBContext.Surveys.Where(A => A.ReserveId == reserve.Id).AnyAsync();
                if (exists)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "قبلا نظر داده اید");

                if(reserve.UserId != user.Id)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "فقط دریافت کننده غذا می تواند نظر ثبت نماید");


                List<SurveyDetailInsertRequest> positive = new();
                if (request.positive != null)
                    JsonConvert.PopulateObject(request.positive, positive);

                List<SurveyDetailInsertRequest> negative = new();
                if (request.negative != null)
                    JsonConvert.PopulateObject(request.negative, negative);


                int pos = 0;
                int neg = 0;

                survey = new Survey()
                {
                    FoodId = reserve.FoodId,
                    UserId = user.Id,
                    CustomerId = user.CustomerId.Value,
                    PositiveIndex = pos,
                    NegativeIndex = neg,
                    DateTime = LocalDate,
                    Score = request.score,
                    ReserveId = reserve.Id,
                    Description = request.description
                };

                if (request.files != null)
                {
                    var k = await GalleryHelper.AddToGallery(_NarijeDBContext, "Survey", request.files.FirstOrDefault(), true);
                    if (k > 0)
                        survey.GalleryId = k;
                }

                var items = await _NarijeDBContext.SurveyItems.Select(A => new
                {
                    Id = A.Id,
                    Value = A.Value
                }).ToListAsync();

                foreach (var item in positive)
                {
                    var value = items.Where(A => A.Id == item.surveyItemId).Select(A => A.Value).FirstOrDefault();
                    pos += Int32.Parse(value);
                    detail.Add(new SurveyDetail()
                    {
                        Survey = survey,
                        SurveyItemId = item.surveyItemId,
                        SurveyValueId = null,
                        Value = Int32.Parse(value)
                    });
                }

                var values = await _NarijeDBContext.SurveryValues.Select(A => new
                {
                    Id = A.Id,
                    Value = A.Value
                }).ToListAsync();


                foreach (var item in negative)
                {
                    int value = 0;
                    if(values.Where(A => A.Id == item.surveyValueId).Any())
                        value = values.Where(A => A.Id == item.surveyValueId).Select(A => A.Value).FirstOrDefault();
                    else
                        value = items.Where(A => A.Id == item.surveyItemId).Select(A => Int32.Parse(A.Value)).FirstOrDefault();

                    neg += value;
                    detail.Add(new SurveyDetail()
                    {
                        Survey = survey,
                        SurveyItemId = item.surveyItemId,
                        SurveyValueId = item.surveyValueId,
                        Value = value
                    });
                }

                survey.PositiveIndex = pos;
                survey.NegativeIndex = neg;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");
            }

            using var Transaction = _NarijeDBContext.Database.BeginTransaction();
            try
            {
                await _NarijeDBContext.Surveys.AddAsync(survey);
                await _NarijeDBContext.SurveyDetails.AddRangeAsync(detail);

                var Result = await _NarijeDBContext.SaveChangesAsync();
                Transaction.Commit();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            }
            catch (Exception ex)
            {
                Transaction.Rollback();
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");
            }


            return await GetAsync(survey.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(SurveyEditRequest request)
        {
            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "ویرایش نظر امکانپذیر نیست");

        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Survey = await _NarijeDBContext.Surveys
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Survey is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.Surveys.Remove(Survey);

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
        public async Task<ApiResponse> CloneAsync(SurveyCloneRequest request)
        {
            var Survey = await _NarijeDBContext.Surveys
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Survey is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region Export
        public async Task<ApiResponse> ExportAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Survey", true);
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

            var Q = _NarijeDBContext.vSurveys
                        .ProjectTo<SurveyResponse>(_IMapper.ConfigurationProvider);

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

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();
            switch (user.Role)
            {
                case (int)EnumRole.user:
                    Q = Q.Where(A => A.userId == user.Id);
                    break;
                case (int)EnumRole.customer:
                    Q = Q.Where(A => A.customerId == user.CustomerId);
                    break;
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var data = Q.ToList<object>();

            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region GetPositiveAsync
        // ------------------
        //  GetPositiveAsync
        // ------------------
        public async Task<ApiResponse> GetPositiveAsync(int? page, int? limit)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ReportSurveyPositive");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.SurveyDetails
                        .Where(A => A.SurveyItem.ItemType == 0)
                        .Select(A => new
                        {
                            id = A.Id,
                            customerId = A.Survey.CustomerId,
                            foodId = A.Survey.FoodId,
                            userId = A.Survey.UserId,
                            datetime = A.Survey.DateTime,
                            value = A.Value,
                            surveyItemId = A.SurveyItemId
                        });

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var M = Q.GroupBy(A => A.surveyItemId).Select(A => new
                        {
                            positiveItem = A.Key,
                            value = A.Sum(B => B.value)
                        });

            var key = query.Filter.Where(A => A.Key.Equals("positiveItem")).FirstOrDefault();
            if (key != null)
                M = M.Where(A => A.positiveItem == Int32.Parse(key.Value));

            var Surveys = await M.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Surveys.Data, _Meta: Surveys.Meta, _Header: header);

        }
        #endregion

        #region ExportPositive
        public async Task<ApiResponse> ExportPositiveAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ReportSurveyPositive", true);
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

            var Q = _NarijeDBContext.SurveyDetails
                        .Where(A => A.SurveyItem.ItemType == 0)
                        .Select(A => new
                        {
                            id = A.Id,
                            customerId = A.Survey.CustomerId,
                            foodId = A.Survey.FoodId,
                            userId = A.Survey.UserId,
                            datetime = A.Survey.DateTime,
                            value = A.Value,
                            surveyItemId = A.SurveyItemId
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

            var M = Q.GroupBy(A => A.surveyItemId).Select(A => new
            {
                positiveItem = A.Key,
                value = A.Sum(B => B.value)
            });

            var k = query.Filter.Where(A => A.Key.Equals("positiveItem")).FirstOrDefault();
            if (k != null)
                M = M.Where(A => A.positiveItem == Int32.Parse(k.Value));
            
            var data = M.ToList<object>();

            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region GetNegativeAsync
        // ------------------
        //  GetNegativeAsync
        // ------------------
        public async Task<ApiResponse> GetNegativeAsync(int? page, int? limit)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ReportSurveyNegative");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.SurveyDetails
                        .Where(A => A.SurveyItem.ItemType == 1)
                        .Select(A => new
                        {
                            id = A.Id,
                            customerId = A.Survey.CustomerId,
                            foodId = A.Survey.FoodId,
                            userId = A.Survey.UserId,
                            datetime = A.Survey.DateTime,
                            value = A.SurveyValueId == null ? A.Value : A.SurveyValue.Value,
                            surveyValueId = A.SurveyItemId,
                            surveyItemId = A.SurveyItemId
                        });

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var M = Q.GroupBy(A =>A.surveyValueId).Select(A => new
            {
                negativeItem = A.Key,
                value = A.Sum(B => B.value)
            });

            var key = query.Filter.Where(A => A.Key.Equals("negativeItem")).FirstOrDefault();
            if (key != null)
                M = M.Where(A => A.negativeItem == Int32.Parse(key.Value));

            var Surveys = await M.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Surveys.Data, _Meta: Surveys.Meta, _Header: header);

        }
        #endregion

        #region ExportNegativeAsync
        public async Task<ApiResponse> ExportNegativeAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ReportSurveyNegative", true);
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

            var Q = _NarijeDBContext.SurveyDetails
                        .Where(A => A.SurveyItem.ItemType == 1)
                        .Select(A => new
                        {
                            id = A.Id,
                            customerId = A.Survey.CustomerId,
                            foodId = A.Survey.FoodId,
                            userId = A.Survey.UserId,
                            datetime = A.Survey.DateTime,
                            value = A.SurveyValueId == null ? A.Value : A.SurveyValue.Value,
                            surveyValueId = A.SurveyItemId,
                            surveyItemId = A.SurveyItemId
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

            var M = Q.GroupBy(A => A.surveyValueId).Select(A => new
            {
                negativeItem = A.Key,
                value = A.Sum(B => B.value)
            });

            var k = query.Filter.Where(A => A.Key.Equals("positiveItem")).FirstOrDefault();
            if (k != null)
                M = M.Where(A => A.negativeItem == Int32.Parse(k.Value));

            var data = M.ToList<object>();

            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region ParatelAsync
        // ------------------
        //  ParatelAsync
        // ------------------
        public async Task<ApiResponse> ParatelAsync()
        {
            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Paratel");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.SurveyDetails
                        .Where(A => A.SurveyItem.ItemType == 1)
                        .Select(A => new
                        {
                            id = A.Id,
                            customerId = A.Survey.CustomerId,
                            foodId = A.Survey.FoodId,
                            userId = A.Survey.UserId,
                            datetime = A.Survey.DateTime,
                            value = A.SurveyValueId == null ? A.Value : A.SurveyValue.Value,
                            surveyValueId = A.SurveyItemId,
                            surveyItemId = A.SurveyItemId,
                            title = A.SurveyItem.HasSeparateItems == false ? A.SurveyItem.Title : A.SurveyValue.Title 
                        });

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var M = await Q.GroupBy(A => A.title).Select(A => new ParatelResponse()
            {
                negativeItem = 0,
                value = A.Sum(B => B.value),
                cf = 0,
                cfp = 0,
                title = A.Key
            }).ToListAsync();

            M = M.OrderByDescending(A => A.value).ToList();

            int cf = 0;
            int i = 1;
            foreach (var item in M)
            {
                item.negativeItem = i++;
                cf += item.value;
                item.cf = cf;
            }
            foreach (var item in M)
            {
                item.cfp = Math.Round(((double)item.cf / ((double)cf)) * 100, 2);
            }

            var x = header.Where(A => A.name.Equals("negativeItem")).FirstOrDefault();
            x.enums.Clear();
            foreach (var item in M)
            {
                x.enums.Add(new EnumResponse()
                {
                    title = item.title,
                    value = item.negativeItem.ToString()
                });
            }


            return new ApiOkResponse(_Message: "SUCCESS", _Data: M, _Meta: null, _Header: header);

        }
        #endregion

        #region ExportParatelAsync
        public async Task<ApiResponse> ExportParatelAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Paratel", true);
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

            var Q = _NarijeDBContext.SurveyDetails
                        .Where(A => A.SurveyItem.ItemType == 1)
                        .Select(A => new
                        {
                            id = A.Id,
                            customerId = A.Survey.CustomerId,
                            foodId = A.Survey.FoodId,
                            userId = A.Survey.UserId,
                            datetime = A.Survey.DateTime,
                            value = A.SurveyValueId == null ? A.Value : A.SurveyValue.Value,
                            surveyValueId = A.SurveyItemId,
                            surveyItemId = A.SurveyItemId
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

            var M = await Q.GroupBy(A => A.surveyValueId).Select(A => new ParatelResponse()
            {
                negativeItem = A.Key,
                value = A.Sum(B => B.value),
                cf = 0,
                cfp = 0,
                title = ""
            }).ToListAsync();

            M = M.OrderByDescending(A => A.value).ToList();

            int cf = 0;
            foreach (var item in M)
            {
                cf += item.value;
                item.cf = cf;
            }
            foreach (var item in M)
            {
                item.cfp = Math.Round(((double)item.cf / ((double)cf)) * 100, 2);
            }

            var data = M.ToList<object>();


            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion
    }
}


