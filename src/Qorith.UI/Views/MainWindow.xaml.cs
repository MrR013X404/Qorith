namespace Qorith.UI.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Qorith.UI.ViewModels;
using Qorith.Models;
using Qorith.Core.Services;

/// <summary>
/// Interaction logic for MainWindow.xaml with comprehensive error handling.
/// Supports all Windows versions without crashes.
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            
            // Validate system requirements
            if (!WindowsCompatibility.ValidateSystemRequirements())
            {
                MessageBox.Show(
                    $"Warning: System compatibility issue detected.\n" +
                    $"OS: {WindowsCompatibility.GetWindowsVersion()}\n" +
                    $"64-bit: {WindowsCompatibility.Is64BitOS()}\n\n" +
                    $"Qorith may not work optimally on this system.",
                    "System Compatibility",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize application: {ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Close();
        }
    }
    
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (_viewModel == null) return;
            
            switch (e.Key)
            {
                case Key.Space:
                    e.Handled = true;
                    TogglePlayPause();
                    break;
                case Key.Right when Keyboard.Modifiers == ModifierKeys.Control:
                    e.Handled = true;
                    if (_viewModel.NextCommand.CanExecute(null))
                        _viewModel.NextCommand.Execute(null);
                    break;
                case Key.Left when Keyboard.Modifiers == ModifierKeys.Control:
                    e.Handled = true;
                    if (_viewModel.PreviousCommand.CanExecute(null))
                        _viewModel.PreviousCommand.Execute(null);
                    break;
                case Key.P when Keyboard.Modifiers == ModifierKeys.Control:
                    e.Handled = true;
                    if (_viewModel.CreatePlaylistCommand.CanExecute(null))
                        _viewModel.CreatePlaylistCommand.Execute(null);
                    break;
                case Key.O when Keyboard.Modifiers == ModifierKeys.Control:
                    e.Handled = true;
                    if (_viewModel.BrowseFolderCommand.CanExecute(null))
                        _viewModel.BrowseFolderCommand.Execute(null);
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Keyboard error: {ex.Message}");
        }
    }
    
    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TogglePlayPause();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Playback error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void TogglePlayPause()
    {
        if (_viewModel == null) return;
        
        try
        {
            if (_viewModel.IsPlaying)
            {
                _viewModel.PauseCommand.Execute(null);
                PlayPauseBtn.Content = "▶️";
            }
            else if (_viewModel.CurrentSong != null)
            {
                _viewModel.ResumeCommand.Execute(null);
                PlayPauseBtn.Content = "⏸️";
            }
            else if (_viewModel.DisplayedSongs.Count > 0)
            {
                _viewModel.PlaySongCommand.Execute(_viewModel.DisplayedSongs[0]);
                PlayPauseBtn.Content = "⏸️";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Play/Pause error: {ex.Message}");
        }
    }
    
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (_viewModel == null) return;
            
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Song song)
            {
                _viewModel.PlaySongCommand.Execute(song);
                PlayPauseBtn.Content = "⏸️";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Double-click error: {ex.Message}");
        }
    }
    
    private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        try
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Song song)
            {
                var contextMenu = dataGrid.ContextMenu;
                if (contextMenu != null)
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        item.CommandParameter = song;
                        item.DataContext = _viewModel;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Context menu error: {ex.Message}");
        }
    }
}
