using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public class MatchingCondition
    {
        public string ExcelPropertyName { get; set; }
        public NodeLevel NodeLevel { get; set; }
        public string XmlPropertyName { get; set; }

        public MatchingCondition(string excelPropertyName, NodeLevel level, string xmlPropertyName)
        {
            this.NodeLevel = level;
            this.ExcelPropertyName = excelPropertyName;
            this.XmlPropertyName = xmlPropertyName;
        }
    }
}
