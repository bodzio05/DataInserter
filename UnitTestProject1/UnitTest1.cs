using System.Collections.Generic;
using DataInserter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string xmlFolder = "C:\\PDXML_Repo\\PDXML\\bin\\Debug\\XML\\SE";
            string excelFile = "C:\\Users\\cnsmacie\\Desktop\\Material_codes2.xlsx";
            string sqlFolder = "C:\\PDXML_Repo\\PDXML\\bin\\Debug\\XML\\SE\\SE_SQL";

            Program program = new Program(xmlFolder, excelFile, sqlFolder);
            program.RunProgram();
        }

        [TestMethod]
        public void TestMethod2()
        {
            string xmlFolder = "C:\\PDXML_Repo\\PDXML\\bin\\Debug\\XML\\SE";
            string sqlFolder = "C:\\PDXML_Repo\\PDXML\\bin\\Debug\\XML\\SE\\SE_SQL";

            List<Material> materials = new List<Material>();
            materials.Add(new Material("NKW815", "test-code"));
            //XMLManipulator.UpdateXML(materials, xmlFolder);
            SQLCreator.CreateSQLFile(materials, sqlFolder);
        }
    }
}
