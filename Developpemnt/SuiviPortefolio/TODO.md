# À faire

## Interface
- [ ] **Adapter les interfaces** (`ICotationFetcher`) si elles n'existent pas dans ton projet.
- [ ] 
- [ ] 

## Base de données
- [ ] Gérer erreurs dans le Repository (logging, try catch)
- [x] Prévoir une sauvegarde de la base SQLite

## Améliorations
- [ ] Ajouter des tests unitaires au dépôt de données
- [x] Pb RelayCommand 
    ok    Dans CompteViewModel.cs (ligne 280-330) : une classe RelayCommand interne/embeddée
        Dans ConsultationsViewModel/RelayCommand.cs : une classe RelayCommand externe
- [x] Vous avez aussi une implémentation manuelle de INotifyPropertyChanged
- [ ] Découpler davantage** (injection de dépendances, ViewModel pour `CompteEditWindow`)



### 📅 **Roadmap suggérée**

#### **Semaine 1 : Fondations**
- [ ] Ajouter `GetById`, `Exists`, `Delete`, `SetDefault` dans `ICompteRepository`
- [ ] Ajouter les tests pour ces nouvelles méthodes

#### **Semaine 2 : Découplage**
- [ ] Injecter `ICompteRepository` et `ICotationFetcher` dans `CompteViewModel`
- [ ] Créer `CompteEditViewModel`
- [ ] Mettre à jour `CompteEditWindow` pour utiliser le ViewModel

#### **Semaine 3 : Tests**
- [ ] Ajouter les tests pour `CompteViewModel` (avec Moq)
- [ ] Ajouter les tests pour `CotationFetcher`

#### **Semaine 4 : Bonus**
- [ ] Ajouter un système de logging
- [ ] Utiliser `HttpClient` comme singleton
- [ ] Ajouter la validation côté modèle