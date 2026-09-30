using System.IO;
using Microsoft.Data.Sqlite;

namespace SuiviPortefolio.Data;

public static class DatabaseLocation
{
    private const string DatabaseFileName = "SuiviPortefeuille.sqlite";
    private const string LocationFileName = "database-directory.txt";
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SuiviPortefolio");
    private static string? _directoryPath;

    public static string DirectoryPath
    {
        get
        {
            EnsureInitialized();
            return _directoryPath!;
        }
    }

    public static string DatabasePath => Path.Combine(DirectoryPath, DatabaseFileName);

    public static void Initialize() => EnsureInitialized();

    public static bool ChangeDirectory(string directoryPath)
    {
        EnsureInitialized();

        var newDirectoryPath = Path.GetFullPath(directoryPath);
        if (string.Equals(
                newDirectoryPath,
                _directoryPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Directory.CreateDirectory(newDirectoryPath);
        var newDatabasePath = Path.Combine(newDirectoryPath, DatabaseFileName);
        SqliteConnection.ClearAllPools();
        CopyDatabase(DatabasePath, newDatabasePath);

        Directory.CreateDirectory(SettingsDirectory);
        var temporaryLocationPath = Path.Combine(SettingsDirectory, $"{LocationFileName}.tmp");
        File.WriteAllText(temporaryLocationPath, newDirectoryPath);
        File.Move(temporaryLocationPath, Path.Combine(SettingsDirectory, LocationFileName), overwrite: true);

        _directoryPath = newDirectoryPath;
        return true;
    }

    private static void EnsureInitialized()
    {
        if (_directoryPath != null)
            return;

        Directory.CreateDirectory(SettingsDirectory);
        var locationFilePath = Path.Combine(SettingsDirectory, LocationFileName);

        if (File.Exists(locationFilePath))
        {
            var configuredPath = File.ReadAllText(locationFilePath).Trim();
            if (!Path.IsPathFullyQualified(configuredPath))
                throw new InvalidDataException("Le dossier configuré pour la base de données n'est pas un chemin absolu.");

            _directoryPath = Path.GetFullPath(configuredPath);
        }
        else
        {
            _directoryPath = SettingsDirectory;
            var legacyDatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);
            var defaultDatabasePath = Path.Combine(_directoryPath, DatabaseFileName);

            if (!File.Exists(defaultDatabasePath) && File.Exists(legacyDatabasePath))
                CopyDatabase(legacyDatabasePath, defaultDatabasePath);
        }

        Directory.CreateDirectory(_directoryPath);
    }

    private static void CopyDatabase(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("La base de données source est introuvable.", sourcePath);

        var sourceConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = sourcePath,
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();
        var destinationConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = destinationPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        using var source = new SqliteConnection(sourceConnectionString);
        using var destination = new SqliteConnection(destinationConnectionString);
        source.Open();
        destination.Open();
        source.BackupDatabase(destination);
    }
}
