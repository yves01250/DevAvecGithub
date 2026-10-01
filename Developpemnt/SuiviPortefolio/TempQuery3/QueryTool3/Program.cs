using System;
using Microsoft.Data.Sqlite;

var dbPath = "C:/Users/yves_/AppData/Local/SuiviPortefolio/SuiviPortefeuille.sqlite";

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = dbPath,
    Mode = SqliteOpenMode.ReadOnly
}.ToString();

using var connection = new SqliteConnection(connectionString);
connection.Open();

Console.WriteLine("================================================================================");
Console.WriteLine("VERIFICATION DES DONNEES BRUTES");
Console.WriteLine("================================================================================\n");

// 1. Tous les comptes avec leur solde
Console.WriteLine("=== TABLE COMPTE (TOUS) ===");
using var cmd1 = connection.CreateCommand();
cmd1.CommandText = "SELECT CpteId, CpteNom, CpteType, CpteDevise, CpteSolde FROM Compte ORDER BY CpteId;";
using var reader1 = cmd1.ExecuteReader();
Console.WriteLine("ID | Nom | Type | Devise | Solde");
Console.WriteLine(new string('-', 70));
while (reader1.Read())
{
    Console.WriteLine(string.Format("{0,2} | {1,-20} | {2,-12} | {3,-6} | {4,12:N2}", 
        reader1.GetInt32(0), reader1.GetString(1), reader1.GetString(2), 
        reader1.GetString(3), reader1.GetDecimal(4)));
}
Console.WriteLine(string.Format("\nTOTAL ESPECES (somme CpteSolde): {0:N2} EUR\n", 
    GetTotalEspeces(connection)));

// 2. Toutes les positions
Console.WriteLine("================================================================================");
Console.WriteLine("=== TABLE POSITION (TOUTES) ===");
using var cmd2 = connection.CreateCommand();
cmd2.CommandText = "SELECT PosId, PosActifId, PosQte, PosPrixMoyen, PosCpteId FROM Position ORDER BY PosCpteId, PosActifId;";
using var reader2 = cmd2.ExecuteReader();
Console.WriteLine("PosID | ActifID | Qte | Prix Moyen | CompteID");
Console.WriteLine(new string('-', 70));
while (reader2.Read())
{
    Console.WriteLine(string.Format("{0,5} | {1,8} | {2,6:N2} | {3,10:N2} | {4,8}", 
        reader2.GetInt32(0), reader2.GetInt32(1), reader2.GetDecimal(2), 
        reader2.GetDecimal(3), reader2.GetInt32(4)));
}
Console.WriteLine();

// 3. Tous les actifs
Console.WriteLine("================================================================================");
Console.WriteLine("=== TABLE ACTIF (TOUS) ===");
using var cmd3 = connection.CreateCommand();
cmd3.CommandText = "SELECT ActifId, ActifNom, ActifTicker, ActifCoursActuel FROM Actif ORDER BY ActifId;";
using var reader3 = cmd3.ExecuteReader();
Console.WriteLine("ActifID | Nom | Ticker | Cours Actuel");
Console.WriteLine(new string('-', 70));
while (reader3.Read())
{
    Console.WriteLine(string.Format("{0,7} | {1,-25} | {2,-8} | {3,12:N2}", 
        reader3.GetInt32(0), reader3.GetString(1), reader3.GetString(2), 
        reader3.GetDecimal(3)));
}
Console.WriteLine();

// 4. Vérification : positions par compte
Console.WriteLine("================================================================================");
Console.WriteLine("=== POSITIONS PAR COMPTE (avec détails actifs) ===");
using var cmd4 = connection.CreateCommand();
cmd4.CommandText = @"
    SELECT 
        c.CpteId, c.CpteNom, 
        pos.PosId, pos.PosActifId, 
        a.ActifNom, a.ActifTicker, 
        pos.PosQte, pos.PosPrixMoyen, 
        a.ActifCoursActuel,
        (pos.PosQte * a.ActifCoursActuel) as Valorisation
    FROM Compte c
    JOIN Position pos ON pos.PosCpteId = c.CpteId
    JOIN Actif a ON pos.PosActifId = a.ActifId
    ORDER BY c.CpteId, a.ActifTicker;
";
using var reader4 = cmd4.ExecuteReader();
Console.WriteLine("CpteID | Compte | PosID | ActifID | Actif | Ticker | Qte | Prix Moyen | Cours | Valorisation");
Console.WriteLine(new string('-', 120));
while (reader4.Read())
{
    Console.WriteLine(string.Format("{0,6} | {1,-15} | {2,5} | {3,7} | {4,-20} | {5,-8} | {6,5:N2} | {7,10:N2} | {8,8:N2} | {9,14:N2}",
        reader4.GetInt32(0), reader4.GetString(1), reader4.GetInt32(2), reader4.GetInt32(3),
        reader4.GetString(4), reader4.GetString(5), reader4.GetDecimal(6), reader4.GetDecimal(7),
        reader4.GetDecimal(8), reader4.GetDecimal(9)));
}
Console.WriteLine();

// 5. Calcul manuel : solde espèces total
Console.WriteLine("================================================================================");
Console.WriteLine("=== VERIFICATION SOLDE ESPECES ===");
decimal totalEspeces = GetTotalEspeces(connection);
Console.WriteLine(string.Format("Total espèces (SUM CpteSolde): {0:N2} EUR", totalEspeces));
Console.WriteLine(string.Format("Attendu par utilisateur: 1906.83 EUR"));
Console.WriteLine(string.Format("Difference: {0:N2} EUR", totalEspeces - 1906.83m));

// 6. Vérification des comptes avec positions
Console.WriteLine("\n================================================================================");
Console.WriteLine("=== COMPTES AVEC POSITIONS ===");
using var cmd6 = connection.CreateCommand();
cmd6.CommandText = @"
    SELECT DISTINCT c.CpteId, c.CpteNom, c.CpteType
    FROM Compte c
    JOIN Position pos ON pos.PosCpteId = c.CpteId
    ORDER BY c.CpteId;
";
using var reader6 = cmd6.ExecuteReader();
Console.WriteLine("CompteID | Nom | Type");
Console.WriteLine(new string('-', 40));
while (reader6.Read())
{
    Console.WriteLine(string.Format("{0,8} | {1,-20} | {2}", 
        reader6.GetInt32(0), reader6.GetString(1), reader6.GetString(2)));
}

// 7. Compte Titre spécifique
Console.WriteLine("\n================================================================================");
Console.WriteLine("=== DETAIL COMPTE TITRE (ID=2) ===");
using var cmd7 = connection.CreateCommand();
cmd7.CommandText = @"
    SELECT 
        c.CpteId, c.CpteNom, c.CpteSolde,
        pos.PosId, a.ActifNom, a.ActifTicker, pos.PosQte, a.ActifCoursActuel
    FROM Compte c
    LEFT JOIN Position pos ON pos.PosCpteId = c.CpteId
    LEFT JOIN Actif a ON pos.PosActifId = a.ActifId
    WHERE c.CpteId = 2
    ORDER BY a.ActifTicker;
";
using var reader7 = cmd7.ExecuteReader();
Console.WriteLine("ID | Nom | Solde | PosID | Actif | Ticker | Qte | Cours");
Console.WriteLine(new string('-', 80));
if (!reader7.Read())
{
    Console.WriteLine("Aucune donnee trouvee pour Compte ID=2");
}
else
{
    do
    {
        Console.WriteLine(string.Format("{0,2} | {1,-20} | {2,10:N2} | {3,5} | {4,-20} | {5,-8} | {6,6:N2} | {7,8:N2}",
            reader7.GetInt32(0), reader7.GetString(1), reader7.GetDecimal(2),
            reader7.IsDBNull(3) ? 0 : reader7.GetInt32(3),
            reader7.IsDBNull(4) ? "NULL" : reader7.GetString(4),
            reader7.IsDBNull(5) ? "NULL" : reader7.GetString(5),
            reader7.IsDBNull(6) ? 0m : reader7.GetDecimal(6),
            reader7.IsDBNull(7) ? 0m : reader7.GetDecimal(7)));
    } while (reader7.Read());
}

static decimal GetTotalEspeces(SqliteConnection conn)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT SUM(CpteSolde) FROM Compte";
    var result = cmd.ExecuteScalar();
    return result == DBNull.Value ? 0m : Convert.ToDecimal(result);
}
