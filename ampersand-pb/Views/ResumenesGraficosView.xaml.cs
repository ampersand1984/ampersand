using ampersand_pb.ViewModels;
using De.TorstenMandelkow.MetroChart;
using System.Windows.Controls;
using System.Windows.Media;

namespace ampersand_pb.Views
{
    /// <summary>
    /// Interaction logic for ResumenesGraficosView.xaml
    /// </summary>
    public partial class ResumenesGraficosView : UserControl
    {
        public ResumenesGraficosView()
        {
            InitializeComponent();
        }

        private void ChartSeries_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clusteredColumnChart = sender as ClusteredColumnChart;
            var selectedItem = clusteredColumnChart.SelectedItem;

            var resumenesGraficosVM = this.DataContext as ResumenesGraficosViewModel;

            resumenesGraficosVM.MostrarSeleccionCommand.Execute(selectedItem);
        }
    }
}
