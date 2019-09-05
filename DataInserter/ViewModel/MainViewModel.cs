using DataInserter.ViewModel.Commands;
using DataInserter.ViewModel.Interfaces;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataInserter.ViewModel
{
    public class MainViewModel: ViewModelBase, IMainViewModel
    {
        #region Properties
        private ExcelReaderViewModel _excelReaderViewModel;
        public ExcelReaderViewModel ExcelReaderViewModel
        {
            get => _excelReaderViewModel;
            set
            {
                _excelReaderViewModel = value;
                NotifyPropertyChanged();
            }
        }

        private XMLManipulatorViewModel _xmlManipulatorViewModel;
        public XMLManipulatorViewModel XmlManipulatorViewModel
        {
            get => _xmlManipulatorViewModel;
            set
            {
                _xmlManipulatorViewModel = value;
                NotifyPropertyChanged();
            }
        }

        private SQLCreatorViewModel _sqlCreatorViewModel;
        public SQLCreatorViewModel SqlCreatorViewModel
        {
            get => _sqlCreatorViewModel;
            set
            {
                _sqlCreatorViewModel = value;
                NotifyPropertyChanged();
            }
        }

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

        private string _xmlFolderPath;
        public string XmlFolderPath
        {
            get => _xmlFolderPath;
            set
            {
                _xmlFolderPath = value;
                NotifyPropertyChanged();
            }
        }

        public bool ExcelReaderSuccess { get; set; }
        #endregion

        #region Fields
        private readonly IDialogService DialogService;
        #endregion

        #region Commands
        public ICommand RunDataInserterCommand { get { return new RelayCommand(RunInserter, AlwaysTrue); } }
        public ICommand ExitCommand { get { return new RelayCommand(OnExitApp, AlwaysTrue); } }

        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            DialogService = new MvvmDialogs.DialogService();

            ExcelReaderViewModel = new ExcelReaderViewModel(this);
            XmlManipulatorViewModel = new XMLManipulatorViewModel(this);
            SqlCreatorViewModel = new SQLCreatorViewModel(this);
        }
        #endregion

        #region Methods
        private void RunInserter()
        {
            ExcelReaderSuccess = ExcelReaderViewModel.RunExcelReader();

            if (!ExcelReaderSuccess || ExcelReaderViewModel.ExcelReaderResult.Count < 1) return;

            if (XmlManipulatorViewModel.EditXmlFiles)
            {
                XmlManipulatorViewModel.RunXmlEditor(ExcelReaderViewModel.ExcelReaderResult);
            }

            if (SqlCreatorViewModel.CreateSqlFile)
            {
                SqlCreatorViewModel.RunSqlCreator(ExcelReaderViewModel.ExcelReaderResult);
            }
        }

        private void OnExitApp()
        {
            System.Windows.Application.Current.MainWindow.Close();
        }
        #endregion
    }
}
