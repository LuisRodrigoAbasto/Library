/*
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cognos.Library.Extensions
{
        public static partial class QueryableExtensions
        {
            public static async Task<dynamic> PaginateAsyncEjemplo<DbQuery>(this IQueryable<DbQuery> source, string filtro) where DbQuery : class
            {
                int? totalCount = null;
                if (!string.IsNullOrEmpty(filtro))
                {
                    var paginate = Newtonsoft.Json.JsonConvert.DeserializeObject<FiltroPaginate>(filtro);
                    if (paginate.requireTotalCount == true)
                    {
                        totalCount = await source.CountAsync();
                    }
                    if (paginate.sort != null)
                    {
                        bool inicio = false;
                        foreach (var item in paginate.sort)
                        {
                            string query = string.Empty;
                            if (inicio) query = item.desc ? "ThenByDescending" : "ThenBy";
                            else query = item.desc ? "OrderByDescending" : "OrderBy";
                            source = source.OrderByMemberUsing(item.selector, query);
                            inicio = true;
                        }
                    }
                    else if (paginate.group != null)
                    {
                        foreach (var item in paginate.group)
                        {
                            source = source.SelectUsing(item.selector);
                        }
                    }
                    if (paginate.take != null && paginate.skip != null)
                    {
                        source = source.Skip(paginate.skip.Value).Take(paginate.take.Value);
                    }
                }
                var pru = source.Expression;

                var obj = new
                {
                    //data = await source.AsNoTracking().ToListAsync(),
                    data = await source.ToListAsync(),
                    totalCount,
                };
                return obj;
            }
            private static IQueryable<TEntity> SelectUsing<TEntity>(this IQueryable<TEntity> source, string select) where TEntity : class
            {
                //var methodCall = GenerateMethodCall<TEntity>(source, "Select", select);
                //return (IQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(methodCall);
                var parameter = Expression.Parameter(typeof(TEntity), "item");
                var member = select.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
                var keySelector = Expression.Lambda(member, parameter);
                var methodCall = Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new[] { parameter.Type, member.Type },
                    source.Expression,
                    Expression.Quote(keySelector));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(methodCall);
            }
            private static IOrderedQueryable<TEntity> OrderByMemberUsing<TEntity>(this IQueryable<TEntity> source, string select, string method) where TEntity : class
            {
                //var methodCall = GenerateMethodCall<TEntity>(source, method, select);
                //return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(methodCall);

                var parameter = Expression.Parameter(typeof(TEntity), "item");
                var member = select.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
                var keySelector = Expression.Lambda(member, parameter);
                var methodCall = Expression.Call(
                    typeof(Queryable),
                    method,
                    new[] { parameter.Type, member.Type },
                    source.Expression,
                    Expression.Quote(keySelector));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(methodCall);
            }
            private static object GetPropertyValue(object obj, string property)
            {
                System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
                return propertyInfo.GetValue(obj, null);
            }
            private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType) where TEntity : class
            {
                // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
                var parameter = Expression.Parameter(typeof(TEntity), "Entity");
                //  create the selector part, but support child properties
                PropertyInfo property;
                Expression propertyAccess;
                if (propertyName.Contains('.'))
                {
                    // support to be sorted on child fields.
                    String[] childProperties = propertyName.Split('.');
                    property = typeof(TEntity).GetProperty(childProperties[0]);
                    propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    for (int i = 1; i < childProperties.Length; i++)
                    {
                        property = typeof(TEntity).GetProperty(childProperties[i]);
                        propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    }
                }
                else
                {
                    property = typeof(TEntity).GetProperty(propertyName);
                    propertyAccess = Expression.MakeMemberAccess(parameter, property);
                }
                resultType = property.PropertyType;
                // Create the order by expression.
                return Expression.Lambda(propertyAccess, parameter);
            }
            private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName) where TEntity : class
            {
                Type type = typeof(TEntity);
                Type selectorResultType;
                LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
                MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName,
                                                new Type[] { type, selectorResultType },
                                                source.Expression, Expression.Quote(selector));
                return resultExp;
            }
            public static Expression<Func<TEntity, string>> GetColumnName<TEntity>(string property)
            {
                var menu = Expression.Parameter(typeof(TEntity), "groupCol");
                var menuProperty = Expression.PropertyOrField(menu, property);
                var lambda = Expression.Lambda<Func<TEntity, string>>(menuProperty, menu);
                return lambda;
            }
            static public IQueryable<IGrouping<TValue, TResult>> addGroupBy<TValue, TResult>(
                    this IQueryable<TResult> query, string columnName)
            {
                var providerType = query.Provider.GetType();

                // Find the specific type parameter (the T in IQueryable<T>)
                const object EmptyfilterCriteria = null;
                var iqueryableT = providerType
                    .FindInterfaces((ty, obj) => ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(IQueryable<>), EmptyfilterCriteria)
                    .FirstOrDefault();
                Type tableType = iqueryableT.GetGenericArguments()[0];
                string tableName = tableType.Name;

                ParameterExpression data = Expression.Parameter(iqueryableT, "query");
                ParameterExpression arg = Expression.Parameter(tableType, tableName);
                MemberExpression nameProperty = Expression.PropertyOrField(arg, columnName);
                Expression<Func<TResult, TValue>> lambda = Expression.Lambda<Func<TResult, TValue>>(nameProperty, arg);
                //here you already have delegate in the form of "TResult => TResult.columnName"
                //return query.GroupBy(lambda);

                //var expression = Expression.Call(typeof(Enumerable), 
                //                                "GroupBy", 
                //                                new Type[] { tableType, typeof(string) },
                //                                data, 
                //                                lambda);
                //var predicate = Expression.Lambda<Func<TResult, String>>(expression, arg); // this is the line that produces the error I describe below
                //var result = query.GroupBy(predicate).AsQueryable();
                //return result;

                MethodCallExpression expression = Expression.Call(typeof(Enumerable),
                                                         "GroupBy",
                                                         new[] { typeof(TResult), typeof(TValue) },
                                                         data,
                                                         lambda);

                var result = Expression.Lambda(expression, data).Compile().DynamicInvoke(query);
                return ((IEnumerable<IGrouping<TValue, TResult>>)result).AsQueryable();
            }
        }
        public class FiltroPaginate
        {
            public int? take { get; set; }
            public int? skip { get; set; }
            public bool? requireTotalCount { get; set; }
            public List<SortFiltro> sort { get; set; }
            public List<GroupFiltro> group { get; set; }

        }
        public class SortFiltro
        {
            public string selector { get; set; }
            public bool desc { get; set; }
        }
        public class GroupFiltro
        {
            public string selector { get; set; }
            public bool isExpanded { get; set; }
        }
        public class GroupResult
        {
            public dynamic key { get; set; }
            public int count { get; set; }
        }
    }
*/