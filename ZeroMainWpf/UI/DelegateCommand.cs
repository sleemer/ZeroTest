using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZeroMainWpf.UI
{
    public sealed class DelegateCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute ?? (Func<bool>)(() => true);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }
        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecute()
        {
            var handler = CanExecuteChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
