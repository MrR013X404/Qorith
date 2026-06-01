namespace Qorith.Core.Interfaces;

using Qorith.Models;

/// <summary>
/// Interface for music library management.
/// </summary>
public interface IMusicLibrary
{
    Task<List<Song>> GetAllSongsAsync();
    
    Task<Song?> GetSongByIdAsync(string id);
    
    Task<List<Song>> SearchSongsAsync(string query);
    
    Task<List<Song>> GetSongsByArtistAsync(string artist);
    
    Task<List<Song>> GetSongsByAlbumAsync(string album);
    
    Task<List<Song>> GetSongsByGenreAsync(string genre);
    
    Task<List<Song>> GetFavoriteSongsAsync();
    
    Task AddSongAsync(Song song);
    
    Task AddSongsAsync(List<Song> songs);
    
    Task UpdateSongAsync(Song song);
    
    Task RemoveSongAsync(string songId);
    
    Task<List<Playlist>> GetAllPlaylistsAsync();
    
    Task<Playlist?> GetPlaylistByIdAsync(string id);
    
    Task CreatePlaylistAsync(Playlist playlist);
    
    Task UpdatePlaylistAsync(Playlist playlist);
    
    Task DeletePlaylistAsync(string playlistId);
    
    Task ScanFolderAsync(string folderPath);
}
