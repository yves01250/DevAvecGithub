
using Microsoft.Data.Sqlite;

namespace SuiviPortefolio.Data;

public class SqliteRepository
{
    private readonly string _connectionString;

    public SqliteRepository(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        InitializeDatabase();
    }

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Portefeuille (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NomPortefeuille TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Actif (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NomActif TEXT NOT NULL,
                Ticker TEXT NOT NULL,
                Isin TEXT NOT NULL,
                Marche TEXT NOT NULL,
                Devise TEXT NOT NULL,
                TypeActif TEXT NOT NULL,
                CoursActuel decimal(18, 2) NOT NULL,
                DateDernierCours TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Compte (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NomCompte TEXT NOT NULL,
                Devise TEXT NOT NULL,
                SoldeEspeces decimal(18, 2) NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ETF (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NomETF TEXT NOT NULL,
                IndiceSuivi TEXT NOT NULL,
                Capitalisant boolean NOT NULL,
                FraisGestion decimal(5, 2) NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Position (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ActifId INTEGER NOT NULL,
                Quantite decimal(18, 2) NOT NULL,
                PrixMoyen decimal(18, 2) NOT NULL
            );
            CREATE TABLE IF NOT EXISTS TransacFin (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TypeTransacFin TEXT NOT NULL,
                Quantite decimal(18, 2) NOT NULL,
                DateTransacFin DATETIME NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Liquidite (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TypeLiquidite TEXT NOT NULL,
                Devise TEXT NOT NULL,
                SoldeLiquidite decimal(18, 2) NOT NULL,
                DateMouvementLiquidite DATETIME NOT NULL
            );
            CREATE TABLE IF NOT EXISTS MouvementTresorerie (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TypeMouvement TEXT NOT NULL,
                MontantMouvement decimal(18, 2) NOT NULL,
                DateMouvement DATETIME NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Dividende (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MontantDividende decimal(18, 2) NOT NULL,
                DateDividende DATETIME NOT NULL,
                ActifId INTEGER NOT NULL,
                SoldeDividende decimal(18, 2) NOT NULL
            );
        ";
        command.ExecuteNonQuery();
        
    }

}  

