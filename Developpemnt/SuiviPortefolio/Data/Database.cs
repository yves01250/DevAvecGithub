
using System.Windows;
using Microsoft.Data.Sqlite;
using SuiviPortefolio.Portefeuille.PortefeuilleModel;

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
        
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
        

        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Portefeuille (
                PtfId INTEGER NOT NULL  PRIMARY KEY AUTOINCREMENT,
                PtfNom TEXT NOT NULL,
                PtfSolde DECIMAL  DEFAULT 0,
                PtfEstDefaut INTEGER DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS Actif ( 
                ActifId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ActifNom TEXT NOT NULL,
                ActifTicker TEXT NOT NULL,
                ActifIsin TEXT NOT NULL,
                ActifMarche TEXT NOT NULL,
                ActifDevise TEXT NOT NULL,
                ActifCoursActuel decimal(18, 2) NOT NULL,
                ActifDateDernierCours TEXT NOT NULL,
                ActifTransID INTEGER NOT NULL,
                FOREIGN KEY ( ActifTransID ) REFERENCES TransacFin( TransId )
            );
            CREATE TABLE IF NOT EXISTS Compte (
                CpteId INTEGER PRIMARY KEY AUTOINCREMENT,
                CpteNom TEXT NOT NULL,
                CpteDevise TEXT NOT NULL,
                CpteSoldeEsp decimal(18, 2) NOT NULL,
                CptePtfId INTEGER NOT NULL,
                FOREIGN KEY ( CptePtfId ) REFERENCES Portefeuille( PtfId )
            );
            CREATE TABLE IF NOT EXISTS ETF (
                ActifEtfId INTEGER PRIMARY KEY AUTOINCREMENT,
                ActifEtfNom TEXT NOT NULL,
                ActifEtfIndiceSuivi TEXT NOT NULL,
                ActifEtfCapit boolean NOT NULL,
                ActifEtfFraisGestion decimal(5, 2) NOT NULL,
                FOREIGN KEY ( ActifEtfId ) REFERENCES Actif( ActifId )
            );
            CREATE TABLE IF NOT EXISTS Position (
                PosId INTEGER PRIMARY KEY AUTOINCREMENT,
                PosActifId INTEGER NOT NULL,
                PosQte decimal(18, 2) NOT NULL,
                PosPrixMoyen decimal(18, 2) NOT NULL,
                FOREIGN KEY ( PosActifId ) REFERENCES Actif( ActifId )
            );
            CREATE TABLE IF NOT EXISTS TransacFin (
                TransId INTEGER PRIMARY KEY AUTOINCREMENT,
                TransType TEXT NOT NULL,
                TransQte decimal(18, 2) NOT NULL,
                TransDateTransac DATETIME NOT NULL,
                TransCpteId INTEGER NOT NULL,
                FOREIGN KEY ( TransCpteId ) REFERENCES Compte( CpteId )
            );
            CREATE TABLE IF NOT EXISTS Liquidite (
                LiqId INTEGER PRIMARY KEY AUTOINCREMENT,
                LiqCpteId INTEGER NOT NULL,
                LiqDevise TEXT NOT NULL,
                LiqSolde decimal(18, 2) NOT NULL,
                LiqDateMvt DATETIME NOT NULL,
                FOREIGN KEY ( LiqCpteId ) REFERENCES Compte( CpteId )
            );
            CREATE TABLE IF NOT EXISTS MouvementTresorerie (
                MvtTresId INTEGER PRIMARY KEY AUTOINCREMENT,
                MvtType INTEGER NOT NULL,
                MvtMontant decimal(18, 2) NOT NULL,
                MvtDate DATETIME NOT NULL,
                MvtCpteId INTEGER NOT NULL,
                FOREIGN KEY ( MvtCpteId ) REFERENCES Compte( CpteId )
            );
            CREATE TABLE IF NOT EXISTS Dividende (
                DvdId INTEGER PRIMARY KEY AUTOINCREMENT,
                DvdMontant decimal(18, 2) NOT NULL,
                DvdDate DATETIME NOT NULL,
                DvdActifId INTEGER NOT NULL,
                DvdSolde decimal(18, 2) NOT NULL,
                FOREIGN KEY ( DvdActifId ) REFERENCES Actif( ActifId )
            );
        ";
        command.ExecuteNonQuery();
        
    }

    public IEnumerable<monPortefeuille> GetAllPortefeuilles()
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();
    using var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT PtfId, PtfNom, PtfSolde FROM Portefeuille";

    var list = new List<monPortefeuille>();
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        list.Add(new monPortefeuille
        {
            PtfId = reader.GetInt32(0),
            PtfNom = reader.GetString(1),
            PtfSolde = reader.GetDecimal(2)
        });
    }
    return list;
}

    public void Ajouter(monPortefeuille portefeuille)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = """
            INSERT INTO Portefeuille
                (PtfNom, PtfSolde)
            VALUES
                ($nom, $solde);
            """;

        command.Parameters.AddWithValue(
            "$nom",
            portefeuille.PtfNom);

        command.Parameters.AddWithValue(
            "$solde",
            portefeuille.PtfSolde);

        command.ExecuteNonQuery();
    }

    public void SetPortefeuilleDefaut(int id)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();
    using var tx = connection.BeginTransaction();

    using (var cmd = connection.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "UPDATE Portefeuille SET PtfEstDefaut = 0";
        cmd.ExecuteNonQuery();
    }

    using (var cmd = connection.CreateCommand())
    {
        cmd.Transaction = tx;
        cmd.CommandText = "UPDATE Portefeuille SET PtfEstDefaut = 1 WHERE PtfId = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    tx.Commit();
}

}  

