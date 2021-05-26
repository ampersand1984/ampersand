using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using MahApps.Metro.Controls.Dialogs;
using Serilog;

namespace ampersand_pb.ViewModels
{
    public class BuscarMovimentosViewModel : BaseViewModel, IMainWindowItem
    {
        public BuscarMovimentosViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;
        }

        private readonly IMovimientosDataAccess _movimientosDA;
        private readonly ConfiguracionModel _configuracionM;
        private Dictionary<ResumenModel, List<BaseMovimiento>> _resumenesDict = new Dictionary<ResumenModel, List<BaseMovimiento>>();

        private bool _busquedaRealizada;

        public string DisplayName => "Buscar en todos";

        public IDialogCoordinator DialogCoordinator { get; set; }

        private ObservableCollection<BaseMovimiento> _movimientosFiltrados;
        public ObservableCollection<BaseMovimiento> MovimientosFiltrados
        {
            get
            {
                return _movimientosFiltrados ?? (_movimientosFiltrados = new ObservableCollection<BaseMovimiento>());
            }
        }

        public bool HayResultados
        {
            get
            {
                return MovimientosFiltrados.Any();
            }
        }

        public bool MostrarLeyendaDeSinResultados
        {
            get
            {
                return _busquedaRealizada && !HayResultados;
            }
        }

        public long CantidadDeResultados
        {
            get
            {
                return MovimientosFiltrados.LongCount();
            }
        }

        private ICommand _buscarCommand;
        public ICommand BuscarCommand
        {
            get
            {
                if (_buscarCommand == null)
                    _buscarCommand = new RelayCommand(param => BuscarCommandExecute(param as string));

                return _buscarCommand;
            }
        }

        private ICommand _editarMovimientoCommand;
        public ICommand EditarMovimientoCommand
        {
            get { return _editarMovimientoCommand ?? (_editarMovimientoCommand = new RelayCommand(param => this.EditarMovimientoCommandExecute(param as BaseMovimiento), param => EditarMovimientoCommandCanExecute(param as BaseMovimiento))); }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(param => SaveCommandExecute(), param => SaveCommandCanExecute());
                return _saveCommand;
            }
        }

        private bool SaveCommandCanExecute()
        {
            return _resumenesDict.Keys.Any(a => a.HuboCambios);
        }

        private async void SaveCommandExecute()
        {
            Log.Information($"BuscarMovimentosViewModel SaveCommandExecute START");
            foreach (var resumenM in _resumenesDict.Keys.Where(a => a.HuboCambios))
            {
                Log.Information($"BuscarMovimentosViewModel SaveCommandExecute {resumenM.TextoPeriodo}");
                try
                {
                    _movimientosDA.SaveMovimientos(resumenM, _resumenesDict[resumenM]);
                    resumenM.HuboCambios = false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"MovimientosViewModel SaveCommandExecute {resumenM}");
                    var messageParam = new MessageParam("Falló la grabación", ex.Message);
                    await ShowMessage(messageParam);
                }
            }
            Log.Information($"BuscarMovimentosViewModel SaveCommandExecute END");
        }

        private void BuscarCommandExecute(string texto)
        {
            _busquedaRealizada = !texto.IsNullOrEmpty();

            if (!_resumenesDict.Any())
            {
                var getTextoeriodo = new Func<DateTime, string>(periodo =>
                {
                    var str = periodo.ToString("MMMM").ToTitle();

                    str += " " + periodo.Year;

                    return str;
                });

                var resumenes = _movimientosDA.GetResumenes();

                foreach (var resumen in resumenes)
                {
                    var movimientosDelResumen = _movimientosDA.GetMovimientos(resumen);

                    movimientosDelResumen.All(a => { a.DescripcionPeriodoResumen = getTextoeriodo(resumen.FechaDeCierre); return true; });

                    _resumenesDict.Add(resumen, movimientosDelResumen.ToList());
                }
            }

            var resultado = Enumerable.Empty<BaseMovimiento>();

            if (!texto.IsNullOrEmpty())
            {
                resultado = _resumenesDict.SelectMany(a => a.Value)
                    .Where(a => a.Descripcion.ToLower().Contains(texto.ToLower()) ||
                                                    a.DescripcionAdicional.ToLower().Contains(texto.ToLower()) ||
                                                    a.Tags.Any(t => t.ToLower().Contains(texto.ToLower())))
                                        .OrderByDescending(a => a.Fecha).ThenBy(a => a.Descripcion);
            }

            _movimientosFiltrados = new ObservableCollection<BaseMovimiento>(resultado);

            OnPropertyChanged("MovimientosFiltrados");
            OnPropertyChanged("HayResultados");
            OnPropertyChanged("CantidadDeResultados");
            OnPropertyChanged("MostrarLeyendaDeSinResultados");
        }

        private bool EditarMovimientoCommandCanExecute(BaseMovimiento param)
        {
            return param != null && param.Tipo != TiposDeMovimiento.Deuda;
        }

        private void EditarMovimientoCommandExecute(BaseMovimiento movimiento)
        {
            var movimientoABMVM = new MovimientoABMViewModel(movimiento, _configuracionM)
            {
                DialogCoordinator = DialogCoordinator,
                PermiteEditarFecha = false
            };
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            OnPublishViewModelEvent(movimientoABMVM);
        }

        private void MovimientoABMVM_CloseEvent(object sender, EventArgs e)
        {
            var movimientoABMVM = sender as MovimientoABMViewModel;
            movimientoABMVM.CloseEvent -= MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent -= MovimientoABMVM_SaveEvent;
            movimientoABMVM = null;
        }

        private async void MovimientoABMVM_SaveEvent(object sender, MovimientoABMSaveEventArgs e)
        {
            Log.Information($"BuscarMovimentosViewModel MovimientoABMVM_SaveEvent START");
            try
            {
                //no compara por instancias porque hay un override de Equals, pero lo trae igual porque coincide
                var kvpOrig = _resumenesDict.First(a => a.Value.Any(m => m.Equals(e.Model)));

                //quito el mov del resumen orig
                kvpOrig.Value.Remove(e.Model);
                kvpOrig.Key.HuboCambios = true;

                //me traigo el resumen que coincida con el Id y el periodo
                var kvpNuevo = _resumenesDict.First(a => a.Key.Id.Equals(e.Model.IdResumen) && a.Key.Periodo == kvpOrig.Key.Periodo);

                //agrego el mov al resumen nuevo
                kvpNuevo.Value.Add(e.Model);
                kvpNuevo.Key.HuboCambios = true;

                //kvpOrig y kvpNuevo pueden llegar a ser el mismo, no jode 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MovimientosViewModel MovimientoABMVM_SaveEvent");
                var messageParam = new MessageParam("Falló la grabación", ex.Message);
                await ShowMessage(messageParam);
            }

            Log.Information($"BuscarMovimentosViewModel MovimientoABMVM_SaveEvent START");
        }

        #region Events

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            var handler = this.PublishViewModelEvent;
            if (handler != null)
                handler(this, new PublishViewModelEventArgs(viewModel));
        }

        private async Task<MessageDialogResult> ShowMessage(MessageParam messageParam)
        {
            var result = await DialogCoordinator.ShowMessageAsync(this, messageParam.Title, messageParam.Message, messageParam.Style, messageParam.Settings);

            return result;
        }

        #endregion
    }
}
