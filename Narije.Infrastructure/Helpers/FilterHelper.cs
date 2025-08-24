using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;

namespace Narije.Infrastructure.Helpers
{
    public static class FilterHelper
    {
        public static QueryModel GetQuery(List<FieldResponse> fields, IHttpContextAccessor _IHttpContextAccessor)
        {

            return GetQuery(fields, _IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
        }


        public static QueryModel GetQuery(List<FieldResponse> fields, string QueryString)
        {
            QueryModel model = new();
            model.Filter = new();
            model.Sort = new();
            model.Search = "";

            var Param = HttpUtility.ParseQueryString(QueryString);
            var AllKey = Param.AllKeys.Where(A => A.StartsWith("filter") && A.Contains("[key]")).ToList();
            foreach (var Key in AllKey)
            {
                var str = $"{Key.Replace("[key]", string.Empty)}[value]";
                var value = Param.AllKeys.Where(A => A.StartsWith(str)).FirstOrDefault();

                if (!string.IsNullOrEmpty(Param[value]))
                {
                    str = $"{Key.Replace("[key]", string.Empty)}[operator]";
                    var op = Param.AllKeys.Where(A => A.StartsWith(str)).FirstOrDefault();

                    model.Filter.Add(new FilterModel()
                    {
                        Key = Param[Key.ToString()],
                        Operator = string.IsNullOrEmpty(Param[op]) ? "eq" : Param[op],
                        Value = Param[value]
                    });

                }

                /*
                var str = $"{Key.Replace("[key]", string.Empty)}[value]";
                var AllValue = Param.AllKeys.Where(A => A.StartsWith(str)).ToList();
                foreach (var Value in AllValue)
                {
                    if (!string.IsNullOrEmpty(Param[Value]))
                    {
                        str = $"{Key.Replace("[key]", string.Empty)}[operator]";
                        var ops = Param.AllKeys.Where(A => A.StartsWith(str)).FirstOrDefault();

                        model.Filter.Add(new FilterModel()
                        {
                            Key = Param[Key.ToString()],
                            Operator = "eq",
                            Value = Param[Value]
                        });
                    }
                }
                */
            }

            AllKey = Param.AllKeys.Where(A => A.StartsWith("sort") && A.Contains("[key]")).ToList();
            foreach (var Key in AllKey)
            {
                var str = $"{Key.Replace("[key]", string.Empty)}[direction]";
                var dir = Param.AllKeys.Where(A => A.StartsWith(str)).FirstOrDefault();

                model.Sort.Add(new SortModel()
                {
                    Key = Param[Key.ToString()],
                    Direction = string.IsNullOrEmpty(Param[dir]) ? "desc" : Param[dir]
                });

            }

            var search = Param.AllKeys.Where(A => A.Equals("search")).FirstOrDefault();
            if (search != null)
            {
                model.Search = Param[search.ToString()];
            }


            if (model.Filter.Count == 0)
            {
                var DefaultFilters = fields.Where(A => A.defaultFilter != null).ToList();

                model.Filter.AddRange(
                            fields.Where(A => A.defaultFilter != null)
                                    .Select(A => new FilterModel()
                                    {
                                        Key = A.name,
                                        Operator = "eq",
                                        Value = A.defaultFilter
                                    }).ToList());
            }

            return model;
        }
    }
}
