using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Services;
public class WeissSchwarzBlakeUnityService
{
    private static ILogger Log = Serilog.Log.ForContext<WeissSchwarzBlakeUnityService>();

    private static readonly string BlakeRegistryKey = @"SOFTWARE\Blake Thoennes\Weiss Schwarz";
    private static readonly string ImportDeckPrefix = @"Deck_";
    private static readonly string ImportDatePrefix = @"Date_";
    private static readonly string ImportVariableName = @"[wstools import]";

    public string? GetExportDeckData()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetExportDeckDataViaRegistry();
        else
            return null;
    }

    public DateTime? GetExportDate()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetExportDateViaRegistry();
        else
            return null;
    }

    public void ExportDeckData(string dataToExport)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            ExportDeckDataViaRegistry(dataToExport);
    }
    public void ExportDeckDate(DateTime dateTime)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            ExportDeckDateViaRegistry(dateTime);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Contains guard clause for non-Windows.")]
    private void ExportDeckDataViaRegistry(string dataToExport)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotImplementedException();
        var blakeKey = Registry.CurrentUser.OpenSubKey(BlakeRegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
        var deckImportKey = GetVariableValue(blakeKey, ImportDeckPrefix, ImportVariableName);
        if (blakeKey is null || deckImportKey is null) return;
        if (!dataToExport.EndsWith("\0")) dataToExport += "\0";
        var dataBytes = System.Text.Encoding.ASCII.GetBytes(dataToExport);
        blakeKey.SetValue(deckImportKey, dataBytes);
    }

    private void ExportDeckDateViaRegistry(DateTime dateTime)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotImplementedException();
        var blakeKey = Registry.CurrentUser.OpenSubKey(BlakeRegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
        var dateImportKey = GetVariableValue(blakeKey, ImportDatePrefix, ImportVariableName);
        if (blakeKey is null || dateImportKey is null) return;
        var dateTimeString = dateTime.ToString("HH:mm  MM/dd/yyyy") + "\0";
        var dataBytes = System.Text.Encoding.ASCII.GetBytes(dateTimeString);
        blakeKey.SetValue(dateImportKey, dataBytes);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Contains guard clause for non-Windows.")]
    private string? GetExportDeckDataViaRegistry()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;
        var nullableBlakeKey = Registry.CurrentUser.OpenSubKey(BlakeRegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (nullableBlakeKey is not RegistryKey blakeKey) return null;
        var deckImportKey = GetVariableValue(blakeKey, ImportDeckPrefix, ImportVariableName);
        if (string.IsNullOrWhiteSpace(deckImportKey)) return null;
        if (blakeKey?.GetValue(deckImportKey) is byte[] byteArray)
            return System.Text.ASCIIEncoding.ASCII.GetString(byteArray);
        else
            return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Contains guard clause for non-Windows.")]
    private DateTime? GetExportDateViaRegistry()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;
        var blakeKey = Registry.CurrentUser.OpenSubKey(BlakeRegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
        var deckImportKey = GetVariableValue(blakeKey, ImportDatePrefix, ImportVariableName);

        if (string.IsNullOrWhiteSpace(deckImportKey)) return null;
        if (blakeKey?.GetValue(deckImportKey) is not byte[] byteArray) return null;
        var dateTimeString = System.Text.ASCIIEncoding.ASCII.GetString(byteArray);
        // 16:32  11/14/2022\0
        var dtStyles = System.Globalization.DateTimeStyles.AllowWhiteSpaces | System.Globalization.DateTimeStyles.AssumeLocal;
        if (DateTime.TryParse(dateTimeString[..^1], CultureInfo.InvariantCulture, dtStyles, out var result))
            return result;
        else
            return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Only privately accessible.")]

#nullable enable
    private string? GetVariableValue(RegistryKey? key, string prefix, string variableName)
        => key?.GetValueNames()
           .Where(sk => sk.StartsWith(prefix))
           .Where(sk => sk.Contains(variableName))
           .FirstOrDefault();

}
