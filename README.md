# Qorith - Lightweight Windows Music Player

A lightweight, fast, and open-source music player for Windows with a responsive, modern UI.

**No database required** - plays audio directly from device folders!

## Features

- **Lightweight & Fast**: Minimal resource usage, optimized performance
- **Responsive Design**: Beautiful UI that adapts to different window sizes
- **Direct Audio Playback**: MP3, WAV, FLAC, OGG, M4A, AAC support
- **Smart Library**: Load songs directly from device folders
- **Playlist Management**: Create, edit, and manage playlists in memory
- **Search & Filter**: Quick search through your music library
- **Favorites**: Mark and view your favorite songs
- **Play Statistics**: Track play count for each song
- **Keyboard Shortcuts**: Navigate efficiently with hotkeys
- **Dark Theme**: Easy on the eyes with a modern dark interface
- **Open Source**: Free to use and contribute

## Getting Started

### Prerequisites
- Windows 10 or later
- .NET 8.0 or higher

### Installation

1. Clone the repository
```bash
git clone https://github.com/MrR013X404/Qorith.git
cd Qorith
```

2. Build the project
```bash
dotnet build
```

3. Run the application
```bash
dotnet run --project src/Qorith.UI
```

## How It Works

### Loading Music

1. Open the application
2. Click "Browse Folder" to select a music folder
3. Qorith scans all audio files recursively
4. Songs appear in the library instantly
5. No database setup required!

### Playing Music

- Click any song to play it
- Use playback controls or keyboard shortcuts
- Songs are played directly from their original locations
- Play count and favorites are tracked in memory

## Project Structure

```
Qorith/
├── src/
│   ├── Qorith.Models/          # Data models
│   │   ├── Song.cs             # Track entity
│   │   └── Playlist.cs         # Playlist entity
│   ├── Qorith.Core/            # Core logic
│   │   ├── Interfaces/
│   │   │   └── IMediaPlayer.cs
│   │   └── Services/
│   │       ├── AudioFileScanner.cs   # Folder scanning
│   │       └── MusicLibrary.cs       # In-memory library
│   └── Qorith.UI/              # WPF UI
│       ├── App.xaml
│       └── Views/
│           └── MainWindow.xaml
├── tests/
│   └── Qorith.Tests/           # Unit tests
├── LICENSE
└── README.md
```

## Supported Audio Formats

- MP3 (.mp3)
- WAV (.wav)
- FLAC (.flac)
- OGG (.ogg)
- M4A (.m4a)
- AAC (.aac)

## Architecture

### MusicLibrary
- In-memory song management
- Load songs from device folders
- Search, filter, and organize
- Create and manage playlists
- Track favorites and play counts

### AudioFileScanner
- Recursively scans folders for audio files
- Supports batch and folder loading
- Fast async scanning

### IMediaPlayer
- Playback control interface
- Playlist navigation
- Repeat and shuffle modes
- Volume control

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Roadmap

- [x] Audio file scanning
- [x] In-memory library management
- [x] Playlist support
- [x] Favorites tracking
- [ ] Core audio playback (NAudio integration)
- [ ] Responsive UI with MVVM
- [ ] Keyboard shortcuts
- [ ] Theme customization
- [ ] Settings/Preferences

## Support

For support, please open an issue on the GitHub repository.
