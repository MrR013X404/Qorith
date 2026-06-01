namespace Qorith.Core.Services;

using Qorith.Models;

/// <summary>
/// In-memory music library that loads and manages songs from device folders.
/// No database - all songs loaded from files only.
/// </summary>
public class MusicLibrary
{
    private readonly AudioFileScanner _scanner;
    private List<Song> _allSongs = new();
    private List<Playlist> _playlists = new();
    private Song? _currentSong;
    
    public MusicLibrary()
    {
        _scanner = new AudioFileScanner();
    }
    
    /// <summary>
    /// Loads all songs from specified folder and subfolders.
    /// </summary>
    public async Task LoadSongsFromFolderAsync(string folderPath)
    {
        _allSongs = await _scanner.ScanFolderAsync(folderPath);
    }
    
    /// <summary>
    /// Loads songs from multiple folders.
    /// </summary>
    public async Task LoadSongsFromFoldersAsync(params string[] folderPaths)
    {
        var allSongs = new List<Song>();
        
        foreach (var path in folderPaths)
        {
            var songs = await _scanner.ScanFolderAsync(path);
            allSongs.AddRange(songs);
        }
        
        _allSongs = allSongs;
    }
    
    /// <summary>
    /// Gets all loaded songs.
    /// </summary>
    public List<Song> GetAllSongs() => new(_allSongs);
    
    /// <summary>
    /// Searches songs by title or artist (case-insensitive).
    /// </summary>
    public List<Song> SearchSongs(string query)
    {
        var lowerQuery = query.ToLower();
        return _allSongs
            .Where(s => s.Title.ToLower().Contains(lowerQuery) || 
                       s.Artist.ToLower().Contains(lowerQuery))
            .ToList();
    }
    
    /// <summary>
    /// Gets all unique artists from loaded songs.
    /// </summary>
    public List<string> GetAllArtists()
    {
        return _allSongs
            .Select(s => s.Artist)
            .Distinct()
            .OrderBy(a => a)
            .ToList();
    }
    
    /// <summary>
    /// Gets all unique albums from loaded songs.
    /// </summary>
    public List<string> GetAllAlbums()
    {
        return _allSongs
            .Select(s => s.Album)
            .Where(a => !string.IsNullOrEmpty(a))
            .Distinct()
            .OrderBy(a => a)
            .ToList();
    }
    
    /// <summary>
    /// Gets songs by artist.
    /// </summary>
    public List<Song> GetSongsByArtist(string artist)
    {
        return _allSongs.Where(s => s.Artist == artist).ToList();
    }
    
    /// <summary>
    /// Gets songs by album.
    /// </summary>
    public List<Song> GetSongsByAlbum(string album)
    {
        return _allSongs.Where(s => s.Album == album).ToList();
    }
    
    /// <summary>
    /// Gets all favorite songs.
    /// </summary>
    public List<Song> GetFavoriteSongs()
    {
        return _allSongs.Where(s => s.IsFavorite).ToList();
    }
    
    /// <summary>
    /// Toggles favorite status for a song.
    /// </summary>
    public void ToggleFavorite(string songId)
    {
        var song = _allSongs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            song.IsFavorite = !song.IsFavorite;
        }
    }
    
    /// <summary>
    /// Increments play count for a song.
    /// </summary>
    public void IncrementPlayCount(string songId)
    {
        var song = _allSongs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            song.PlayCount++;
        }
    }
    
    /// <summary>
    /// Gets the most played songs.
    /// </summary>
    public List<Song> GetMostPlayedSongs(int count = 10)
    {
        return _allSongs
            .OrderByDescending(s => s.PlayCount)
            .Take(count)
            .ToList();
    }
    
    /// <summary>
    /// Creates a new playlist in memory.
    /// </summary>
    public Playlist CreatePlaylist(string name, string description = "")
    {
        var playlist = new Playlist 
        { 
            Name = name,
            Description = description
        };
        
        _playlists.Add(playlist);
        return playlist;
    }
    
    /// <summary>
    /// Gets all playlists.
    /// </summary>
    public List<Playlist> GetAllPlaylists() => new(_playlists);
    
    /// <summary>
    /// Adds song to playlist.
    /// </summary>
    public void AddSongToPlaylist(string playlistId, Song song)
    {
        var playlist = _playlists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist != null && !playlist.Songs.Any(s => s.Id == song.Id))
        {
            playlist.Songs.Add(song);
            playlist.Modified = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Removes song from playlist.
    /// </summary>
    public void RemoveSongFromPlaylist(string playlistId, string songId)
    {
        var playlist = _playlists.FirstOrDefault(p => p.Id == playlistId);
        if (playlist != null)
        {
            var song = playlist.Songs.FirstOrDefault(s => s.Id == songId);
            if (song != null)
            {
                playlist.Songs.Remove(song);
                playlist.Modified = DateTime.Now;
            }
        }
    }
    
    /// <summary>
    /// Gets song count.
    /// </summary>
    public int GetSongCount() => _allSongs.Count;
}
