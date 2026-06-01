namespace Qorith.Models;

/// <summary>
/// Represents a song/track in the music library.
/// </summary>
public class Song
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string Title { get; set; }
    
    public required string Artist { get; set; }
    
    public string Album { get; set; } = string.Empty;
    
    public required string FilePath { get; set; }
    
    public TimeSpan Duration { get; set; }
    
    public int Year { get; set; }
    
    public string Genre { get; set; } = string.Empty;
    
    public int TrackNumber { get; set; }
    
    public DateTime DateAdded { get; set; } = DateTime.Now;
    
    public int PlayCount { get; set; }
    
    public bool IsFavorite { get; set; }
}
