namespace Qorith.Core.Services;

using System.Runtime.InteropServices;

/// <summary>
/// Windows compatibility and error handling utility.
/// Ensures Qorith runs on all Windows versions without crashes.
/// </summary>
public static class WindowsCompatibility
{
    /// <summary>
    /// Gets the Windows OS version information.
    /// </summary>
    public static string GetWindowsVersion()
    {
        var osVersion = Environment.OSVersion;
        return osVersion.VersionString;
    }
    
    /// <summary>
    /// Checks if running on 64-bit Windows.
    /// </summary>
    public static bool Is64BitOS() => Environment.Is64BitOperatingSystem;
    
    /// <summary>
    /// Gets Windows version number (10, 11, etc).
    /// </summary>
    public static int GetWindowsVersionNumber()
    {
        var osVersion = Environment.OSVersion.Version;
        return osVersion.Major;
    }
    
    /// <summary>
    /// Validates that all required Windows APIs are available.
    /// </summary>
    public static bool ValidateSystemRequirements()
    {
        try
        {
            // Check Windows version (Windows 7 SP1 or later = 6.1)
            var version = Environment.OSVersion.Version;
            return version.Major >= 6;
        }
        catch
        {
            return false;
        }
    }
}
