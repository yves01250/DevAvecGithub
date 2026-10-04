# 📋 Exemples de tests pour SuiviPortefolio

> **Date** : 03/10/2026  
> **Auteur** : Mistral Vibe (Assistant IA)  
> **Projet** : SuiviPortefolio

---

## 📌 Introduction

Ce document contient des **exemples de tests unitaires et d'intégration** pour compléter la couverture de tests de ton projet **SuiviPortefolio**. Ces exemples sont **prêts à l'emploi** et peuvent être copiés directement dans ton projet.

> **📁 Emplacement recommandé** : Place ces fichiers dans le dossier `Tests/` de ton projet.

---

---

## 📊 Structure des tests proposés

| **Fichier** | **Type de tests** | **Classe testée** | **Méthodes couvertes** |
|-------------|------------------|-------------------|----------------------|
| `Tests/ComptesTests/CompteRepositoryMissingMethodsTests.cs` | Tests d'intégration | `CompteRepository` | `GetById`, `Delete`, `SetDefault`, `Exists` |
| `Tests/ComptesTests/CompteViewModelTests.cs` | Tests unitaires | `CompteViewModel` | Commandes, propriétés, `CanExecute` |
| `Tests/InfrastructureTests/RelayCommandTests.cs` | Tests unitaires | `RelayCommand` | Constructeur, `CanExecute`, `Execute`, `RaiseCanExecuteChanged` |
| `Tests/InfrastructureTests/CotationFetcherTests.cs` | Tests unitaires | `CotationFetcher` | `FetchSnapshotAsync` |

---

---

## 📌 1. Tests pour CompteRepository (méthodes manquantes)

**Fichier** : `Tests/ComptesTests/CompteRepositoryMissingMethodsTests.cs`

### 🎯 Objectif
Compléter la couverture des méthodes **CRUD** de `CompteRepository` :
- `GetById` : Récupérer un compte par son ID
- `Delete` : Supprimer un compte (avec vérification des contraintes)
- `SetDefault` : Définir un compte comme compte par défaut
- `Exists` : Vérifier si un compte existe

### 📝 Code complet

Voir le fichier **`Tests/ComptesTests/CompteRepositoryMissingMethodsTests.cs`** déjà créé dans ton projet.

### 🔍 Exemples de tests inclus

#### Test de `GetById`
```csharp
[Fact]
public void GetById_RetourneLeCompteCorrespondant()
{
    var repository = new CompteRepository(_databasePath);
    var compteId = repository.Add(new Compte { CpteNom = "Test" });
    var result = repository.GetById(compteId);
    Assert.NotNull(result);
    Assert.Equal("Test", result.CpteNom);
}
```

#### Test de `Delete` avec vérification des références
```csharp
[Fact]
public void Delete_LanceExceptionSiCompteEstReference()
{
    // Ajoute une position qui référence le compte
    // ...
    var exception = Assert.Throws<InvalidOperationException>(() => repository.Delete(compteId));
    Assert.Contains("ne peut pas être supprimé car il contient des positions", exception.Message);
}
```

#### Test de `SetDefault`
```csharp
[Fact]
public void SetDefault_DefinitLeCompteCommeDefautEtDesactiveLesAutres()
{
    var compte1Id = repository.Add(new Compte { CpteEstDefaut = true });
    var compte2Id = repository.Add(new Compte { CpteEstDefaut = false });
    repository.SetDefault(compte2Id);
    
    var compte1 = repository.GetById(compte1Id);
    var compte2 = repository.GetById(compte2Id);
    Assert.False(compte1!.CpteEstDefaut);
    Assert.True(compte2!.CpteEstDefaut);
}
```

---

---

## 📌 2. Tests pour CompteViewModel

**Fichier** : `Tests/ComptesTests/CompteViewModelTests.cs`

### 🎯 Objectif
Tester la **logique métier** du ViewModel avec **Moq** :
- Initialisation des commandes
- Chargement des comptes
- Gestion de `CanExecute`
- Appels au repository

> **⚠ Requiert le package NuGet Moq** :
> ```xml
> <PackageReference Include="Moq" Version="4.20.71" />
> ```

### 📝 Code complet

Voir le fichier **`Tests/ComptesTests/CompteViewModelTests.cs`** déjà créé dans ton projet.

### 🔍 Exemples de tests inclus

#### Test d'initialisation
```csharp
[Fact]
public void Constructor_InitialiseLesCommandes()
{
    var viewModel = new CompteViewModel(mockRepository.Object, mockCotationFetcher.Object);
    Assert.NotNull(viewModel.AjouterCompteCommand);
    Assert.NotNull(viewModel.ModifierCompteCommand);
}
```

#### Test de `CanExecute`
```csharp
[Fact]
public void ModifierCompteCommand_CanExecute_RetourneFalseSiAucunCompteSelectionne()
{
    viewModel.CompteSelectionne = null;
    Assert.False(viewModel.ModifierCompteCommand.CanExecute(null));
}
```

#### Test avec Moq
```csharp
[Fact]
public async Task DefinirCompteDefautAsync_AppelleRepositorySetDefault()
{
    var compte = new Compte { CpteId = 1 };
    viewModel.CompteSelectionne = compte;
    mockRepository.Setup(r => r.SetDefault(1)).Verifiable();
    
    await viewModel.DefinirCompteDefautAsync();
    mockRepository.Verify(r => r.SetDefault(1), Times.Once);
}
```

---

---

## 📌 3. Tests pour RelayCommand

**Fichier** : `Tests/InfrastructureTests/RelayCommandTests.cs`

### 🎯 Objectif
Tester le comportement de `RelayCommand` :
- Constructeur
- `CanExecute` (avec et sans predicate)
- `Execute` (async et sync)
- `CanExecuteChanged`

### 📝 Code complet

Voir le fichier **`Tests/InfrastructureTests/RelayCommandTests.cs`** déjà créé dans ton projet.

Ce fichier inclut **aussi une version centralisée de `RelayCommand`** que tu peux utiliser dans tout ton projet.

### 🔍 Exemples de tests inclus

#### Test du constructeur
```csharp
[Fact]
public void Constructor_WithAsyncExecute_InitialiseCorrectement()
{
    var command = new RelayCommand(_ => Task.CompletedTask);
    Assert.NotNull(command);
    Assert.True(command.CanExecute(null));
}
```

#### Test de `CanExecute` avec predicate
```csharp
[Fact]
public void CanExecute_RetourneFalseSiPredicateRetourneFalse()
{
    var command = new RelayCommand(_ => Task.CompletedTask, _ => false);
    Assert.False(command.CanExecute(null));
}
```

#### Test de `Execute` async
```csharp
[Fact]
public async Task Execute_AttendLaTacheAsync()
{
    var taskCompleted = false;
    var command = new RelayCommand(async _ =>
    {
        await Task.Delay(10);
        taskCompleted = true;
    });
    
    command.Execute(null);
    await Task.Delay(20);
    Assert.True(taskCompleted);
}
```

---

---

## 📌 4. Tests pour CotationFetcher

**Fichier** : `Tests/InfrastructureTests/CotationFetcherTests.cs`

### 🎯 Objectif
Tester la récupération des cotations avec **Mock HttpClient** :
- Réponse valide
- Symbole invalide
- JSON invalide
- Gestion des exceptions

> **⚠ Requiert les packages NuGet** :
> ```xml
> <PackageReference Include="Moq" Version="4.20.71" />
> ```

### 📝 Code complet

Voir le fichier **`Tests/InfrastructureTests/CotationFetcherTests.cs`** déjà créé dans ton projet.

Ce fichier inclut **aussi une implémentation exemple de `CotationFetcher`** que tu peux adapter.

### 🔍 Exemples de tests inclus

#### Test avec réponse valide
```csharp
[Fact]
public async Task FetchSnapshotAsync_RetourneCotationValide()
{
    var mockHandler = CreateMockHttpMessageHandler(
        HttpStatusCode.OK,
        new { chart = new { result = new[] { new { 
            meta = new { currency = "EUR" },
            indicators = new { quote = new[] { new { close = new[] { 100.50m } } } }
        } } });
    
    var httpClient = new HttpClient(mockHandler.Object);
    var fetcher = new CotationFetcher(httpClient);
    var result = await fetcher.FetchSnapshotAsync("EURUSD=X");
    
    Assert.NotNull(result);
    Assert.Equal("EUR", result.Devise);
    Assert.Equal(100.50m, result.Close);
}
```

#### Test avec symbole invalide
```csharp
[Fact]
public async Task FetchSnapshotAsync_RetourneNullSiSymboleInvalide()
{
    var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.NotFound, null);
    var httpClient = new HttpClient(mockHandler.Object);
    var fetcher = new CotationFetcher(httpClient);
    
    var result = await fetcher.FetchSnapshotAsync("INVALID");
    Assert.Null(result);
}
```

#### Test avec JSON invalide
```csharp
[Fact]
public async Task FetchSnapshotAsync_RetourneNullSiReponseInvalide()
{
    var mockHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, "Invalid JSON");
    var httpClient = new HttpClient(mockHandler.Object);
    var fetcher = new CotationFetcher(httpClient);
    
    var result = await fetcher.FetchSnapshotAsync("EURUSD=X");
    Assert.Null(result);
}
```

---

---

## 📌 Comment exécuter ces tests ?

### 1. Ajouter les packages NuGet nécessaires

Dans ton fichier **`SuiviPortefolio.Tests.csproj`**, ajoute :

```xml
<ItemGroup>
    <PackageReference Include="Moq" Version="4.20.71" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.10.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.0">
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        <PrivateAssets>all</PrivateAssets>
    </PackageReference>
</ItemGroup>
```

### 2. Exécuter les tests

- **Via Visual Studio** : Ouvre le **Test Explorer** (Test > Test Explorer) et clique sur **Run All**
- **Via CLI** : Dans le dossier `Tests/`, exécute :
  ```bash
  dotnet test
  ```

---

---

## 📌 Bonnes pratiques pour les tests

### 1. Nommage des tests
- Utilise le format **`Méthode_Scenario_ResultatAttendu`**
- Exemple : `Delete_SupprimeLeCompteEtRetourneTrue`

### 2. Structure AAA
- **Arrange** : Prépare les données et les mocks
- **Act** : Appelle la méthode à tester
- **Assert** : Vérifie le résultat

### 3. Isolation
- Chaque test doit être **indépendant** des autres
- Utilise `IDisposable` pour nettoyer les ressources (ex: bases de données temporaires)

### 4. Mock des dépendances
- Utilise **Moq** pour simuler les dépendances externes (HttpClient, Repository, etc.)
- Exemple : `var mockRepository = new Mock<ICompteRepository>();`

### 5. Tests rapides
- Évite les opérations lentes dans les tests unitaires
- Utilise des bases de données **en mémoire** (SQLite) pour les tests d'intégration

---

---

## 📌 Prochaines étapes

1. **Copier les fichiers** dans ton projet `Tests/`
2. **Ajouter Moq** via NuGet
3. **Exécuter les tests** pour vérifier qu'ils passent
4. **Adapter les interfaces** (`ICotationFetcher`) si elles n'existent pas dans ton projet
5. **Compléter les tests** avec tes propres scénarios

---

> **💡 Astuce** : Tu peux **étendre** ces exemples en ajoutant d'autres scénarios de test (ex: tests de validation, tests d'erreur, etc.).

---

> **📌 Note** : Ces fichiers sont **déjà créés** dans ton projet. Vérifie leur existence dans `Tests/ComptesTests/` et `Tests/InfrastructureTests/`.
