using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Card.API.Utilities
{
    using Microsoft.Win32;
    using System.Runtime.InteropServices;

    public static class InstalledApplications
    {
        public static string GetApplicationInstallPath(string nameOfAppToFind)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string installedPath;
                string keyName;

                // search in: CurrentUser
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.CurrentUser, keyName, "DisplayName", nameOfAppToFind) ;
                if (!string.IsNullOrEmpty(installedPath))
                {
                    return installedPath;
                }

                // search in: LocalMachine_32
                keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", nameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    return installedPath;
                }

                // search in: LocalMachine_64
                keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                installedPath = ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", nameOfAppToFind);
                if (!string.IsNullOrEmpty(installedPath))
                {
                    return installedPath;
                }

                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

#pragma warning disable CA1416 // Validate platform compatibility
        private static string ExistsInSubKey(RegistryKey root, string subKeyName, string attributeName, string nameOfAppToFind)
        {
            RegistryKey? subkey;
            string displayName;

            using (RegistryKey? key = root.OpenSubKey(subKeyName))
            {
                if (key is not null)
                {
                    foreach (string kn in key.GetSubKeyNames())
                    {
                        using (subkey = key.OpenSubKey(kn))
                        {
                            displayName = subkey?.GetValue(attributeName) as string ?? string.Empty;
                            if (nameOfAppToFind.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                return subkey?.GetValue("InstallLocation") as string ?? string.Empty;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
#pragma warning restore CA1416 // Validate platform compatibility

    }
}
