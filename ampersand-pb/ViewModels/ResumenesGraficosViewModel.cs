using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.ViewModels
{
    public class ResumenesGraficosViewModel : BaseViewModel, IMainWindowItem
    {
        private const string TOTALES_MENSUAL = "Totales mensuales";
        private const string TOTALES_POR_TARJETA_MENSUAL = "Totales mensuales por tarjeta";
        private const string TOTALES_POR_TAGS_MENSUAL = "Totales mensuales por categoría";

        public ResumenesGraficosViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            TiposDeGraficos = new List<string>()
            {
                TOTALES_MENSUAL,
                TOTALES_POR_TARJETA_MENSUAL,
                TOTALES_POR_TAGS_MENSUAL
            };

            _graficoSeleccionado = TOTALES_POR_TARJETA_MENSUAL;

            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;

            _resumenes = _movimientosDA.GetResumenes();
            if (_resumenes.Any())
            {
                Periodos = _resumenes.GroupBy(a => a.Periodo)
                    .Select(grp => new PeriodoModel
                    {
                        Periodo = grp.First().Periodo,
                        TextoPeriodo = grp.First().TextoPeriodo
                    });

                _minimoPeriodo = (DateTime.Today.Year - 1) + "01";//Enero del año pasado
                //o junio si ya estamos mes 6
                if (DateTime.Today.Month > 5)
                    _minimoPeriodo = (DateTime.Today.Year - 1) + "06";//Junio del año pasado

                if (int.Parse(_minimoPeriodo) < int.Parse(_resumenes.Min(a => a.Periodo)))
                    _minimoPeriodo = _resumenes.Min(a => a.Periodo);//a menos que no exista

            }
            _maximoPeriodo = _resumenes.Max(a => a.Periodo);
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                if (_seleccionDeMediosDePagoVM != null)
                {
                    _seleccionDeMediosDePagoVM.SeleccionDeMediosDePagoCambiada -= SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada;
                    _seleccionDeMediosDePagoVM.Dispose();
                }
                _seleccionDeMediosDePagoVM = null;
            }
            base.Dispose(dispose);
        }

        private readonly ConfiguracionModel _configuracionM;

        private IMovimientosDataAccess _movimientosDA;
        private IEnumerable<ResumenModel> _resumenes;

        public string DisplayName
        {
            get
            {
                return "Gráficos por mes";
            }
        }

        public IEnumerable<string> TiposDeGraficos { get; private set; }

        private string _graficoSeleccionado;
        public string GraficoSeleccionado
        {
            get
            {
                return _graficoSeleccionado;
            }
            set
            {
                _graficoSeleccionado = value;
                OnPropertyChanged("GraficoSeleccionado");
            }
        }

        public IEnumerable<PeriodoModel> Periodos { get; private set; }

        private string _minimoPeriodo;
        public string MinimoPeriodo
        {
            get
            {
                return _minimoPeriodo;
            }
            set
            {
                _minimoPeriodo = value;
                OnPropertyChanged("MinimoPeriodo");
            }
        }

        private string _maximoPeriodo;
        public string MaximoPeriodo
        {
            get
            {
                return _maximoPeriodo;
            }
            set
            {
                _maximoPeriodo = value;
                OnPropertyChanged("MaximoPeriodo");
            }
        }

        private IEnumerable<DatosDelGrafico> _itemsDelGrafico;
        public IEnumerable<DatosDelGrafico> ItemsDelGrafico
        {
            get
            {
                if (_itemsDelGrafico == null)
                    _itemsDelGrafico = GetDatosDelGrafico();
                return _itemsDelGrafico;
            }
        }

        private SeleccionDeMediosDePagoViewModel _seleccionDeMediosDePagoVM;
        public SeleccionDeMediosDePagoViewModel SeleccionDeMediosDePagoVM
        {
            get
            {
                if (_seleccionDeMediosDePagoVM == null)
                {
                    _seleccionDeMediosDePagoVM = new SeleccionDeMediosDePagoViewModel(_configuracionM);
                    _seleccionDeMediosDePagoVM.SeleccionDeMediosDePagoCambiada += SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada;
                }
                return _seleccionDeMediosDePagoVM;
            }
        }

        public IDialogCoordinator DialogCoordinator { get; set; }

        private IEnumerable<DatosDelGrafico> GetDatosDelGrafico()
        {
            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            var resumenesPorFecha = _resumenes.Where(a => mediosDePago.Contains(a.Id))
                                              .Where(a => int.Parse(a.Periodo) >= int.Parse(MinimoPeriodo))
                                              .Where(a => int.Parse(a.Periodo) <= int.Parse(MaximoPeriodo));

            var periodos = resumenesPorFecha.Select(a => a.Periodo)
                                            .Distinct()
                                            .OrderBy(a => a);

            var resultado = new List<DatosDelGrafico>();

            switch (GraficoSeleccionado)
            {
                case TOTALES_MENSUAL:
                    {
                        var items = new List<ItemGrafico>();
                        foreach (var periodo in periodos)
                        {
                            items.Add(new ItemGrafico
                            {
                                Descripcion = resumenesPorFecha.FirstOrDefault(a => a.Periodo.Equals(periodo)).TextoPeriodo,
                                Id = periodo,
                                Monto = resumenesPorFecha.Where(a => a.Periodo.Equals(periodo)).Sum(a => a.Total)
                            });
                        }

                        var datosDelGrafico = new DatosDelGrafico
                        {
                            ChartTitle = TOTALES_MENSUAL,
                            ChartSubTitle = "",
                            Items = items
                        };
                        resultado.Add(datosDelGrafico);
                    }
                    break;

                case TOTALES_POR_TARJETA_MENSUAL:
                    {
                        var tipos = resumenesPorFecha.Select(a => a.Descripcion).Distinct();

                        foreach (var tipo in tipos)
                        {
                            var datosDelGrafico = new DatosDelGrafico
                            {
                                ChartTitle = tipo,
                                ChartSubTitle = "",
                            };

                            var resumenesPorTipo = resumenesPorFecha.Where(a => a.Descripcion.Equals(tipo))
                                                             .Where(a => periodos.Contains(a.Periodo))
                                                             .Select(a => new ItemGrafico
                                                             {
                                                                 Id = a.Periodo,
                                                                 Descripcion = a.TextoPeriodo,
                                                                 Monto = a.Total
                                                             })
                                                             .ToList();

                            foreach (var periodo in periodos)
                            {
                                if (!resumenesPorTipo.Any(a => a.Id.Equals(periodo)))
                                {
                                    var month = int.Parse(periodo.Substring(4));
                                    var year = int.Parse(periodo.Substring(0, 4));
                                    var descripcion = new DateTime(year, month, 20).ToString("MMMM");
                                    descripcion = descripcion.Substring(0, 1).ToUpper() + descripcion.Substring(1);
                                    descripcion += " " + year;

                                    var resumenVacio = new ItemGrafico
                                    {
                                        Id = periodo,
                                        Descripcion = descripcion
                                    };
                                    resumenesPorTipo.Add(resumenVacio);
                                }
                            }

                            datosDelGrafico.Items = resumenesPorTipo.OrderBy(a => a.Id).ToList();

                            resultado.Add(datosDelGrafico);
                        }
                    }
                    break;

                case TOTALES_POR_TAGS_MENSUAL:
                    {
                        var movimientos = new List<KeyValuePair<string, IEnumerable<BaseMovimiento>>>();
                        //cargo los resúmenes
                        foreach (var resumen in resumenesPorFecha)
                        {
                            var movimientosPorResumen = _movimientosDA.GetMovimientos(resumen);
                            movimientos.Add(new KeyValuePair<string, IEnumerable<BaseMovimiento>>(resumen.Periodo, movimientosPorResumen));
                        }

                        var tags = movimientos.SelectMany(a => a.Value).SelectMany(b => b.Tags).Distinct();
                        foreach (var tag in tags)
                        {
                            var items = new List<ItemGrafico>();
                            foreach (var periodo in periodos)
                            {
                                var movimientosPorTagDelPeriodo = movimientos.Where(a => a.Key.Equals(periodo))
                                                                             .SelectMany(a => a.Value)
                                                                             .Where(a => a.Tags.Any(b => b.Equals(tag)))
                                                                             .ToList();

                                var month = int.Parse(periodo.Substring(4));
                                var year = int.Parse(periodo.Substring(0, 4));
                                var descripcion = new DateTime(year, month, 20).ToString("MMMM");
                                descripcion = descripcion.Substring(0, 1).ToUpper() + descripcion.Substring(1);
                                descripcion += " " + year;

                                items.Add(new ItemGrafico()
                                {
                                    Id = periodo,
                                    Descripcion = descripcion,
                                    Monto = movimientosPorTagDelPeriodo.Sum(a => a.Monto)
                                });
                            }
                            var datosDelGrafico = new DatosDelGrafico
                            {
                                ChartTitle = tag,
                                ChartSubTitle = "",
                                Items = items
                            };

                            resultado.Add(datosDelGrafico);
                        }

                        //sin tags
                        var itemsSinTags = new List<ItemGrafico>();
                        foreach (var periodo in periodos)
                        {
                            var movimientosSinTagDelPeriodo = movimientos.Where(a => a.Key.Equals(periodo))
                                                                         .SelectMany(a => a.Value)
                                                                         .Where(a => !a.Tags.Any())
                                                                         .ToList();

                            var month = int.Parse(periodo.Substring(4));
                            var year = int.Parse(periodo.Substring(0, 4));
                            var descripcion = new DateTime(year, month, 20).ToString("MMMM");
                            descripcion = descripcion.Substring(0, 1).ToUpper() + descripcion.Substring(1);
                            descripcion += " " + year;

                            itemsSinTags.Add(new ItemGrafico()
                            {
                                Id = periodo,
                                Descripcion = descripcion,
                                Monto = movimientosSinTagDelPeriodo.Sum(a => a.Monto)
                            });
                        }
                        var datosDelGraficoSinCategoria = new DatosDelGrafico
                        {
                            ChartTitle = "Sin categoría",
                            ChartSubTitle = "",
                            Items = itemsSinTags
                        };

                        resultado.Add(datosDelGraficoSinCategoria);
                    }
                    break;

                default:
                    break;
            }

            resultado = resultado.OrderByDescending(a => a.Items.Count())
                                 .OrderByDescending(a => a.Items.Sum(b => b.Monto))
                                 .ToList();

            return resultado;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.Equals("MinimoPeriodo") ||
                propertyName.Equals("MaximoPeriodo") ||
                propertyName.Equals("GraficoSeleccionado"))
            {
                _itemsDelGrafico = null;
                OnPropertyChanged("ItemsDelGrafico");
            }
        }

        private void SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada(object sender, EventArgs e)
        {
            _itemsDelGrafico = null;
            OnPropertyChanged("ItemsDelGrafico");
        }

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            var handler = this.PublishViewModelEvent;
            if (handler != null)
                handler(this, new PublishViewModelEventArgs(viewModel));
        }
    }

    public class DatosDelGrafico
    {
        public DatosDelGrafico()
        {
            ChartTitle = "ChartTitle";
            ChartSubTitle = "ChartSubTitle";
        }
        public string ChartTitle { get; set; }
        public string ChartSubTitle { get; set; }
        public IEnumerable<ItemGrafico> Items { get; set; }
    }

    public class ItemGrafico
    {
        public string Descripcion { get; internal set; }
        public string Id { get; internal set; }
        public decimal Monto { get; internal set; }
    }
}
