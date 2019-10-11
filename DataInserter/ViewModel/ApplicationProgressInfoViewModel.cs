using DataInserter.Model;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace DataInserter.ViewModel
{
    public class ApplicationProgressInfoViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Properties
        public bool? DialogResult { get { return false; } }

        private string _excelReaderStatus;
        public string ExcelReaderStatus
        {
            get => _excelReaderStatus;
            set
            {
                _excelReaderStatus = value;
                NotifyPropertyChanged();
            }
        }

        private string _excelReaderProgress;
        public string ExcelReaderProgress
        {
            get => _excelReaderProgress;
            set
            {
                _excelReaderProgress = value;
                NotifyPropertyChanged();
            }
        }

        private string _excelReaderInfo;
        public string ExcelReaderInfo
        {
            get => _excelReaderInfo;
            set
            {
                _excelReaderInfo = value;
                NotifyPropertyChanged();
            }
        }

        private string _xmlEditorStatus;
        public string XmlEditorStatus
        {
            get => _xmlEditorStatus;
            set
            {
                _xmlEditorStatus = value;
                NotifyPropertyChanged();
            }
        }

        private string _xmlEditorProgress;
        public string XmlEditorProgress
        {
            get => _xmlEditorProgress;
            set
            {
                _xmlEditorProgress = value;
                NotifyPropertyChanged();
            }
        }

        private string _xmlEditorInfo;
        public string XmlEditorInfo
        {
            get => _xmlEditorInfo;
            set
            {
                _xmlEditorInfo = value;
                NotifyPropertyChanged();
            }
        }

        private string _sqlCreatorStatus;
        public string SqlCreatorStatus
        {
            get => _sqlCreatorStatus;
            set
            {
                _sqlCreatorStatus = value;
                NotifyPropertyChanged();
            }
        }

        private string _sqlCreatorProgress;
        public string SqlCreatorProgress
        {
            get => _sqlCreatorProgress;
            set
            {
                _sqlCreatorProgress = value;
                NotifyPropertyChanged();
            }
        }

        private string _sqlCreatorInfo;
        public string SqlCreatorInfo
        {
            get => _sqlCreatorInfo;
            set
            {
                _sqlCreatorInfo = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Fields
        private readonly MainViewModel mainViewModel;
        #endregion

        #region Commands
        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public ApplicationProgressInfoViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            SetExcelReaderInitialStatus();
            SetXmlEditoInitialStatus();
            SetSqlCreatorInitialStatus();
        }
        #endregion

        #region Methods
        private void SetExcelReaderInitialStatus()
        {
            this.ExcelReaderStatus = mainViewModel.ExcelReaderViewModel.Status.CurrentStatus.ToString();
            this.ExcelReaderProgress = String.Format("Progress: {0} of {1}.", this.mainViewModel.ExcelReaderViewModel.Status.CurrentActionNumber, this.mainViewModel.ExcelReaderViewModel.Status.TotalActionsNumber);
            this.ExcelReaderInfo = mainViewModel.ExcelReaderViewModel.Status.Info;
        }

        private void SetXmlEditoInitialStatus()
        {
            this.XmlEditorStatus = mainViewModel.XmlManipulatorViewModel.Status.CurrentStatus.ToString();
            this.XmlEditorProgress = String.Format("Progress: {0} of {1}.", this.mainViewModel.XmlManipulatorViewModel.Status.CurrentActionNumber, this.mainViewModel.XmlManipulatorViewModel.Status.TotalActionsNumber);
            this.XmlEditorInfo = mainViewModel.XmlManipulatorViewModel.Status.Info;
        }

        private void SetSqlCreatorInitialStatus()
        {
            this.SqlCreatorStatus = mainViewModel.SqlCreatorViewModel.Status.CurrentStatus.ToString();
            this.SqlCreatorProgress = String.Format("Progress: {0} of {1}.", this.mainViewModel.SqlCreatorViewModel.Status.CurrentActionNumber, this.mainViewModel.SqlCreatorViewModel.Status.TotalActionsNumber);
            this.SqlCreatorInfo = mainViewModel.SqlCreatorViewModel.Status.Info;
        }

        private void Close()
        {
            var window = System.Windows.Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window != null)
            {
                window.Close();
            }
        }
        #endregion
    }
}
