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

        private List<MatchedData> _excelReaderResult;
        public List<MatchedData> ExcelReaderResult
        {
            get => _excelReaderResult;
            set
            {
                _excelReaderResult = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Fields
        private readonly IMainViewModel mainViewModel;
        private readonly IDialogService DialogService;
        private Excel.Range xlRange;

        private int colsCount;
        private int rowsCount;
        private int mtrNameIndex;
        private int stdNameIntex;

        private bool stdKeyExists;
        private bool mtrKeyExists;
        #endregion

        #region Commands
        public ICommand SearchForExcelFileCommand { get { return new RelayCommand(SearchForExcelFile, AlwaysTrue); } }
        public ICommand AddConditionCommand { get { return new RelayCommand(AddCondition, AlwaysTrue); } }
        public ICommand DeleteConditionCommand { get { return new RelayCommand(DeleteCondition, AlwaysTrue); } }
        public ICommand ModifyConditionCommand { get { return new RelayCommand(ModifyCondition, AlwaysTrue); } }
        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public ExcelReaderViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.Status = new Status(StatusEnum.Waiting);
            this.ExcelReaderResult = new List<MatchedData>();
            this.DialogService = new MvvmDialogs.DialogService();

            Conditions = new ObservableCollection<MatchingCondition>();
        }
        #endregion

        #region UI Methods
        private void SearchForExcelFile()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            var found = ofd.ShowDialog();
            if (found == false) ExcelPath = "";
            ExcelPath = ofd.FileName;
        }

        private void AddCondition()
        {
            NewConditionViewModel dialog = new NewConditionViewModel(this);
            var result = DialogService.ShowDialog<NewConditionView>(this, dialog);
        }

        public void ModifyCondition()
        {
            NewConditionViewModel dialog = new NewConditionViewModel(this, SelectedCondition);
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
            if(File.Exists(ExcelPath) && 
                Conditions.Count > 0 && 
                SheetNumber > 0 && 
                (Conditions.Any(c=>c.XmlPropertyName == XmlNodes.StandardName) || Conditions.Any(c=>c.XmlPropertyName == XmlNodes.Name)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RunExcelReader()
        {
            if (CanBeExecuted())
            {
                this.ExcelReaderResult = ReadExcel();
            }

            if (ExcelReaderResult != null && ExcelReaderResult.Count != 0)
            {
                return true;
            }
            else
            {
                System.Windows.MessageBox.Show("Can't run program. Check the settings and try again.");
                return false;
            }
        }
        #endregion

        #region Reader Methods
        private List<MatchedData> ReadExcel()
        {
            this.Status.CurrentStatus = StatusEnum.InProgress;

            #region OpenExcelFile

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(ExcelPath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[SheetNumber];
            this.xlRange = xlWorksheet.UsedRange;
            #endregion

            List<MatchedData> materials = CreateMatchedList();

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

            this.Status.CurrentStatus = StatusEnum.Ended;

            return materials;
        }

        private List<MatchedData> CreateMatchedList()
        {
            IdentifyRowsAndColumnsNumber();

            if (!IdentifyStandardAndName())
            {
                return null;
            }
 
            List<MatchedData> materials = new List<MatchedData>();

            for (int i = 2; i <= rowsCount; i++)
            {
                string[] row = ReadRow(i);

                string stdName = "";
                string mtrName = "";

                if (stdKeyExists && mtrKeyExists)
                {
                    stdName = row[stdNameIntex - 1];
                    mtrName = row[mtrNameIndex - 1];
                }
                else if (stdKeyExists && !mtrKeyExists)
                {
                    stdName = row[stdNameIntex - 1];
                }
                else if (!stdKeyExists && mtrKeyExists)
                {
                    mtrName = row[mtrNameIndex - 1];
                }

                for (int j = 1; j < colsCount; j++)
                {
                    this.Status.CurrentActionNumber++;

                    if (j == mtrNameIndex || j == stdNameIntex)
                    {
                        continue;
                    }

                    var condition = Conditions.FirstOrDefault(c => c.ExcelPropertyName == xlRange.Cells[j][1].Value2.ToString());

                    if (IsNotEmpty(stdName, mtrName, row[j - 1], condition) && !materials.Any(d=>d.StandardName == stdName && d.MaterialName == mtrName && d.PropertyValue == row[j - 1] && d.RootCondition == condition))
                    {
                        materials.Add(new MatchedData()
                        {
                            StandardName = stdName,
                            MaterialName = mtrName,
                            RootCondition = condition,
                            PropertyValue = row[j - 1]
                        });
                    }
                }
            }

            return materials;
        }

        private string[] ReadRow(int i)
        {
            string[] row = new string[colsCount];

            for (int j = 0; j < colsCount; j++)
            {
                string value;
                try
                {
                    value = xlRange.Cells[j + 1][i].Value2.ToString();
                }
                catch (Exception ex)
                {
                    value = string.Empty;
                }

                row[j] = value;
            }

            return row;
        }

        private void IdentifyRowsAndColumnsNumber()
        {
            colsCount = CountNotEmptyColumns(xlRange.Columns.Count);
            rowsCount = CountNotEmptyRows(xlRange.Rows.Count);

            this.Status.CurrentActionNumber = 0;
            this.Status.TotalActionsNumber = rowsCount*rowsCount;
        }

        private int CountNotEmptyRows(int number)
        {
            int emptyCounter = 0;
            int counter = 0;
            int lastNotEmptyCell = 0;

            for (int i = 1; i <= number; i++)
            {
                if (emptyCounter > 10)
                {
                    break;
                }

                try
                {
                    var value = xlRange.Cells[1][i].Value2.ToString();
                    emptyCounter = 0;
                    counter++;
                    lastNotEmptyCell = counter;
                }
                catch (Exception)
                {
                    counter++;
                    emptyCounter++;
                }
            }

            return lastNotEmptyCell;
        }

        private int CountNotEmptyColumns(int number)
        {
            int emptyCounter = 0;
            int counter = 0;
            int lastNotEmptyCell = 0;

            for (int i = 1; i <= number; i++)
            {
                if (emptyCounter > 10)
                {
                    break;
                }

                try
                {
                    var value = xlRange.Cells[i][1].Value2.ToString();
                    emptyCounter = 0;
                    counter++;
                    lastNotEmptyCell = counter;
                }
                catch (Exception)
                {
                    counter++;
                    emptyCounter++;
                }
            }

            return lastNotEmptyCell;
        }

        private bool IdentifyStandardAndName()
        {
            for (int i = 1; i <= colsCount; i++)
            {
                string cellContent;
                try
                {
                    cellContent = xlRange.Cells[i][1].Value2.ToString();
                }
                catch (Exception ex)
                {
                    cellContent = string.Empty;
                    continue;
                }

                foreach (var condition in Conditions)
                {
                    if (condition.ExcelPropertyName == cellContent && condition.XmlPropertyName == XmlNodes.Name)
                    {
                        mtrNameIndex = i;
                        mtrKeyExists = true;
                    }

                    if (condition.ExcelPropertyName == cellContent && condition.XmlPropertyName == XmlNodes.StandardName)
                    {
                        stdNameIntex = i;
                        stdKeyExists = true;
                    }

                    if (mtrKeyExists && stdKeyExists)
                    {
                        return true;
                    }
                }

                if (mtrKeyExists || stdKeyExists)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNotEmpty(string stdName, string mtrName, string propValue, MatchingCondition condition)
        {
            if ((stdName == "" && mtrName == "") || propValue == "" || condition == null)
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
