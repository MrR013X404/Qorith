namespace Qorith.Models;

/// <summary>
/// Represents a playlist containing multiple songs.
/// </summary>
public class Playlist
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string Name { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public List<Song> Songs { get; set; } = new();
    
    public DateTime Created { get; set; } = DateTime.Now;
    
    public DateTime Modified { get; set; } = DateTime.Now;
    
    public int SongCount => Songs.Count;
    
    public TimeSpan TotalDuration => TimeSpan.FromSeconds(Songs.Sum(s => s.Duration.TotalSeconds));
}
