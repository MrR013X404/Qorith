namespace Qorith.Core.Services;

using Qorith.Core.Interfaces;
using Qorith.Models;

/// <summary>
/// Media player implementation using NAudio for playback.
/// Handles playback control, playlist navigation, and audio management.
/// </summary>
public class MediaPlayer : IMediaPlayer, IDisposable
{
    private NAudio.Wave.IWavePlayer? _wavePlayer;
    private NAudio.Wave.AudioFileReader? _audioFileReader;
    private List<Song> _playlist = new();
    private int _currentIndex = -1;
    private RepeatMode _repeatMode = RepeatMode.None;
    private bool _shuffleMode = false;
    private Random _random = new();
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private float _volume = 1.0f;
    private System.Timers.Timer? _positionTimer;
    
    public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    public event EventHandler<SongChangedEventArgs>? SongChanged;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    
    public Song? CurrentSong => _currentIndex >= 0 && _currentIndex < _playlist.Count ? _playlist[_currentIndex] : null;
    
    public TimeSpan CurrentPosition
    {
        get => _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
        set
        {
            if (_audioFileReader != null)
                _audioFileReader.CurrentTime = value;
        }
    }
    
    public TimeSpan Duration => _audioFileReader?.TotalTime ?? TimeSpan.Zero;
    
    public PlaybackState PlaybackState
    {
        get => _playbackState;
        private set
        {
            if (_playbackState != value)
            {
                _playbackState = value;
                PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs { NewState = value });
            }
        }
    }
    
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_wavePlayer != null)
                _wavePlayer.Volume = _volume;
        }
    }
    
    public MediaPlayer()
    {
        InitializePlayer();
    }
    
    private void InitializePlayer()
    {
        try
        {
            _wavePlayer = new NAudio.Wave.WaveOutEvent();
            _wavePlayer.Volume = _volume;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize wave player: {ex.Message}");
        }
    }
    
    public async Task PlayAsync(Song song)
    {
        if (!File.Exists(song.FilePath))
            return;
        
        await StopAsync();
        
        _playlist = new List<Song> { song };
        _currentIndex = 0;
        
        await PlayCurrentAsync();
    }
    
    public async Task PlayAsync(List<Song> songs, int startIndex = 0)
    {
        if (!songs.Any() || startIndex < 0 || startIndex >= songs.Count)
            return;
        
        await StopAsync();
        
        _playlist = new List<Song>(songs);
        _currentIndex = startIndex;
        
        await PlayCurrentAsync();
    }
    
    private async Task PlayCurrentAsync()
    {
        if (CurrentSong == null)
            return;
        
        try
        {
            _audioFileReader = new NAudio.Wave.AudioFileReader(CurrentSong.FilePath)
            {
                Volume = _volume
            };
            
            _wavePlayer?.Init(_audioFileReader);
            _wavePlayer?.Play();
            
            PlaybackState = PlaybackState.Playing;
            SongChanged?.Invoke(this, new SongChangedEventArgs { NewSong = CurrentSong });
            
            StartPositionTimer();
            
            // Wait for playback to finish
            await WaitForPlaybackToFinishAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Playback error: {ex.Message}");
            PlaybackState = PlaybackState.Stopped;
        }
    }
    
    private async Task WaitForPlaybackToFinishAsync()
    {
        while (_wavePlayer?.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
            await Task.Delay(100);
        }
        
        if (PlaybackState == PlaybackState.Playing)
        {
            PlaybackState = PlaybackState.Stopped;
            await HandlePlaybackFinished();
        }
    }
    
    private async Task HandlePlaybackFinished()
    {
        StopPositionTimer();
        
        switch (_repeatMode)
        {
            case RepeatMode.RepeatOne:
                await PlayCurrentAsync();
                break;
            case RepeatMode.RepeatAll:
            case RepeatMode.None:
                Next();
                if (_currentIndex < _playlist.Count)
                    await PlayCurrentAsync();
                break;
        }
    }
    
    public void Pause()
    {
        if (_wavePlayer?.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
            _wavePlayer.Pause();
            PlaybackState = PlaybackState.Paused;
            StopPositionTimer();
        }
    }
    
    public void Resume()
    {
        if (_wavePlayer?.PlaybackState == NAudio.Wave.PlaybackState.Paused)
        {
            _wavePlayer.Play();
            PlaybackState = PlaybackState.Playing;
            StartPositionTimer();
        }
    }
    
    public async Task StopAsync()
    {
        StopPositionTimer();
        _wavePlayer?.Stop();
        _audioFileReader?.Dispose();
        _audioFileReader = null;
        PlaybackState = PlaybackState.Stopped;
        await Task.CompletedTask;
    }
    
    public void Next()
    {
        if (_playlist.Count == 0) return;
        
        if (_shuffleMode)
            _currentIndex = _random.Next(_playlist.Count);
        else
            _currentIndex = (_currentIndex + 1) % _playlist.Count;
    }
    
    public void Previous()
    {
        if (_playlist.Count == 0) return;
        
        if (_shuffleMode)
            _currentIndex = _random.Next(_playlist.Count);
        else
            _currentIndex = (_currentIndex - 1 + _playlist.Count) % _playlist.Count;
    }
    
    public void SetRepeatMode(RepeatMode mode)
    {
        _repeatMode = mode;
    }
    
    public void SetShuffleMode(bool enabled)
    {
        _shuffleMode = enabled;
    }
    
    private void StartPositionTimer()
    {
        if (_positionTimer == null)
        {
            _positionTimer = new System.Timers.Timer(100);
            _positionTimer.Elapsed += (s, e) =>
            {
                PositionChanged?.Invoke(this, new PositionChangedEventArgs { Position = CurrentPosition });
            };
        }
        
        _positionTimer.Start();
    }
    
    private void StopPositionTimer()
    {
        if (_positionTimer != null)
        {
            _positionTimer.Stop();
        }
    }
    
    public void Dispose()
    {
        _positionTimer?.Dispose();
        _audioFileReader?.Dispose();
        _wavePlayer?.Dispose();
    }
}
