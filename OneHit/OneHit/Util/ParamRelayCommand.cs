using System;
using System.Windows.Input;

namespace OneHit.Util
{
    public class ParamRelayCommand<T> : ICommand
    {
        private Action<T> _handler;
        public ParamRelayCommand(Action<T> handler)
        {
            _handler = handler;
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _handler((T)parameter);
        }
    }
}
