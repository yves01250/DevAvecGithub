using Microsoft.Data.Sqlite;

namespace SuiviPortefolio.Data;

internal static class PositionRebuilder
{
    public static void RebuildAll(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();
        var actifIds = new List<int>();

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "SELECT DISTINCT TransActifId FROM TransacFin;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                actifIds.Add(reader.GetInt32(0));
        }

        foreach (var actifId in actifIds)
            RebuildForAsset(connection, transaction, actifId);

        transaction.Commit();
    }

    public static void RebuildForAsset(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int actifId)
    {
        using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM Position WHERE PosActifId = $actifId;";
            delete.Parameters.AddWithValue("$actifId", actifId);
            delete.ExecuteNonQuery();
        }

        var positions = new Dictionary<int, (decimal Quantity, decimal AveragePrice)>();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT TransCpteId, TransType, TransQte, TransPrix
                FROM TransacFin
                WHERE TransActifId = $actifId
                ORDER BY datetime(TransDateTransac), TransId;
                """;
            command.Parameters.AddWithValue("$actifId", actifId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var accountId = reader.GetInt32(0);
                var type = reader.GetString(1);
                var quantity = reader.GetDecimal(2);
                var price = reader.GetDecimal(3);
                positions.TryGetValue(accountId, out var position);

                if (type == "Vente")
                {
                    position.Quantity -= quantity;
                    if (position.Quantity <= 0)
                        position.AveragePrice = 0;
                }
                else
                {
                    var newQuantity = position.Quantity + quantity;
                    position.AveragePrice = newQuantity <= 0
                        ? 0
                        : position.Quantity <= 0
                            ? price
                            : ((position.Quantity * position.AveragePrice) + (quantity * price)) / newQuantity;
                    position.Quantity = newQuantity;
                }

                positions[accountId] = position;
            }
        }

        foreach (var (accountId, position) in positions)
        {
            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO Position (PosActifId, PosQte, PosPrixMoyen, PosCpteId)
                VALUES ($actifId, $quantity, $averagePrice, $accountId);
                """;
            insert.Parameters.AddWithValue("$actifId", actifId);
            insert.Parameters.AddWithValue("$quantity", position.Quantity);
            insert.Parameters.AddWithValue("$averagePrice", position.AveragePrice);
            insert.Parameters.AddWithValue("$accountId", accountId);
            insert.ExecuteNonQuery();
        }
    }
}
