namespace Qorith.UI;

using System.Windows;
using System.Windows.Input;
using Qorith.UI.Views;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize main window
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
