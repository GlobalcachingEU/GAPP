using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GAPPSF.Commands
{
    public class AsyncDelegateCommand : ICommand
    {
        protected readonly Predicate<object> _canExecute;
        protected Func<object, Task> _asyncExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncDelegateCommand(Func<object, Task> execute)
            : this(execute, null)
        {
        }

        public AsyncDelegateCommand(Func<object, Task> asyncExecute,
                       Predicate<object> canExecute)
        {
            _asyncExecute = asyncExecute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return Core.ApplicationData.Instance.UIIsIdle;
            }

            return Core.ApplicationData.Instance.UIIsIdle && _canExecute(parameter);
        }


        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }



        public virtual async Task ExecuteAsync(object parameter)
        {
            Core.ApplicationData.Instance.BeginActiviy();
            await _asyncExecute(parameter);
            Core.ApplicationData.Instance.EndActiviy();
        }

    }
}
