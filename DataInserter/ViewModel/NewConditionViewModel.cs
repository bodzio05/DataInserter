using DataInserter.Model;
using DataInserter.ViewModel.Commands;
using DataInserter.ViewModel.Interfaces;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DataInserter.ViewModel
{
    public class NewConditionViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Properties
        public bool? DialogResult { get { return false; } }

        private string _excelColumnName;
        public string ExcelColumnName
        {
            get => _excelColumnName;
            set
            {
                _excelColumnName = value;
                NotifyPropertyChanged();
            }
        }

        private NodeLevel _nodeLevel;
        public NodeLevel NodeLevel
        {
            get => _nodeLevel;
            set
            {
                _nodeLevel = value;

                if (NodeLevel == NodeLevel.Parameter)
                {
                    EditParameterMode = true;
                    XmlNodeName = XmlNodes.Parameter;
                }
                else
                {
                    EditParameterMode = false;
                }
                NotifyPropertyChanged();
            }
        }

        private XmlNodes _xmlNodeName;
        public XmlNodes XmlNodeName
        {
            get => _xmlNodeName;
            set
            {
                _xmlNodeName = value;
                NotifyPropertyChanged();
            }
        }

        private string _parameterDescription;
        public string ParameterDescription
        {
            get => _parameterDescription;
            set
            {
                _parameterDescription = value;
                NotifyPropertyChanged();
            }
        }

        private int _parameterContext;
        public int ParameterContext
        {
            get => _parameterContext;
            set
            {
                if (!Int32.TryParse(value.ToString(), out int result))
                    _parameterContext = 2;
                else
                    _parameterContext = value;
                NotifyPropertyChanged();
            }
        }

        private string _parameterValueType;
        public string ParameterValueType
        {
            get => _parameterValueType;
            set
            {
                _parameterValueType = value;
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

        private bool _deleteFlag = false;
        public bool DeleteFlag
        {
            get => _deleteFlag;
            set
            {
                _deleteFlag = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsEnabled));
                //if (value)
                //    IsEnabled = false;
                //else
                //    IsEnabled = true;
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            //get => _isEnabled;
            //set
            //{
            //    _isEnabled = value;
            //    NotifyPropertyChanged();
            //}
            get
            {
                if (DeleteFlag || EditParameterMode)
                    return false;
                else 
                    return true;
            }
        }

        private bool _editParameterMode;
        public bool EditParameterMode
        {
            get => _editParameterMode;
            set
            {
                _editParameterMode = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }



        #endregion

        #region Fields
        private ExcelReaderViewModel ExcelReaderViewModel;
        #endregion

        #region Commands
        public ICommand AddAndCloseCommand { get { return new RelayCommand(AddAndClose, AlwaysTrue); } }
        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public NewConditionViewModel(ExcelReaderViewModel excelViewModel)
        {
            this.ExcelReaderViewModel = excelViewModel;
        }

        //public NewConditionViewModel(ExcelReaderViewModel excelViewModel, MatchingCondition selectedCondition)
        //{
        //    this.ExcelReaderViewModel = excelViewModel;
        //    this.SelectedCondition = selectedCondition;
        //}
        #endregion

        #region Methods
        private void AddAndClose()
        {
            if (DeleteFlag && EditParameterMode)
            {
                MessageBox.Show("You cannot delete parameter! In development.");
            }
            else if (DeleteFlag)
            {
                ExcelReaderViewModel.DeleteCondition = new DeletingCondition(this.ExcelColumnName, this.NodeLevel);
                ExcelReaderViewModel.AllowDeletingSQL = true;
            }
            else
            {
                if (NodeLevel == NodeLevel.Parameter)
                {
                    ExcelReaderViewModel.Conditions.Add(new MatchingCondition(this.ExcelColumnName, this.NodeLevel, new Parameter(this.ExcelColumnName, this.ParameterDescription, this.ParameterContext, this.ParameterValueType)));
                }
                else
                {
                    ExcelReaderViewModel.Conditions.Add(new MatchingCondition(this.ExcelColumnName, this.NodeLevel, this.XmlNodeName));
                }

                //if (SelectedCondition == null)
                //{
                //    ExcelReaderViewModel.Conditions.Add(new MatchingCondition(this.ExcelColumnName, this.NodeLevel, this.XmlNodeName));
                //}
                //else
                //{
                //    SelectedCondition.ExcelPropertyName = this.ExcelColumnName;
                //    SelectedCondition.NodeLevel = this.NodeLevel;
                //    SelectedCondition.XmlPropertyName = this.XmlNodeName;
                //}
            }

            Close();
        }

        private void Close()
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window != null)
            {
                window.Close();
            }
        }
        #endregion
    }
}
