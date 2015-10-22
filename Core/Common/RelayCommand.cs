using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ampersand.Core.Common
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        private bool _updateSourceBeforeExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute, bool updateBeforeExecute = false)
        {
            _execute = execute;
            _canExecute = canExecute;
            _updateSourceBeforeExecute = updateBeforeExecute;

        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            try
            {
                if (_updateSourceBeforeExecute)
                {
                    TextBox txb = Keyboard.FocusedElement as TextBox;
                    BindingExpression be = null;

                    if (txb != null)
                        be = txb.GetBindingExpression(TextBox.TextProperty);

                    if (be != null)
                        if (be.ParentBinding.UpdateSourceTrigger == UpdateSourceTrigger.Default || be.ParentBinding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                            be.UpdateSource();
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine(this.GetType().Name + ".Execute(" + (string)parameter + ") : " + ex.Message);
            }

            if (CanExecute(parameter))
            {
                _execute(parameter);
            }
        }
    }
}
