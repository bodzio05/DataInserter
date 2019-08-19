using DataInserter.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataInserter.ViewModel
{
    public class Program
    {
        //public ICommand RunProgramCommand { get { return new RelayCommand(action => this.RunProgram()); } }
        //public ICommand BrowseFilesCommand { get { return new RelayCommand(action => this.BrowseExcelFiles()); } }
        //public ICommand BrowseFolderCommand { get { return new RelayCommand(action => this.BrowseFolder()); } }


        public string XmlFolderPath { get; set; }
        public string SqlFolderPath { get; set; }
        public string MaterialCodePath { get; set; }
        public string GroupReferencePath { get; set; }

        public bool PerformMaterialCodes { get; set; }
        public bool PerformAutomaticGroupReferences { get; set; }
        public bool PerformManualGroupReferences { get; set; }

        public Program()
        {

        }

        public void RunComponents(string xmlFolderPath, string excelFilePath, string sqlFolderPath)
        {

            List<Material> materials = ExcelReader.ReadExcel(excelFilePath);
            XMLManipulator.UpdateXML(materials, xmlFolderPath);
            SQLCreator.CreateSQLFile(materials, sqlFolderPath);
        }

        private void RunProgram()
        {
            if(!CanBeExecuted())
            {
                System.Windows.MessageBox.Show("Can't execute. Try again with different settings or files.");
                return;
            }

            XmlFolderPath = BrowseFolder();
            SqlFolderPath = BrowseFolder();

            if (PerformMaterialCodes)
            {
                MaterialCodePath = BrowseExcelFiles();
            }

            if (PerformAutomaticGroupReferences)
            {
                GroupReferencePath = BrowseExcelFiles();
            }
            
            RunComponents(XmlFolderPath, MaterialCodePath, SqlFolderPath);

            System.Windows.MessageBox.Show("Done! You can exit.");
        }

        private string BrowseExcelFiles()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "EXCEL Files (*.xlsx)|*.xlsx" };
            var found = ofd.ShowDialog();
            if (found == false) return "";
            return ofd.FileName;
        }

        private string BrowseFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                return dialog.SelectedPath;
            }
        }

        private bool CanBeExecuted()
        {
            if (XmlFolderPath == "" || SqlFolderPath == "")
            {
                System.Windows.MessageBox.Show("Can't run program.");
                return false;
            }
            else if (PerformMaterialCodes && MaterialCodePath == "")
            {
                System.Windows.MessageBox.Show("Can't add MaterialCodes. Select the path to the .xls file");
                return false;
            }
            else if (PerformAutomaticGroupReferences && GroupReferencePath == "")
            {
                System.Windows.MessageBox.Show("Can't add GroupReference. Select the path to the .xls file");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
