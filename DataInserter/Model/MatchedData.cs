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
        public int StdVersion { get; set; }
        public int MtrVersion { get; set; }
    }
}
