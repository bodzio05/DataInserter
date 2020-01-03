using DataInserter.Model;
using DataInserter.ViewModel.Commands;
using DataInserter.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DataInserter.ViewModel
{
    public class DeletingSQLGeneratorViewModel : ViewModelBase, IViewModel
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

        private Status _status;
        public Status Status
        {
            get => _status;
            set
            {
                _status = value;
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

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool _allowDeleting;
        public bool AllowDeleting
        {
            get => _allowDeleting;
            set
            {
                _allowDeleting = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Fields
        private readonly IMainViewModel mainViewModel;
        private List<MatchedData> _deleteList = new List<MatchedData>();
        private bool _versionSpecified;
        #endregion

        #region Commands
        public ICommand SearchForSqlFolderCommand { get { return new RelayCommand(SearchForSqlFolder, AlwaysTrue); } }

        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public DeletingSQLGeneratorViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.Status = new Status(StatusEnum.Waiting);
        }
        #endregion

        #region UI Methods
        public void RunSqlGenerator(List<MatchedData> data, bool versionSpecified)
        {
            this._deleteList = data;
            this._versionSpecified = versionSpecified;

            if (CanBeExecuted())
            {
                GenerateDeletingSQL();
            }
            else
            {
                MessageBox.Show("Can't run program. Check the settings and try again.");
                return;
            }
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
            if (Directory.Exists(SqlFolderPath) && (SqlFileName != "" || SqlFileName.Length > 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Generator methods
        public void GenerateDeletingSQL()
        {
            List<string> fileContent = new List<string>();
            foreach (var data in _deleteList)
            {
                fileContent.Add(CreateQuery(data));
            }

            WriteToFile(fileContent, this.SqlFolderPath, this.SqlFileName);
        }

        private string CreateQuery(MatchedData data)
        {
            switch (data.DeletingCondition.NodeLevel)
            {
                case NodeLevel.Standard:
                    if (_versionSpecified)
                    {
                        return String.Format(
                            "DELETE FROM TOJMGR.PD_MTRSTANDARDS WHERE STANDARDNAME = '{0}' AND VERSION = '{1}';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_MATERIALS WHERE MTRSTDNAME = '{0}' AND STDVERSION = '{1}';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PARAMETERS WHERE REFKEY LIKE '%M-{0}-{1}%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PRIMITIVES WHERE REFKEY LIKE '%M-{0}-{1}%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_DIMPOINTS WHERE REFKEY LIKE '%M-{0}-{1}%';" + Environment.NewLine,
                            data.StandardName, data.StdVersion
                            );
                    }
                    else
                    {
                        return String.Format(
                            "DELETE FROM TOJMGR.PD_MTRSTANDARDS WHERE STANDARDNAME = '{0}';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_MATERIALS WHERE MTRSTDNAME = '{0}';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PARAMETERS WHERE REFKEY LIKE '%M-{0}-%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PRIMITIVES WHERE REFKEY LIKE '%M-{0}-%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_DIMPOINTS WHERE REFKEY LIKE '%M-{0}-%';" + Environment.NewLine,
                            data.StandardName
                            );
                    }

                case NodeLevel.Material:
                    if (_versionSpecified)
                    {
                        return String.Format(
                            "DELETE FROM TOJMGR.PD_MATERIALS WHERE MTRNAME = '{0}' AND VERSION = '{1}';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PARAMETERS WHERE REFKEY LIKE '%M-{0}-{1}%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PRIMITIVES WHERE REFKEY LIKE '%M-{0}-{1}%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_DIMPOINTS WHERE REFKEY LIKE '%M-{0}-{1}%';" + Environment.NewLine,
                            data.MaterialName, data.MtrVersion
                            );
                    }
                    else
                    {
                        return String.Format(
                            "DELETE FROM TOJMGR.PD_MATERIALS WHERE MTRNAME = '{0}';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PARAMETERS WHERE REFKEY LIKE '%M-{0}-%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_PRIMITIVES WHERE REFKEY LIKE '%M-{0}-%';" + Environment.NewLine +
                            "DELETE FROM TOJMGR.PD_DIMPOINTS WHERE REFKEY LIKE '%M-{0}-%';" + Environment.NewLine,
                            data.MaterialName
                            );
                    }
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
