using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataInserter
{
    public static class XMLManipulator
    {
        private static List<string> XMLFiles { get; set; }

        public static void UpdateXML(List<Material> materials, string xMLFolderPath)
        {
            GetFiles(xMLFolderPath);
            EditFiles(materials);
        }

        private static void EditFiles(List<Material> importedMaterials)
        {
            foreach (var filePath in XMLFiles)
            {
                XDocument document = XDocument.Load(filePath);
                var materials = document.Root.Elements().Where(e => e.Name == "Material").ToList();

                if (materials == null)
                {
                    continue;
                }

                foreach (var material in materials)
                {
                    string version = SetVersion(material);
                    string elementName = material.Element("Name").Value;
                    var matchedMaterial = importedMaterials.FirstOrDefault(m => m.Name == elementName);

                    if (matchedMaterial == null)
                    {
                        continue;
                    }
                    else
                    {
                        matchedMaterial.Version = version;

                        if (material.Descendants("MaterialCode").ToList().Count > 0)
                        {
                            material.Element("MaterialCode").Value = matchedMaterial.Code;
                        }
                        else
                        { 
                            XElement materialCode = new XElement("MaterialCode", matchedMaterial.Code);
                            material.FirstNode.AddAfterSelf(materialCode);
                        }
                    }
                }
                document.Save(filePath);
            }
        }

        private static string SetVersion(XElement material)
        {
            return material.Descendants("Version").FirstOrDefault().Value;
        }

        private static void GetFiles(string path)
        {
            XMLFiles = Directory.GetFiles(path, "*.xml").ToList();
        }
    }
}
