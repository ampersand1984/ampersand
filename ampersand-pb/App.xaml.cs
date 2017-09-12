using ampersand_pb.ViewModels;
using ampersand_pb.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;

namespace ampersand_pb
{
    public partial class App : Application
    {
        private static Window _window;
        private static MainWindowViewModel _mainWindowVM;
        private static EventHandler _eventHandler;

        protected override void OnStartup(StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox),
                  System.Windows.Controls.TextBox.GotFocusEvent,
                  new RoutedEventHandler(TextBox_GotFocus));

            base.OnStartup(e);
            this.Exit += new ExitEventHandler(App_Exit);
            _eventHandler = new EventHandler(mainWindow_RequestClose);

            _window = new MainWindow()
            {
                //WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };

            _mainWindowVM = new MainWindowViewModel(DialogCoordinator.Instance);

            _mainWindowVM.CloseEvent += _eventHandler;

            _window.DataContext = _mainWindowVM;

            _window.Show();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            _eventHandler = null;
            _mainWindowVM = null;
            _window = null;
        }

        private void mainWindow_RequestClose(object sender, EventArgs e)
        {
            _mainWindowVM.CloseEvent -= _eventHandler;
            _window.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as System.Windows.Controls.TextBox).SelectAll();
        }
    }
}
