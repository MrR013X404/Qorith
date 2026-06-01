namespace Qorith.UI.ViewModels;

using System.Windows.Input;
using System.Collections.ObjectModel;
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
    private float _volume = 1.0f;
    private string _searchQuery = string.Empty;
    private bool _isPlaying = false;
    private ObservableCollection<Song> _songs = new();
    private ObservableCollection<Song> _playlist = new();
    private RepeatMode _repeatMode = RepeatMode.None;
    private bool _shuffleEnabled = false;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // Commands
    public ICommand PlayCommand { get; private set; }
    public ICommand PauseCommand { get; private set; }
    public ICommand ResumeCommand { get; private set; }
    public ICommand StopCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand PreviousCommand { get; private set; }
    public ICommand ToggleFavoriteCommand { get; private set; }
    public ICommand BrowseFolderCommand { get; private set; }
    public ICommand ToggleRepeatCommand { get; private set; }
    public ICommand ToggleShuffleCommand { get; private set; }
    
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
            _volume = value;
            _mediaPlayer.Volume = value;
            OnPropertyChanged();
        }
    }
    
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            FilterSongs();
            OnPropertyChanged();
        }
    }
    
    public bool IsPlaying
    {
        get => _isPlaying;
        set { _isPlaying = value; OnPropertyChanged(); }
    }
    
    public ObservableCollection<Song> Songs
    {
        get => _songs;
        set { _songs = value; OnPropertyChanged(); }
    }
    
    public ObservableCollection<Song> Playlist
    {
        get => _playlist;
        set { _playlist = value; OnPropertyChanged(); }
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
    
    public MainWindowViewModel()
    {
        _musicLibrary = new MusicLibrary();
        _mediaPlayer = new MediaPlayer();
        
        InitializeCommands();
        SubscribeToMediaPlayerEvents();
    }
    
    private void InitializeCommands()
    {
        PlayCommand = new RelayCommand(async (param) =>
        {
            if (param is Song song)
                await _mediaPlayer.PlayAsync(song);
        });
        
        PauseCommand = new RelayCommand(_ => _mediaPlayer.Pause());
        ResumeCommand = new RelayCommand(_ => _mediaPlayer.Resume());
        StopCommand = new RelayCommand(async _ => await _mediaPlayer.StopAsync());
        NextCommand = new RelayCommand(_ => _mediaPlayer.Next());
        PreviousCommand = new RelayCommand(_ => _mediaPlayer.Previous());
        
        ToggleFavoriteCommand = new RelayCommand((param) =>
        {
            if (param is Song song)
            {
                _musicLibrary.ToggleFavorite(song.Id);
                OnPropertyChanged(nameof(Songs));
            }
        });
        
        BrowseFolderCommand = new RelayCommand(async _ => await BrowseAndLoadFolder());
        
        ToggleRepeatCommand = new RelayCommand(_ =>
        {
            RepeatMode = (RepeatMode)(((int)RepeatMode + 1) % 3);
        });
        
        ToggleShuffleCommand = new RelayCommand(_ =>
        {
            ShuffleEnabled = !ShuffleEnabled;
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
        // This will be called from the UI with folder path
        // For now, it's a placeholder
        await Task.CompletedTask;
    }
    
    public async Task LoadMusicFromFolder(string folderPath)
    {
        await _musicLibrary.LoadSongsFromFolderAsync(folderPath);
        RefreshSongList();
    }
    
    private void RefreshSongList()
    {
        var songs = _musicLibrary.GetAllSongs();
        Songs = new ObservableCollection<Song>(songs);
    }
    
    private void FilterSongs()
    {
        var allSongs = _musicLibrary.GetAllSongs();
        
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            Songs = new ObservableCollection<Song>(allSongs);
        }
        else
        {
            var filtered = _musicLibrary.SearchSongs(SearchQuery);
            Songs = new ObservableCollection<Song>(filtered);
        }
    }
    
    public string GetFormattedTime(TimeSpan time)
    {
        return $"{time.Minutes:D2}:{time.Seconds:D2}";
    }
    
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
