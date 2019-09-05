using Confidence.Platform.ServiceModel.Data;
using Confident_ConfidenceTools.ConfidenceWrappers.Interfaces.Materials;
using DataInserter.Model;
using DataInserter.ViewModel.Commands;
using DataInserter.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tools;

namespace DataInserter.ViewModel
{
    public class SQLCreatorViewModel : ViewModelBase, IViewModel
    {
        #region Properties
        private bool? _isRunnable;
        public bool? IsRunnable
        {
            get => _isRunnable;
            set
            {
                _isRunnable = value;
                NotifyPropertyChanged();
            }
        }

        private string _sqlFolderPath;
        public string SqlFolderPath
        {
            get => _sqlFolderPath;
            set
            {
                _sqlFolderPath = value;
                NotifyPropertyChanged();
            }
        }

        private string _sqlFileName;
        public string SqlFileName
        {
            get => _sqlFileName;
            set
            {
                _sqlFileName = value;
                NotifyPropertyChanged();
            }
        }

        private bool _createSqlFile;
        public bool CreateSqlFile
        {
            get => _createSqlFile;
            set
            {
                _createSqlFile = value;
                NotifyPropertyChanged();
            }
        }

        private bool _createRollbackSql;
        public bool CreateRollbackSql
        {
            get => _createRollbackSql;
            set
            {
                _createRollbackSql = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Fields
        private readonly IMainViewModel mainViewModel;
        private List<MatchedData> excelReaderResult = new List<MatchedData>();

        private List<MaterialStandard> allStandards = new List<MaterialStandard>();
        #endregion

        #region Commands
        public ICommand SearchForSqlFolderCommand { get { return new RelayCommand(SearchForSqlFolder, AlwaysTrue); } }

        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public SQLCreatorViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }
        #endregion

        #region UI Methods
        public void RunSqlCreator(List<MatchedData> data)
        {
            this.excelReaderResult = data;
            bool standardListIsNotEmpty = false;

            if (CanBeExecuted())
            {
                if (data.Any(d=>d.RootCondition.NodeLevel == NodeLevel.Standard))
                {
                    standardListIsNotEmpty = GetStandardsFromDb();
                }     
            }
            else
            {
                MessageBox.Show("Can't run program. Check the settings and try again.");
                return;
            }

            CreateSQLFile();
        }

        private void SearchForSqlFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                this.SqlFolderPath = dialog.SelectedPath;
            }
        }

        private bool CanBeExecuted()
        {
            if (!ConfidenceTools.CheckConnection())
            {
                MessageBox.Show("No Cofidence connection.");
                return false;
            }

            if (Directory.Exists(SqlFolderPath) && (SqlFileName != "" || SqlFileName.Length > 1 ))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Confidence methods
        private bool GetStandardsFromDb()
        {
            allStandards = ConfidenceTools.SelectAllStandards().ToList();

            if (allStandards.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Creator methods
        private void CreateSQLFile()
        {
            List<string> fileContent = new List<string>();
            List<string> rollbackFileContent = new List<string>();
            foreach (var data in this.excelReaderResult)
            {
                fileContent.Add(CreateQuery(data));
                rollbackFileContent.Add(RollbackQuery(data));
            }

            WriteToFile(fileContent, this.SqlFolderPath, this.SqlFileName);
            WriteToFile(rollbackFileContent, this.SqlFolderPath, this.SqlFileName + "Rollback");
        }

        private string RollbackQuery(MatchedData data)
        {
            switch (data.RootCondition.NodeLevel)
            {
                case NodeLevel.Standard:
                    return String.Format("UPDATE TOJMGR.{0} SET {1} = '{2}' WHERE STANDARDNAME='{3}';",
                        data.RootCondition.DatabaseTableName,
                        data.RootCondition.DatabaseFieldName,
                        data.PropertyValue,
                        data.MaterialName);
                case NodeLevel.Material:
                    return String.Format("UPDATE TOJMGR.{0} SET {1} = '{2}' WHERE MTRNAME='{3}';", 
                        data.RootCondition.DatabaseTableName, 
                        data.RootCondition.DatabaseFieldName, 
                        data.PropertyValue, 
                        data.MaterialName);
                case NodeLevel.Parameter:
                    break;
                default:
                    break;
            }

            return "";
        }

        private string CreateQuery(MatchedData data)
        {
            switch (data.RootCondition.NodeLevel)
            {
                case NodeLevel.Standard:
                    var standard = allStandards.SingleOrDefault(s => s.Name == data.StandardName);
                    return String.Format("UPDATE TOJMGR.{0} SET {1} = '{2}' WHERE STANDARDNAME='{3}' AND VERSION='{4}';",
                        data.RootCondition.DatabaseTableName,
                        data.RootCondition.DatabaseFieldName,
                        data.PropertyValue,
                        data.MaterialName,
                        standard.Version);
                case NodeLevel.Material:
                    var material = ConfidenceTools.SelectSingleMaterial(data.MaterialName).FirstOrDefault();
                    return String.Format("UPDATE TOJMGR.{0} SET {1} = '{2}' WHERE MTRNAME='{3}' AND VERSION='{4}';",
                        data.RootCondition.DatabaseTableName,
                        data.RootCondition.DatabaseFieldName,
                        data.PropertyValue,
                        data.MaterialName,
                        material.Version);
                case NodeLevel.Parameter:
                    break;
                default:
                    break;
            }
            return "";
        }

        private void WriteToFile(List<string> list, string path, string fileName)
        {
            using (StreamWriter sw = File.AppendText(path + "\\" + fileName + ".sql"))
            {
                foreach (string line in list)
                {
                    sw.WriteLine(line);
                }
            }
        }
        #endregion
    }
}
