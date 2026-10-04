using SuiviPortefolio.Comptes.CompteModel;

namespace SuiviPortefolio.Comptes.CompteRepository;

// TODO: Ajouter méthodes utiles (GetById, Exists ..)

public interface ICompteRepository
{
    List<Compte> GetAll();
    List<CompteExposition> GetExpositions();
    int Add(Compte compte);
    void Update(Compte compte);
    void Delete(int id);
    void SetDefault(int id);
}
