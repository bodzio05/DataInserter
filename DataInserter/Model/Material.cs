using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter
{
    public class Material
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Version { get; set; }
        public string Standard { get; set; }

        public Material()
        {

        }

        public Material(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public Material(string standard, string name, string code)
        {
            Standard = standard;
            Name = name;
            Code = code;
        }
    }
}
