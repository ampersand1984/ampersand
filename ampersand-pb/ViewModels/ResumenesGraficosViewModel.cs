using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using ampersand_pb.Properties;
using MahApps.Metro.Controls.Dialogs;
using static ampersand_pb.ViewModels.MovimientosViewModel;

namespace ampersand_pb.ViewModels
{
    public class ResumenesGraficosViewModel : BaseViewModel, IMainWindowItem
    {
        private const string TOTALES_MENSUAL = "Totales mensuales";
        private const string TOTALES_POR_TARJETA_MENSUAL = "Totales mensuales por tarjeta";
        private const string TOTALES_POR_TAGS_MENSUAL = "Totales mensuales por categoría";
        private const string MOVIMIENTOS_POR_TAGS_SEMANAL = "Movimientos semanales por categoría";
        private const string TOTALES_DE_CUOTAS = "Totales mensuales de cuotas";
        private const string DIFERENCIA_CON_MES_ANTERIOR = "Diferencia con mes anterior por categoría";

        public ResumenesGraficosViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            TiposDeGraficos = new List<TipoDeGrafico>()
            {
                new TipoDeGrafico { Descripcion = TOTALES_MENSUAL, Tipo = TiposDeAgrupacion.Totales },
                new TipoDeGrafico { Descripcion = TOTALES_POR_TARJETA_MENSUAL, Tipo = TiposDeAgrupacion.MedioDePago },
                new TipoDeGrafico { Descripcion = TOTALES_POR_TAGS_MENSUAL, Tipo = TiposDeAgrupacion.Tag },
                //new TipoDeGrafico { Descripcion = MOVIMIENTOS_POR_TAGS_SEMANAL, Tipo = TiposDeAgrupacion.TagSemanales },
                new TipoDeGrafico { Descripcion = TOTALES_DE_CUOTAS, Tipo = TiposDeAgrupacion.Cuotas },
                new TipoDeGrafico { Descripcion = DIFERENCIA_CON_MES_ANTERIOR, Tipo = TiposDeAgrupacion.DiferenciaConMesAnterior }
            };

            _incluyeAjenos = Settings.Default.IncluirMovimientosAjenos;
            _graficoSeleccionado = (TiposDeAgrupacion)Settings.Default.TipoDeGraficoEnResumen;

            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;
        }

        private void SetPeriodos()
        {
            var periodos = _resumenes.GroupBy(a => a.Periodo)
                    .Select(grp => new PeriodoModel
                    {
                        Periodo = grp.First().Periodo,
                        TextoPeriodo = grp.First().TextoPeriodo
                    }).ToList();

            if (GraficoSeleccionado == TiposDeAgrupacion.Cuotas)
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

        private TiposDeAgrupacion _graficoSeleccionado;
        public TiposDeAgrupacion GraficoSeleccionado
        {
            get
            {
                return _graficoSeleccionado;
            }
            set
            {
                _graficoSeleccionado = value;
                _itemsDelGrafico = null;
                OnPropertyChanged("GraficoSeleccionado");
                OnPropertyChanged("ItemsDelGrafico");
                OnPropertyChanged("MostrarTags");
                OnPropertyChanged("MostrarDiferenciaConMesAnterior");
                OnPropertyChanged("MostrarPeriodos");
                ActualizarGraficoSettings();
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
            set
            {
                _incluyeAjenos = value;
                OnPropertyChanged("IncluyeAjenos");
            }
        }

        private IEnumerable<string> _tags;
        public IEnumerable<string> Tags
        {
            get
            {
                if (_tags == null)
                    _tags = _configuracionM.Tags.Select(a => a.Tag).ToList();
                return _tags;
            }
        }

        private string _tagSeleccionado;
        public string TagSeleccionado
        {
            get { return _tagSeleccionado ?? (_tagSeleccionado = Tags.First()); }
            set { _tagSeleccionado = value; OnPropertyChanged("TagSeleccionado"); }
        }

        public bool MostrarTags
        {
            get { return GraficoSeleccionado == TiposDeAgrupacion.TagSemanales; }
        }

        public bool MostrarDiferenciaConMesAnterior
        {
            get { return GraficoSeleccionado == TiposDeAgrupacion.DiferenciaConMesAnterior; }
        }

        public bool MostrarPeriodos
        {
            get { return GraficoSeleccionado != TiposDeAgrupacion.DiferenciaConMesAnterior; }
        }

        public string TotalDiferencia
        {
            get
            {
                return GraficoSeleccionado == TiposDeAgrupacion.DiferenciaConMesAnterior && _itemsDelGrafico != null ?
                    "$" + ItemsDelGrafico.Sum(a => a.Items.ElementAt(1).Monto - a.Items.ElementAt(0).Monto).ToString("N0") :
                    "";
            }
        }

        private IEnumerable<DatosDelGrafico> _itemsDelGrafico;
        public IEnumerable<DatosDelGrafico> ItemsDelGrafico
        {
            get
            {
                if (_itemsDelGrafico == null)
                    CargarItemsDelGrafico();
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

        private void ActualizarGraficoSettings()
        {
            var tipoDeGrafiSeleccionado = (int)GraficoSeleccionado;

            var actualizar = Settings.Default.TipoDeGraficoEnResumen != tipoDeGrafiSeleccionado ||
                             Settings.Default.IncluirMovimientosAjenos != _incluyeAjenos;

            if (actualizar)
            {
                Settings.Default.IncluirMovimientosAjenos = _incluyeAjenos;
                Settings.Default.TipoDeGraficoEnResumen = tipoDeGrafiSeleccionado;
                Settings.Default.Save();
            }
        }

        private bool MostrarSeleccionCommandCanExecute(ItemGrafico itemGrafico)
        {
            return itemGrafico != null;
        }

        private void MostrarSeleccionCommandExecute(ItemGrafico itemGrafico)
        {
            switch (GraficoSeleccionado)
            {
                case TiposDeAgrupacion.Totales:
                    {
                        var resumenAgrupado = _movimientosDA.GetResumen(itemGrafico.Id);

                        var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM) { DialogCoordinator = DialogCoordinator };
                        OnPublishViewModelEvent(movimientosVM);
                    }
                    break;

                case TiposDeAgrupacion.MedioDePago:
                    {
                        var resumenAgrupado = _movimientosDA.GetResumen(itemGrafico.Id);

                        var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM) { DialogCoordinator = DialogCoordinator };
                        movimientosVM.GraficosSelectedItem = new AgrupacionItem() { Tipo = GraficoSeleccionado, Id = itemGrafico.Grupo };
                        OnPublishViewModelEvent(movimientosVM);
                    }
                    break;

                case TiposDeAgrupacion.Tag:
                    {
                        var resumenAgrupado = _movimientosDA.GetResumen(itemGrafico.Id);

                        var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM) { DialogCoordinator = DialogCoordinator };
                        movimientosVM.GraficosSelectedItem = new AgrupacionItem() { Tipo = GraficoSeleccionado, Id = itemGrafico.Grupo, Descripcion = itemGrafico.Grupo };
                        OnPublishViewModelEvent(movimientosVM);
                    }
                    break;

                default:
                    break;
            }
        }

        private void CargarItemsDelGrafico()
        {
            var task = new Task<IEnumerable<DatosDelGrafico>>(() =>
            {
                if (_resumenes == null)
                    CargarResumenes();

                var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

                var resumenesPorFecha = _resumenes.Where(a => mediosDePago.Contains(a.Id))
                                                  .Where(a => int.Parse(a.Periodo) >= int.Parse(MinimoPeriodo))
                                                  .Where(a => int.Parse(a.Periodo) <= int.Parse(MaximoPeriodo))
                                                  .ToList();

                var periodos = resumenesPorFecha.Select(a => a.Periodo)
                                                .Distinct()
                                                .OrderBy(a => a)
                                                .ToList();

                var resultado = new List<DatosDelGrafico>();

                SetPeriodos();

                switch (GraficoSeleccionado)
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

                    case TiposDeAgrupacion.DiferenciaConMesAnterior:
                    case TiposDeAgrupacion.Tag:
                        {
                            if (GraficoSeleccionado == TiposDeAgrupacion.DiferenciaConMesAnterior)
                            {
                                periodos = periodos.Where(a => a.Equals(periodos.Last()) || a.Equals(periodos.ElementAt(periodos.Count - 2))).ToList();

                                resumenesPorFecha = resumenesPorFecha.Where(a => periodos.Contains(a.Periodo)).ToList();
                            }

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

                                var datosDelGrafico = GetDatosDelGrafico(tag, items, GraficoSeleccionado);

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
                            var datosDelGraficoSinCategoria = GetDatosDelGrafico("Sin categoría", itemsSinTags, GraficoSeleccionado);

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

                    //case TiposDeAgrupacion.TagSemanales:
                    //    {
                    //        var movimientos = new List<KeyValuePair<string, IEnumerable<BaseMovimiento>>>();
                    //        //cargo los resúmenes
                    //        foreach (var resumen in resumenesPorFecha)
                    //        {
                    //            var movimientosPorResumen = _movimientosDA.GetMovimientos(resumen);

                    //            movimientosPorResumen = movimientosPorResumen.Where(a => a.Tags.Contains(TagSeleccionado));

                    //            if (!IncluyeAjenos)
                    //                movimientosPorResumen = movimientosPorResumen.Where(a => !a.EsAjeno);

                    //            movimientos.Add(new KeyValuePair<string, IEnumerable<BaseMovimiento>>(resumen.Periodo, movimientosPorResumen));
                    //        }

                    //        var items = new List<ItemGrafico>();
                    //        foreach (var periodo in periodos)
                    //        {
                    //            var movimientosDelPeriodo = movimientos.Where(a => a.Key.Equals(periodo))
                    //                                                         .SelectMany(a => a.Value)
                    //                                                         .ToList();
                    //            //armo las fechas
                    //            var primerFecha = movimientosDelPeriodo.First().Fecha;
                    //            while (primerFecha.DayOfWeek != DayOfWeek.Friday)
                    //                primerFecha = primerFecha.AddDays(-1);



                    //            items.Add(new ItemGrafico()
                    //            {
                    //                Id = periodo,
                    //                Descripcion = periodo,
                    //                Monto = movimientosDelPeriodo.Sum(a => a.Monto),
                    //                Grupo = "tag"
                    //            });

                    //            var datosDelGrafico = new DatosDelGrafico
                    //            {
                    //                ChartTitle = "tag",
                    //                ChartSubTitle = "",
                    //                Items = items
                    //            };

                    //            resultado.Add(datosDelGrafico);
                    //        }
                    //    }
                    //    break;

                    default:
                        break;
                }

                resultado = GraficoSeleccionado != TiposDeAgrupacion.DiferenciaConMesAnterior ?
                    resultado.OrderByDescending(a => a.Items.Count())
                             .OrderByDescending(a => a.Items.Sum(b => b.Monto))
                             .ToList() :
                    resultado.Cast<DatosDelGraficoDiferencial>().OrderByDescending(a => Math.Abs(a.Diferencia))
                             .ToList<DatosDelGrafico>();

                return resultado;
            });
            task.ContinueWith(t =>
            {
                _itemsDelGrafico = t.Result;
                OnPropertyChanged("ItemsDelGrafico");
                OnPropertyChanged("TotalDiferencia");
            });

            task.Start();
        }

        private void CargarResumenes()
        {
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

        private static DatosDelGrafico GetDatosDelGrafico(string chartTitle, List<ItemGrafico> items, TiposDeAgrupacion graficoSeleccionado)
        {
            if (graficoSeleccionado == TiposDeAgrupacion.DiferenciaConMesAnterior)
            {
                return new DatosDelGraficoDiferencial
                {
                    ChartTitle = chartTitle,
                    ChartSubTitle = "",
                    Items = items
                };
            }
            else
                return new DatosDelGrafico
                {
                    ChartTitle = chartTitle,
                    ChartSubTitle = "",
                    Items = items
                };
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
                OnPropertyChanged("TotalDiferencia");
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

    public class DatosDelGraficoDiferencial : DatosDelGrafico
    {

        public string DiferenciaMonto
        {
            get
            {
                var signo = Diferencia > 0 ? " " : "";

                return ("$" + signo + Diferencia.ToString("N0")).Replace(",", "").PadLeft(10);
            }
        }

        public bool SeGastoMenos
        {
            get
            {
                return Diferencia <= 0;
            }
        }

        public decimal Diferencia
        {
            get
            {
                return Items.ElementAt(1).Monto - Items.ElementAt(0).Monto;
            }
        }

        public string DiferenciaPorcentaje
        {
            get
            {
                var result = string.Empty;

                if (Items.ElementAt(0).Monto > 0)
                {
                    var diferenciaPorcentaje = ((Items.ElementAt(1).Monto * 100) / Items.ElementAt(0).Monto);
                    diferenciaPorcentaje = diferenciaPorcentaje - 100;

                    var strDiferenciaPorcentaje = diferenciaPorcentaje.ToString("N0");

                    if (diferenciaPorcentaje != 0 && Math.Abs(diferenciaPorcentaje) > 10)
                    {
                        var signo = diferenciaPorcentaje > 0 ? "+" : "";
                        result = $"{signo}{strDiferenciaPorcentaje} %";
                    }
                    else
                        result = "~ 0 %";
                }
                else
                    result = "100 %";

                return result.PadLeft(10);
            }
        }
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
