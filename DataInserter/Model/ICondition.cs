using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public interface ICondition
    {
        string ExcelPropertyName { get; set; }
        NodeLevel NodeLevel { get; set; }
    }
}
