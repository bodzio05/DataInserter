using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public class DeletingCondition :ICondition 
    {
        public string ExcelPropertyName { get; set; }
        public NodeLevel NodeLevel { get; set; }

        public DeletingCondition(string excelPropertyName, NodeLevel level)
        {
            this.ExcelPropertyName = excelPropertyName;
            this.NodeLevel = level;
        }
    }
}
