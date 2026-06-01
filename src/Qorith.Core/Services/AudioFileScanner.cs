namespace Qorith.Core.Services;

using System.IO;
using Qorith.Models;

/// <summary>
/// Enhanced audio file scanner with Windows compatibility and error handling.
/// Supports all Windows OS versions without crashes.
/// </summary>
public class AudioFileScanner
{
    private static readonly string[] SupportedFormats = { ".mp3", ".wav", ".flac", ".ogg", ".m4a", ".aac", ".wma" };
    private static readonly object _lockObject = new();
    
    /// <summary>
    /// Scans a folder and returns all audio files found with error handling.
    /// </summary>
    public async Task<List<Song>> ScanFolderAsync(string folderPath)
    {
        var songs = new List<Song>();
        
        if (string.IsNullOrWhiteSpace(folderPath))
            return songs;
        
        if (!Directory.Exists(folderPath))
            return songs;
        
        return await Task.Run(() =>
        {
            try
            {
                var files = GetAudioFilesRecursive(folderPath);
                
                foreach (var file in files)
                {
                    try
                    {
                        // Skip system files and shortcuts
                        var attributes = File.GetAttributes(file);
                        if ((attributes & FileAttributes.System) == FileAttributes.System)
                            continue;
                        
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Length == 0)
                            continue;
                        
                        var song = new Song
                        {
                            Title = Path.GetFileNameWithoutExtension(file),
                            Artist = "Unknown",
                            FilePath = file,
                            DateAdded = DateTime.Now,
                            Duration = TimeSpan.Zero
                        };
                        
                        lock (_lockObject)
                        {
                            songs.Add(song);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip protected files
                        continue;
                    }
                    catch (IOException)
                    {
                        // Skip locked files
                        continue;
                    }
                    catch (Exception)
                    {
                        // Skip any other file read errors
                        continue;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Folder access denied - return what we have
                return songs;
            }
            catch (PathTooLongException)
            {
                // Handle paths longer than 260 characters (Windows legacy limit)
                return songs;
            }
            catch (Exception)
            {
                // Return whatever we've scanned so far
                return songs;
            }
            
            return songs;
        });
    }
    
    /// <summary>
    /// Recursively gets audio files from folder, handling Windows path limitations.
    /// </summary>
    private List<string> GetAudioFilesRecursive(string folderPath)
    {
        var files = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(folderPath);
        
        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            
            try
            {
                // Get files in current directory
                try
                {
                    var dirFiles = Directory.GetFiles(currentPath);
                    foreach (var file in dirFiles)
                    {
                        try
                        {
                            if (SupportedFormats.Contains(Path.GetExtension(file).ToLower()))
                            {
                                files.Add(file);
                            }
                        }
                        catch { /* Skip problematic files */ }
                    }
                }
                catch (UnauthorizedAccessException) { /* Skip inaccessible directories */ }
                catch (PathTooLongException) { /* Skip long paths */ }
                
                // Get subdirectories
                try
                {
                    var subDirs = Directory.GetDirectories(currentPath);
                    foreach (var subDir in subDirs)
                    {
                        try
                        {
                            queue.Enqueue(subDir);
                        }
                        catch { /* Skip problematic subdirectories */ }
                    }
                }
                catch (UnauthorizedAccessException) { /* Skip inaccessible directories */ }
                catch (PathTooLongException) { /* Skip long paths */ }
            }
            catch (Exception)
            {
                // Continue scanning other directories
                continue;
            }
        }
        
        return files;
    }
    
    /// <summary>
    /// Gets all audio files from a specific folder (non-recursive) with error handling.
    /// </summary>
    public List<Song> GetFilesFromFolder(string folderPath)
    {
        var songs = new List<Song>();
        
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return songs;
        
        try
        {
            var files = Directory.GetFiles(folderPath)
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
                catch
                {
                    // Skip problematic files
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Folder access denied
        }
        catch (PathTooLongException)
        {
            // Path too long
        }
        catch (Exception)
        {
            // Other errors
        }
        
        return songs;
    }
    
    /// <summary>
    /// Gets supported audio file formats.
    /// </summary>
    public string[] GetSupportedFormats() => SupportedFormats;
}
