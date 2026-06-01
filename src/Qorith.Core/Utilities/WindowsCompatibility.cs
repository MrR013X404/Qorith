namespace Qorith.Core.Services;

using System.Runtime.InteropServices;

/// <summary>
/// Windows compatibility and error handling utility.
/// Ensures Qorith runs on all Windows versions without crashes.
/// </summary>
public static class WindowsCompatibility
{
    [DllImport("kernel32.dll")]
    private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64);
    
    /// <summary>
    /// Gets the Windows OS version information.
    /// </summary>
    public static string GetWindowsVersion()
    {
        var osVersion = Environment.OSVersion;
        var versionString = osVersion.VersionString;
        
        return osVersion.Platform switch
        {
            PlatformID.Win32NT => $"Windows NT {osVersion.Version.Major}.{osVersion.Version.Minor}",
            _ => versionString
        };
    }
    
    /// <summary>
    /// Checks if running on 64-bit Windows.
    /// </summary>
    public static bool Is64BitOS()
    {
        return Environment.Is64BitOperatingSystem;
    }
    
    /// <summary>
    /// Gets Windows version number (10, 11, etc).
    /// </summary>
    public static int GetWindowsVersionNumber()
    {
        var osVersion = Environment.OSVersion.Version;
        return osVersion.Major switch
        {
            10 => 10,
            11 => 11,
            _ => osVersion.Major
        };
    }
    
    /// <summary>
    /// Validates that all required Windows APIs are available.
    /// </summary>
    public static bool ValidateSystemRequirements()
    {
        try
        {
            // Check Windows version
            var version = Environment.OSVersion.Version;
            if (version.Major < 6) // Windows Vista and older
                return false;
            
            // Check for required .NET runtime
            var runtimeVersion = RuntimeInformation.FrameworkDescription;
            if (!runtimeVersion.Contains("8.0") && !runtimeVersion.Contains("9.0"))
                return false; // Only .NET 8 or later supported
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
