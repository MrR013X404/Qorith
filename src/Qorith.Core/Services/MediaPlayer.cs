namespace Qorith.Core.Services;

using NAudio.Wave;
using Qorith.Core.Interfaces;
using Qorith.Models;

/// <summary>
/// Robust media player implementation with comprehensive error handling.
/// Supports all Windows versions without crashes.
/// </summary>
public class MediaPlayer : IMediaPlayer, IDisposable
{
    private IWavePlayer? _wavePlayer;
    private AudioFileReader? _audioFileReader;
    private List<Song> _playlist = new();
    private int _currentIndex = -1;
    private RepeatMode _repeatMode = RepeatMode.None;
    private bool _shuffleMode = false;
    private Random _random = new();
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private float _volume = 1.0f;
    private System.Timers.Timer? _positionTimer;
    private bool _isDisposed = false;
    private readonly object _lockObject = new();
    
    public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    public event EventHandler<SongChangedEventArgs>? SongChanged;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    
    public Song? CurrentSong => _currentIndex >= 0 && _currentIndex < _playlist.Count ? _playlist[_currentIndex] : null;
    
    public TimeSpan CurrentPosition
    {
        get
        {
            try
            {
                return _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
        set
        {
            try
            {
                if (_audioFileReader != null)
                    _audioFileReader.CurrentTime = value;
            }
            catch { /* Ignore seek errors */ }
        }
    }
    
    public TimeSpan Duration
    {
        get
        {
            try
            {
                return _audioFileReader?.TotalTime ?? TimeSpan.Zero;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
    
    public PlaybackState PlaybackState
    {
        get => _playbackState;
        private set
        {
            lock (_lockObject)
            {
                if (_playbackState != value)
                {
                    _playbackState = value;
                    PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs { NewState = value });
                }
            }
        }
    }
    
    public float Volume
    {
        get => _volume;
        set
        {
            try
            {
                _volume = Math.Clamp(value, 0f, 1f);
                if (_wavePlayer != null)
                    _wavePlayer.Volume = _volume;
            }
            catch { /* Ignore volume errors */ }
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
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Volume = _volume;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Wave player initialization error: {ex.Message}");
            try
            {
                // Fallback to DirectSound output if WaveOut fails
                _wavePlayer = new DirectSoundOut();
                _wavePlayer.Volume = _volume;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("All audio outputs failed");
            }
        }
    }
    
    public async Task PlayAsync(Song song)
    {
        if (song == null || string.IsNullOrWhiteSpace(song.FilePath))
            return;
        
        try
        {
            if (!File.Exists(song.FilePath))
                return;
            
            await StopAsync();
            
            _playlist = new List<Song> { song };
            _currentIndex = 0;
            
            await PlayCurrentAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Play error: {ex.Message}");
            PlaybackState = PlaybackState.Stopped;
        }
    }
    
    public async Task PlayAsync(List<Song> songs, int startIndex = 0)
    {
        try
        {
            if (!songs?.Any() ?? true || startIndex < 0 || startIndex >= songs.Count)
                return;
            
            await StopAsync();
            
            _playlist = new List<Song>(songs);
            _currentIndex = startIndex;
            
            await PlayCurrentAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Playlist play error: {ex.Message}");
            PlaybackState = PlaybackState.Stopped;
        }
    }
    
    private async Task PlayCurrentAsync()
    {
        if (CurrentSong == null)
            return;
        
        try
        {
            _audioFileReader?.Dispose();
            _audioFileReader = new AudioFileReader(CurrentSong.FilePath)
            {
                Volume = _volume
            };
            
            _wavePlayer?.Init(_audioFileReader);
            _wavePlayer?.Play();
            
            PlaybackState = PlaybackState.Playing;
            SongChanged?.Invoke(this, new SongChangedEventArgs { NewSong = CurrentSong });
            
            StartPositionTimer();
            
            await WaitForPlaybackToFinishAsync();
        }
        catch (FileNotFoundException)
        {
            System.Diagnostics.Debug.WriteLine($"File not found: {CurrentSong?.FilePath}");
            PlaybackState = PlaybackState.Stopped;
            Next();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Playback error: {ex.Message}");
            PlaybackState = PlaybackState.Stopped;
        }
    }
    
    private async Task WaitForPlaybackToFinishAsync()
    {
        try
        {
            while (_wavePlayer?.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }
            
            if (PlaybackState == PlaybackState.Playing)
            {
                PlaybackState = PlaybackState.Stopped;
                await HandlePlaybackFinished();
            }
        }
        catch { /* Ignore wait errors */ }
    }
    
    private async Task HandlePlaybackFinished()
    {
        try
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
        catch { /* Ignore error */ }
    }
    
    public void Pause()
    {
        try
        {
            if (_wavePlayer?.PlaybackState == PlaybackState.Playing)
            {
                _wavePlayer.Pause();
                PlaybackState = PlaybackState.Paused;
                StopPositionTimer();
            }
        }
        catch { /* Ignore pause errors */ }
    }
    
    public void Resume()
    {
        try
        {
            if (_wavePlayer?.PlaybackState == PlaybackState.Paused)
            {
                _wavePlayer.Play();
                PlaybackState = PlaybackState.Playing;
                StartPositionTimer();
            }
        }
        catch { /* Ignore resume errors */ }
    }
    
    public async Task StopAsync()
    {
        try
        {
            StopPositionTimer();
            _wavePlayer?.Stop();
            _audioFileReader?.Dispose();
            _audioFileReader = null;
            PlaybackState = PlaybackState.Stopped;
        }
        catch { /* Ignore stop errors */ }
        
        await Task.CompletedTask;
    }
    
    public void Next()
    {
        try
        {
            if (_playlist.Count == 0) return;
            
            if (_shuffleMode)
                _currentIndex = _random.Next(_playlist.Count);
            else
                _currentIndex = (_currentIndex + 1) % _playlist.Count;
        }
        catch { /* Ignore next errors */ }
    }
    
    public void Previous()
    {
        try
        {
            if (_playlist.Count == 0) return;
            
            if (_shuffleMode)
                _currentIndex = _random.Next(_playlist.Count);
            else
                _currentIndex = (_currentIndex - 1 + _playlist.Count) % _playlist.Count;
        }
        catch { /* Ignore previous errors */ }
    }
    
    public void SetRepeatMode(RepeatMode mode)
    {
        try
        {
            _repeatMode = mode;
        }
        catch { /* Ignore */ }
    }
    
    public void SetShuffleMode(bool enabled)
    {
        try
        {
            _shuffleMode = enabled;
        }
        catch { /* Ignore */ }
    }
    
    private void StartPositionTimer()
    {
        try
        {
            if (_positionTimer == null)
            {
                _positionTimer = new System.Timers.Timer(100);
                _positionTimer.Elapsed += (s, e) =>
                {
                    try
                    {
                        PositionChanged?.Invoke(this, new PositionChangedEventArgs { Position = CurrentPosition });
                    }
                    catch { /* Ignore position update errors */ }
                };
            }
            
            if (!_positionTimer.Enabled)
                _positionTimer.Start();
        }
        catch { /* Ignore timer errors */ }
    }
    
    private void StopPositionTimer()
    {
        try
        {
            if (_positionTimer != null && _positionTimer.Enabled)
                _positionTimer.Stop();
        }
        catch { /* Ignore */ }
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        try
        {
            StopPositionTimer();
            _positionTimer?.Dispose();
            _audioFileReader?.Dispose();
            _wavePlayer?.Dispose();
        }
        catch { /* Ignore disposal errors */ }
        
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
    
    ~MediaPlayer()
    {
        Dispose();
    }
}
