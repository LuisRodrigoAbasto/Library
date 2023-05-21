using System.Collections.Generic;
using System.Data;

namespace A.Data.SqlClientCore.Model
{
    public partial class ADbConfig
    {
        public ADbConfig() { }
        public string SqlQuery { get; set; }
        public int Timeout { get; set; } = 500;
        public CommandType CommandType { get; set; } = CommandType.Text;
    }
    public partial class ADbConfig<TParameter> : ADbConfig
    {
        public ADbConfig() { }
        public IEnumerable<TParameter> Parameters { get; set; }
    }
}
