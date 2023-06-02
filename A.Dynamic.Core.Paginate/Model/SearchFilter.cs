using Newtonsoft.Json;
using System.Collections.Generic;

namespace A.Dynamic.Core.Paginate.Model
{
    public class Filters
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "fields")]
        public string Fields { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "where")]
        //public string Where { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "sort")]
        public string Sort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "skip")]
        public int? Skip { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "take")]
        public int? Take { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "requiretotalcount")]
        public bool? RequireTotalCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isloadingall")]
        public bool? IsLoadingAll { get; set; }
    }
    public class SearchFilter : Filters
    {
        public Dictionary<string, string> Filters { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "totalsummary")]
        public IEnumerable<Summary> TotalSummary { get; set; }
    }

    public class Summary
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "orden")]
        public int Index { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "selector")]
        public string Selector { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "summaryType")]
        public string SummaryType { get; set; }
    }
    public class SummaryItem : Summary
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "value")]
        public dynamic Value { get; set; }
    }
}
