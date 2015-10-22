using System;
using System.Windows.Input;
using ampersand.Core;
using ampersand.Core.Common;
using System.Windows.Controls;

namespace ampersand_pb.ViewModels
{
    public class CalendarioViewModel : BaseViewModel
    {
        public CalendarioViewModel(CalendarMode calendarMode)
        {
            this.CalendarMode = calendarMode;
        }

        public CalendarioViewModel()
            :this(CalendarMode.Month)
        {
        }

        public CalendarMode CalendarMode { get; private set; }

        private DateTime _fecha = DateTime.Today;
        public DateTime Fecha
        {
            get
            {
                return _fecha;
            }
            set
            {
                _fecha = value;
                OnPropertyChanged("Fecha");
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(param => this.OnRequestSaveEvent(this.Fecha));
                return _saveCommand;
            }
        }

        public event EventHandler<CalendarioSaveEventArg> SaveEvent;
        private void OnRequestSaveEvent(DateTime fecha)
        {
            var handler = this.SaveEvent;
            if (handler != null)
                handler(this, new CalendarioSaveEventArg(fecha));
        }
    }

    public class CalendarioSaveEventArg : EventArgs
    {
        public CalendarioSaveEventArg(DateTime fecha)
        {
            Fecha = fecha;
        }

        public DateTime Fecha { get; private set; }
    }
}


