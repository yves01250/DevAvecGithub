using SuiviPortefolio.Transactions.TransModel;

namespace SuiviPortefolio.Transactions.TransRepository;

public interface ITransRepository
{
    IReadOnlyList<Cotation> GetCotations();
    int EnsureActif(Cotation cotation);
    IReadOnlyList<TransactionFinanciere> GetRecent(int actifId, int count = 5);
    void Save(TransactionFinanciere transaction, Cotation cotation, decimal? oldQuantity = null);
}
