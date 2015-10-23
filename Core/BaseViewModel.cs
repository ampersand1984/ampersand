using ampersand.Core.Common;
using System;

namespace ampersand.Core
{
    public class BaseViewModel : NotifyObject, IDisposable
    {
        public BaseViewModel()
        {

        }

        ~BaseViewModel()
	    {
            Dispose(false);
	    }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {

        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => this.OnRequestCloseEvent());
                return _closeCommand;
            }
        }
                    
        public event EventHandler CloseEvent;
        protected virtual void OnRequestCloseEvent()
        {
            EventHandler handler = this.CloseEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
