using System.Runtime.InteropServices;

namespace AtelierResleriana.Windows
{
    public static class KnownFolders
    {
        /// <summary>
        /// %LOCALAPPDATA%\Desktop
        /// </summary>
        public static string AppDataDesktop { get => GetKnownFolderPath(KnownFolderIds.AppDataDesktop); }

        /// <summary>
        /// %LOCALAPPDATA%\Documents
        /// </summary>
        public static string AppDataDocuments { get => GetKnownFolderPath(KnownFolderIds.AppDataDocuments); }

        /// <summary>
        /// %LOCALAPPDATA%\Favorites
        /// </summary>
        public static string AppDataFavorites { get => GetKnownFolderPath(KnownFolderIds.AppDataFavorites); }

        /// <summary>
        /// %LOCALAPPDATA%\ProgramData
        /// </summary>
        public static string AppDataProgramData { get => GetKnownFolderPath(KnownFolderIds.AppDataProgramData); }

        /// <summary>
        /// %ALLUSERSPROFILE%\OEM Links
        /// </summary>
        public static string CommonOEMLinks { get => GetKnownFolderPath(KnownFolderIds.CommonOEMLinks); }

        /// <summary>
        /// %ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs
        /// </summary>
        public static string CommonPrograms { get => GetKnownFolderPath(KnownFolderIds.CommonPrograms); }

        /// <summary>
        /// %ALLUSERSPROFILE%\Microsoft\Windows\Start Menu
        /// </summary>
        public static string CommonStartMenu { get => GetKnownFolderPath(KnownFolderIds.CommonStartMenu); }

        /// <summary>
        /// %ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs\StartUp
        /// </summary>
        public static string CommonStartup { get => GetKnownFolderPath(KnownFolderIds.CommonStartup); }

        /// <summary>
        /// %APPDATA%\Microsoft\Windows\Cookies
        /// </summary>
        public static string Cookies { get => GetKnownFolderPath(KnownFolderIds.Cookies); }

        /// <summary>
        /// %USERPROFILE%\Desktop
        /// </summary>
        public static string Desktop { get => GetKnownFolderPath(KnownFolderIds.Desktop); }

        /// <summary>
        /// %ALLUSERSPROFILE%\Microsoft\Windows\DeviceMetadataStore
        /// </summary>
        public static string DeviceMetadataStore { get => GetKnownFolderPath(KnownFolderIds.DeviceMetadataStore); }

        /// <summary>
        /// %USERPROFILE%\Documents
        /// </summary>
        public static string Documents { get => GetKnownFolderPath(KnownFolderIds.Documents); }

        /// <summary>
        /// %APPDATA%\Microsoft\Windows\Libraries\Documents.library-ms
        /// </summary>
        public static string DocumentsLibrary { get => GetKnownFolderPath(KnownFolderIds.DocumentsLibrary); }

        /// <summary>
        /// %USERPROFILE%\Downloads
        /// </summary>
        public static string Downloads { get => GetKnownFolderPath(KnownFolderIds.Downloads); }

        /// <summary>
        /// %USERPROFILE%\Favorites
        /// </summary>
        public static string Favorites { get => GetKnownFolderPath(KnownFolderIds.Favorites); }

        /// <summary>
        /// %windir%\Fonts
        /// </summary>
        public static string Fonts { get => GetKnownFolderPath(KnownFolderIds.Fonts); }

        /// <summary>
        /// %APPDATA%\Microsoft\Windows\Libraries
        /// </summary>
        public static string Libraries { get => GetKnownFolderPath(KnownFolderIds.Libraries); }

        /// <summary>
        /// %USERPROFILE%\Links
        /// </summary>
        public static string Links { get => GetKnownFolderPath(KnownFolderIds.Links); }

        /// <summary>
        /// %LOCALAPPDATA% (%USERPROFILE%\AppData\Local)
        /// </summary>
        public static string LocalAppData { get => GetKnownFolderPath(KnownFolderIds.LocalAppData); }

        /// <summary>
        /// %USERPROFILE%\AppData\LocalLow
        /// </summary>
        public static string LocalAppDataLow { get => GetKnownFolderPath(KnownFolderIds.LocalAppDataLow); }

        /// <summary>
        /// %USERPROFILE%\Music
        /// </summary>
        public static string Music { get => GetKnownFolderPath(KnownFolderIds.Music); }

        /// <summary>
        /// %APPDATA%\Microsoft\Windows\Libraries\Music.library-ms
        /// </summary>
        public static string MusicLibrary { get => GetKnownFolderPath(KnownFolderIds.MusicLibrary); }

        /// <summary>
        /// %USERPROFILE%\Pictures
        /// </summary>
        public static string Pictures { get => GetKnownFolderPath(KnownFolderIds.Pictures); }

        /// <summary>
        /// %USERPROFILE%\Music\Playlists
        /// </summary>
        public static string Playlists { get => GetKnownFolderPath(KnownFolderIds.Playlists); }

        /// <summary>
        /// %USERPROFILE% (%SystemDrive%\Users\%USERNAME%)
        /// </summary>
        public static string Profile { get => GetKnownFolderPath(KnownFolderIds.Profile); }

        /// <summary>
        /// %ALLUSERSPROFILE% (%ProgramData%, %SystemDrive%\ProgramData)
        /// </summary>
        public static string ProgramData { get => GetKnownFolderPath(KnownFolderIds.ProgramData); }

        /// <summary>
        /// %ProgramFiles% (%SystemDrive%\Program Files)
        /// </summary>
        public static string ProgramFiles { get => GetKnownFolderPath(KnownFolderIds.ProgramFiles); }

        /// <summary>
        /// %ProgramFiles% (%SystemDrive%\Program Files) (x64)
        /// </summary>
        public static string ProgramFilesX64 { get => GetKnownFolderPath(KnownFolderIds.ProgramFilesX64); }

        /// <summary>
        /// %ProgramFiles% (%SystemDrive%\Program Files) (x86)
        /// </summary>
        public static string ProgramFilesX86 { get => GetKnownFolderPath(KnownFolderIds.ProgramFilesX86); }

        /// <summary>
        /// %ProgramFiles%\Common Files
        /// </summary>
        public static string ProgramFilesCommon { get => GetKnownFolderPath(KnownFolderIds.ProgramFilesCommon); }

        /// <summary>
        /// %ProgramFiles%\Common Files (x64)
        /// </summary>
        public static string ProgramFilesCommonX64 { get => GetKnownFolderPath(KnownFolderIds.ProgramFilesCommonX64); }

        /// <summary>
        /// %ProgramFiles%\Common Files (x86)
        /// </summary>
        public static string ProgramFilesCommonX86 { get => GetKnownFolderPath(KnownFolderIds.ProgramFilesCommonX86); }

        /// <summary>
        /// %APPDATA%\Microsoft\Windows\Start Menu\Programs
        /// </summary>
        public static string Programs { get => GetKnownFolderPath(KnownFolderIds.Programs); }

        /// <summary>
        /// %PUBLIC% (%SystemDrive%\Users\Public)
        /// </summary>
        public static string Public { get => GetKnownFolderPath(KnownFolderIds.Public); }

        /// <summary>
        /// %PUBLIC%\Desktop
        /// </summary>
        public static string PublicDesktop { get => GetKnownFolderPath(KnownFolderIds.PublicDesktop); }

        /// <summary>
        /// %PUBLIC%\Documents
        /// </summary>
        public static string PublicDocuments { get => GetKnownFolderPath(KnownFolderIds.PublicDocuments); }

        /// <summary>
        /// %PUBLIC%\Downloads
        /// </summary>
        public static string PublicDownloads { get => GetKnownFolderPath(KnownFolderIds.PublicDownloads); }

        /// <summary>
        /// %APPDATA% (%USERPROFILE%\AppData\Roaming)
        /// </summary>
        public static string RoamingAppData { get => GetKnownFolderPath(KnownFolderIds.RoamingAppData); }

        public static string GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                {
                    return Marshal.PtrToStringAuto(pszPath);
                }
                throw Marshal.GetExceptionForHR(hr);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pszPath);
                }
            }
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
    }
}
