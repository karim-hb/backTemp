using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.Public;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace Narije.Infrastructure.Helpers
{
    public class HeaderHelper
    {
        public static async Task<List<FieldResponse>> GetHeader(NarijeDBContext _NarijeDBContext, IHttpContextAccessor _IHttpContextAccessor, string TableName, bool JustExport = false)
        {
            List<FieldResponse> headers = new();

            var enums = await _NarijeDBContext.Enums.Where(A => A.TableName.ToLower().Equals(TableName.ToLower()) || A.TableName.Equals("Public")).ToListAsync();
            List<Header> dbheader;

            int Admin = 1; //نمایش فرانت     

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;


            if (Identity != null)
            {
                var role = Identity.FindFirst(ClaimTypes.Role).Value;
                if (role != null)
                {
                    if (role.Equals("supervisor"))
                        Admin = 2;
                    if (role.Equals("customer"))
                        Admin = 3;
                }

            }

            if (JustExport)
            {
                dbheader = await _NarijeDBContext.Headers
                                    .Where(A => A.TableName.ToLower().Equals(TableName.ToLower()) && A.Export)
                                    .OrderBy(A => A.ExportOrder)
                                    .ToListAsync();
            }
            else
            {
                dbheader = await _NarijeDBContext.Headers
                                    .Where(A => A.TableName.ToLower().Equals(TableName.ToLower()) && (A.ShowInList || A.HasFilter))
                                    .OrderBy(A => A.ColumnOrder)
                                    .ToListAsync();
            }

            foreach (var field in dbheader)
            {
                switch (field.AdminColumn)
                {
                    case 0:
                        break;
                    case 1: //front
                        if (Admin != 1)
                            continue;
                        break;
                    case 2: //admin
                        if (Admin != 2)
                            continue;
                        break;
                    case 3: //customer
                        if (Admin != 3)
                            continue;
                        break;
                    case 4: //admin and customer
                        if (Admin != 3 && Admin != 2)
                            continue;
                        break;
                }
                //if ((field.AdminColumn ^ Admin) == 0)
                //    continue;
                FieldResponse header = new FieldResponse()
                {
                    name = field.FieldName,
                    title = field.Title,
                    showInList = field.ShowInList,
                    hasFilter = field.HasFilter,
                    hasOrder = field.HasOrder,
                    showInExtra = field.ShowInExtra,
                    style = field.Style,
                    styleDark = field.StyleDark,
                    type = field.ColumnType,
                    value = null,
                    order = field.ColumnOrder,
                    link = field.Link,
                    filterOrder = field.FilterOrder,
                    defaultFilter = field.DefaultFilter,
                    colSpan = field.ColumnSpan,
                    enums = new()
                };

                if (field.ColumnType.Equals("enum") || field.ColumnType.Equals("urlLink"))
                {
                    header.enums = enums
                                    .Where(A => A.FieldName.ToLower().Equals(field.FieldName.ToLower()) && A.TableName.ToLower().Equals(TableName.ToLower()))
                                    .OrderBy(A => A.ColumnOrder)
                                    .Select(A => new EnumResponse()
                                    {
                                        title = A.Title,
                                        value = A.Value,
                                        style = A.Style,
                                        styleDark = A.StyleDark
                                    })
                                    .ToList();

                    if ((header.enums == null) || (header.enums.Count == 0))
                    {
                        header.enums = enums
                                        .Where(A => A.FieldName.ToLower().Equals(field.FieldName.ToLower()) && A.TableName.Equals("Public"))
                                        .OrderBy(A => A.ColumnOrder)
                                        .Select(A => new EnumResponse()
                                        {
                                            title = A.Title,
                                            value = A.Value,
                                            style = A.Style,
                                            styleDark = A.StyleDark
                                        })
                                        .ToList();
                    }
                }

                if (header.hasFilter && (header.enums.Count == 0))
                {
                    switch (field.FieldName)
                    {
                        case "customerId":
                            header.enums = await _NarijeDBContext.Customers.Where(A => A.Active && A.ParentId != null).Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "jobId":
                            header.enums = await _NarijeDBContext.Job.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "settlementId":
                            header.enums = await _NarijeDBContext.Settlement.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "dishId":
                            header.enums = await _NarijeDBContext.Dish.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "mealType":
                            header.enums = await _NarijeDBContext.Meal.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "branchId":
                            header.enums = await _NarijeDBContext.Branch.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "customerParentId":
                            header.enums = await _NarijeDBContext.Customers.Where(A => A.Active && A.ParentId == null).Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "parentId":
                            header.enums = await _NarijeDBContext.Customers.Where(A => A.Active && A.ParentId == null).Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "accessId":
                            header.enums = await _NarijeDBContext.AccessProfiles.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "id":
                            if (TableName.ToLower().Equals("foodgroup"))
                            {
                                header.enums = await _NarijeDBContext.FoodGroups.Select(A => new EnumResponse()
                                {
                                    title = A.Title,
                                    value = A.Id.ToString()

                                }).ToListAsync();
                            }
                            break;
                        case "groupId":
                            header.enums = await _NarijeDBContext.FoodGroups.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "productType":
                            header.enums = await _NarijeDBContext.FoodType.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
              
                        case "positiveItem":
                            header.enums = await _NarijeDBContext.SurveyItems.Where(A => A.ItemType == 0).Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
             

                        case "negativeItem":
                            header.enums = await _NarijeDBContext.SurveyItems.Where(A => A.ItemType == 1).Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "currentMonthMenuInfo":
                            header.enums = await _NarijeDBContext.MenuInfo.Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                        case "foodId":
                            header.enums = await _NarijeDBContext.Foods.Where(A => A.Active).Select(A => new EnumResponse()
                            {
                                title = A.Title,
                                value = A.Id.ToString()

                            }).ToListAsync();
                            break;
                    }
                }

                headers.Add(header);
            }

            /*
            foreach (var field in fields)
            {
                FieldResponse header = new FieldResponse()
                {
                    name = field.Name,
                    title = field.Name,
                    showInList = true,
                    hasFilter = false,
                    hasOrder = true,
                    showInExtra = false,
                    style = null,
                    type = "string",
                    value = null,
                    order = 0,
                    enums = new()
                };

                var f = dbheader.Where(A => A.FieldName.ToLower().Equals(field.Name.ToLower())).FirstOrDefault();
                if (f != null)
                {
                    header.name = f.FieldName;
                    header.title = f.Title;
                    header.showInList = f.ShowInList;
                    header.hasFilter = f.HasFilter;
                    header.hasOrder = f.HasOrder;
                    header.showInExtra = f.ShowInExtra;
                    header.type = f.ColumnType;
                    header.style = f.Style;
                    header.order = f.ColumnOrder;
                }

                header.enums = enums
                                .Where(A => A.FieldName.ToLower().Equals(field.Name.ToLower()))
                                .Select(A => new EnumResponse()
                                {
                                    title = A.Title,
                                    value = A.Value
                                })
                                .ToList();

                headers.Add(header);
            }
            */

            headers = headers.ToList();

            return headers;
        }
    }
}
