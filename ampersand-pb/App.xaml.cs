using System;
using System.Threading;
using System.Windows;
using ampersand_pb.ViewModels;
using ampersand_pb.Views;
using MahApps.Metro.Controls.Dialogs;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ampersand_pb
{
    public partial class App : Application
    {
        private static Window _window;
        private static MainWindowViewModel _mainWindowVM;
        private static EventHandler _eventHandler;

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new ThreadIdEnricher())
                .WriteTo.File($"{System.AppDomain.CurrentDomain.FriendlyName}-{Environment.MachineName}-.log",
                outputTemplate: "{Timestamp:yyyyMMdd HH:mm:ss.fff} ({ThreadId}) [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Application start");

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
            Log.Information("Application end");
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

    class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ThreadId", Thread.CurrentThread.ManagedThreadId));
        }
    }
}
