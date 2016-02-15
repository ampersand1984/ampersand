using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.ViewModels
{
    public class ResumenesGraficosViewModel: BaseViewModel, IMainWindowItem
    {

        public ResumenesGraficosViewModel(IMovimientosDataAccess movimientosDA)
        {
            _movimientosDA = movimientosDA;

            _resumenes = _movimientosDA.GetResumenes();

            MinimoPeriodo = int.Parse(_resumenes.Min(a => a.Periodo));
            MaximoPeriodo = int.Parse(_resumenes.Max(a => a.Periodo));
            
            _minimoPeriodoSeleccionado = int.Parse(DateTime.Today.AddYears(-1).GetPeriodo());
            _maximoPeriodoSeleccionado = MaximoPeriodo;
        }

        private IMovimientosDataAccess _movimientosDA;
        private IEnumerable<ResumenModel> _resumenes;

        public string DisplayName
        {
            get
            {
                return "Resumenes gráficos";
            }
        }

        public int MinimoPeriodo { get; private set; }

        public int MaximoPeriodo { get; private set; }

        private int _minimoPeriodoSeleccionado;
        public int MinimoPeriodoSeleccionado
        {
            get
            {
                return _minimoPeriodoSeleccionado;
            }

            set
            {
                _minimoPeriodoSeleccionado = value;
                OnPropertyChanged("MinimoPeriodoSeleccionado");
            }
        }

        private int _maximoPeriodoSeleccionado;
        public int MaximoPeriodoSeleccionado
        {
            get
            {
                return _maximoPeriodoSeleccionado;
            }

            set
            {
                _maximoPeriodoSeleccionado = value;
                OnPropertyChanged("MaximoPeriodoSeleccionado");
            }
        }

        private IEnumerable<IEnumerable<ResumenModel>> _resumenesAgrupados;

        public IEnumerable<IEnumerable<ResumenModel>> ResumenesAgrupados
        {
            get
            {
                if (_resumenesAgrupados == null)
                    _resumenesAgrupados = GetResumenesAgrupados();
                return _resumenesAgrupados;
            }
        }

        private IEnumerable<IEnumerable<ResumenModel>> GetResumenesAgrupados()
        {

            var periodos = _resumenes.Select(a => a.Periodo).Distinct().OrderBy(a => a);

            var tipos = _resumenes.Select(a => a.Descripcion).Distinct();

            var resumenesAgrupados = new List<IEnumerable<ResumenModel>>();

            foreach (var tipo in tipos)
            {
                var resumenesPorTipo = _resumenes.Where(a => a.Descripcion.Equals(tipo)).ToList();

                foreach (var periodo in periodos)
                {
                    if (!resumenesPorTipo.Any(a => a.Periodo.Equals(periodo)))
                    {
                        var month = int.Parse(periodo.Substring(4));
                        var year = int.Parse(periodo.Substring(0, 4));
                        var resumenVacio = new ResumenModel
                        {
                            FechaDeCierre = new DateTime(year, month, 20),
                            Descripcion = tipo,
                            Tipo = TipoMovimiento.Credito
                        };
                        resumenesPorTipo.Add(resumenVacio);
                    }
                }

                resumenesPorTipo = resumenesPorTipo.OrderBy(a => a.Periodo).ToList();

                resumenesAgrupados.Add(resumenesPorTipo);
            }

            return resumenesAgrupados;
        }

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            var handler = this.PublishViewModelEvent;
            if (handler != null)
                handler(this, new PublishViewModelEventArgs(viewModel));
        }
    }
}
