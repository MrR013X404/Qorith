namespace Qorith.Core.Interfaces;

using Qorith.Models;

/// <summary>
/// Interface for media playback control.
/// </summary>
public interface IMediaPlayer
{
    event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    event EventHandler<SongChangedEventArgs>? SongChanged;
    event EventHandler<PositionChangedEventArgs>? PositionChanged;
    
    Song? CurrentSong { get; }
    
    TimeSpan CurrentPosition { get; set; }
    
    TimeSpan Duration { get; }
    
    PlaybackState PlaybackState { get; }
    
    float Volume { get; set; }
    
    Task PlayAsync(Song song);
    
    Task PlayAsync(List<Song> songs, int startIndex = 0);
    
    void Pause();
    
    void Resume();
    
    void Stop();
    
    void Next();
    
    void Previous();
    
    void SetRepeatMode(RepeatMode mode);
    
    void SetShuffleMode(bool enabled);
}

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}

public enum RepeatMode
{
    None,
    RepeatOne,
    RepeatAll
}

public class PlaybackStateChangedEventArgs : EventArgs
{
    public required PlaybackState NewState { get; set; }
}

public class SongChangedEventArgs : EventArgs
{
    public required Song NewSong { get; set; }
}

public class PositionChangedEventArgs : EventArgs
{
    public required TimeSpan Position { get; set; }
}
