using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

using System.Collections.Generic;

using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Security.Claims;

using Azure;
using System.Threading;
using System.Web;
using AngleSharp.Dom;
using IdGen;
using MediatR;
using System.Linq.Expressions;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore.Query;
using Narije.Core.DTOs.Generic;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Export;
using Narije.Core.DTOs.ViewModels.Wallet;
using Narije.Core.Entities;
using Narije.Core.Seedwork;
using Narije.Core.Attributes;

using Narije.Core.Interfaces.GenericRepository;
using Narije.Infrastructure.Helpers;
using Narije.Infrastructure.Contexts;

namespace Narije.Infrastructure.Repositories
{
    public class GenericRepository<T, TId, TRequest, TResponse> : IGenericRepository<T, TId, TRequest, TResponse>
        where T : class, IBaseEntity<TId>, new()
        where TRequest : class, IBaseRequest<TId>, new()
        where TResponse : class

    {
        protected readonly IConfiguration _IConfiguration;
        protected readonly IHttpContextAccessor _IHttpContextAccessor;
        protected readonly NarijeDBContext _NarijeDBContext;
        protected readonly IMapper _IMapper;




        // ------------------
        // Constructor
        // ------------------
        public GenericRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor,
            NarijeDBContext _NarijeDBContext, IMapper _IMapper)
        {
            this._IConfiguration = _IConfiguration;
            this._IHttpContextAccessor = _IHttpContextAccessor;
            this._NarijeDBContext = _NarijeDBContext;
            this._IMapper = _IMapper;
        }

        private async Task<Core.Entities.User> CheckAccess()
        {
            //Check Access
            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;
            int id = Convert.ToInt32(Identity.FindFirst("Id").Value);
            var User = await _NarijeDBContext.Users
                                     .Where(A => A.Id == id)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (User is null)
                return null;

            if (User.Active == false)
                return null;

            return User;

        }


        public async Task<TEntity> FindAsyncNoTracking<TEntity>(TId id) where TEntity : class
        {
            // دریافت DbSet از نوع خاص TEntity
            var dbSet = _NarijeDBContext.Set<TEntity>();

            // به دست آوردن موجودیت از طریق ID
            var entity = await dbSet.FindAsync(id);

            if (entity != null)
            {
                // غیرفعال کردن رهگیری تغییرات
                _NarijeDBContext.Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }


        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(TId id, Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            try
            {
                IQueryable<T> baseQuery = _NarijeDBContext.Set<T>();
                if (include != null) baseQuery = include(baseQuery);
                if (filter != null) baseQuery = baseQuery.Where(filter);
                QueryModel q = new QueryModel()
                {
                    Filter = new List<FilterModel>() { new FilterModel() {

                        Key = "id",
                        Operator = "eq",
                        Value = id.ToString()
                        }
                    }
                };
                if (typeof(TResponse) != typeof(SameEntityResponse))
                {
                    var projectedQuery = baseQuery.ProjectTo<TResponse>(_IMapper.ConfigurationProvider);
                    projectedQuery = projectedQuery.QueryDynamic("", q.Filter);
                    var entities = await GetPaged(projectedQuery, 1, 1);
                    if (entities.Data?.Count < 1)
                    {
                        return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "Entity not found");
                    }
                    return new ApiOkResponse(_Message: "SUCCEED", _Data: entities.Data[0]);
                }
                else
                {
                    var response = await FindAsyncNoTracking<T>(id);

                    if (response == null)
                    {
                        return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "Entity not found");
                    }
                    return new ApiOkResponse(_Message: "SUCCEED", _Data: response);
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit, Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, object>>? orderBy = null, bool? isDesc = null, bool? active = null)
        {
            try
            {


                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, GetDbSetName());
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);


                IQueryable<T> baseQuery = _NarijeDBContext.Set<T>();
                if (include != null) baseQuery = include(baseQuery);
                if (filter != null) baseQuery = baseQuery.Where(filter);



                // برای Type های خاص که نیاز به پروجکشن دارند

                if (typeof(TResponse) != typeof(SameEntityResponse))
                {
                    var projectedQuery = baseQuery.ProjectTo<TResponse>(_IMapper.ConfigurationProvider);
                    if (active != null)
                    {
                        QueryModel q = new QueryModel()
                        {
                            Filter = new List<FilterModel>() { new FilterModel() {

                        Key = "active",
                        Operator = "eq",
                        Value = active.ToString()
                        }
                            }
                        };
                        projectedQuery = projectedQuery.QueryDynamic("", q.Filter);
                    }

                    projectedQuery = projectedQuery.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                    var entities = await GetPaged(projectedQuery, page.Value, limit.Value);

                    return new ApiOkResponse(_Message: "SUCCESS", _Data: entities.Data, _Meta: entities.Meta, _Header: header);
                }
                else
                {
                    // برای بارگذاری بدون پیگیری تغییرات
                    var noTrackingQuery = baseQuery.AsNoTracking(); 

                    noTrackingQuery = noTrackingQuery.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                    if (active != null)
                    {
                        QueryModel q = new QueryModel()
                        {
                            Filter = new List<FilterModel>() { new FilterModel() {

                        Key = "active",
                        Operator = "eq",
                        Value = active.ToString()
                        }
                            }
                        };
                        noTrackingQuery = noTrackingQuery.QueryDynamic("", q.Filter);
                    }

                    var entities = await GetPaged(noTrackingQuery, page.Value, limit.Value);
                 

                    return new ApiOkResponse(_Message: "SUCCESS", _Data: entities.Data, _Meta: entities.Meta, _Header: header);
                }
            }
            catch (Exception ex)

            {

                throw;
            }
        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(TRequest request)
        {
            try
            {
                var entity = _IMapper.Map<T>(request);

                // بررسی اینکه آیا TRequest از نوع IFileRequest است
                if (request is IFileRequest filerequest)
                {
                    var fileEntity = entity as IBaseGalaryEntity<TId>;

                    if (filerequest.files != null && filerequest.files.Any())
                    {
                        var file = filerequest.files.First();
                        var fileGalleryId = await GalleryHelper.AddToGallery(_NarijeDBContext, GetDbSetName(), file);
                        if (fileGalleryId > 0)
                        {
                            fileEntity.GalleryId = fileGalleryId;
                        }
                    }
                }

                // عملیات عادی ذخیره سازی
                entity.CreatedAt = DateTime.Now;
                var user = await CheckAccess();
                entity.CreatorUserId = user.Id;

                await _NarijeDBContext.Set<T>().AddAsync(entity);

                var result = await _NarijeDBContext.SaveChangesAsync();

                if (result < 0)
                    return new ApiErrorResponse(StatusCodes.Status405MethodNotAllowed, "Data not saved! Please try again");

                return await GetAsync((entity as dynamic).Id);
            }
            catch (Exception ex)
            {
                throw; // برای مدیریت خطا، می‌توانید بهبودهای بیشتری اعمال کنید
            }
        }
        #endregion


        #region PatchAsync

        public async Task<ApiResponse> PatchAsync(TId id, TRequest request)
        {
            try
            {
                var entity = await _NarijeDBContext.Set<T>().FindAsync(id);
                if (entity == null)
                    return new ApiErrorResponse(StatusCodes.Status404NotFound, "دیتا یافت نشد");

                if (request is IFileRequest filerequest)
                {
                    var fileEntity = entity as IBaseGalaryEntity<TId>;

                    if (filerequest.files != null && filerequest.files.Any())
                    {
                        var file = filerequest.files.First();
                        var fileGalleryId = await GalleryHelper.AddToGallery(_NarijeDBContext, GetDbSetName(), file);
                        if (fileGalleryId > 0)
                        {
                            fileEntity.GalleryId = fileGalleryId;
                        }
                    }
                }

                _IMapper.Map(request, entity);
                _NarijeDBContext.Set<T>().Update(entity);

                var result = await _NarijeDBContext.SaveChangesAsync();

                if (result < 0)
                    return new ApiErrorResponse(StatusCodes.Status405MethodNotAllowed,
                        "Data not updated! Please try again");

                return await GetAsync((entity as dynamic).Id);
            }
            catch (Exception ex)
            {
                throw; // مدیریت خطا
            }
        }

        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(TRequest request)
        {
            try
            {


                var entity = await _NarijeDBContext.Set<T>().FindAsync(request.Id);


                if (entity == null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound,
                        _Message: "دیتا جهت به روز رسانی یافت نشد");

                _IMapper.Map(request, entity);

                if (request is IFileRequest filerequest)
                {
                    var fileEntity = entity as IBaseGalaryEntity<TId>;

                    if (filerequest.files != null && filerequest.files.Any())
                    {
                        var file = filerequest.files.First();
                        var fileGalleryId = await GalleryHelper.AddToGallery(_NarijeDBContext, GetDbSetName(), file);
                        if (fileGalleryId > 0)
                        {
                            fileEntity.GalleryId = fileGalleryId;
                        }
                    }
                }



                _NarijeDBContext.Set<T>().Update(entity);

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "Data not saved! Please try again");

                return await GetAsync((entity as dynamic).Id);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        #region EditActiveAsync
        // ------------------
        //  EditActiveAsync
        // ------------------
        public async Task<ApiResponse> EditActiveAsync(TId id)
        {
            var entity = await _NarijeDBContext.Set<T>().FindAsync(id);

            if (entity == null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "Data not found for editing");

            (entity as dynamic).Active = !(entity as dynamic).Active;

            _NarijeDBContext.Set<T>().Update(entity);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "Data not saved! Please try again");

            return await GetAsync((entity as dynamic).Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(TId id)
        {
            try
            {


                var entity = await _NarijeDBContext.Set<T>().FindAsync(id);

                if (entity == null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "Data not found for deletion");

                _NarijeDBContext.Set<T>().Remove(entity);

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "Data not saved! Please try again");

                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion

        public IUnitOfWork UnitOfWork => throw new NotImplementedException();

        #region ExportAsync
        // ------------------
        //  ExportAsync
        // ------------------
        public async Task<ApiResponse> ExportAsync(string ParentIdName = null, Expression<Func<T, bool>>? filter = null,
                   Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();

            var TableName = _NarijeDBContext.Set<T>().EntityType.ClrType.Name;


            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, GetDbSetName(),true);

            result.header = dbheader.Select(A => A.title).ToList();

            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Set<T>().AsQueryable();
            if (include != null) Q = include(Q);
            if (filter != null) Q = Q.Where(filter);

            var Param = HttpUtility.ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            if (ParentIdName != null)
            {
                var parentKey = Param.AllKeys.FirstOrDefault(A => A.Equals(ParentIdName, StringComparison.OrdinalIgnoreCase));

                if (parentKey != null)
                {
                    string parentIdValue = Param[parentKey.ToString()];
                    if (!string.IsNullOrEmpty(parentIdValue))
                    {
                        int shippingCompanyId = Convert.ToInt32(parentIdValue);
                        Q = Q.Where(q => EF.Property<int>(q, ParentIdName) == shippingCompanyId);
                    }
                }

            }


            var key = Param.AllKeys.Where(A => A.Equals("ids")).FirstOrDefault();
            if (key != null)
            {
                string ids = "";
                ids = Param[key.ToString()];
                if (ids != "")
                {
                    int[] nids = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    Q = Q.Where("(@0.Contains(id))", nids);
                }
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            List<object> data = new();
            switch (TableName.ToLower())
            {
                case "wallet":
                    data = await Q.ProjectTo<WalletResponse>(_IMapper.ConfigurationProvider).ToListAsync<object>();
                    break;
                default:
                    data = await Q.ToListAsync<object>();
                    break;
            }

            var MapToTable = true;
            if (TableName == "Supplier")
                MapToTable = false;

            result.body = ExportHelper.MakeResult(data, dbheader, MapToTable);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
        }

        #endregion


        #region ImportAsync

        public async Task<ApiResponse> ImportDataAsync(int companyId, List<IFormFile> files, List<string> fieldNames, string keyFieldName, bool isUpdate, Action<T, string, object> setField)
        {
            List<T> data = new List<T>();
            try
            {
                var stream = files[0].OpenReadStream();
                StreamReader reader = new StreamReader(stream);

                var TableName = _NarijeDBContext.Set<T>().EntityType.ClrType.Name;

                var headers = new List<Header>();
                if (isUpdate)
                    headers = await _NarijeDBContext.Headers.Where(A => A.TableName.Equals(TableName) && A.ImportUpdate == true).ToListAsync();
                else
                    headers = await _NarijeDBContext.Headers.Where(A => A.TableName.Equals(TableName) && A.Import).ToListAsync();

                var fieldMappings = headers.ToDictionary(
                    fieldName => fieldName.FieldName,
                    fieldName => fieldName.Title
                );




                if (files[0].FileName.ToLower().Contains(".csv"))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        Dictionary<string, int> fieldIndices = new Dictionary<string, int>();
                        csv.Read();

                        for (int j = 0; j < csv.Parser.Count; j++)
                        {
                            var content = csv.GetField(j);
                            foreach (var fieldMapping in fieldMappings)
                            {
                                if (content.Equals(fieldMapping.Value))
                                    fieldIndices[fieldMapping.Key] = j;
                            }
                        }

                        while (csv.Read())
                        {
                            T item = null;
                            if (isUpdate)
                            {
                                var idValue = csv.GetField(fieldIndices["id"]);
                                if (!string.IsNullOrEmpty(idValue))
                                {
                                    int id = Convert.ToInt32(idValue);
                                    item = await _NarijeDBContext.Set<T>().FindAsync(id);
                                }
                            }

                            if (item == null)
                            {
                                item = new T();
                                await _NarijeDBContext.AddAsync(item);  // اضافه کردن رکورد جدید
                            }
                            else
                            {
                                _NarijeDBContext.Update(item);  // به‌روزرسانی رکورد موجود
                            }

                            foreach (var field in fieldIndices)
                            {
                                var propertyInfo = typeof(T).GetProperties()
                                    .FirstOrDefault(p => p.Name.Equals(field.Key, StringComparison.OrdinalIgnoreCase));

                                if (propertyInfo != null)
                                {
                                    var cellValue = csv.GetField(field.Value);
                                    object value = null;

                                    if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                                    {
                                        int.TryParse(cellValue, out int intValue);
                                        value = intValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(float?))
                                    {
                                        float.TryParse(cellValue, out float floatValue);
                                        value = floatValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(double) || propertyInfo.PropertyType == typeof(double?))
                                    {
                                        double.TryParse(cellValue, out double doubleValue);
                                        value = doubleValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                                    {
                                        DateTime.TryParse(cellValue, out DateTime dateValue);
                                        value = dateValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?))
                                    {
                                        bool.TryParse(cellValue, out bool boolValue);
                                        value = boolValue;
                                    }
                                    else
                                    {
                                        value = cellValue;
                                    }

                                    if (value != null)
                                    {
                                        propertyInfo.SetValue(item, value);
                                    }
                                }
                            }

                            data.Add(item);
                        }
                    }
                }
                else
                {
                    // Excel File Processing (Similar to CSV with adjustments for Excel)
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        int rows = worksheet.Dimension.Rows;
                        int columns = worksheet.Dimension.Columns;

                        Dictionary<string, int> fieldIndices = new Dictionary<string, int>();

                        for (int j = 1; j <= columns; j++)
                        {
                            string content = worksheet.Cells[1, j].Value.ToString();
                            foreach (var fieldMapping in fieldMappings)
                            {
                                if (content.Equals(fieldMapping.Value))
                                    fieldIndices[fieldMapping.Key] = j;
                            }
                        }

                        for (int i = 2; i <= rows; i++)
                        {
                            bool isRowEmpty = true;
                            for (int j = 1; j <= columns; j++)
                            {
                                if (!string.IsNullOrWhiteSpace(worksheet.Cells[i, j].Text.ToString()))
                                {
                                    isRowEmpty = false;
                                    break;
                                }
                            }

                            if (isRowEmpty)
                                continue;
                            T item = null;
                            if (isUpdate)
                            {
                                var idValue = worksheet.Cells[i, fieldIndices["id"]].Text.ToString();
                                if (!string.IsNullOrEmpty(idValue))
                                {
                                    int id = Convert.ToInt32(idValue);
                                    item = await _NarijeDBContext.Set<T>().FindAsync(id);
                                }
                            }

                            if (item == null)
                            {
                                item = new T();
                                setField(item, keyFieldName, companyId);
                                await _NarijeDBContext.AddAsync(item);  // اضافه کردن رکورد جدید
                            }
                            else
                            {
                                _NarijeDBContext.Update(item);  // به‌روزرسانی رکورد موجود
                            }

                            foreach (var field in fieldIndices)
                            {
                                var propertyInfo = typeof(T).GetProperties()
                                    .FirstOrDefault(p => p.Name.Equals(field.Key, StringComparison.OrdinalIgnoreCase));

                                if (propertyInfo != null)
                                {
                                    var cellValue = worksheet.Cells[i, field.Value].Text.ToString();
                                    object value = null;

                                    if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                                    {
                                        int.TryParse(cellValue, out int intValue);
                                        value = intValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(float?))
                                    {
                                        float.TryParse(cellValue, out float floatValue);
                                        value = floatValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(double) || propertyInfo.PropertyType == typeof(double?))
                                    {
                                        double.TryParse(cellValue, out double doubleValue);
                                        value = doubleValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                                    {
                                        DateTime.TryParse(cellValue, out DateTime dateValue);
                                        value = dateValue;
                                    }
                                    else if (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?))
                                    {
                                        value = cellValue == "1" ? true : false;

                                    }
                                    else
                                    {
                                        value = cellValue;
                                    }

                                    if (value != null)
                                    {
                                        propertyInfo.SetValue(item, value);
                                    }
                                }
                            }

                            data.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "فایل به درستی انتخاب نشده است");
            }

            await _NarijeDBContext.SaveChangesAsync();

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
        }

        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAllAsync(List<TId> ids = null, object parentId = null)
        {
            try
            {
                IQueryable<T> query = _NarijeDBContext.Set<T>();


                var parentProperty = typeof(T).GetProperties()
                                              .FirstOrDefault(p => p.GetCustomAttributes(typeof(ParentIdAttribute), true).Any());

                if (parentProperty == null && parentId is not null)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "ParentId field not found.");
                }


                var parentIdValue = parentId is not null ? Convert.ChangeType(parentId, parentProperty.PropertyType) : null;
                if (parentIdValue == null && parentId is not null)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "Type conversion failed.");
                }

                if (ids != null && ids.Any())
                {

                    var entities = query.AsEnumerable()
                          .Where(e => ids.Contains((TId)typeof(T).GetProperty("Id").GetValue(e))).ToList();
                    _NarijeDBContext.Set<T>().RemoveRange(entities);
                }
                else if (parentId is not null)
                {

                    var entities = query.AsEnumerable().Where(e => parentProperty.GetValue(e).Equals(parentIdValue))
                                              .ToList();
                    _NarijeDBContext.Set<T>().RemoveRange(entities);
                }
                else
                {
                    var entities = await query.ToListAsync();
                    _NarijeDBContext.Set<T>().RemoveRange(entities);
                }

                var result = await _NarijeDBContext.SaveChangesAsync();
                if (result < 0)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "Data not saved! Please try again");
                }

                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        #region UpdateAllActiveAsync
        // ------------------
        //  UpdateAsync
        // ------------------
        public async Task<ApiResponse> UpdateAllActiveAsync(List<TId> ids = null, object parentId = null, bool activeValue = false)
        {
            try
            {
                IQueryable<T> query = _NarijeDBContext.Set<T>();

                var parentProperty = typeof(T).GetProperties()
                                              .FirstOrDefault(p => p.GetCustomAttributes(typeof(ParentIdAttribute), true).Any());

                if (parentProperty == null && parentId is not null)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "ParentId field not found.");
                }


                var parentIdValue = parentId is not null ? Convert.ChangeType(parentId, parentProperty.PropertyType) : null;
                if (parentIdValue == null && parentId is not null)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "Type conversion failed.");
                }


                if (ids != null && ids.Any())
                {
                    var entities = query.AsEnumerable()
                                        .Where(e => ids.Contains((TId)typeof(T).GetProperty("Id").GetValue(e)))
                                        .ToList();

                    entities.ForEach(e => typeof(T).GetProperty("Active").SetValue(e, activeValue));
                }

                else if (parentId is not null)
                {
                    var entities = query.AsEnumerable()
                                        .Where(e => parentProperty.GetValue(e).Equals(parentIdValue))
                                        .ToList();


                    entities.ForEach(e => typeof(T).GetProperty("Active").SetValue(e, activeValue));
                }

                else
                {
                    var entities = await query.ToListAsync();


                    entities.ForEach(e => typeof(T).GetProperty("Active").SetValue(e, activeValue));
                }


                var result = await _NarijeDBContext.SaveChangesAsync();
                if (result < 0)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "Data not updated! Please try again");
                }

                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                throw;
            }
        }

        #endregion



        public async Task<IQueryable<TCollection>> GetCollectionAsync<TCollection>(
         Expression<Func<T, bool>> filterExpression,
         string collectionPropertyName) where TCollection : class
        {

            var dataQuery = _NarijeDBContext.Set<T>().AsQueryable().Include(collectionPropertyName);


            var filteredData = dataQuery.Where(filterExpression);


            var dataList = await filteredData.ToListAsync();


            var resultCollection = dataList.SelectMany(item =>
                (ICollection<TCollection>)typeof(T).GetProperty(collectionPropertyName).GetValue(item)).AsQueryable();

            return resultCollection;
        }

        /// <summary>
        /// تابع صفحه بندی
        /// </summary>
        private async Task<Core.DTOs.Public.PagedResult<T>> GetPaged(IQueryable<T> query, int Page, int Limit)
        {
            try
            {


                var Meta = new MetaResult()
                {
                    CurrentPage = Page,
                    Limit = Limit,
                    Total = await query.CountAsync(),
                    TotalInPage = Limit,
                    Prev = Page - 1
                };

                if (Limit == 0)
                    Limit = 100;
                var PageCount = (double)Meta.Total / Limit;
                Meta.TotalPage = (int)Math.Ceiling(PageCount);

                if (Meta.Prev < 0)
                    Meta.Prev = null;

                var response = await query.Skip((Page - 1) * Limit).Take(Limit).ToListAsync();

                var Result = new Core.DTOs.Public.PagedResult<T>
                {
                    Data = response,
                    Meta = Meta
                };

                return Result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<Core.DTOs.Public.PagedResult<TResponse>> GetPaged(IQueryable<TResponse> query, int Page, int Limit)
        {
            var Meta = new MetaResult()
            {
                CurrentPage = Page,
                Limit = Limit,
                Total = await query.CountAsync(),
                TotalInPage = Limit,
                Prev = Page - 1
            };

            if (Limit == 0)
                Limit = 100;
            var PageCount = (double)Meta.Total / Limit;
            Meta.TotalPage = (int)Math.Ceiling(PageCount);

            if (Meta.Prev < 0)
                Meta.Prev = null;

            var response = await query.Skip((Page - 1) * Limit).Take(Limit).ToListAsync();

            var Result = new Core.DTOs.Public.PagedResult<TResponse>
            {
                Data = response,
                Meta = Meta
            };

            return Result;
        }

        private string GetDbSetName()
        {
            var dbSetType = typeof(DbSet<T>);
            var properties = _NarijeDBContext?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties == null)
            {
                throw new InvalidOperationException("Failed to retrieve properties from the DbContext.");
            }

            var property = properties.FirstOrDefault(p => dbSetType.IsAssignableFrom(p.PropertyType));

            if (property == null)
            {
                throw new InvalidOperationException($"No DbSet<{typeof(T).Name}> found in the DbContext.");
            }

            return property.Name;
        }




    }
}
