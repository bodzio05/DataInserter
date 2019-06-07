using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter
{
    public static class SQLCreator
    {
        public static void CreateSQLFile(List<Material> matchedaterials, string sqlFolderPath)
        {
            List<string> fileContent = new List<string>();
            List<string> rollbackFileContent = new List<string>();
            foreach (var material in matchedaterials)
            {
                fileContent.Add(CreateQuery(material));
                rollbackFileContent.Add(RollbackQuery(material));
            }

            WriteToFile(fileContent, sqlFolderPath, "MaterialCodeUpdate");
            WriteToFile(rollbackFileContent, sqlFolderPath, "MaterialCodeUpdateRollback");

        }

        private static string RollbackQuery(Material material)
        {
            if (Int32.TryParse(material.Version, out int result))
            {
                return "UPDATE TOJMGR.PD_MATERIALS SET MATERIALCODE = '' WHERE MTRNAME = '" + material.Name + "' AND VERSION = " + material.Version + ";";
            }
            else
            {
                return "UPDATE TOJMGR.PD_MATERIALS SET MATERIALCODE = '' WHERE MTRNAME = '" + material.Name + "';";
            }
        }

        private static string CreateQuery(Material material)
        {
            if (Int32.TryParse(material.Version, out int result))
            {
                return "UPDATE TOJMGR.PD_MATERIALS SET MATERIALCODE = '" + material.Code + "' WHERE MTRNAME = '" + material.Name + "' AND VERSION = " + material.Version + ";";
            }
            else
            {
                return "UPDATE TOJMGR.PD_MATERIALS SET MATERIALCODE = '" + material.Code + "' WHERE MTRNAME = '" + material.Name + "';";
            }
        }

        private static void WriteToFile(List<string> list, string path, string fileName)
        {
            using (StreamWriter sw = File.AppendText(path + "\\" + fileName + ".sql"))
            {
                foreach (string line in list)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }
}
