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

        private string _xmlNodeName;
        public string XmlNodeName
        {
            get => _xmlNodeName;
            set
            {
                _xmlNodeName = value;
                NotifyPropertyChanged();
            }
        }

        private MatchingCondition _newCondition;
        public MatchingCondition NewCondition
        {
            get => _newCondition;
            set
            {
                _newCondition = value;
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
        #endregion

        #region Methods
        private void AddAndClose()
        {
            ExcelReaderViewModel.Conditions.Add(new MatchingCondition(this.ExcelColumnName, this.NodeLevel, this.XmlNodeName));
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
