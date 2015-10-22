using ResumenParser.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ResumenParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void DataGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var row = e.Row;
            var itemSource = dataGrid.ItemsSource;

            //var x = new DataGridRow();
            //x.iss
        }
    }
}
