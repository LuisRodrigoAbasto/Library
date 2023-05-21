using Abasto.Dynamic.Interfaces;
using Abasto.Dynamic.Model;
using Abasto.Dynamic.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Abasto.Dynamic.DevExtreme
{
    public class PaginateExecute : IPaginateExecute
    {
        private readonly IQueryable _source;
        private readonly QueryDevExtreme _queryFilter = new QueryDevExtreme();
        private FilterDevExtreme _filterClient = new FilterDevExtreme();
        public bool isGroup = false;
        public PaginateExecute(IQueryable source, FilterDevExtreme filter, Action<QueryDevExtreme> options)
        {
            this._source = source;
            this._filterClient = filter;
            options?.Invoke(_queryFilter);
            //this._options = options;
        }
        public async Task<IPaginateResult<T>> PaginateExecuteResultAsync<T>(CancellationToken cancellationToken = default)
        {
            return await PaginateQueryResult<T>(true, cancellationToken);
        }
        public IPaginateResult<T> PaginateExecuteResult<T>()
        {
            return PaginateQueryResult<T>(false).GetAwaiter().GetResult();
        }
        private async Task<IPaginateResult<T>> PaginateQueryResult<T>(bool async, CancellationToken cancellationToken = default)
        {
            IQueryable query = _source.AsQueryable();

            Type type = _source.ElementType;
            PropertyDescriptorCollection property = TypeDescriptor.GetProperties(type);

            if (_filterClient != null)
            {
                query = QueryWhere(query, _filterClient, property);
                if (_filterClient.IsLoadingAll == true)
                {
                    query = (async ? await query.ToDynamicListAsync<object>(cancellationToken)
                        : ((IQueryable<object>)query).ToList()).AsQueryable();
                    async = false; // desactivar async para que busque los datos estatico
                }
            }
            else _filterClient = new FilterDevExtreme();
            var queryReader = PaginateQuery<T>(query, _filterClient, _queryFilter, property, async);
            var paginateQuery = async ? await queryReader : queryReader.GetAwaiter().GetResult();
            return paginateQuery;
        }
        private async Task<IPaginateResult<T>> PaginateQuery<T>(IQueryable source, FilterDevExtreme filterClient, QueryDevExtreme queryFilter, PropertyDescriptorCollection property, bool async, CancellationToken cancellationToken = default)
        {
            PaginateResult<T> paginateQuery = new PaginateResult<T>();
            var QueryCount = QueryCountAsync(source, filterClient, async, cancellationToken);
            paginateQuery.TotalCount = async ? await QueryCount : QueryCount.GetAwaiter().GetResult();
            var QuerySummary = QuerySummaryAsync(source, filterClient, paginateQuery.TotalCount, async);
            paginateQuery.Summary = async ? await QuerySummary : QuerySummary.GetAwaiter().GetResult();
            source = QueryOrderBy(source, filterClient, queryFilter, property);
            IQueryable query = QueryGroupBy(source, filterClient, queryFilter, property);
            var QueryGroupCount = QueryGroupCountAsync(source, filterClient, async, cancellationToken);
            paginateQuery.GroupCount = async ? await QueryGroupCount : QueryGroupCount.GetAwaiter().GetResult();
            query = QuerySkipTake(query, filterClient);
            var data = async ? await query.ToDynamicListAsync<T>() : query.ToDynamicList<T>();
            if (filterClient.Group != null && string.IsNullOrEmpty(filterClient.DataField))
            {
                List<object> list = ToListGroupBy(data.ToDynamicList<object>(), filterClient, queryFilter);
                paginateQuery.Data = list.ToDynamicList<T>();
            }
            else paginateQuery.Data = data;

            isGroup = filterClient.Group != null;
            return paginateQuery;
        }
        private IQueryable QueryWhere(IQueryable query, FilterDevExtreme filterClient, PropertyDescriptorCollection property)
        {
            if (filterClient.SearchExpr != null
                && filterClient.SearchOperation != null
                && filterClient.SearchValue != null)
            {
                //FilterSearch(filterClient);
                query = query.Where($"{filterClient.SearchExpr}.Contains(@0)", filterClient.SearchValue);
            }
            if (filterClient.Filter == null) return query;
            List<object> param = new List<object>();
            string consulta = ConsultaWhere(filterClient.Filter, property, ref param);
            if (!string.IsNullOrEmpty(consulta)) query = query.Where(consulta, param.ToArray());
            return query;
        }
        private static void FilterSearch(FilterDevExtreme filterClient)
        {
            if (typeof(string) == filterClient.SearchExpr.GetType())
            {
                if (filterClient.SearchExpr.ToString().Contains(","))
                {
                    filterClient.SearchExpr = filterClient.SearchExpr.ToString().Split(',');
                    FilterSearch(filterClient);
                    return;
                }

                var filter = new List<object>();
                filter.Add(filterClient.SearchExpr);
                filter.Add(filterClient.SearchOperation);
                filter.Add(filterClient.SearchValue);

                //if (filterClient.filter != null)
                //{
                //    filters = filterClient.filter.ToList();
                //    filters.Add(filter.ToArray());

                //    var filtros = filterClient.filter.ToList();
                //    filtros.Add("and");
                //    filtros.Add(filters.ToArray());
                //    filterClient.filter = filtros.ToArray();
                //}
                //else
                //{
                //    filters.AddRange(filter.ToArray());
                //    filterClient.filter = filters.ToArray();
                //}
            }
            else
            {
                var searchExpr = new List<object>((IEnumerable<object>)filterClient.SearchExpr);
                var filters = new List<object>();
                int i = 0;
                foreach (var item in searchExpr)
                {
                    if (i > 0)
                    {
                        filters.Add("or");
                    }
                    var filter = new List<object>();
                    filter.Add(item);
                    filter.Add(filterClient.SearchOperation);
                    filter.Add(filterClient.SearchValue);
                    filters.Add(filter.ToArray());
                    i++;
                }
                if (filterClient.Filter != null)
                {
                    var filter = filterClient.Filter.ToList();
                    filter.Add("and");
                    //filter.Add(filters);
                    //filterClient.filter = filter.ToArray();
                }
                else
                {
                    //filterClient.filter = filters.ToArray();
                }
            }
        }
        private async Task<List<SummaryItem>> QuerySummaryAsync(IQueryable query, FilterDevExtreme filterClient, int? totalCount, bool async, CancellationToken cancellationToken = default)
        {
            if (filterClient.TotalSummary == null) return null;

            List<SummaryItem> summaryItem = filterClient.TotalSummary.Select((x, index) => new SummaryItem
            {
                Index = index,
                Selector = x.Selector,
                SummaryType = x.SummaryType,
                Value = (object)null
            }).ToList();
            foreach (var item in summaryItem)
            {
                object total = null;
                if (totalCount != 0)
                {
                    try
                    {
                        if (item.SummaryType == "count")
                        {
                            if (totalCount != null) total = totalCount;
                            else
                            {
                                var queryCount = QueryCount(query, async, cancellationToken);
                                totalCount = async ? await queryCount : queryCount.GetAwaiter().GetResult();
                                total = totalCount;
                            }
                        }
                        else if (!string.IsNullOrEmpty(item.Selector) && !string.IsNullOrEmpty(item.SummaryType))
                        {
                            if (item.SummaryType == "sum") total = query.Aggregate("Sum", item.Selector);
                            else if (item.SummaryType == "avg") total = query.Aggregate("Average", item.Selector);
                            else if (item.SummaryType == "min") total = query.Aggregate("Min", item.Selector);
                            else if (item.SummaryType == "max") total = query.Aggregate("Max", item.Selector);
                        }
                        else continue;
                    }
                    catch
                    {
                        total = 0;
                    }
                }

                item.Value = total;
            }
            if (summaryItem.Count() == 0) return null;
            return summaryItem.OrderBy(x => x.Index).ToList();
        }
        private IQueryable QueryOrderBy(IQueryable source, FilterDevExtreme filterClient, QueryDevExtreme queryFilter, PropertyDescriptorCollection property)
        {
            if (filterClient.Group != null) return source;
            else if (filterClient.Sort == null && string.IsNullOrEmpty(queryFilter.orderBy)) return source;
            //else if (filterClient.sort == null && filterClient.key == null && string.IsNullOrEmpty(queryFilter.orderBy)) return source;
            //else if (filterClient.sort == null && filterClient.key != null && string.IsNullOrEmpty(queryFilter.orderBy))
            //{
            //    filterClient.sort = new List<SortFilter>();
            //    foreach (string item in filterClient.key)
            //    {
            //        var sort = new SortFilter();
            //        sort.Selector = item;
            //        var propertyType = property.GetProperty(item);
            //        if (typeof(string) == propertyType) sort.Desc = false;
            //        else sort.Desc = true;
            //        filterClient.sort.Add(sort);
            //    }
            //}
            IOrderedQueryable query = (IOrderedQueryable)source.AsQueryable();
            bool orderBy = true;
            if (filterClient.Sort != null)
            {
                foreach (var item in filterClient.Sort)
                {
                    string order = item.Selector;
                    if (item.Selector.Contains(",")) order = order.Replace(",", $"{(item.Desc ? " desc @" : " asc @")}").Replace("@", ",");
                    else order += item.Desc ? " desc" : " asc";
                    if (orderBy) query = query.OrderBy(order);
                    else query = query.ThenBy(order);
                    orderBy = false;
                }
            }
            if (!string.IsNullOrEmpty(queryFilter.orderBy))
            {
                if (orderBy) query = query.OrderBy(queryFilter.orderBy);
                else query = query.ThenBy(queryFilter.orderBy);
            }
            return query.AsQueryable();
        }
        private IQueryable QueryGroupBy(IQueryable source, FilterDevExtreme filterClient, QueryDevExtreme queryFilter, PropertyDescriptorCollection property)
        {
            if (filterClient.Group == null) return source;
            string consulta = string.Empty, key = string.Empty, order = string.Empty;
            List<object> groupObject = new List<object>();
            var listaGroup = new List<string>();
            int count = filterClient.GroupSummary != null ? 0 : filterClient.Group.Count() - 1,
                ini = 1;
            foreach (var item in filterClient.Group)
            {
                bool keyMultiple = item.Selector.Contains(",");
                string groupBy = item.Selector;
                if (!keyMultiple)
                {
                    if (item.GroupInterval != null)
                    {
                        Type groupIntervalType = item.GroupInterval.GetType();
                        if (groupIntervalType == typeof(string))
                        {
                            if ("year,month,day".Contains(item.GroupInterval.ToString()))
                            {
                                groupBy = $"SqlFunctions.DatePart(\"{item.GroupInterval}\",{item.Selector})";
                            }
                        }
                        else if (groupIntervalType.GetTypeIsNumeric())
                        {
                            int c = groupObject.Count();
                            object value = property.GetConvertValueTruncate(item.Selector, item.GroupInterval);
                            groupObject.Add(value);
                            groupBy = $"DbFunctions.Truncate((({item.Selector})/@{c}),0)*@{c}";
                        }
                    }
                }
                if (string.IsNullOrEmpty(consulta)) consulta = "it.Key as key";
                else if (!keyMultiple) consulta += $", it.GroupBy({groupBy}).Select(new (it.Key as key";
                else consulta += $", it.GroupBy(new ({item.Selector})).Select(new (it.Key as key";

                if (string.IsNullOrEmpty(filterClient.DataField) && count < ini) consulta += $",it.Count() as count";
                if (filterClient.GroupSummary != null)
                {
                    var summary = new List<string>();
                    foreach (var grup in filterClient.GroupSummary)
                    {
                        if (grup.SummaryType == "count") continue;
                        else if (string.IsNullOrEmpty(grup.Selector)) continue;
                        else if (summary.Where(x => x == (grup.Selector + grup.SummaryType)).Any()) continue;
                        else if (grup.SummaryType == "sum") consulta += $",it.Sum({grup.Selector})";
                        else if (grup.SummaryType == "avg") consulta += $",it.Average({grup.Selector})";
                        else if (grup.SummaryType == "max") consulta += $",it.Min({grup.Selector})";
                        else if (grup.SummaryType == "min") consulta += $",it.Max({grup.Selector})";
                        else continue;
                        summary.Add($"{grup.Selector}{grup.SummaryType}");
                        consulta += $" as {grup.Selector}{grup.SummaryType}";
                    }
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = groupBy;
                    order = item.Desc != true ? "asc" : "desc";
                }
                ini++;
            }
            if (filterClient.IsLoadingAll == true) consulta += ",it.ToList() as data";
            int contar = filterClient.Group.Count();
            for (int i = 1; i < contar; i++) consulta += ")).OrderBy(key).ToList() as items";

            if (!key.Contains(")") && key.Contains(","))
            {
                var keySplit = key.Split(',');
                key = "new (" + key + ")";
                var orderBy = order;
                order = string.Empty;
                foreach (var k in keySplit) order += "key." + k + " " + orderBy + ",";
                order = order.Trim(',');
            }
            else order = "key " + order;
            IQueryable query = source.GroupBy(key, groupObject.ToArray()).Select($"new({consulta})").OrderBy(order);
            return query;
        }
        private List<object> ToListGroupBy(List<object> data, FilterDevExtreme filterClient, QueryDevExtreme queryFilter)
        {
            if (data == null) return null;
            var group = new List<object>();
            List<SummaryItem> summaryDefault = new List<SummaryItem>();
            if (filterClient.GroupSummary != null)
            {
                summaryDefault = filterClient.GroupSummary.Select((x, i) => new SummaryItem
                {
                    Index = i,
                    Selector = x.Selector,
                    SummaryType = x.SummaryType,
                    Value = (object)null,
                }).ToList();
            }

            foreach (var item in data)
            {
                var row = new PaginateGroup();
                Type type = item.GetType();
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
                List<SummaryItem> summaryItem = summaryDefault;
                foreach (PropertyDescriptor prop in properties)
                {
                    var valor = properties[prop.Name].GetValue(item);
                    if (prop.Name == "key") row.key = valor;
                    else if (prop.Name == "items")
                    {
                        var items = new List<object>((IEnumerable<object>)valor);
                        row.items = ToListGroupBy(items, filterClient, queryFilter);
                    }
                    else if (prop.Name == "data")
                    {
                        if (!string.IsNullOrEmpty(queryFilter.orderBy))
                        {
                            row.items = ((IQueryable<object>)new List<object>((IEnumerable<object>)valor)
                                .AsQueryable().OrderBy(queryFilter.orderBy)).ToList();
                        }
                        else row.items = new List<object>((IEnumerable<object>)valor);
                    }
                    else if (filterClient.GroupSummary != null)
                    {
                        if (prop.Name == "count") row.count = valor;
                        var summarry = summaryItem.Where(x => (x.Selector + x.SummaryType) == prop.Name || (x.SummaryType == "count" && prop.Name == "count")).Select(x => new { x.Index }).ToList();
                        foreach (var sum in summarry)
                        {
                            summaryItem.Where(x => x.Index == sum.Index).FirstOrDefault().Value = valor;
                        }
                    }
                    else if (prop.Name == "count") row.count = valor;
                }
                if (filterClient.GroupSummary != null)
                {
                    row.summary = summaryItem.OrderBy(x => x.Index).Select(x => x.Value).ToList();
                }
                if (row.items != null) row.count = row.items.Count();
                group.Add(row);
            }
            return group;
        }
        private IQueryable QuerySkipTake(IQueryable query, FilterDevExtreme filterClient)
        {
            if (filterClient.Skip != null) query = query.Skip(filterClient.Skip.Value);
            if (filterClient.Take != null) query = query.Take(filterClient.Take.Value);
            return query;
        }
        private async Task<int?> QueryCountAsync(IQueryable query, FilterDevExtreme filterClient, bool async, CancellationToken cancellationToken = default)
        {
            if (filterClient.RequireTotalCount != true) return null;
            var totalCount = async ? await QueryCount(query, async, cancellationToken) : QueryCount(query, async).GetAwaiter().GetResult();
            return totalCount;
        }
        private async Task<int?> QueryGroupCountAsync(IQueryable query, FilterDevExtreme filterClient, bool async, CancellationToken cancellationToken = default)
        {
            if (filterClient.RequireGroupCount != true) return null;
            return async ? await QueryCount(query, async, cancellationToken) : QueryCount(query, async).GetAwaiter().GetResult();
        }
        private async Task<int> QueryCount(IQueryable query, bool async, CancellationToken cancellationToken = default)
        {
            //return async ? await ((IQueryable<object>)query).CountAsync(cancellationToken) : ((IQueryable<object>)query).Count();
            var total = ((IQueryable<object>)query).Count();
            return async ? await Task.FromResult(total) : total;
        }
        private string ConsultaWhere(JArray data, PropertyDescriptorCollection property, ref List<object> param)
        {
            string consulta = string.Empty;
            if (data == null) return consulta;
            string columna = string.Empty, operador = string.Empty, union = string.Empty, recursiva = string.Empty;
            object valor = null;
            int c = 0;
            foreach (object item in data)
            {
                Type type = item != null ? item.GetType() : null;
                if (type == typeof(JArray) || type == typeof(Array) || type == typeof(object[]))
                {
                    if (string.IsNullOrEmpty(union) && !string.IsNullOrEmpty(recursiva)) continue;
                    var recurso = ConsultaWhere((JArray)item, property, ref param);
                    recursiva += (string.IsNullOrEmpty(recurso)) ? string.Empty : $" {union} ({recurso})";
                    c = 3;
                    union = string.Empty; columna = string.Empty; operador = string.Empty; valor = null;
                }
                else
                {
                    if (c == 0) columna = item.ToString();
                    else if (c == 1) operador = item.ToString();
                    else if (c == 2) valor = item != null ? item : null;
                    else if (c == 3)
                    {
                        union = string.IsNullOrEmpty(recursiva) ? string.Empty : item.ToString();
                        c = -1;
                    }
                    c++;
                }

                if (c == 3)
                {
                    if (!string.IsNullOrEmpty(columna) && !string.IsNullOrEmpty(operador))
                    {
                        if (!columna.Contains(","))
                        {
                            TypeProperty typeProperty = new TypeProperty();
                            if (columna.Contains("."))
                            {
                                var col = columna.Split('.');
                                columna = col[0];
                                typeProperty = property.GetTypeProperty(columna);
                                if (typeProperty.isNullable) columna += ".Value";
                                if (typeProperty.type == typeof(DateTime)) valor = typeof(int).GetConvertValue(valor);
                                else valor = typeProperty.type.GetConvertValue(valor);
                                columna += "." + col[1];
                            }
                            else
                            {
                                typeProperty = property.GetTypeProperty(columna);
                                if (operador == "contains")
                                {
                                    valor = typeof(string).GetConvertValue(valor);
                                    if (typeProperty.isNullable) columna += ".Value";
                                }
                                else
                                {
                                    valor = typeProperty.type.GetConvertValue(valor);
                                }
                            }
                            consulta += QueryString(columna, operador, valor, typeProperty, ref param);
                        }
                        else
                        {
                            var listColumna = columna.Split(',');
                            JObject key = (JObject)valor;
                            int ini = 0;
                            foreach (var col in listColumna)
                            {
                                columna = col;
                                var typeProperty = property.GetTypeProperty(columna);
                                object value = key[col];
                                value = typeProperty.type.GetConvertValue(value);
                                if (operador == "contains" && typeProperty.isNumeric)
                                {
                                    if (typeProperty.isNullable) columna += ".Value";
                                }
                                if (ini > 0) consulta += " and ";
                                consulta += QueryString(columna, operador, value, typeProperty, ref param);
                                columna = string.Empty;
                                ini++;
                            }
                        }
                    }
                }
            }
            consulta = $"{recursiva} {consulta}";
            return consulta.Trim();
        }
        private string QueryString(string columna, string operador, object valor, TypeProperty type, ref List<object> param)
        {
            //if ((typeof(string) == type.type?valor == null?true:string.IsNullOrEmpty(valor.ToString()):false)) return $"string.IsNullOrEmpty({columna}) ";
            if (valor == null) return $"{columna} == null ";
            int pos = param.Count();
            if (operador == "=") operador = $" == @{pos}";
            else if (operador == ">" || operador == ">=" || operador == "<" || operador == "<=") operador = $" {operador} @{pos}";
            else if (operador == "contains")
            {
                if (type.type == typeof(string)) operador = $".Contains(@{pos})";
                else operador = $".ToString().Contains(@{pos})";
            }
            param.Add(valor);
            return $"{columna}{operador} ";
        }

    }
}
