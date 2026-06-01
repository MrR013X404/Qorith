namespace Qorith.Core.Services;

using NAudio.Wave;
using Qorith.Core.Interfaces;
using Qorith.Models;

/// <summary>
/// Modern media player implementation with comprehensive error handling.
/// Supports all Windows versions without crashes.
/// </summary>
public class MediaPlayer : IMediaPlayer, IAsyncDisposable
{
    private IWavePlayer? _wavePlayer;
    private AudioFileReader? _audioFileReader;
    private List<Song> _playlist = [];
    private int _currentIndex = -1;
    private RepeatMode _repeatMode = RepeatMode.None;
    private bool _shuffleMode;
    private readonly Random _random = new();
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private float _volume = 1.0f;
    private System.Timers.Timer? _positionTimer;
    private bool _isDisposed;
    private readonly ReaderWriterLockSlim _lock = new();
    
    public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    public event EventHandler<SongChangedEventArgs>? SongChanged;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    
    public Song? CurrentSong
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _currentIndex >= 0 && _currentIndex < _playlist.Count ? _playlist[_currentIndex] : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
    
    public TimeSpan CurrentPosition
    {
        get => _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
        set
        {
            try
            {
                if (_audioFileReader is not null)
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
            try
            {
                _volume = Math.Clamp(value, 0f, 1f);
                if (_wavePlayer is not null)
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
            _wavePlayer = new WaveOutEvent { DesiredLatency = 100 };
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
        if (song?.FilePath is null || !File.Exists(song.FilePath))
            return;
        
        try
        {
            await StopAsync();
            
            _lock.EnterWriteLock();
            try
            {
                _playlist = [song];
                _currentIndex = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
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
            if (songs is null || songs.Count == 0 || startIndex < 0 || startIndex >= songs.Count)
                return;
            
            await StopAsync();
            
            _lock.EnterWriteLock();
            try
            {
                _playlist = new List<Song>(songs);
                _currentIndex = startIndex;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
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
        var currentSong = CurrentSong;
        if (currentSong is null)
            return;
        
        try
        {
            _audioFileReader?.Dispose();
            _audioFileReader = new AudioFileReader(currentSong.FilePath)
            {
                Volume = _volume
            };
            
            _wavePlayer?.Init(_audioFileReader);
            _wavePlayer?.Play();
            
            PlaybackState = PlaybackState.Playing;
            SongChanged?.Invoke(this, new SongChangedEventArgs { NewSong = currentSong });
            
            StartPositionTimer();
            
            await WaitForPlaybackToFinishAsync();
        }
        catch (FileNotFoundException)
        {
            System.Diagnostics.Debug.WriteLine($"File not found: {currentSong?.FilePath}");
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
                    _lock.EnterReadLock();
                    try
                    {
                        if (_currentIndex < _playlist.Count)
                            await PlayCurrentAsync();
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
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
        _lock.EnterWriteLock();
        try
        {
            if (_playlist.Count == 0) return;
            
            if (_shuffleMode)
                _currentIndex = _random.Next(_playlist.Count);
            else
                _currentIndex = (_currentIndex + 1) % _playlist.Count;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public void Previous()
    {
        _lock.EnterWriteLock();
        try
        {
            if (_playlist.Count == 0) return;
            
            if (_shuffleMode)
                _currentIndex = _random.Next(_playlist.Count);
            else
                _currentIndex = (_currentIndex - 1 + _playlist.Count) % _playlist.Count;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public void SetRepeatMode(RepeatMode mode) => _repeatMode = mode;
    
    public void SetShuffleMode(bool enabled) => _shuffleMode = enabled;
    
    private void StartPositionTimer()
    {
        try
        {
            _positionTimer ??= new System.Timers.Timer(100) { AutoReset = true };
            
            _positionTimer.Elapsed += (s, e) =>
            {
                try
                {
                    PositionChanged?.Invoke(this, new PositionChangedEventArgs { Position = CurrentPosition });
                }
                catch { /* Ignore position update errors */ }
            };
            
            _positionTimer.Start();
        }
        catch { /* Ignore timer errors */ }
    }
    
    private void StopPositionTimer()
    {
        try
        {
            if (_positionTimer is not null)
                _positionTimer.Stop();
        }
        catch { /* Ignore */ }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        
        try
        {
            StopPositionTimer();
            _positionTimer?.Dispose();
            _audioFileReader?.Dispose();
            _wavePlayer?.Dispose();
            _lock.Dispose();
        }
        catch { /* Ignore disposal errors */ }
        
        _isDisposed = true;
        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }
    
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }
    
    ~MediaPlayer()
    {
        Dispose();
    }
}
