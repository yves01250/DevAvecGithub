# SuiviPortefolio

Application de bureau Windows pour suivre un portefeuille d'investissement, ses comptes et ses transactions financières.

## Fonctionnalités

- Gestion des portefeuilles et sélection du portefeuille courant.
- Gestion des comptes associés à un portefeuille.
- Enregistrement et consultation des transactions financières.
- Recherche de cotations d'actifs.
- Calcul du montant total d'une transaction à partir de la quantité et des frais.
- Persistance locale dans une base SQLite.
- Interface WPF avec thème clair et mode sombre.

## Prérequis

- Windows.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- Visual Studio 2022 ou un autre éditeur compatible avec WPF et .NET.

Le projet cible `net10.0-windows` et utilise Windows Presentation Foundation (WPF).

## Installation

Clonez le dépôt puis placez-vous dans son dossier :

```powershell
git clone <URL_DU_DEPOT>
cd SuiviPortefolio
```

Restaurez les dépendances NuGet :

```powershell
dotnet restore .\SuiviPortefolio.slnx
```

## Exécuter l'application

Pour lancer l'application depuis la ligne de commande :

```powershell
dotnet run --project .\SuiviPortefolio.csproj
```

La base `SuiviPortefeuille.sqlite` est créée dans le dossier de sortie de l'application au premier démarrage. Son schéma est initialisé automatiquement par `SqliteRepository`.

Pour utiliser Visual Studio, ouvrez [`SuiviPortefolio.slnx`](./SuiviPortefolio.slnx), sélectionnez le projet `SuiviPortefolio` comme projet de démarrage, puis lancez-le avec `F5`.

## Tests

La suite de tests couvre notamment :

- la liaison entre la vue principale et le modèle de vue ;
- la persistance des portefeuilles dans SQLite ;
- l'impact des achats et des ventes sur le solde d'un compte.

Exécutez tous les tests avec :

```powershell
dotnet test .\SuiviPortefolio.slnx
```

## Organisation du projet

```text
SuiviPortefolio/
├── Comptes/
│   ├── CompteModel/          Modèle des comptes
│   ├── CompteRepository/     Accès aux données des comptes
│   ├── CompteView/           Vues WPF des comptes
│   └── CompteViewModel/      Logique de présentation des comptes
├── Data/                     Initialisation et accès à SQLite
├── Portefeuille/
│   ├── PortefeuilleModel/    Modèle des portefeuilles
│   ├── PortefeuilleView/     Fenêtre principale
│   └── PortefeuilleViewModel/
├── Transactions/
│   ├── TransModel/           Modèles des cotations et transactions
│   ├── TransRepository/      Accès aux données des transactions
│   ├── TransView/            Vues WPF des transactions
│   └── TransViewModel/
├── Ressources/               Styles et thèmes WPF
├── Tests/                    Tests unitaires, d'intégration et d'interface
├── App.xaml                  Ressources globales de l'application
└── SuiviPortefolio.csproj
```

## Données locales

La base SQLite contient notamment les tables suivantes :

- `Portefeuille`
- `Compte`
- `Actif`
- `ETF`
- `Position`
- `TransacFin`
- `Liquidite`
- `MouvementTresorerie`
- `Dividende`

Le fichier de base de données est local à l'application et n'est pas nécessaire au clonage du projet. Pour repartir d'une base vierge, arrêtez l'application puis supprimez `SuiviPortefeuille.sqlite` dans le dossier de sortie.

## Technologies

- C#
- .NET 10
- WPF
- SQLite via `Microsoft.Data.Sqlite`
- xUnit et `Microsoft.NET.Test.Sdk`

## Licence

Aucune licence open source n'est actuellement définie dans le dépôt.
