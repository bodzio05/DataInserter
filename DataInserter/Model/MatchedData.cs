using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public class MatchedData
    {
        public string StandardName { get; set; }
        public string MaterialName { get; set; }
        public string PropertyValue { get; set; }
        public MatchingCondition RootCondition { get; set; }
        public string StdVersion { get; set; }
        public string MtrVersion { get; set; }
    }
}
