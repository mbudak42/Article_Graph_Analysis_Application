using System.Windows;

namespace Article_Graph_Analysis_Application;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show($"FATAL ERROR:\n\n{ex?.Message}\n\nStack:\n{ex?.StackTrace}", 
                           "Uygulama Hatası", 
                           MessageBoxButton.OK, 
                           MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"UI ERROR:\n\n{args.Exception.Message}\n\nStack:\n{args.Exception.StackTrace}", 
                           "Arayüz Hatası", 
                           MessageBoxButton.OK, 
                           MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}