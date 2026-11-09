using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using RetroGameCoverDownloader.Helpers;
using RetroGameCoverDownloader.Models;
using Serilog;

namespace RetroGameCoverDownloader.Managers;

public static class SettingsManager
{
    private const string SettingsFileName = "settings.dat";
    private const string LegacySettingsFileName = "settings.xml";

    private static readonly string AppDataPath = Path.Combine(
        AppInfo.LocalAppDataFolderPath, SettingsFileName);

    private static string? _loadedFromPath;

    // Optional entropy mixed into DPAPI. This is not a secret key (DPAPI derives the
    // real key from the current Windows user's credentials); it just ties the blob to
    // this application so it can't be trivially decrypted by unrelated CurrentUser code.
    private static readonly byte[] Entropy = "RetroGameCoverDownloader.Settings.v2"u8.ToArray();

    private static readonly byte[] AesMagicBytes = "RGCDA1"u8.ToArray();
    private const int AesKeySize = 32;
    private const int AesNonceSize = 12;
    private const int AesTagSize = 16;

    private static readonly Lock KeyLock = new();
    private static readonly string KeyFilePath = Path.Combine(
        AppInfo.LocalAppDataFolderPath, "settings.key");

    // When nothing has been loaded yet, default to the (writable) per-user AppData
    // location. This must match the fallback used by SaveSettings() so that
    // DefaultSettingsFilePath and the actual save target stay consistent.
    public static string DefaultSettingsFilePath => _loadedFromPath ?? AppDataPath;

    public static AppSettings LoadSettings()
    {
        if (File.Exists(AppDataPath))
        {
            _loadedFromPath = AppDataPath;
            return LoadFromDat(AppDataPath);
        }

        var legacyPath = Path.Combine(AppContext.BaseDirectory, LegacySettingsFileName);
        if (File.Exists(legacyPath))
        {
            var migrated = MigrateFromLegacyXml(legacyPath);
            return migrated;
        }

        _loadedFromPath = null;
        return new AppSettings();
    }

    public static AppSettings LoadSettings(string filePath)
    {
        if (!File.Exists(filePath))
            return new AppSettings();

        return LoadFromDat(filePath);
    }

    public static void SaveSettings(AppSettings settings)
    {
        var path = _loadedFromPath ?? AppDataPath;
        SaveSettings(settings, path);
    }

    public static void SaveSettings(AppSettings settings, string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings);
            var plainBytes = Encoding.UTF8.GetBytes(json);
            var encrypted = EncryptBytes(plainBytes);

            File.WriteAllBytes(filePath, encrypted);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SettingsManager] Failed to save settings to {FilePath}.", filePath);
            throw;
        }
    }

    private static AppSettings LoadFromDat(string filePath)
    {
        try
        {
            var encrypted = File.ReadAllBytes(filePath);
            var plainBytes = DecryptBytes(encrypted);
            var json = Encoding.UTF8.GetString(plainBytes);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (CryptographicException ex)
        {
            Log.Error(ex, "[SettingsManager] Settings file is corrupt or was encrypted on a different machine. Creating new settings.");
            return new AppSettings();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SettingsManager] Failed to load {FilePath}. Creating new settings.", filePath);
            return new AppSettings();
        }
    }

    private static AppSettings MigrateFromLegacyXml(string legacyPath)
    {
        try
        {
            var settings = LegacyXmlParser(legacyPath);

            // Save to new format at the app data path
            var datPath = AppDataPath;
            var json = JsonSerializer.Serialize(settings);
            var plainBytes = Encoding.UTF8.GetBytes(json);
            var encrypted = EncryptBytes(plainBytes);

            var directory = Path.GetDirectoryName(datPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(datPath, encrypted);
            _loadedFromPath = datPath;

            return settings;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[SettingsManager] Failed to migrate legacy settings.xml. Creating new settings.");
            return new AppSettings();
        }
    }

    private static AppSettings LegacyXmlParser(string filePath)
    {
        var settings = new AppSettings();

        try
        {
            var doc = new XmlDocument();
            doc.Load(filePath);

            var tokenNode = doc.SelectSingleNode("/AppSettings/GitHubToken");
            if (tokenNode != null && !string.IsNullOrEmpty(tokenNode.InnerText))
            {
                try
                {
                    settings.GitHubToken = DecryptLegacyString(tokenNode.InnerText);
                }
                catch (Exception ex) when (ex is CryptographicException or FormatException)
                {
                    settings.GitHubToken = OperatingSystem.IsWindows() ? tokenNode.InnerText : null;
                }
            }

            var useProxyNode = doc.SelectSingleNode("/AppSettings/UseProxy");
            settings.UseProxy = useProxyNode != null && bool.TryParse(useProxyNode.InnerText, out var up) && up;

            var hostNode = doc.SelectSingleNode("/AppSettings/ProxyHost");
            settings.ProxyHost = hostNode?.InnerText;

            var portNode = doc.SelectSingleNode("/AppSettings/ProxyPort");
            if (portNode != null && int.TryParse(portNode.InnerText, System.Globalization.CultureInfo.InvariantCulture, out var port))
            {
                settings.ProxyPort = port;
            }

            var userNode = doc.SelectSingleNode("/AppSettings/ProxyUsername");
            settings.ProxyUsername = userNode?.InnerText;

            var passNode = doc.SelectSingleNode("/AppSettings/ProxyPassword");
            if (passNode != null && !string.IsNullOrEmpty(passNode.InnerText))
            {
                try
                {
                    settings.ProxyPassword = DecryptLegacyString(passNode.InnerText);
                }
                catch (Exception ex) when (ex is CryptographicException or FormatException)
                {
                    settings.ProxyPassword = OperatingSystem.IsWindows() ? passNode.InnerText : null;
                }
            }

            var extNodes = doc.SelectNodes("/AppSettings/FileExtensions/Extension");
            if (extNodes != null)
            {
                settings.FileExtensions.Clear();
                foreach (XmlNode node in extNodes)
                {
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        settings.FileExtensions.Add(node.InnerText.Trim());
                }

                if (settings.FileExtensions.Count == 0)
                {
                    settings.FileExtensions = [..AppSettings.DefaultExtensions];
                }
            }
        }
        catch
        {
            return new AppSettings();
        }

        return settings;
    }

    private static string DecryptLegacyString(string encryptedText)
    {
        if (!OperatingSystem.IsWindows())
            throw new CryptographicException("Legacy DPAPI settings can only be decrypted on Windows.");

        var bytes = Convert.FromBase64String(encryptedText);
        var unprotected = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(unprotected);
    }

    // Windows uses DPAPI (CurrentUser scope): the key is derived from the logged-in
    // user's credentials by the OS and never leaves the machine. Linux and macOS use
    // AES-GCM with a 256-bit key stored in the per-user AppData folder with user-only
    // permissions (0600 on Unix-like systems).
    private static byte[] EncryptBytes(byte[] plainBytes)
    {
        return OperatingSystem.IsWindows()
            ? ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser)
            : EncryptWithAesGcm(plainBytes);
    }

    private static byte[] DecryptBytes(byte[] encryptedBytes)
    {
        if (encryptedBytes.AsSpan().StartsWith(AesMagicBytes))
            return DecryptWithAesGcm(encryptedBytes);

        if (OperatingSystem.IsWindows())
            return ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);

        throw new CryptographicException(
            "The settings file uses Windows DPAPI and cannot be decrypted on this platform.");
    }

    private static byte[] EncryptWithAesGcm(byte[] plainBytes)
    {
        var key = GetOrCreateKey();
        var nonce = RandomNumberGenerator.GetBytes(AesNonceSize);
        var tag = new byte[AesTagSize];
        var ciphertext = new byte[plainBytes.Length];

        using var aes = new AesGcm(key, AesTagSize);
        aes.Encrypt(nonce, plainBytes, ciphertext, tag);

        var result = new byte[AesMagicBytes.Length + AesNonceSize + AesTagSize + ciphertext.Length];
        var offset = 0;
        AesMagicBytes.CopyTo(result, offset);
        offset += AesMagicBytes.Length;
        nonce.CopyTo(result, offset);
        offset += AesNonceSize;
        tag.CopyTo(result, offset);
        offset += AesTagSize;
        ciphertext.CopyTo(result, offset);
        return result;
    }

    private static byte[] DecryptWithAesGcm(byte[] encryptedBytes)
    {
        if (encryptedBytes.Length < AesMagicBytes.Length + AesNonceSize + AesTagSize)
            throw new CryptographicException("The encrypted settings file is too short.");

        var offset = AesMagicBytes.Length;
        var nonce = encryptedBytes.AsSpan(offset, AesNonceSize).ToArray();
        offset += AesNonceSize;
        var tag = encryptedBytes.AsSpan(offset, AesTagSize).ToArray();
        offset += AesTagSize;
        var ciphertext = encryptedBytes.AsSpan(offset).ToArray();

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(GetOrCreateKey(), AesTagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }

    private static byte[] GetOrCreateKey()
    {
        lock (KeyLock)
        {
            if (File.Exists(KeyFilePath))
            {
                var existingKey = File.ReadAllBytes(KeyFilePath);
                if (existingKey.Length == AesKeySize)
                    return existingKey;

                throw new CryptographicException("The settings encryption key file is invalid.");
            }

            var directory = Path.GetDirectoryName(KeyFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var key = RandomNumberGenerator.GetBytes(AesKeySize);
            var temporaryPath = KeyFilePath + ".tmp";
            File.WriteAllBytes(temporaryPath, key);

            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(temporaryPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            File.Move(temporaryPath, KeyFilePath, overwrite: true);

            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(KeyFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            return key;
        }
    }
}
