using Abasto.Dynamic.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace Abasto.Dynamic.Model
{
    public class PaginateResult : IPaginateResult
    {
        public PaginateResult() { }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "data")]
        public List<dynamic> Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "totalCount")]
        public int? TotalCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "groupCount")]
        public int? GroupCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "summary")]
        public List<SummaryItem> Summary { get; set; }
    }

    public class PaginateResult<T> : PaginateResult, IPaginateResult<T>
    {
        public PaginateResult() { }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "data")]
        public new List<T> Data { get { return base.Data as List<T>; } set { base.Data = value.ToDynamicList(); } }
    }
    public class PaginateGroup
    {
        public object key { get; set; }
        public List<object> items { get; set; }
        public object count { get; set; }
        public List<object> summary { get; set; }
    }
}