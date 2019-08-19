using DataInserter.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using DataInserter.Model;
using System.Windows.Input;
using DataInserter.ViewModel.Commands;
using MvvmDialogs;
using DataInserter.View;
using System.Collections.ObjectModel;
using System.IO;

namespace DataInserter.ViewModel
{
    public class ExcelReaderViewModel: ViewModelBase
    {
        #region Properties
        private bool _isRunnable;
        public bool IsRunnable
        {
            get => _isRunnable;
            set
            {
                _isRunnable = value;
                NotifyPropertyChanged();
            }
        }

        private string _excelPath;
        public string ExcelPath
        {
            get => _excelPath;
            set
            {
                _excelPath = value;
                NotifyPropertyChanged();
            }
        }

        private int _sheetNumber = 1;
        public int SheetNumber
        {
            get => _sheetNumber;
            set
            {
                if(value>0)
                { 
                    _sheetNumber = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<MatchingCondition> _conditions;
        public ObservableCollection<MatchingCondition> Conditions
        {
            get => _conditions;
            set
            {
                _conditions = value;
                NotifyPropertyChanged();
            }
        }

        private MatchingCondition _selectedCondition;
        public MatchingCondition SelectedCondition
        {
            get => _selectedCondition;
            set
            {
                _selectedCondition = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Fields
        private readonly IMainViewModel mainViewModel;
        private readonly IDialogService DialogService;
        #endregion

        #region Commands
        public ICommand SearchForExcelFileCommand { get { return new RelayCommand(SearchForExcelFile, AlwaysTrue); } }
        public ICommand AddConditionCommand { get { return new RelayCommand(AddCondition, AlwaysTrue); } }
        public ICommand DeleteConditionCommand { get { return new RelayCommand(DeleteCondition, AlwaysTrue); } }
        //public ICommand RunExcelReaderCommand { get { return new RelayCommand(RunExcelReader, CanBeExecuted); } }
        public ICommand RunExcelReaderCommand { get { return new RelayCommand(RunExcelReader, AlwaysTrue); } }
        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public ExcelReaderViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.DialogService = new MvvmDialogs.DialogService();

            Conditions = new ObservableCollection<MatchingCondition>();
        }
        #endregion

        #region UI Methods


        private void SearchForExcelFile()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "EXCEL Files (*.xlsx)|*.xlsx" };
            var found = ofd.ShowDialog();
            if (found == false) ExcelPath = "";
            ExcelPath = ofd.FileName;
        }

        private void AddCondition()
        {
            NewConditionViewModel dialog = new NewConditionViewModel(this);
            var result = DialogService.ShowDialog<NewConditionView>(this, dialog);
        }

        public void DeleteCondition()
        {
            if (SelectedCondition != null && Conditions.Contains(SelectedCondition))
            {
                Conditions.Remove(SelectedCondition);
            }
        }

        private bool CanBeExecuted()
        {
            if(File.Exists(ExcelPath) && Conditions.Count > 0 && SheetNumber > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RunExcelReader()
        {
            if (CanBeExecuted())
            {
                ReadExcel();
            }
            else
            {
                System.Windows.MessageBox.Show("Can't run program. Check the settings and try again.");
            }
        }
        #endregion

        #region Reader Methods
        private List<Material> ReadExcel()
        {
            #region OpenExcelFile

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(ExcelPath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[SheetNumber];
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

                if (previousWasEmpty && emptyRowCounter > 10)
                {
                    endReading = true;
                }

                if (rowCounter == 1 || row[0] == "" || row[1] == "")
                {
                    rowCounter++;
                    continue;
                }

                rowCounter++;

                Material material = new Material(row[1], row[0]);
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
        #endregion
    }
}
