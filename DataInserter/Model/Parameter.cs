using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.Model
{
    public class Parameter
    {
        #region Properties
        public string Name { get; set; }
        public int Number { get; set; }
        public string Description { get; set; }
        public int Context { get; set; }
        public string Value { get; set; }
        public string ValueOptions { get; set; }
        public string ValueType { get; set; }
        public string Unit { get; set; }

        public string StandardName { get; set; }
        public string MaterialName { get; set; }
        #endregion

        #region Constructors
        public Parameter() { }

        public Parameter(string parameterName, string parameterDescription, int parameterContext, string parameterValueType)
        {
            this.Name = parameterName;
            this.Description = parameterDescription;
            this.Context = parameterContext;
            this.ValueType = parameterValueType;
        }
        #endregion
    }
}
