namespace Qorith.UI.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Qorith.UI.ViewModels;
using Qorith.Models;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    
    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
    }
    
    private void Window_KeyDown(object sender, KeyEventArgs e)
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
    
    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        TogglePlayPause();
    }
    
    private void TogglePlayPause()
    {
        if (_viewModel == null) return;
        
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
    
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel == null) return;
        
        var dataGrid = sender as DataGrid;
        if (dataGrid?.SelectedItem is Song song)
        {
            _viewModel.PlaySongCommand.Execute(song);
            PlayPauseBtn.Content = "⏸️";
        }
    }
    
    private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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
}
