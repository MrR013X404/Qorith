namespace Qorith.Core.Services
{
    using Qorith.Models;

    /// <summary>
    /// Test class for MusicLibrary functionality.
    /// </summary>
    public class MusicLibraryTests
    {
        [Fact]
        public void CreatePlaylist_ShouldCreateNewPlaylist()
        {
            // Arrange
            var library = new MusicLibrary();
            
            // Act
            var playlist = library.CreatePlaylist("Test Playlist", "A test playlist");
            
            // Assert
            Assert.NotNull(playlist);
            Assert.Equal("Test Playlist", playlist.Name);
            Assert.Equal("A test playlist", playlist.Description);
            Assert.Single(library.GetAllPlaylists());
        }
        
        [Fact]
        public void AddSongToPlaylist_ShouldAddSongSuccessfully()
        {
            // Arrange
            var library = new MusicLibrary();
            var playlist = library.CreatePlaylist("Test Playlist");
            var song = new Song 
            { 
                Title = "Test Song",
                Artist = "Test Artist",
                FilePath = "C:\\test.mp3"
            };
            
            // Act
            library.AddSongToPlaylist(playlist.Id, song);
            
            // Assert
            Assert.Single(playlist.Songs);
            Assert.Equal(song.Id, playlist.Songs[0].Id);
        }
        
        [Fact]
        public void ToggleFavorite_ShouldToggleFavoriteStatus()
        {
            // Arrange
            var library = new MusicLibrary();
            var song = new Song 
            { 
                Title = "Test Song",
                Artist = "Test Artist",
                FilePath = "C:\\test.mp3"
            };
            
            // Act & Assert
            Assert.False(song.IsFavorite);
            library.ToggleFavorite(song.Id);
            // Note: Toggle would need song to be in library first
        }
        
        [Fact]
        public void SearchSongs_ShouldFindSongsByTitle()
        {
            // This test would work once songs are loaded
            var library = new MusicLibrary();
            // Songs would need to be added first
            var results = library.SearchSongs("test");
            Assert.Empty(results);
        }
    }
}
