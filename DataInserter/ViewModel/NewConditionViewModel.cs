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

        public NewConditionViewModel(ExcelReaderViewModel excelViewModel, MatchingCondition selectedCondition)
        {
            this.ExcelReaderViewModel = excelViewModel;
            this.SelectedCondition = selectedCondition;
        }
        #endregion

        #region Methods
        private void AddAndClose()
        {
            if (SelectedCondition == null)
            {
                ExcelReaderViewModel.Conditions.Add(new MatchingCondition(this.ExcelColumnName, this.NodeLevel, this.XmlNodeName));
                Close();
            }
            else
            {
                SelectedCondition.ExcelPropertyName = this.ExcelColumnName;
                SelectedCondition.NodeLevel = this.NodeLevel;
                SelectedCondition.XmlPropertyName = this.XmlNodeName;
                Close();
            }

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
