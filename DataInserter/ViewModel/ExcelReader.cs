using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace DataInserter
{
    public static class ExcelReader
    {
        public static List<Material> ReadExcel(string filePath)
        {
            #region OpenExcelFile

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            #endregion

            List<Material> materials = CreateList(xlRange);

            #region CleanUp
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
            #endregion

            return materials;
        }

        private static List<Material> CreateList(Excel.Range xlRange)
        {
            List<Material> materials = new List<Material>();

            int rowCounter = 1;
            bool writeMaterialToList = false;

            int emptyRowCounter = 0;
            bool previousWasEmpty = false;
            bool endReading = false;

            while (!endReading)
            {
                string[] row = new string[2];

                for (int i = 0; i < 2; i++)
                {
                    string value;
                    try
                    {
                        value = xlRange.Cells[i + 1][rowCounter].Value2.ToString();
                    }
                    catch (Exception)
                    {
                        value = string.Empty;
                    }

                    row[i] = value;
                }

                if (row[0] == "" && row[1] == "")
                {
                    emptyRowCounter++;
                    previousWasEmpty = true;
                }

                if (row[0] != "" || row[1] != "")
                {
                    emptyRowCounter = 0;
                    previousWasEmpty = false;
                }

                if (previousWasEmpty && emptyRowCounter>10)
                {
                    endReading = true;
                }

                if (rowCounter == 1 || row[0] == "" || row[1] == "")                                                   
                {
                    rowCounter++;
                    continue;
                }

                rowCounter++;

                Material material = new Material(row[1],row[0]);
                writeMaterialToList = IsNotEmpty(material);

                if (writeMaterialToList)
                {
                    materials.Add(material);
                }
            }

            return materials;
        }

        private static bool IsNotEmpty(Material material)
        {
            if (material.Code == "" || material.Name == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
