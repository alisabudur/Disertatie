using System;
using System.Windows.Input;

namespace SoftwareQualityPrediction.Utils
{
    public class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;
        

        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute();
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        #endregion

        private Action _action;
        private Func<bool> _canExecute;
    }
}
