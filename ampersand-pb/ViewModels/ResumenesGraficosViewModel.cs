using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using MahApps.Metro.Controls.Dialogs;
using static ampersand_pb.ViewModels.MovimientosViewModel;

namespace ampersand_pb.ViewModels
{
    public class ResumenesGraficosViewModel : BaseViewModel, IMainWindowItem
    {
        private const string TOTALES_MENSUAL = "Totales mensuales";
        private const string TOTALES_POR_TARJETA_MENSUAL = "Totales mensuales por tarjeta";
        private const string TOTALES_POR_TAGS_MENSUAL = "Totales mensuales por categoría";
        private const string TOTALES_DE_CUOTAS = "Totales mensuales de cuotas";

        public ResumenesGraficosViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            TiposDeGraficos = new List<TipoDeGrafico>()
            {
                new TipoDeGrafico { Descripcion = TOTALES_MENSUAL, Tipo = TiposDeAgrupacion.Totales },
                new TipoDeGrafico { Descripcion = TOTALES_POR_TARJETA_MENSUAL, Tipo = TiposDeAgrupacion.MedioDePago },
                new TipoDeGrafico { Descripcion = TOTALES_POR_TAGS_MENSUAL, Tipo = TiposDeAgrupacion.Tag },
                new TipoDeGrafico { Descripcion = TOTALES_DE_CUOTAS, Tipo = TiposDeAgrupacion.Cuotas }
            };

            _graficoSeleccionado = TiposDeGraficos.ElementAt(1);

            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;

            _resumenes = _movimientosDA.GetResumenes();
            if (_resumenes.Any())
            {
                SetPeriodos();

                _minimoPeriodo = (DateTime.Today.Year - 1) + "01";//Enero del año pasado
                //o junio si ya estamos mes 6
                if (DateTime.Today.Month > 5)
                    _minimoPeriodo = (DateTime.Today.Year - 1) + "06";//Junio del año pasado

                if (int.Parse(_minimoPeriodo) < int.Parse(_resumenes.Min(a => a.Periodo)))
                    _minimoPeriodo = _resumenes.Min(a => a.Periodo);//a menos que no exista

            }
            _maximoPeriodo = _resumenes.Max(a => a.Periodo);
        }

        private void SetPeriodos()
        {
            var periodos = _resumenes.GroupBy(a => a.Periodo)
                    .Select(grp => new PeriodoModel
                    {
                        Periodo = grp.First().Periodo,
                        TextoPeriodo = grp.First().TextoPeriodo
                    }).ToList();

            if (GraficoSeleccionado.Tipo == TiposDeAgrupacion.Cuotas)
            {
                for (int i = 1; i < 13; i++)
                {
                    var fechaMax = (_maximoPeriodo + "01").ToDateTime().AddMonths(i);

                    periodos.Add(new PeriodoModel
                    {
                        Periodo = fechaMax.GetPeriodo(),
                        TextoPeriodo = fechaMax.ToString("MMMM").ToTitle() + " " + fechaMax.Year
                    });
                }
            }
            Periodos = periodos.OrderBy(a => a.Periodo);
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

        public IEnumerable<TipoDeGrafico> TiposDeGraficos { get; private set; }

        private TipoDeGrafico _graficoSeleccionado;
        public TipoDeGrafico GraficoSeleccionado
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

        private bool _incluyeAjenos;
        public bool IncluyeAjenos
        {
            get { return _incluyeAjenos; }
            set { _incluyeAjenos = value; OnPropertyChanged("IncluyeAjenos"); }
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

        private ICommand _mostrarSeleccionCommand;
        public ICommand MostrarSeleccionCommand
        {
            get
            {
                if (_mostrarSeleccionCommand == null)
                    _mostrarSeleccionCommand = new RelayCommand(param => MostrarSeleccionCommandExecute(param as ItemGrafico), param => MostrarSeleccionCommandCanExecute(param as ItemGrafico));
                return _mostrarSeleccionCommand;
            }
        }

        private bool MostrarSeleccionCommandCanExecute(ItemGrafico itemGrafico)
        {
            return itemGrafico != null;
        }

        private void MostrarSeleccionCommandExecute(ItemGrafico itemGrafico)
        {
            switch (GraficoSeleccionado.Tipo)
            {
                case TiposDeAgrupacion.Totales:
                    {
                        var resumenAgrupado = _movimientosDA.GetResumen(itemGrafico.Id);

                        var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM);
                        OnPublishViewModelEvent(movimientosVM);
                    }
                    break;

                case TiposDeAgrupacion.MedioDePago:
                    {
                        var resumenAgrupado = _movimientosDA.GetResumen(itemGrafico.Id);

                        var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM);
                        movimientosVM.GraficosSelectedItem = new AgrupacionItem() { Tipo = GraficoSeleccionado.Tipo, Id = itemGrafico.Grupo };
                        OnPublishViewModelEvent(movimientosVM);
                    }
                    break;

                case TiposDeAgrupacion.Tag:
                    {
                        var resumenAgrupado = _movimientosDA.GetResumen(itemGrafico.Id);

                        var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM);
                        movimientosVM.GraficosSelectedItem = new AgrupacionItem() { Tipo = GraficoSeleccionado.Tipo, Id = itemGrafico.Grupo, Descripcion = itemGrafico.Grupo };
                        OnPublishViewModelEvent(movimientosVM);
                    }
                    break;

                default:
                    break;
            }
        }

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

            SetPeriodos();

            switch (GraficoSeleccionado.Tipo)
            {
                case TiposDeAgrupacion.Totales:
                    {
                        var totales = new List<dynamic>()
                        {
                            new { getTotal = new Func<ResumenModel, decimal>(r => r.GetTotal(IncluyeAjenos)), chartTitle = TOTALES_MENSUAL },
                            new { getTotal = new Func<ResumenModel, decimal>(r => r.GetTotalDeuda(IncluyeAjenos)), chartTitle = "Deuda de tarjetas" },
                            new { getTotal = new Func<ResumenModel, decimal>(r => r.GetTotalSinDeuda(IncluyeAjenos)), chartTitle = TOTALES_MENSUAL + " sin deuda" }
                        };

                        foreach (var tot in totales)
                        {
                            var items = new List<ItemGrafico>();
                            foreach (var periodo in periodos)
                            {
                                items.Add(new ItemGrafico
                                {
                                    Descripcion = resumenesPorFecha.FirstOrDefault(a => a.Periodo.Equals(periodo)).TextoPeriodo,
                                    Id = periodo,
                                    Monto = resumenesPorFecha.Where(a => a.Periodo.Equals(periodo)).Sum(a => (decimal)tot.getTotal(a))
                                });
                            }

                            resultado.Add(new DatosDelGrafico
                            {
                                ChartTitle = tot.chartTitle,
                                ChartSubTitle = "",
                                Items = items
                            });
                        }
                    }
                    break;

                case TiposDeAgrupacion.MedioDePago:
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
                                                                 Monto = a.GetTotal(IncluyeAjenos),
                                                                 Grupo = a.Id
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

                case TiposDeAgrupacion.Tag:
                    {
                        var movimientos = new List<KeyValuePair<string, IEnumerable<BaseMovimiento>>>();
                        //cargo los resúmenes
                        foreach (var resumen in resumenesPorFecha)
                        {
                            var movimientosPorResumen = _movimientosDA.GetMovimientos(resumen);

                            if (!IncluyeAjenos)
                                movimientosPorResumen = movimientosPorResumen.Where(a => !a.EsAjeno);

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
                                    Monto = movimientosPorTagDelPeriodo.Sum(a => a.Monto),
                                    Grupo = tag
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
                                Monto = movimientosSinTagDelPeriodo.Sum(a => a.Monto),
                                Grupo = TagModel.SIN_CATEGORIA
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

                case TiposDeAgrupacion.Cuotas:
                    {
                        _maximoPeriodo = Periodos.Last().Periodo;

                        var movimientos = new Dictionary<string, List<BaseMovimiento>>();
                        //cargo los resúmenes
                        foreach (var resumen in resumenesPorFecha.OrderBy(a => a.Periodo))
                        {
                            var movimientosPorResumen = _movimientosDA.GetMovimientos(resumen).ToList();

                            movimientosPorResumen.RemoveAll(a => a.Cuota.IsNullOrEmpty());

                            if (!IncluyeAjenos)
                                movimientosPorResumen.RemoveAll(a => a.EsAjeno);

                            if (movimientos.Keys.Contains(resumen.Periodo))
                                movimientos[resumen.Periodo].AddRange(movimientosPorResumen);
                            else
                                movimientos[resumen.Periodo] = movimientosPorResumen;
                        }

                        while (movimientos.Keys.Last() != MaximoPeriodo)
                        {
                            var ultimoPeriodo = movimientos.Keys.Last();
                            var nuevoPeriodo = (ultimoPeriodo + "01").ToDateTime().AddMonths(1).GetPeriodo();
                            var nuevosMovimientos = MovimientosViewModel.GetMovimientosProyectados(movimientos[ultimoPeriodo]);

                            movimientos[nuevoPeriodo] = nuevosMovimientos.ToList();
                        }


                        var items = new List<ItemGrafico>();

                        foreach (var periodo in movimientos.Keys)
                        {
                            items.Add(new ItemGrafico
                            {
                                Id = periodo,
                                Descripcion = periodo,
                                Monto = movimientos[periodo].Sum(a => a.Monto),
                                Grupo = periodo
                            });
                        }

                        var datosDelGrafico = new DatosDelGrafico
                        {
                            ChartTitle = "Totales de deuda de tarjetas",
                            ChartSubTitle = "",
                            Items = items
                        };

                        resultado.Add(datosDelGrafico);

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
                propertyName.Equals("GraficoSeleccionado") ||
                propertyName.Equals("IncluyeAjenos"))
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
        public string Grupo { get; internal set; }
    }

    public class TipoDeGrafico
    {
        public TiposDeAgrupacion Tipo { get; set; }

        public string Descripcion { get; set; }
    }
}
