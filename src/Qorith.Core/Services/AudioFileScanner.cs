namespace Qorith.Core.Services;

using System.Collections.Concurrent;
using System.IO;
using Qorith.Models;

/// <summary>
/// Enhanced audio file scanner with modern .NET standards and error handling.
/// Supports all Windows OS versions without crashes.
/// </summary>
public class AudioFileScanner
{
    private static readonly IReadOnlySet<string> SupportedFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".flac", ".ogg", ".m4a", ".aac", ".wma"
    };
    
    /// <summary>
    /// Scans a folder and returns all audio files found with error handling.
    /// </summary>
    public async Task<List<Song>> ScanFolderAsync(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return [];
        }
        
        return await Task.Run(() =>
        {
            var songs = new ConcurrentBag<Song>();
            
            try
            {
                var files = GetAudioFilesRecursive(folderPath);
                
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
                {
                    try
                {
                        // Skip system files and shortcuts
                        var attributes = File.GetAttributes(file);
                        if ((attributes & FileAttributes.System) == FileAttributes.System)
                            return;
                        
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Length == 0)
                            return;
                        
                        var song = new Song
                        {
                            Title = Path.GetFileNameWithoutExtension(file),
                            Artist = "Unknown",
                            FilePath = file,
                            DateAdded = DateTime.Now,
                            Duration = TimeSpan.Zero
                        };
                        
                        songs.Add(song);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip protected files
                    }
                    catch (IOException)
                    {
                        // Skip locked files
                    }
                    catch (Exception)
                    {
                        // Skip any other file read errors
                    }
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Folder access denied - return what we have
            }
            catch (PathTooLongException)
            {
                // Handle paths longer than 260 characters (Windows legacy limit)
            }
            catch (Exception)
            {
                // Return whatever we've scanned so far
            }
            
            return [..songs.OrderBy(s => s.Title)];
        });
    }
    
    /// <summary>
    /// Recursively gets audio files from folder, handling Windows path limitations.
    /// </summary>
    private static List<string> GetAudioFilesRecursive(string folderPath)
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
                            if (SupportedFormats.Contains(Path.GetExtension(file)))
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
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return [];
        }
        
        try
        {
            var files = Directory.GetFiles(folderPath)
                .Where(f => SupportedFormats.Contains(Path.GetExtension(f)))
                .ToList();
            
            var songs = new List<Song>();
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
            
            return songs;
        }
        catch (UnauthorizedAccessException)
        {
            // Folder access denied
            return [];
        }
        catch (PathTooLongException)
        {
            // Path too long
            return [];
        }
        catch (Exception)
        {
            // Other errors
            return [];
        }
    }
    
    /// <summary>
    /// Gets supported audio file formats.
    /// </summary>
    public IReadOnlySet<string> GetSupportedFormats() => SupportedFormats;
}
