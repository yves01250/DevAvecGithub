---
name: Guide d'installation et d'utilisation - SuiviPortefolio
description: Guide complet pour installer, configurer et utiliser SuiviPortefolio avec Visual Studio Code
author: SuiviPortefolio Team
version: 1.0.0
---

<!-- Badges / Shields -->
<p align="center">
  <a href="#suiviportefolio">
    <img src="../app.ico" alt="SuiviPortefolio Logo" width="100" height="100">
  </a>
  
  <h1 align="center">SuiviPortefolio</h1>
  
  <p align="center">
    Application de gestion de portefeuille d'investissement pour Windows
    <br />
    <br />
    <!-- Badges -->
    <a href="https://github.com/">
      <img src="https://img.shields.io/badge/Platform-Windows-0078D4?logo=windows&logoColor=white" alt="Platform: Windows">
    </a>
    <a href="https://dotnet.microsoft.com/">
      <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10.0">
    </a>
    <a href="https://github.com/">
      <img src="https://img.shields.io/badge/WPF-0078D4?logo=windows&logoColor=white" alt="WPF">
    </a>
    <a href="https://github.com/">
      <img src="https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white" alt="SQLite">
    </a>
    <a href="https://github.com/">
      <img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License: MIT">
    </a>
    <a href="https://github.com/">
      <img src="https://img.shields.io/badge/Build-Passing-brightgreen.svg" alt="Build: Passing">
    </a>
    <a href="https://github.com/">
      <img src="https://img.shields.io/badge/Tests-Passing-brightgreen.svg" alt="Tests: Passing">
    </a>
  </p>
</p>

---

## 📋 Table des matières

1. [📌 À propos](#-à-propos)
2. [✅ Prérequis](#-prérequis)
3. [🛠️ Installation](#-installation)
   - [Installation avec Git Clone](#git-clone)
   - [Installation manuelle](#installation-manuelle)
4. [🔧 Configuration](#-configuration)
   - [Configuration de VS Code](#configuration-de-vs-code)
   - [Configuration du projet](#configuration-du-projet)
5. [🚀 Build et Exécution](#-build-et-exécution)
   - [Build avec Visual Studio Code](#build-avec-visual-studio-code)
   - [Build avec CLI .NET](#build-avec-cli-net)
   - [Exécution de l'application](#exécution-de-lapplication)
6. [🎯 Utilisation](#-utilisation)
   - [Première utilisation](#première-utilisation)
   - [Gestion des portefeuilles](#gestion-des-portefeuilles)
   - [Gestion des comptes](#gestion-des-comptes)
   - [Gestion des transactions](#gestion-des-transactions)
   - [Consultations et analyses](#consultations-et-analyses)
7. [📊 Structure du projet](#-structure-du-projet)
8. [⚙️ Personnalisation](#-personnalisation)
   - [Thème clair/sombre](#thème-clairsombre)
   - [Langue](#langue)
9. [🐛 Dépannage](#-dépannage)
   - [Problèmes courants](#problèmes-courants)
   - [Erreurs de build](#erreurs-de-build)
   - [Problèmes de base de données](#problèmes-de-base-de-données)
10. [🧪 Tests](#-tests)
11. [📦 Création d'un installateur](#-création-dun-installateur)
12. [🤝 Contribution](#-contribution)
13. [📜 License](#-license)

---

## 📌 À propos

**SuiviPortefolio** est une application de bureau Windows développée en **C# avec WPF** et **.NET 10** pour suivre et gérer votre portefeuille d'investissement. Elle permet de :

- ✅ Gérer plusieurs portefeuilles et comptes
- ✅ Enregistrer et consulter les transactions financières
- ✅ Calculer automatiquement les soldes et valorisations
- ✅ Consulter l'historique et analyser les performances
- ✅ Gérer les dividendes et mouvements de trésorerie
- ✅ Importer/exporter les données

---

## ✅ Prérequis

### Système d'exploitation
- **Windows 10** (version 1903 ou supérieure)
- **Windows 11**

### Logiciels requis

| Logiciel | Version | Lien de téléchargement | Vérification |
|---------|---------|----------------------|--------------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 ou supérieur | [Télécharger](https://dotnet.microsoft.com/download/dotnet/10.0) | `dotnet --version` |
| [Visual Studio Code](https://code.visualstudio.com/) | Dernière version | [Télécharger](https://code.visualstudio.com/download) | `code --version` |
| [Git](https://git-scm.com/) | 2.x ou supérieur | [Télécharger](https://git-scm.com/download/win) | `git --version` |

### Extensions VS Code recommandées

| Extension | ID | Description |
|-----------|----|-------------|
| C# | ms-dotnettools.csharp | Support C# pour VS Code |
| .NET Core Test Explorer | formulahendry.dotnet-test-explorer | Exécuter les tests .NET |
| NuGet Package Manager | nuget.nuget-package-manager | Gestion des packages |
| GitLens | eamodio.gitlens | Superpuissance Git |
| Material Icon Theme | PKief.material-icon-theme | Icônes modernes |

---

## 🛠️ Installation

### 📥 Installation avec Git Clone

```bash
# Ouvrir un terminal (PowerShell, CMD, ou Git Bash)
cd C:\chemin\vers\votre\dossier\de\projets

# Cloner le dépôt
git clone https://github.com/votre-utilisateur/SuiviPortefolio.git

# Accéder au dossier du projet
cd SuiviPortefolio

# Vérifier que tout est en place
ls
```

### 📁 Installation manuelle

1. Télécharger le code source depuis GitHub (bouton "Code" > "Download ZIP")
2. Extraire le fichier ZIP dans un dossier de votre choix
3. Renommer le dossier en `SuiviPortefolio` (optionnel)

---

## 🔧 Configuration

### Configuration de VS Code

1. **Ouvrir le projet dans VS Code**
   ```bash
   code SuiviPortefolio
   ```

2. **Installer les dépendances**
   - VS Code détectera automatiquement le projet .NET
   - Une fenêtre de notification apparaîtra pour proposer d'ajouter les assets nécessaires
   - Cliquez sur "Yes" pour ajouter les assets de build et de debug

3. **Configurer l'environnement**
   - Ouvrir la palette de commandes : `Ctrl+Shift+P`
   - Rechercher et sélectionner : **"C#: Select .NET Core Test Logger Format"**
   - Choisir **"trx"** ou **"junit"** selon votre préférence

### Configuration du projet

1. **Restaurer les packages NuGet**
   ```bash
   dotnet restore SuiviPortefolio.slnx
   ```

2. **Vérifier la configuration du projet**
   - Ouvrir `SuiviPortefolio.csproj`
   - Vérifier que la cible est bien :
     ```xml
     <TargetFramework>net10.0-windows</TargetFramework>
     <UseWPF>true</UseWPF>
     ```

---

## 🚀 Build et Exécution

### Build avec Visual Studio Code

1. **Ouvrir le terminal intégré** : `Ctrl+\` (accent grave)
2. **Builder le projet** :
   ```bash
   dotnet build SuiviPortefolio.slnx
   ```
3. **Vérifier le build** :
   - Le dossier `bin\Debug\net10.0-windows` doit être créé
   - Aucune erreur ne doit être affichée

### Build avec CLI .NET

```bash
# Naviguer vers le dossier du projet
cd C:\chemin\vers\SuiviPortefolio

# Builder en mode Debug
dotnet build --configuration Debug

# Builder en mode Release
dotnet build --configuration Release

# Builder avec nettoyage préalable
dotnet clean && dotnet build
```

### Exécution de l'application

#### Depuis VS Code
1. Appuyer sur `F5` pour démarrer le debug
2. Ou cliquer sur le bouton "Run and Debug" dans la barre latérale
3. Sélectionner **.NET Core Launch (console)** si demandé

#### Depuis la ligne de commande
```bash
# Exécuter en mode Debug
dotnet run --project SuiviPortefolio.csproj

# Exécuter en mode Release
dotnet run --project SuiviPortefolio.csproj --configuration Release

# Exécuter avec des arguments
dotnet run --project SuiviPortefolio.csproj --args "arg1" "arg2"
```

#### Fichier exécutable
Après le build, vous pouvez aussi exécuter directement :
```bash
# Naviguer vers le dossier de sortie
cd bin\Debug\net10.0-windows

# Exécuter l'application
SuiviPortefolio.exe
```

---

## 🎯 Utilisation

### Première utilisation

1. **Lancement de l'application**
   - Au premier démarrage, une base de données SQLite sera créée automatiquement
   - Emplacement par défaut : `%LOCALAPPDATA%\SuiviPortefolio\SuiviPortefeuille.sqlite`

2. **Création du premier portefeuille**
   - Cliquer sur "Ajouter/Modifier un portefeuille"
   - Renseigner le nom du portefeuille (ex: "Mon Portefeuille Personnel")
   - Sélectionner le type de portefeuille
   - Définir la devise par défaut (ex: EUR, USD)

3. **Création du premier compte**
   - Sélectionner l'onglet "Comptes"
   - Cliquer sur "Ajouter un compte"
   - Remplir les informations : nom, type, devise, solde initial

### Gestion des portefeuilles

| Action | Étapes |
|--------|-------|
| **Créer** | Onglet Portefeuille > "Ajouter/Modifier un portefeuille" |
| **Modifier** | Sélectionner le portefeuille > "Modifier le portefeuille sélectionné" |
| **Supprimer** | Sélectionner le portefeuille > "Supprimer le portefeuille sélectionné" |
| **Définir par défaut** | Sélectionner le portefeuille > "Définir comme portefeuille par défaut" |
| **Changer de portefeuille** | Utiliser la liste déroulante en haut de l'onglet Portefeuille |

### Gestion des comptes

| Action | Étapes |
|--------|-------|
| **Créer** | Onglet Comptes > "Ajouter un compte" |
| **Modifier** | Sélectionner le compte > "Modifier le compte sélectionné" |
| **Définir par défaut** | Sélectionner le compte > "Définir comme compte par défaut" |
| **Actualiser valorisations** | Cliquer sur "Actualiser les estimations" pour recalculer les valorisations |

**Types de comptes disponibles** :
- Compte Espèces (compte bancaire classique)
- Compte Titre (compte de courtage)
- PEA (Plan d'Épargne en Actions)
- CTO (Compte Titre Ordinaire)
- Assurance Vie

### Gestion des transactions

| Type | Description | Onglet |
|------|-------------|--------|
| **Achat** | Achat d'un actif financier | Transactions |
| **Vente** | Vente d'un actif financier | Transactions |
| **Dividende** | Réception d'un dividende | Transactions |
| **Frais** | Frais de courtage ou autres | Transactions |
| **Virement** | Transfert entre comptes | Mouvements espèces |
| **Dépôt** | Dépôt d'argent sur un compte | Mouvements espèces |
| **Retrait** | Retrait d'argent d'un compte | Mouvements espèces |

**Champs communs** :
- Date de la transaction
- Compte concerné
- Montant
- Actif (pour les achats/ventes)
- Quantité (pour les achats/ventes)
- Description

### Consultations et analyses

L'onglet **Consultations** permet de :

1. **Visualiser les positions** : Liste de tous vos actifs avec valorisation
2. **Analyser les performances** : Graphique d'évolution de la valeur investie
3. **Comparer les portefeuilles** : Analyse comparative
4. **Sélection multiple** : Sélectionner plusieurs positions pour analyse groupée

**Options de période** :
- 1 mois
- 3 mois
- 6 mois
- 1 an
- 5 ans
- Tout

---

## 📊 Structure du projet

```
SuiviPortefolio/
├── .gitignore                    # Fichiers à ignorer par Git
├── .vs/                         # Configuration Visual Studio
├── App.xaml                      # Ressources globales de l'application
├── App.xaml.cs                   # Code-behind de l'application
├── SuiviPortefolio.csproj         # Configuration du projet
├── SuiviPortefolio.slnx          # Solution Visual Studio
├── ThemeManager.cs               # Gestionnaire de thème
│
├── Comptes/                      # Module de gestion des comptes
│   ├── CompteModel/              # Modèles de données
│   │   ├── Compte.cs            # Modèle Compte
│   │   ├── CompteExposition.cs  # Modèle d'exposition
│   │   └── CompteValorisation.cs # Modèle de valorisation
│   ├── CompteRepository/         # Accès aux données
│   │   ├── CompteRepository.cs  # Implémentation du dépôt
│   │   └── ICompteRepository.cs # Interface du dépôt
│   ├── CompteView/               # Vues WPF
│   │   ├── CompteEditWindow.xaml # Fenêtre d'édition
│   │   └── CompteView.xaml       # Vue principale des comptes
│   └── CompteViewModel/          # Logique de présentation
│       └── CompteViewModel.cs    # ViewModel des comptes
│
├── Consultations/                # Module de consultations
│   └── ConsultationsView/        # Vues de consultation
│       ├── AnalysisWindow.xaml  # Fenêtre d'analyse
│       └── ConsultationsView.xaml # Vue principale
│
├── Data/                         # Module de données
│   └── DatabaseLocation.cs       # Gestion de l'emplacement de la base
│
├── Dividendes/                   # Module de dividendes
│
├── Installer/                    # Scripts d'installation
│   ├── Build-Installer.ps1       # Script PowerShell pour créer l'installateur
│   └── SuiviPortefolio.iss       # Script Inno Setup
│
├── Portefeuille/                 # Module de portefeuilles
│   ├── PortefeuilleModel/        # Modèles de données
│   ├── PortefeuilleView/         # Vues WPF
│   │   ├── EditMainWindow.xaml   # Fenêtre d'édition principale
│   │   └── MainWindow.xaml       # Fenêtre principale de l'application
│   └── PortefeuilleViewModel/    # Logique de présentation
│
├── Properties/                   # Propriétés du projet
│   └── Resources.resx            # Ressources localisées
│
├── Ressources/                   # Ressources WPF
│   ├── DarkTheme.xaml            # Thème sombre
│   ├── Styles.xaml               # Styles globaux
│   └── Theme.xaml                # Thème par défaut
│
├── Tests/                        # Tests unitaires et d'intégration
│
├── Transactions/                 # Module de transactions
│   ├── MouvementView/            # Vues des mouvements
│   │   └── MouvementView.xaml    # Vue des mouvements d'espèces
│   ├── TransModel/               # Modèles de données
│   ├── TransRepository/          # Accès aux données
│   └── TransView/                # Vues des transactions
│       └── TransView.xaml        # Vue principale des transactions
│
└── Docs/                         # Documentation
    ├── guide_installation_utilisation.md  # Ce guide
    └── preconisations_ecrans_faible_resolution.md
```

---

## ⚙️ Personnalisation

### Thème clair/sombre

L'application supporte deux thèmes : **clair** et **sombre**.

**Basculer entre les thèmes** :
1. Cliquer sur le bouton "Basculer en mode sombre" en haut à droite de la fenêtre principale
2. Le thème change instantanément et la préférence est sauvegardée

**Personnaliser les couleurs** :
- Modifier les fichiers :
  - `Ressources/Theme.xaml` (thème clair)
  - `Ressources/DarkTheme.xaml` (thème sombre)
- Redéfinir les pinceaux de couleur (`SolidColorBrush`)

### Langue

L'application est configurée en **français** par défaut.

**Changer la langue** :
1. Modifier dans `App.xaml.cs` :
   ```csharp
   CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
   CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
   ```
2. Redémarrer l'application

---

## 🐛 Dépannage

### Problèmes courants

| Problème | Cause possible | Solution |
|----------|----------------|----------|
| Application ne démarre pas | .NET 10 non installé | Installer le [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Fenêtre vide au démarrage | Problème de bindings WPF | Vérifier les bindings dans les fichiers XAML |
| Base de données introuvable | Premier lancement ou dossier déplacé | L'application crée automatiquement une nouvelle base dans `%LOCALAPPDATA%\SuiviPortefolio` |
| Valorisations non calculées | Problème de connexion Internet | Vérifier la connexion et réessayer |
| Taux de change manquants | API Yahoo Finance indisponible | Attendre que l'API soit de nouveau disponible |

### Erreurs de build

| Erreur | Solution |
|--------|----------|
| `MSB3644: Le framework ciblé .NET 10.0 n'est pas installé` | Installer le .NET 10 SDK |
| `CS1061: 'X' ne contient pas de définition pour 'Y'` | Vérifier les noms des propriétés/méthodes |
| `XamlParseException` | Vérifier la syntaxe XAML |
| `SqliteException` | Vérifier que SQLite est bien référencé |

**Commandes utiles** :
```bash
# Nettoyer et reconstruire
dotnet clean
dotnet build

# Vérifier les dépendances
dotnet list SuiviPortefolio.csproj package

# Vérifier la version de .NET
dotnet --version
```

### Problèmes de base de données

**Emplacement de la base de données** :
- Par défaut : `%LOCALAPPDATA%\SuiviPortefolio\SuiviPortefeuille.sqlite`
- Personnalisable via : **Fichier > Choisir le dossier de la base de données...**

**Réinitialiser la base de données** :
1. Arrêter l'application
2. Supprimer le fichier `SuiviPortefeuille.sqlite`
3. Redémarrer l'application (une nouvelle base vide sera créée)

**Importer une ancienne base** :
1. Placer votre ancienne base à côté de l'exécutable
2. Au premier démarrage, l'application détectera et importera automatiquement l'ancienne base

---

## 🧪 Tests

L'application inclut une suite de tests unitaires et d'intégration.

### Exécuter les tests

**Depuis VS Code** :
1. Ouvrir le terminal intégré
2. Exécuter :
   ```bash
   dotnet test SuiviPortefolio.slnx
   ```

**Depuis la ligne de commande** :
```bash
# Exécuter tous les tests
dotnet test

# Exécuter les tests avec plus de détails
dotnet test --logger trx --verbosity detailed

# Exécuter les tests avec couverture de code
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

**Types de tests inclus** :
- ✅ Tests unitaires des modèles
- ✅ Tests d'intégration des repositories
- ✅ Tests de liaison WPF (View-ViewModel)
- ✅ Tests de persistance SQLite

---

## 📦 Création d'un installateur

L'application inclut un script pour générer un **installateur Windows** (.exe).

### Prérequis pour créer l'installateur
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) installé
- .NET 10 SDK installé

### Étapes pour créer l'installateur

1. **Exécuter le script PowerShell** :
   ```powershell
   .\Installer\Build-Installer.ps1
   ```

2. **Ce que fait le script** :
   - Publie l'application en version autonome pour Windows x64
   - Met à jour le numéro de version dans le script Inno Setup
   - Compile le script Inno Setup (`SuiviPortefolio.iss`)
   - Génère l'installateur dans `Artifacts\Setup`

3. **Résultat** :
   - Fichier : `Artifacts\Setup\SuiviPortefolio_Setup.exe`
   - Installateur autonome prêt à être distribué

### Personnaliser l'installateur

Modifier le fichier `Installer\SuiviPortefolio.iss` pour :
- Changer le nom de l'application
- Modifier l'icône de l'installateur
- Ajouter des fichiers supplémentaires
- Configurer les raccourcis

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Voici comment contribuer :

### Rapports de bugs
1. Vérifier que le bug n'est pas déjà rapporté
2. Ouvrir une issue sur GitHub avec :
   - Une description claire du bug
   - Les étapes pour reproduire
   - Le comportement attendu vs. le comportement réel
   - La version de l'application et du .NET SDK
   - Des captures d'écran si utile

### Suggestions de fonctionnalités
1. Ouvrir une issue avec le label "enhancement"
2. Décrire la fonctionnalité souhaitée
3. Expliquer le cas d'usage
4. Proposer une solution si possible

### Contribution au code
1. Forker le dépôt
2. Créer une branche pour votre fonctionnalité (`git checkout -b feature/nouvelle-fonctionnalite`)
3. Faire vos modifications
4. Ajouter des tests pour vos modifications
5. Valider que tous les tests passent
6. Pousser vos modifications (`git push origin feature/nouvelle-fonctionnalite`)
7. Ouvrir une Pull Request

---

## 📜 License

Ce projet est sous license **MIT**.

```
MIT License

Copyright (c) 2024 SuiviPortefolio

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## 📞 Support

Pour toute question ou problème :
- Consulter la [documentation officielle](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- Ouvrir une issue sur le dépôt GitHub
- Contacter l'équipe de développement

---

## 📝 Historique des versions

| Version | Date | Description |
|---------|------|-------------|
| 1.0.0 | 2024-XX-XX | Version initiale |
| 1.0.1 | 2024-XX-XX | Corrections de bugs |
| 1.1.0 | 2024-XX-XX | Ajout des graphiques |

---

## 🎯 Roadmap

### Prochaines fonctionnalités
- [ ] Synchronisation cloud (OneDrive, Google Drive)
- [ ] Import depuis des fichiers CSV/Excel
- [ ] Export des données en PDF
- [ ] Notifications de dividendes à venir
- [ ] Application mobile (Xamarin/MAUI)

### Améliorations prévues
- [ ] Interface utilisateur améliorée
- [ ] Performances optimisées
- [ ] Plus de devises supportées
- [ ] Intégration avec des APIs financières supplémentaires

---

<p align="center">
  Made with ❤️ and C#
</p>
