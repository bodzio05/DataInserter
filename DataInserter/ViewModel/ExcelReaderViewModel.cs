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

        private List<MatchedData> _deleteList;
        public List<MatchedData> DeleteList
        {
            get => _deleteList;
            set
            {
                _deleteList = value;
                NotifyPropertyChanged();
            }
        }

        private DeletingCondition _deleteCondition;
        public DeletingCondition DeleteCondition
        {
            get => _deleteCondition;
            set
            {
                _deleteCondition = value;
                DeleteConditionKey = value.ExcelPropertyName;
                NotifyPropertyChanged();
            }
        }

        private string _deleteConditionKey;
        public string DeleteConditionKey
        {
            get => _deleteConditionKey;
            set
            {
                _deleteConditionKey = value;
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

        private bool _allowDeletingSQL;
        public bool AllowDeletingSQL
        {
            get => _allowDeletingSQL;
            set
            {
                _allowDeletingSQL = value;
                NotifyPropertyChanged();
                mainViewModel.AllowDeleting = true;
            }
        }

        private bool _versionSpecified;
        public bool VersionSpecified
        {
            get => _versionSpecified;
            set
            {
                _versionSpecified = value;
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
        private int stdNameIndex;
        private int delKeyIndex;
        private int mtrVersionIndex;

        private Dictionary<string, int> columns;

        private bool stdKeyExists;
        private bool mtrKeyExists;
        private bool delKeyExists;
        private bool mtrVersionExists;
        #endregion

        #region Commands
        public ICommand SearchForExcelFileCommand { get { return new RelayCommand(SearchForExcelFile, AlwaysTrue); } }
        public ICommand AddConditionCommand { get { return new RelayCommand(AddCondition, AlwaysTrue); } }

        //public ICommand DeleteConditionCommand { get { return new RelayCommand(DeleteCondition, AlwaysTrue); } }
        //public ICommand ModifyConditionCommand { get { return new RelayCommand(ModifyCondition, AlwaysTrue); } }
        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public ExcelReaderViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.Status = new Status(StatusEnum.Waiting);
            this.ExcelReaderResult = new List<MatchedData>();
            this.columns = new Dictionary<string, int>();
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

            foreach (var condition in Conditions)
            {
                if (condition.Type == ConditionType.EditParameterCondition)
                {
                    condition.DatabaseTableName = DatabaseTable.PD_PARAMETERS;
                    condition.DatabaseFieldName = DatabaseFields.PARNAME;
                    condition.XmlPropertyName = XmlNodes.Parameter;
                }
            }
        }

        //public void ModifyCondition()
        //{
            //NewConditionViewModel dialog = new NewConditionViewModel(this, SelectedCondition);
            //var result = DialogService.ShowDialog<NewConditionView>(this, dialog);
        //}

        //public void DeleteCondition()
        //{
            //if (SelectedCondition != null && Conditions.Contains(SelectedCondition))
            //{
            //    Conditions.Remove(SelectedCondition);
            //}
        //}

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

            if (ExcelReaderResult.Count != 0)
            {
                return true;
            }
            else if (ExcelReaderResult.Count == 0 && AllowDeletingSQL)
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

            DeleteList = new List<MatchedData>();
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

            if (!IdentifyColumns())
            {
                return null;
            }

            if (AllowDeletingSQL && (DeleteCondition != null || DeleteConditionKey != ""))
            {
                IdentifyDeletingColumn();
            }

            var mtrVersionCondition = Conditions.SingleOrDefault(c => (c.XmlPropertyName == XmlNodes.Version && c.NodeLevel == NodeLevel.Material));

            if (mtrVersionCondition != null)
            {
                IdentifyMtrVersionColumn(mtrVersionCondition);
            }
 
            List<MatchedData> materials = new List<MatchedData>();

            for (int i = 2; i <= rowsCount; i++)
            {
                string[] row = ReadRow(i);

                string stdName = "";
                string mtrName = "";
                string delName = "";
                string mtrVersion = "";


                if (stdKeyExists)
                {
                    stdName = row[stdNameIndex - 1];
                }

                if (mtrKeyExists)
                {
                    mtrName = row[mtrNameIndex - 1];
                }

                if (delKeyExists)
                {
                    delName = row[delKeyIndex - 1];
                }

                if (mtrVersionExists)
                {
                    mtrVersion = row[mtrVersionIndex - 1];
                }


                for (int j = 1; j <= colsCount; j++)
                {
                    //this.Status.CurrentActionNumber++;

                    if (j == delKeyIndex && row[j-1].ToUpper() == "TRUE")
                    {
                        DeleteList.Add(new MatchedData() { StandardName = stdName, MaterialName = mtrName, MtrVersion = mtrVersion, DeletingCondition = DeleteCondition});
                        continue;
                    }

                    if (j == mtrNameIndex || j == stdNameIndex)
                    {
                        continue;
                    }

                    string cellContent;

                    try
                    {
                        cellContent = xlRange.Cells[j][i].Value2.ToString();
                    }
                    catch (Exception ex)
                    {
                        cellContent = "";                      
                    }

                    var property = columns.FirstOrDefault(c => c.Value == j).Key;

                    //var condition = Conditions.FirstOrDefault(c => c.ExcelPropertyName == cellContent);
                    var condition = Conditions.FirstOrDefault(c => c.ExcelPropertyName == property);

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

        private void CreateHeadersList()
        {
            throw new NotImplementedException();
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

        private bool IdentifyColumns()
        {
            for (int j = 1; j <= colsCount; j++)
            {
                string cellContent;
                try
                {
                    cellContent = xlRange.Cells[j][1].Value2.ToString();
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
                        mtrNameIndex = j;
                        mtrKeyExists = true;
                    }
                    else if (condition.ExcelPropertyName == cellContent && condition.XmlPropertyName == XmlNodes.StandardName)
                    {
                        stdNameIndex = j;
                        stdKeyExists = true;
                    }

                    if (!columns.ContainsKey(cellContent))
                    {
                        columns.Add(cellContent, j);
                    }
                }
            }
            
            if (mtrKeyExists || stdKeyExists)
                return true;
            else
                return false;
        }

        private bool IdentifyDeletingColumn()
        {
            for (int j = 1; j <= colsCount; j++)
            {
                string cellContent;
                try
                {
                    cellContent = xlRange.Cells[j][1].Value2.ToString();
                }
                catch (Exception ex)
                {
                    cellContent = string.Empty;
                    continue;
                }

                if (DeleteCondition.ExcelPropertyName == cellContent)
                {
                    delKeyIndex = j;
                    delKeyExists = true;
                }

                if (delKeyExists)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IdentifyMtrVersionColumn(MatchingCondition condition)
        {
            for (int j = 1; j <= colsCount; j++)
            {
                string cellContent;
                try
                {
                    cellContent = xlRange.Cells[j][1].Value2.ToString();
                }
                catch (Exception ex)
                {
                    cellContent = string.Empty;
                    continue;
                }

                if (condition.ExcelPropertyName == cellContent)
                {
                    mtrVersionIndex = j;
                    mtrVersionExists = true;
                }

                if (mtrVersionExists)
                {
                    VersionSpecified = true;
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
