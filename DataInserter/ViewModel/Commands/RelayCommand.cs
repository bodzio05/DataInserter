using System;
using System.Diagnostics;
using System.Windows.Input;

namespace DataInserter.ViewModel.Commands
{
    class RelayCommand : ICommand
    {
        #region Fields 
        readonly Action<object> _execute;
        readonly Func<object, bool> _canExecute;
        object lockThis = new object();
        #endregion // Fields 

        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null) { }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion // Constructors 

        #region ICommand Members 
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                lock (lockThis)
                {
                    if (_canExecute != null)
                        CommandManager.RequerySuggested += value;
                }

            }
            remove
            {
                lock (lockThis)
                {
                    if (_canExecute != null)
                        CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion // ICommand Members 
    }
}
