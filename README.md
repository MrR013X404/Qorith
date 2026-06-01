# Qorith - Lightweight Windows Music Player

A lightweight, fast, and open-source music player for Windows with a responsive, modern UI.

## Features

- **Lightweight & Fast**: Minimal resource usage, optimized performance
- **Responsive Design**: Beautiful UI that adapts to different window sizes
- **Format Support**: MP3, WAV, FLAC, OGG, and more
- **Playlist Management**: Create, edit, and save playlists
- **Search & Filter**: Quick search through your music library
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

## Project Structure

```
Qorith/
├── src/
│   ├── Qorith.UI/          # WPF UI layer
│   ├── Qorith.Core/        # Core music player logic
│   └── Qorith.Models/      # Data models
├── tests/
│   └── Qorith.Tests/       # Unit tests
├── docs/                   # Documentation
└── assets/                 # UI assets and icons
```

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

- [ ] Core audio playback
- [ ] Responsive UI framework
- [ ] Playlist management
- [ ] Library browser
- [ ] Search functionality
- [ ] Keyboard shortcuts
- [ ] Settings/Preferences
- [ ] Theme customization

## Support

For support, please open an issue on the GitHub repository.
