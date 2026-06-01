namespace Qorith.UI.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Input;
using Qorith.Models;
using Qorith.Core.Services;
using Qorith.Core.Interfaces;

/// <summary>
/// ViewModel for the main window with MVVM pattern.
/// Handles UI state, commands, and bindings.
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly MusicLibrary _musicLibrary;
    private readonly MediaPlayer _mediaPlayer;
    private Song? _currentSong;
    private TimeSpan _currentPosition;
    private TimeSpan _duration;
    private float _volume = 0.8f;
    private string _searchQuery = string.Empty;
    private bool _isPlaying;
    private ObservableCollection<Song> _displayedSongs = new();
    private RepeatMode _repeatMode = RepeatMode.None;
    private bool _shuffleEnabled;
    private int _currentSongIndex = -1;
    private string _statusMessage = "Ready";
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // Commands
    public ICommand PlaySongCommand { get; private set; }
    public ICommand PauseCommand { get; private set; }
    public ICommand ResumeCommand { get; private set; }
    public ICommand StopCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand PreviousCommand { get; private set; }
    public ICommand ToggleFavoriteCommand { get; private set; }
    public ICommand BrowseFolderCommand { get; private set; }
    public ICommand ToggleRepeatCommand { get; private set; }
    public ICommand ToggleShuffleCommand { get; private set; }
    public ICommand CreatePlaylistCommand { get; private set; }
    
    // Properties
    public Song? CurrentSong
    {
        get => _currentSong;
        set { _currentSong = value; OnPropertyChanged(); }
    }
    
    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        set { _currentPosition = value; OnPropertyChanged(); }
    }
    
    public TimeSpan Duration
    {
        get => _duration;
        set { _duration = value; OnPropertyChanged(); }
    }
    
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            _mediaPlayer.Volume = _volume;
            OnPropertyChanged();
        }
    }
    
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            FilterAndDisplaySongs();
            OnPropertyChanged();
        }
    }
    
    public bool IsPlaying
    {
        get => _isPlaying;
        set { _isPlaying = value; OnPropertyChanged(); }
    }
    
    public ObservableCollection<Song> DisplayedSongs
    {
        get => _displayedSongs;
        set { _displayedSongs = value; OnPropertyChanged(); }
    }
    
    public RepeatMode RepeatMode
    {
        get => _repeatMode;
        set
        {
            _repeatMode = value;
            _mediaPlayer.SetRepeatMode(value);
            OnPropertyChanged();
        }
    }
    
    public bool ShuffleEnabled
    {
        get => _shuffleEnabled;
        set
        {
            _shuffleEnabled = value;
            _mediaPlayer.SetShuffleMode(value);
            OnPropertyChanged();
        }
    }
    
    public string RepeatModeDisplay => RepeatMode switch
    {
        RepeatMode.None => "Repeat: Off",
        RepeatMode.RepeatOne => "Repeat: One",
        RepeatMode.RepeatAll => "Repeat: All",
        _ => "Repeat: Off"
    };
    
    public string ShuffleDisplay => ShuffleEnabled ? "Shuffle: On" : "Shuffle: Off";
    
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }
    
    public MainWindowViewModel()
    {
        _musicLibrary = new MusicLibrary();
        _mediaPlayer = new MediaPlayer();
        
        InitializeCommands();
        SubscribeToMediaPlayerEvents();
        StatusMessage = "Ready - Click 'Open Folder' to load music";
    }
    
    private void InitializeCommands()
    {
        PlaySongCommand = new RelayCommand(async (param) =>
        {
            if (param is Song song)
            {
                var songs = _musicLibrary.GetAllSongs();
                _currentSongIndex = songs.FindIndex(s => s.Id == song.Id);
                await _mediaPlayer.PlayAsync(songs, Math.Max(0, _currentSongIndex));
                _musicLibrary.IncrementPlayCount(song.Id);
                StatusMessage = $"Playing: {song.Title}";
            }
        });
        
        PauseCommand = new RelayCommand(_ =>
        {
            _mediaPlayer.Pause();
            StatusMessage = "Paused";
        });
        
        ResumeCommand = new RelayCommand(_ =>
        {
            _mediaPlayer.Resume();
            StatusMessage = $"Playing: {CurrentSong?.Title}";
        });
        
        StopCommand = new RelayCommand(async _ =>
        {
            await _mediaPlayer.StopAsync();
            StatusMessage = "Stopped";
        });
        
        NextCommand = new RelayCommand(async _ =>
        {
            _mediaPlayer.Next();
            var songs = _musicLibrary.GetAllSongs();
            if (_currentSongIndex < songs.Count - 1)
            {
                _currentSongIndex++;
                await _mediaPlayer.PlayAsync(songs, _currentSongIndex);
                StatusMessage = $"Playing: {CurrentSong?.Title}";
            }
        });
        
        PreviousCommand = new RelayCommand(async _ =>
        {
            if (_currentSongIndex > 0)
            {
                _currentSongIndex--;
                var songs = _musicLibrary.GetAllSongs();
                await _mediaPlayer.PlayAsync(songs, _currentSongIndex);
                StatusMessage = $"Playing: {CurrentSong?.Title}";
            }
        });
        
        ToggleFavoriteCommand = new RelayCommand((param) =>
        {
            if (param is Song song)
            {
                _musicLibrary.ToggleFavorite(song.Id);
                RefreshDisplayedSongs();
                StatusMessage = song.IsFavorite ? $"Added '{song.Title}' to favorites" : $"Removed '{song.Title}' from favorites";
            }
        });
        
        BrowseFolderCommand = new RelayCommand(async _ => await BrowseAndLoadFolder());
        
        ToggleRepeatCommand = new RelayCommand(_ =>
        {
            RepeatMode = (RepeatMode)(((int)RepeatMode + 1) % 3);
            OnPropertyChanged(nameof(RepeatModeDisplay));
            StatusMessage = RepeatModeDisplay;
        });
        
        ToggleShuffleCommand = new RelayCommand(_ =>
        {
            ShuffleEnabled = !ShuffleEnabled;
            OnPropertyChanged(nameof(ShuffleDisplay));
            StatusMessage = ShuffleDisplay;
        });
        
        CreatePlaylistCommand = new RelayCommand(_ =>
        {
            var playlistName = PromptForInput("Create New Playlist", "Enter playlist name:");
            if (!string.IsNullOrWhiteSpace(playlistName))
            {
                _musicLibrary.CreatePlaylist(playlistName);
                StatusMessage = $"Created playlist: {playlistName}";
            }
        });
    }
    
    private void SubscribeToMediaPlayerEvents()
    {
        _mediaPlayer.PlaybackStateChanged += (s, e) =>
        {
            IsPlaying = e.NewState == PlaybackState.Playing;
        };
        
        _mediaPlayer.SongChanged += (s, e) =>
        {
            CurrentSong = e.NewSong;
            Duration = _mediaPlayer.Duration;
        };
        
        _mediaPlayer.PositionChanged += (s, e) =>
        {
            CurrentPosition = e.Position;
        };
    }
    
    public async Task BrowseAndLoadFolder()
    {
        try
        {
            // Use FolderBrowserDialog from WPF (no external dependency needed)
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder containing music files",
                UseNewFolderButton = false
            };
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    StatusMessage = $"Loading music from {folderDialog.SelectedPath}...";
                    await _musicLibrary.LoadSongsFromFolderAsync(folderDialog.SelectedPath);
                    RefreshDisplayedSongs();
                    StatusMessage = $"Loaded {_musicLibrary.GetSongCount()} songs from {folderDialog.SelectedPath}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading folder: {ex.Message}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening folder dialog: {ex.Message}";
        }
    }
    
    private void RefreshDisplayedSongs()
    {
        var songs = _musicLibrary.GetAllSongs();
        DisplayedSongs = new ObservableCollection<Song>(songs);
        FilterAndDisplaySongs();
    }
    
    private void FilterAndDisplaySongs()
    {
        var allSongs = _musicLibrary.GetAllSongs();
        
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            DisplayedSongs = new ObservableCollection<Song>(allSongs);
        }
        else
        {
            var filtered = _musicLibrary.SearchSongs(SearchQuery);
            DisplayedSongs = new ObservableCollection<Song>(filtered);
        }
    }
    
    private string PromptForInput(string title, string prompt)
    {
        var window = new System.Windows.Window
        {
            Title = title,
            Width = 350,
            Height = 160,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            ResizeMode = System.Windows.ResizeMode.NoResize
        };
        
        var stackPanel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(15) };
        
        var label = new System.Windows.Controls.TextBlock 
        { 
            Text = prompt, 
            Margin = new System.Windows.Thickness(0, 0, 0, 10),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
        };
        stackPanel.Children.Add(label);
        
        var textBox = new System.Windows.Controls.TextBox
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51)),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            Padding = new System.Windows.Thickness(8),
            Margin = new System.Windows.Thickness(0, 0, 0, 15),
            Height = 35,
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 102, 204)),
            BorderThickness = new System.Windows.Thickness(1)
        };
        stackPanel.Children.Add(textBox);
        
        var buttonPanel = new System.Windows.Controls.StackPanel 
        { 
            Orientation = System.Windows.Controls.Orientation.Horizontal, 
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        };
        
        var okButton = new System.Windows.Controls.Button 
        { 
            Content = "OK", 
            Width = 80, 
            Height = 35,
            Margin = new System.Windows.Thickness(5), 
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 102, 204)),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        okButton.Click += (s, e) => { window.DialogResult = true; window.Close(); };
        
        var cancelButton = new System.Windows.Controls.Button 
        { 
            Content = "Cancel", 
            Width = 80, 
            Height = 35,
            Margin = new System.Windows.Thickness(5), 
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)),
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        cancelButton.Click += (s, e) => { window.DialogResult = false; window.Close(); };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stackPanel.Children.Add(buttonPanel);
        
        window.Content = stackPanel;
        
        if (window.ShowDialog() == true)
            return textBox.Text;
        
        return string.Empty;
    }
    
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
