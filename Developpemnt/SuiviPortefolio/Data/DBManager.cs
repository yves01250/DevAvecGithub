using System;
using System.IO;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace SuiviPortefolio.Data
{
    

    public static class DatabaseManager
    {
        public static bool BackupDatabase(string sourcePath, string destinationPath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException("Le fichier source n'existe pas.");

                    // Créer le répertoire de destination s'il n'existe pas
                    var destinationDirectory = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                File.Copy(sourcePath, destinationPath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool RestoreDatabase(string backupPath, string targetPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    throw new FileNotFoundException("Le fichier de sauvegarde n'existe pas.");

                // Fermer les connexions existantes (à adapter selon votre code)
                // SQLiteConnection.ClearAllPools();

                File.Copy(backupPath, targetPath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de restauration : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
