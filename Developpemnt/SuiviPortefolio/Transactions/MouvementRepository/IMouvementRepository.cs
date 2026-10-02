using SuiviPortefolio.Transactions.MouvementModel;

namespace SuiviPortefolio.Transactions.MouvementRepository;

public interface IMouvementRepository
{
    IReadOnlyList<MouvementEspece> GetRecent(int compteId, int count = 10);
    IReadOnlyList<MouvementEspece> GetAllByCompte(int compteId);
    void Save(MouvementEspece mouvement);
    void Delete(long mouvementId);
    decimal GetSoldeCompte(int compteId);
}
