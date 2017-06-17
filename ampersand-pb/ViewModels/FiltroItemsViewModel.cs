using ampersand.Core;
using ampersand.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class FiltroItemsViewModel: BaseViewModel
    {
        public FiltroItemsViewModel(Filtro filtro, IEnumerable<string> valores)
        {
            _filtro = filtro;

            Items = valores.Select(a => new FiltroItem() { Descripcion = a }).ToList();
        }

        private Filtro _filtro;

        public IEnumerable<FiltroItem> Items { get; private set; }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(para => SaveCommandExecute(), param => SaveCommandCanExecute());
                return _saveCommand;
            }
        }

        private bool SaveCommandCanExecute()
        {
            return true;
        }

        private void SaveCommandExecute()
        {
            var aplicado = !(Items.All(a => a.Selected) || Items.All(a => !a.Selected));

            _filtro.Aplicado = aplicado;
            _filtro.Valores = aplicado ?
                Items.Where(a => a.Selected)
                     .Select(a => a.Descripcion)
                     .ToList() :
                null;
            OnSaveEvent();
            CloseCommand.Execute(null);
        }

        public event EventHandler<FiltroViewModelSaveEventArgs> SaveEvent;
        private void OnSaveEvent()
        {
            var handler = this.SaveEvent;
            if (handler != null)
                handler(this, new FiltroViewModelSaveEventArgs(_filtro));
        }
    }

    public class FiltroItem: NotifyObject
    {
        public string Descripcion { get; set; }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; OnPropertyChanged("Selected"); }
        }

    }

    public class FiltroViewModelSaveEventArgs: EventArgs
    {
        public FiltroViewModelSaveEventArgs(Filtro filtro)
        {
            Filtro = filtro;
        }

        public Filtro Filtro { get; private set; }
    }
}
