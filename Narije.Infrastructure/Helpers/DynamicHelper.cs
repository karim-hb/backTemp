using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using System.Linq.Dynamic.Core;

namespace Narije.Infrastructure.Helpers
{
    public static class DynamicHelper
    {
        private static readonly MethodInfo OrderByMethod =
            typeof(Queryable).GetMethods().Single(method =>
            method.Name == "OrderBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo OrderByDescendingMethod =
            typeof(Queryable).GetMethods().Single(method =>
            method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        public static IQueryable<T> FilterDynamic<T>(this IQueryable<T> query, string fieldName, ICollection<string> values)
        {
            var param = Expression.Parameter(typeof(T), "e");
            var prop = Expression.PropertyOrField(param, fieldName);
            var body = Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(string) },
                Expression.Constant(values), prop);
            var predicate = Expression.Lambda<Func<T, bool>>(body, param);

            //var expression = Expression.Equal(prop, new[] { typeof(string) },
            //    Expression.Constant(values));
            return query.Where(predicate);
        }

        public static bool PropertyExists<T>(this IQueryable<T> source, string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                BindingFlags.Public | BindingFlags.Instance) != null;
        }
        public static bool PropertyExists<T>(this IList<T> source, string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase |
                BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static DbContext GetDbContext(IQueryable query)
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var queryCompiler = typeof(EntityQueryProvider).GetField("_queryCompiler", bindingFlags).GetValue(query.Provider);
            var queryContextFactory = queryCompiler.GetType().GetField("_queryContextFactory", bindingFlags).GetValue(queryCompiler);

            var dependencies = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", bindingFlags).GetValue(queryContextFactory);
            var queryContextDependencies = typeof(DbContext).Assembly.GetType(typeof(QueryContextDependencies).FullName);
            var stateManagerProperty = queryContextDependencies.GetProperty("StateManager", bindingFlags | BindingFlags.Public).GetValue(dependencies);
            var stateManager = (IStateManager)stateManagerProperty;

            return stateManager.Context;
        }

        public static IQueryable<T> QueryDynamic<T>(this IQueryable<T> query, string search, List<FilterModel> filters)
        {

            //string filter = "x => x.Name == \"AA\" && @0.Any(y => y.Id == x.Id && y.Name != x.Name)";
            string filter = "A => ";
            foreach (var item in filters)
            {
                if (!query.PropertyExists(item.Key))
                    continue;

                var op = "==";
                var suffix = "";
                switch (item.Operator)
                {
                    case "eq":
                        op = "==";
                        break;
                    case "gt":
                        op = ">";
                        break;
                    case "ge":
                        op = ">=";
                        break;
                    case "lt":
                        op = "<";
                        break;
                    case "le":
                        op = "<=";
                        break;
                    case "no":
                        op = "!=";
                        break;
                    case "lk":
                        op = ".Contains(";
                        suffix = ")";
                        break;
                }
                if (filter.Length > 5)
                    filter += " && ";
                var isNumeric = int.TryParse(item.Value, out _);
                if ((!isNumeric) && (!item.Value.StartsWith("\"")))
                    item.Value = $"\"{item.Value}\"";
                filter += $"A.{item.Key}{op}{item.Value}{suffix} ";
            }

            //if any filter exists
            if (filter.Length > 5)
            {
                var exp = DynamicExpressionParser.ParseLambda<T, bool>(ParsingConfig.Default, false, filter, null);
                query = query.Where(exp);

                //var func = exp.Compile();
            }

           
            if ((search != null) && (search != ""))
            {

                var _NarijehContext = (NarijeDBContext)GetDbContext(query);

                var searchfields = _NarijehContext.Searches.Where(A => A.TableName.Equals(query.ElementType.Name.Replace("Response", ""))).ToList();
                if ((searchfields != null) && (searchfields.Count() > 0))
                {
                    filter = "A => ";
                    foreach (var field in searchfields)
                    {
                        switch (field.FieldType)
                        {
                            case "string":
                                {
                                    if (filter.Length > 5)
                                        filter += " || ";
                                    var fieldNames = new List<string> { "Fname", "Lname", "UserFullName", "User" };
                                    if (fieldNames.Contains(field.FieldName))
                                    {
                                        if (searchfields.Select(s => s.FieldName.ToLower()).Contains("Fname".ToLower()) &&
                                            searchfields.Select(s => s.FieldName.ToLower()).Contains("Lname".ToLower()))
                                            filter +=
                                                $"(A.Fname.Replace(\" \",\"\") + A.Lname.Replace(\" \",\"\")).Contains(\"{search.Replace(" ", "")}\")";
                                        if (searchfields.Select(s => s.FieldName.ToLower())
                                            .Contains("UserFullName".ToLower()))
                                            filter += $"A.UserFullName.Contains(\"{search}\") || ";
                                        if (searchfields.Select(s => s.FieldName.ToLower()).Contains("User".ToLower()))
                                            filter += $"A.User.Contains(\"{search}\") || ";
                                    }
                                    else
                                    {
                                        filter = filter.Replace("||  ||", "||");
                                        filter += $"A.{field.FieldName}.Contains(\"{search}\")";
                                    }

                                    break;
                                }
                            case "number":
                                {
                                    if (filter.Length > 5)
                                        filter += " || ";

                                    filter += $"A.{field.FieldName}.ToString().Equals(\"{search}\")";
                                    break;
                                }
                        }

                    }
                    if (filter.Length > 5)
                    {
                        var exp = DynamicExpressionParser.ParseLambda<T, bool>(ParsingConfig.Default, false, filter, null);
                        query = query.Where(exp);
                        //query = query.Where(filter);
                    }
                    return query;

                }

                var d = query.FirstOrDefault();
                if (d != null)
                {
                    filter = "A => ";
                    var fields = d.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(field => !field.PropertyType.FullName.Contains("LazyLoader")
                                   && !field.Name.Equals("LazyLoader", StringComparison.OrdinalIgnoreCase));

                    foreach (var field in fields)
                    {
                        if (field.Name.Equals("user") || field.Name.Equals("userName") || field.Name.Equals("defaultAddress") || field.Name.Equals("applier") || field.Name.Equals("applicant") || field.Name.Equals("leasing"))
                            continue;

                        if (field.PropertyType.Name.Equals("String"))
                        {
                            if (filter.Length > 5)
                                filter += " || ";

                            filter += $"A.{field.Name}.Contains(\"{search}\")";
                        }
                        else if (field.Name.ToLower().Equals("id") || field.Name.ToLower().Equals("orderid") || field.Name.ToLower().Equals("userid") || field.Name.ToLower().Equals("productid"))
                        {
                            if (filter.Length > 5)
                                filter += " || ";

                            filter += $"A.{field.Name}.ToString().Equals(\"{search}\")";
                        }
                    }
                    if (filter.Length > 5)
                    {
                        var exp = DynamicExpressionParser.ParseLambda<T, bool>(ParsingConfig.Default, false, filter, null);
                        query = query.Where(exp);
                        //query = query.Where(filter);
                    }
                }
            }

            return query;
        }


        private static readonly MethodInfo SortByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);

        private static readonly MethodInfo SortByDescendingMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);

        private static readonly MethodInfo ThenByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);

        private static readonly MethodInfo ThenByDescendingMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);

        public static IQueryable<T> OrderDynamic<T>(this IQueryable<T> query, List<SortModel> order)
        {

            if ((order is null) || (order.Count == 0))
            {
                if (typeof(T).GetProperty("id") != null)
                {
                    ParameterExpression paramterExpression = Expression.Parameter(typeof(T));
                    Expression orderByProperty = Expression.Property(paramterExpression, "id");
                    LambdaExpression lambda = Expression.Lambda(orderByProperty, paramterExpression);
                    MethodInfo genericMethod;
                    genericMethod = OrderByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
                    object ret = genericMethod.Invoke(null, new object[] { query, lambda });
                    query = (IQueryable<T>)ret;
                }

                return query;
            }

            bool isFirstSort = true;

            foreach (var item in order)
            {
                if (!query.PropertyExists(item.Key))
                    continue;

                ParameterExpression paramterExpression = Expression.Parameter(typeof(T));
                Expression orderByProperty = Expression.Property(paramterExpression, item.Key);
                LambdaExpression lambda = Expression.Lambda(orderByProperty, paramterExpression);

                MethodInfo genericMethod;

                if (isFirstSort)
                {

                    genericMethod = item.Direction == "asc"
                        ? SortByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type)
                        : SortByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);

                    isFirstSort = false;
                }
                else
                {

                    genericMethod = item.Direction == "asc"
                        ? ThenByMethod.MakeGenericMethod(typeof(T), orderByProperty.Type)
                        : ThenByDescendingMethod.MakeGenericMethod(typeof(T), orderByProperty.Type);
                }

                object ret = genericMethod.Invoke(null, new object[] { query, lambda });
                query = (IQueryable<T>)ret;
            }

            return query;
        }

    }

}
