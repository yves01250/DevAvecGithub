# Re-engineering du projet SuiviPortefolio

## 1. Objet du document

Ce document présente une rétro-conception du projet **SuiviPortefolio** à partir
du code source existant. Il décrit :

- l'architecture technique et l'organisation des dossiers ;
- les responsabilités des principaux composants ;
- le schéma relationnel SQLite observé dans `Data/Database.cs` ;
- les flux fonctionnels de recherche de cotation et d'enregistrement d'une
  transaction ;
- les diagrammes UML sous forme de blocs Mermaid.

Les diagrammes sont volontairement centrés sur les composants réellement
présents dans le projet. Les éléments non implémentés ou seulement préparés
dans le schéma de base sont signalés comme tels.

## 2. Vue d'ensemble

SuiviPortefolio est une application de bureau Windows basée sur :

- **.NET 10** et **WPF** ;
- un modèle de présentation proche de **MVVM** ;
- **SQLite** via `Microsoft.Data.Sqlite` ;
- **Yahoo Finance** comme source distante de recherche et de cours ;
- une base locale `SuiviPortefeuille.sqlite`, créée dans le répertoire de
  sortie de l'application.

Le point d'entrée WPF est [`App.xaml`](./App.xaml). La fenêtre principale est
[`Portefeuille/PortefeuilleView/MainWindow.xaml`](./Portefeuille/PortefeuilleView/MainWindow.xaml),
qui expose trois zones fonctionnelles :

1. Portefeuilles ;
2. Comptes ;
3. Transactions.

## 3. Architecture logique

```mermaid
flowchart TD
    App[App.xaml / App.xaml.cs] --> MainWindow[MainWindow.xaml]
    MainWindow --> PortfolioVM[MainViewModel]
    MainWindow --> AccountView[CompteView]
    MainWindow --> TransactionView[TransView]

    PortfolioVM --> SqliteRepository[SqliteRepository]
    AccountView --> AccountVM[CompteViewModel]
    TransactionView --> TransactionVM[TransactionViewModel]

    AccountVM --> AccountRepository[CompteRepository]
    TransactionVM --> TransactionRepository[TransRepository]
    TransactionView --> QuoteDialog[TransEditWindow]
    QuoteDialog --> QuoteFetcher[CotationFetcher]

    SqliteRepository --> SQLite[(SuiviPortefeuille.sqlite)]
    AccountRepository --> SQLite
    TransactionRepository --> SQLite
    QuoteFetcher --> Yahoo[Yahoo Finance HTTP API]
```

### 3.1 Couches identifiées

| Couche | Responsabilité | Composants principaux |
|---|---|---|
| Présentation WPF | Fenêtres, contrôles, événements utilisateur et thèmes | `MainWindow`, `CompteView`, `TransView`, `TransEditWindow` |
| Présentation / état | Sélection, commandes, validation simple, rafraîchissement des collections | `MainViewModel`, `CompteViewModel`, `TransactionViewModel` |
| Domaine | Objets métier manipulés par l'interface et la persistance | `monPortefeuille`, `Compte`, `Cotation`, `TransactionFinanciere` |
| Persistance | Création du schéma, requêtes SQLite, transactions SQL | `SqliteRepository`, `CompteRepository`, `TransRepository` |
| Intégration externe | Recherche de valeurs et récupération du dernier cours | `CotationFetcher`, Yahoo Finance |
| Infrastructure | Thèmes, sauvegarde et restauration du fichier SQLite | `ThemeManager`, `DatabaseManager` |

Le découpage est fonctionnel plutôt que strictement hexagonal : certaines vues
créent directement leur ViewModel, et les repositories construisent eux-mêmes
leur chaîne de connexion.

## 4. Structure du projet

```text
SuiviPortefolio/
├── App.xaml(.cs)                         Point d'entrée et ressources WPF
├── SuiviPortefolio.csproj                Projet .NET 10 WPF
├── REVERSE_ENGINEERING.md                Ce document
├── Data/
│   ├── Database.cs                       Schéma SQLite et repository portefeuille
│   ├── DBManager.cs                      Sauvegarde/restauration SQLite
│   └── RechercheCotations.cs             Client Yahoo Finance
├── Portefeuille/
│   ├── PortefeuilleModel/Portefeuille.cs Modèle monPortefeuille
│   ├── PortefeuilleView/                  MainWindow et fenêtre d'édition
│   └── PortefeuilleViewModel/             MainViewModel
├── Comptes/
│   ├── CompteModel/Compte.cs              Compte et TypeCompte
│   ├── CompteRepository/                  ICompteRepository / CompteRepository
│   ├── CompteView/                        Contrôles et fenêtres WPF
│   └── CompteViewModel/                   CompteViewModel
├── Transactions/
│   ├── TransModel/                        Cotation / TransactionFinanciere
│   ├── TransRepository/                   ITransRepository / TransRepository
│   ├── TransView/                         TransView / TransEditWindow
│   └── TransViewModel/                    TransactionViewModel
├── Ressources/                            Thèmes et styles WPF
└── Tests/
    ├── PortefolioServicesTests/           Tests d'intégration portefeuille
    ├── PortefolioTests/                   Tests UI
    └── TransactionsTests/                 Tests de solde de compte
```

Les répertoires `bin/`, `obj/` et `.vs/` sont des artefacts de compilation ou
d'environnement et ne font pas partie de l'architecture fonctionnelle.

## 5. Modèle objet principal

```mermaid
classDiagram
    class monPortefeuille {
        +int PtfId
        +string PtfNom
        +string PtfType
        +string PtfDevise
        +decimal PtfSolde
        +bool PtfEstDefaut
    }

    class Compte {
        +int CpteId
        +string CpteNom
        +TypeCompte CpteType
        +string CpteDevise
        +decimal CpteSolde
        +bool CpteEstDefaut
        +int CptePtfId
    }

    class TypeCompte {
        <<enumeration>>
        PEA
        CompteTitre
        CompteCourant
        Livret
        Autre
    }

    class Cotation {
        +long Id
        +string Symbol
        +string Isin
        +string Name
        +string Instrument
        +string Marche
        +string Devise
        +double Close
        +DateTime Date
        +DateTime CreatedAt
    }

    class TransactionFinanciere {
        +long TransId
        +string TransType
        +decimal TransQte
        +DateTime TransDateTransac
        +int TransCpteId
        +long TransActifId
        +string Isin
        +string Symbol
        +decimal TransPrix
        +decimal TransFrais
        +string CompteNom
    }

    monPortefeuille "1" --> "0..*" Compte : contient
    Compte --> TypeCompte
    Compte "1" --> "0..*" TransactionFinanciere : finance
    Cotation "1" --> "0..*" TransactionFinanciere : instrument
```

`Cotation` est le modèle applicatif de l'actif financier. En base, il est
persisté dans la table `Actif`; `TransactionFinanciere` conserve les références
vers `Compte` et `Actif`.

## 6. Schéma de données SQLite

Le schéma ci-dessous est extrait de `Data/Database.cs`. Les tables `ETF`,
`Liquidite`, `MouvementTresorerie` et `Dividende` sont préparées par la base,
mais leur utilisation n'est pas exposée par les écrans actuellement présents.

```mermaid
erDiagram
    Portefeuille ||--o{ Compte : "possède"
    Compte ||--o{ Position : "détient"
    Actif ||--o{ Position : "est détenu dans"
    Compte ||--o{ TransacFin : "supporte"
    Actif ||--o{ TransacFin : "concerne"
    Actif ||--o| ETF : "décrit"
    Compte ||--o{ Liquidite : "contient"
    Compte ||--o{ MouvementTresorerie : "enregistre"
    Actif ||--o{ Dividende : "verse"

    Portefeuille {
        INTEGER PtfId PK
        TEXT PtfNom
        TEXT PtfType
        TEXT PtfDevise
        DECIMAL PtfSolde
        INTEGER PtfEstDefaut
    }
    Compte {
        INTEGER CpteId PK
        TEXT CpteNom
        TEXT CpteType
        TEXT CpteDevise
        DECIMAL CpteSolde
        INTEGER CpteEstDefaut
        INTEGER CptePtfId FK
    }
    Actif {
        INTEGER ActifId PK
        TEXT ActifNom
        TEXT ActifTicker
        TEXT ActifIsin
        TEXT ActifMarche
        TEXT ActifDevise
        DECIMAL ActifCoursActuel
        TEXT ActifDateDernierCours
        INTEGER ActifTransID
    }
    ETF {
        INTEGER ActifEtfId PK, FK
        TEXT ActifEtfNom
        TEXT ActifEtfIndiceSuivi
        BOOLEAN ActifEtfCapit
        DECIMAL ActifEtfFraisGestion
    }
    Position {
        INTEGER PosId PK
        INTEGER PosActifId FK
        DECIMAL PosQte
        DECIMAL PosPrixMoyen
        INTEGER PosCpteId FK
    }
    TransacFin {
        INTEGER TransId PK
        TEXT TransType
        DECIMAL TransQte
        DATETIME TransDateTransac
        INTEGER TransCpteId FK
        INTEGER TransActifId FK
        DECIMAL TransPrix
        DECIMAL TransFrais
    }
    Liquidite {
        INTEGER LiqId PK
        INTEGER LiqCpteId FK
        TEXT LiqDevise
        DECIMAL LiqSolde
        DATETIME LiqDateMvt
    }
    MouvementTresorerie {
        INTEGER MvtTresId PK
        INTEGER MvtType
        DECIMAL MvtMontant
        DATETIME MvtDate
        INTEGER MvtCpteId FK
    }
    Dividende {
        INTEGER DvdId PK
        DECIMAL DvdMontant
        DATETIME DvdDate
        INTEGER DvdActifId FK
        DECIMAL DvdSolde
    }
```

### Contraintes et règles observées

- Les clés primaires sont généralement auto-incrémentées.
- Les clés étrangères sont activées par `PRAGMA foreign_keys = ON` dans
  `SqliteRepository`.
- Une position est unique pour le couple `(PosCpteId, PosActifId)`.
- Une transaction est un achat ou une vente selon `TransType`.
- La création ou la mise à jour d'une transaction recalcule la position et le
  solde du compte dans une même transaction SQLite.
- Le compte ou portefeuille par défaut est géré par une remise à zéro globale
  puis une affectation ciblée.

## 7. Flux de recherche et sélection d'une cotation

```mermaid
sequenceDiagram
    actor Utilisateur
    participant Vue as TransEditWindow
    participant Fetcher as CotationFetcher
    participant Yahoo as Yahoo Finance
    participant VM as TransactionViewModel

    Utilisateur->>Vue: Saisit un nom ou ticker
    Vue->>Fetcher: SearchAsync(name)
    Fetcher->>Yahoo: GET /v1/finance/search?q=...
    Yahoo-->>Fetcher: JSON quotes[]
    Fetcher-->>Vue: IReadOnlyList<Cotation>
    Vue-->>Utilisateur: Affiche les résultats
    Utilisateur->>Vue: Sélectionne un résultat
    Vue->>Fetcher: FetchSnapshotAsync(symbol)
    Fetcher->>Yahoo: GET /v8/finance/chart/{symbol}?range=1d
    Yahoo-->>Fetcher: JSON chart.result.meta + séries
    Fetcher-->>Vue: Cotation enrichie
    Vue-->>Utilisateur: Affiche cours, devise, marché et date
    Utilisateur->>Vue: Clique sur Utiliser
    Vue-->>VM: Retourne la cotation sélectionnée
```

`CotationFetcher` :

- refuse une recherche vide ;
- encode le texte et le symbole dans l'URL ;
- configure un `HttpClient` statique avec décompression et délai de 30 secondes ;
- extrait le dernier `close` exploitable ;
- utilise `regularMarketPrice` et `regularMarketTime` comme repli ;
- transforme les réponses inattendues en `InvalidOperationException`.

Les erreurs réseau et de format sont affichées par `TransEditWindow` à
l'utilisateur via des boîtes de dialogue.

## 8. Flux d'enregistrement d'une transaction

```mermaid
sequenceDiagram
    actor Utilisateur
    participant VM as TransactionViewModel
    participant Repo as TransRepository
    participant DB as SQLite

    Utilisateur->>VM: Saisit quantité, cours, frais et type
    VM->>VM: Valide les nombres et recalcule le total
    Utilisateur->>VM: Clique sur Enregistrer
    VM->>Repo: Save(transaction, cotation, ancienneQuantite)
    Repo->>DB: BEGIN
    Repo->>DB: EnsureActif(cotation)
    alt Nouvelle transaction
        Repo->>DB: INSERT TransacFin
    else Modification
        Repo->>DB: UPDATE TransacFin
        Repo->>DB: Lit l'ancienne transaction
    end
    Repo->>DB: UPSERT Position
    Repo->>DB: Corrige l'ancien solde si nécessaire
    Repo->>DB: Met à jour le solde du compte
    Repo->>DB: COMMIT
    Repo-->>VM: Transaction persistée
    VM->>Repo: GetRecent(actifId)
    Repo-->>VM: Transactions récentes
    VM-->>Utilisateur: Rafraîchit l'écran
```

La variation de solde observée est :

- achat : `-(quantité × prix) - frais` ;
- vente : `+(quantité × prix) - frais`.

La position utilise une quantité signée : positive pour un achat, négative
pour une vente. Lorsqu'une transaction est modifiée, l'ancienne variation est
retirée avant d'appliquer la nouvelle.

## 9. Cycle de démarrage et persistance

```mermaid
sequenceDiagram
    participant WPF as Application WPF
    participant Main as MainWindow
    participant VM as MainViewModel
    participant Repo as SqliteRepository
    participant DB as SQLite

    WPF->>Main: StartupUri
    Main->>VM: new MainViewModel()
    VM->>Repo: new SqliteRepository(dbPath)
    Repo->>DB: Ouvre ou crée SuiviPortefeuille.sqlite
    Repo->>DB: CREATE TABLE IF NOT EXISTS
    VM->>Repo: GetAllPortefeuilles()
    Repo-->>VM: Liste des portefeuilles
    VM-->>Main: DataContext et sélection courante
    Main-->>WPF: Interface disponible
```

Le chemin de base est construit avec
`AppDomain.CurrentDomain.BaseDirectory`, ce qui rend la base locale au dossier
de sortie de l'application.

## 10. Responsabilités par composant

### `Data`

- [`Data/Database.cs`](./Data/Database.cs) : initialise le schéma et réalise
  les opérations de portefeuille.
- [`Data/DBManager.cs`](./Data/DBManager.cs) : copie/restaure le fichier SQLite
  et affiche les erreurs à l'utilisateur.
- [`Data/RechercheCotations.cs`](./Data/RechercheCotations.cs) : adapte l'API
  Yahoo Finance vers le modèle `Cotation`.

### Portefeuille

- `MainViewModel` charge la collection, expose les commandes CRUD et gère le
  portefeuille sélectionné.
- `MainWindow` héberge les onglets et les menus de sauvegarde/restauration.

### Comptes

- `CompteViewModel` charge les comptes, sélectionne le compte par défaut et
  délègue les opérations à `ICompteRepository`.
- `CompteRepository` encapsule les requêtes SQL de la table `Compte`.

### Transactions

- `TransactionViewModel` gère les sélections, la validation des champs, le
  calcul du total et le rafraîchissement des transactions récentes.
- `TransEditWindow` réalise la recherche et la prévisualisation d'une cotation.
- `TransRepository` assure la cohérence entre `Actif`, `TransacFin`, `Position`
  et `Compte`.

## 11. Points d'attention pour une re-engineering future

1. **Injection de dépendances** : injecter les repositories et le client Yahoo
   au lieu de les construire dans les ViewModels ou les fenêtres.
2. **Contrats typés** : remplacer les valeurs textuelles `"Achat"` et `"Vente"`
   par un enum partagé.
3. **Séparation des responsabilités** : déplacer les boîtes de dialogue et
   validations d'interface vers des services testables.
4. **Modèle d'actif** : unifier explicitement `Cotation` et la table `Actif`
   afin d'éviter les conversions implicites.
5. **Dates et fuseaux** : conserver les dates en UTC ou documenter clairement
   la conversion opérée depuis les timestamps Yahoo.
6. **Migrations SQLite** : formaliser les évolutions de schéma plutôt que de
   limiter les changements à `AddColumnIfMissing`.
7. **Cohérence du portefeuille courant** : propager explicitement l'identifiant
   du portefeuille sélectionné lors de la création d'un compte, au lieu de
   dépendre d'une valeur par défaut.
8. **Tests** : ajouter des tests ciblés pour les erreurs Yahoo, les ventes
   invalides, les positions négatives et la modification d'une transaction.

## 12. Références du code analysé

- [`README.md`](./README.md)
- [`SuiviPortefolio.csproj`](./SuiviPortefolio.csproj)
- [`Data/RechercheCotations.cs`](./Data/RechercheCotations.cs)
- [`Data/Database.cs`](./Data/Database.cs)
- [`Transactions/TransRepository/TransRepository.cs`](./Transactions/TransRepository/TransRepository.cs)
- [`Transactions/TransViewModel/TransViewModel.cs`](./Transactions/TransViewModel/TransViewModel.cs)
- [`Comptes/CompteRepository/CompteRepository.cs`](./Comptes/CompteRepository/CompteRepository.cs)
- [`Portefeuille/PortefeuilleViewModel/MainViewModel.cs`](./Portefeuille/PortefeuilleViewModel/MainViewModel.cs)

