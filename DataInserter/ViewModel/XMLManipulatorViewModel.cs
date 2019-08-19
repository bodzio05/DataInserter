using DataInserter.ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInserter.ViewModel
{
    public class XMLManipulatorViewModel : ViewModelBase, IViewModel
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
        #endregion

        #region Fields
        private readonly IMainViewModel mainViewModel;
        #endregion

        #region Commands


        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }
        #endregion

        #region Constructors
        public XMLManipulatorViewModel(IMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }
        #endregion

        #region Methods

        #endregion
    }
}
