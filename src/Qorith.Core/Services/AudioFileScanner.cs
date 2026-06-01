namespace Qorith.Core.Services;

using Qorith.Models;
using System.IO;

/// <summary>
/// Service for scanning and loading audio files from device folders.
/// </summary>
public class AudioFileScanner
{
    private static readonly string[] SupportedFormats = { ".mp3", ".wav", ".flac", ".ogg", ".m4a", ".aac" };
    
    /// <summary>
    /// Scans a folder and returns all audio files found.
    /// </summary>
    public async Task<List<Song>> ScanFolderAsync(string folderPath)
    {
        var songs = new List<Song>();
        
        if (!Directory.Exists(folderPath))
            return songs;
        
        return await Task.Run(() =>
        {
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => SupportedFormats.Contains(Path.GetExtension(f).ToLower()))
                .ToList();
            
            foreach (var file in files)
            {
                try
                {
                    var song = new Song
                    {
                        Title = Path.GetFileNameWithoutExtension(file),
                        Artist = "Unknown",
                        FilePath = file,
                        DateAdded = DateTime.Now
                    };
                    
                    songs.Add(song);
                }
                catch (Exception)
                {
                    // Skip files that can't be read
                }
            }
            
            return songs;
        });
    }
    
    /// <summary>
    /// Gets all audio files from a specific folder (non-recursive).
    /// </summary>
    public List<Song> GetFilesFromFolder(string folderPath)
    {
        var songs = new List<Song>();
        
        if (!Directory.Exists(folderPath))
            return songs;
        
        try
        {
            var files = Directory.GetFiles(folderPath, "*.*")
                .Where(f => SupportedFormats.Contains(Path.GetExtension(f).ToLower()))
                .ToList();
            
            foreach (var file in files)
            {
                var song = new Song
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    Artist = "Unknown",
                    FilePath = file,
                    DateAdded = DateTime.Now
                };
                
                songs.Add(song);
            }
        }
        catch (Exception)
        {
            // Return empty list if folder can't be accessed
        }
        
        return songs;
    }
    
    /// <summary>
    /// Gets supported audio file formats.
    /// </summary>
    public string[] GetSupportedFormats() => SupportedFormats;
}
