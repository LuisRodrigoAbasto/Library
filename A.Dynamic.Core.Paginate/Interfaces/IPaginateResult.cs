﻿using A.Dynamic.Core.Paginate.Model;
using System.Collections.Generic;

namespace A.Dynamic.Core.Paginate.Interfaces
{
    public interface IPaginateResult
    {
        List<dynamic> Data { get; set; }
        int? TotalCount { get; set; }
        int? GroupCount { get; set; }
        List<SummaryItem> Summary { get; set; }
    }
    public interface IPaginateResult<T> : IPaginateResult
    {
        new List<T> Data { get; set; }
    }
}
