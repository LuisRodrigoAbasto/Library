using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Abasto.Dynamic.Model
{
    public class FilterDevExtreme : SearchFilter
    {
        //public object[] key { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "dataField")]
        public string DataField { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "requireGroupCount")]
        public bool? RequireGroupCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "sort")]
        public new List<SortFilter> Sort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "group")]
        public List<GroupFilter> Group { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "filter")]
        public JArray Filter { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "totalSummary")]
        public new List<Summary> TotalSummary { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "groupSummary")]
        public List<Summary> GroupSummary { get; set; }

        //esto es para SelectBox,Autocomplete,DrowBox
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "searchExpr")]
        public object SearchExpr { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "searchOperation")]
        public object SearchOperation { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "searchValue")]
        public object SearchValue { get; set; }

    }
    public class SortFilter
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "selector")]
        public string Selector { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "desc")]
        public bool Desc { get; set; }
    }
    public class GroupFilter
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "selector")]
        public string Selector { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "groupInterval")]
        public object GroupInterval { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isExpanded")]
        public bool IsExpanded { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "desc")]
        public bool? Desc { get; set; }
    }
}
