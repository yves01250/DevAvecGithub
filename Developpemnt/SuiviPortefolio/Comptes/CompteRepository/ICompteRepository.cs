using SuiviPortefolio.Comptes.CompteModel;

namespace SuiviPortefolio.Comptes.CompteRepository;

public interface ICompteRepository
{
    List<Compte> GetAll();
    void Add(Compte compte);
    void Update(Compte compte);
    void Delete(int id);
    void SetDefault(int id);
}
