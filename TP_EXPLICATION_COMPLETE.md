# 📚 TP COMPLET - GESTION DES HÔTELS AVEC ASP.NET CORE MVC

---

## 📋 TABLE DES MATIÈRES

1. [Introduction](#introduction)
2. [Architecture Générale](#architecture-générale)
3. [Les Modèles (Entities)](#les-modèles)
4. [La Base de Données](#la-base-de-données)
5. [Configuration](#configuration)
6. [Les Contrôleurs](#les-contrôleurs)
7. [Les Vues](#les-vues)
8. [Sécurité](#sécurité)
9. [Relations BD](#relations-bd)
10. [Étapes Résumées](#étapes-résumées)
11. [Concepts Clés](#concepts-clés)
12. [Questions du Professeur](#questions-du-professeur)

---

## 🎯 Introduction <a name="introduction"></a>

Ce TP implémente une **application Web complète** pour gérer une base de données d'hôtels et leurs appréciations.

**Technologie utilisée:**
- Framework: ASP.NET Core MVC (.NET 10)
- ORM: Entity Framework Core 10.0.2
- Base de données: SQL Server Express LocalDB
- Frontend: Razor Views + Bootstrap CSS
- Architecture: Code-First

---

## 🏗️ Architecture Générale <a name="architecture-générale"></a>

### Qu'est-ce que le pattern MVC ?

**MVC = Model-View-Controller**

C'est un **pattern architectural** qui sépare l'application en **3 parties distinctes**:

```
┌─────────────────────────────────────────┐
│         UTILISATEUR (Navigateur)        │
└─────────────────────────────────────────┘
           ↓                    ↑
       Request              Response
           ↓                    ↑
┌─────────────────────────────────────────┐
│            CONTROLLER                   │ ← Traite les requêtes HTTP
│   (HotelsController,                    │   Appelle les modèles
│    AppreciationsController)             │   Sélectionne les vues
└─────────────────────────────────────────┘
           ↓                    ↑
        Récupère              Envoie
        données               données
           ↓                    ↑
┌─────────────────────────────────────────┐
│            MODEL                        │ ← Représente les données
│   (Hotel.cs,                            │   Contient la logique métier
│    Appreciation.cs,                     │   Utilise DbContext pour la BD
│    HotellerieDbContext.cs)              │
└─────────────────────────────────────────┘
           ↓
      Base de données
    (SQL Server LocalDB)
           ↓
┌─────────────────────────────────────────┐
│            VIEW                         │ ← Affiche les données
│   (*.cshtml files)                      │   HTML + Razor syntax
│   (Index.cshtml, Create.cshtml, etc.)   │
└─────────────────────────────────────────┘
```

**En résumé simple:**
- **Model** = Les données et comment on les manipule
- **View** = Comment on les affiche à l'écran
- **Controller** = Le "chef d'orchestre" qui demande les données et choisit quoi afficher

---

## 📦 Les Modèles (Entities) <a name="les-modèles"></a>

### Qu'est-ce qu'une Entity ?

Une **Entity** (entité) est une **classe C# qui représente une table de la base de données**.

Chaque **propriété publique** devient une **colonne** dans la table.

### Classe Hotel.cs

```csharp
public class Hotel
{
    // PRIMARY KEY - Clé Primaire
    public int Id { get; set; }  // Id auto-incrémenté (1, 2, 3...)

    // PROPRIÉTÉS AVEC VALIDATIONS - Data Annotations
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [StringLength(20, MinimumLength = 3, 
        ErrorMessage = "Le nom doit avoir entre 3 et 20 caractères.")]
    public string Nom { get; set; }

    [Range(1, 5, ErrorMessage = "Les étoiles doivent être entre 1 et 5.")]
    public int Etoiles { get; set; }

    [Required(ErrorMessage = "La ville est obligatoire.")]
    public string Ville { get; set; }

    [Required(ErrorMessage = "Le site web est obligatoire.")]
    [Url(ErrorMessage = "Le site web doit être une URL valide.")]
    public string SiteWeb { get; set; }

    // PROPRIÉTÉS OPTIONNELLES - Nullable
    public string? Tel { get; set; }   // ? = peut être null
    public string? Pays { get; set; }

    // RELATION ONE-TO-MANY
    public virtual ICollection<Appreciation>? Appreciations { get; set; }
}
```

**Vocabulaire technique important:**

| Terme | Explication |
|-------|-------------|
| **Primary Key (Clé Primaire)** | Identifiant unique pour chaque hôtel (Id). Pas deux hôtels ne peuvent avoir le même Id |
| **[Required]** | Cette propriété est obligatoire. Si vide, validation échoue |
| **[StringLength]** | Limite le nombre de caractères |
| **[Range]** | Limite une valeur numérique entre min et max |
| **[Url]** | Valide que c'est une URL correcte (commence par http://) |
| **Data Annotations** | Les attributs [Required], [Range], etc. Ce sont des **métadonnées** qui décrivent les règles de validation |
| **Nullable Type** | `string?` signifie que cette propriété peut être NULL (vide) |
| **virtual ICollection** | Relation One-to-Many. Un hôtel peut avoir plusieurs appréciations |

### Classe Appreciation.cs

```csharp
public class Appreciation
{
    public int Id { get; set; }  // PRIMARY KEY

    [Required(ErrorMessage = "Le nom de la personne est obligatoire.")]
    [Display(Name = "Nom Personne")]  // Label du formulaire
    public string NomPers { get; set; }

    [Required(ErrorMessage = "Le commentaire est obligatoire.")]
    [DataType(DataType.MultilineText)]  // TextArea HTML
    public string Commentaire { get; set; }

    [Range(1, 10, ErrorMessage = "La note doit être entre 1 et 10.")]
    public int Note { get; set; } = 5;  // Valeur par défaut = 5

    // FOREIGN KEY - Clé Étrangère
    public int HotelId { get; set; }  // Référence à Hotel

    // NAVIGATION PROPERTY
    public virtual Hotel? Hotel { get; set; }  // L'hôtel associé
}
```

**Nouveaux termes:**

| Terme | Explication |
|-------|-------------|
| **[Display(Name = "...")]** | Le label affiché dans le formulaire HTML |
| **[DataType(DataType.MultilineText)]** | Affiche une `<textarea>` au lieu d'un `<input type="text">` |
| **Foreign Key (Clé Étrangère)** | `HotelId` = référence l'Id d'un Hotel. Crée une relation entre les tables |
| **Navigation Property** | `Hotel` = permet d'accéder directement à l'objet Hotel associé |
| **Valeur par défaut** | `= 5;` signifie que si on ne précise pas la Note, elle sera 5 |

---

## 💾 La Base de Données <a name="la-base-de-données"></a>

### Qu'est-ce que DbContext ?

**DbContext** est une **classe qui représente une session de travail avec la base de données**.

C'est comme un **pont entre votre code C# et la base de données SQL**.

```csharp
public class HotellerieDbContext : DbContext
{
    // CONSTRUCTEUR - Reçoit les options de configuration
    public HotellerieDbContext(DbContextOptions<HotellerieDbContext> options) 
        : base(options) { }

    // DbSets = Collections des entités
    // Chaque DbSet<T> représente une TABLE dans la BD
    public virtual DbSet<Hotel> Hotels { get; set; }           // Table Hotels
    public virtual DbSet<Appreciation> Appreciations { get; set; }  // Table Appreciations
}
```

**Vocabulaire:**

| Terme | Explication |
|-------|-------------|
| **DbSet<Hotel>** | Représente la table `Hotels` dans la BD. Vous l'utilisez pour SELECT, INSERT, UPDATE, DELETE |
| **DbContextOptions** | Configuration: chaîne de connexion, provider SQL Server, etc. |
| **virtual DbSet** | Permet à EF Core de "surcharger" le comportement (pattern de conception appelé **Virtual Pattern**) |

### Entity Framework Core (EF Core)

**EF Core** est un **ORM (Object-Relational Mapping)**.

**ORM = Traduit les objets C# en requêtes SQL** (et vice-versa)

```
C# Code:                          SQL Auto-généré:
_context.Hotels.ToList()   →      SELECT * FROM Hotels;
_context.Hotels
  .FirstOrDefault(h => h.Id == 1) → SELECT * FROM Hotels WHERE Id = 1;
_context.Hotels.Add(hotel);       → INSERT INTO Hotels (Nom, Etoiles...) VALUES (...)
_context.SaveChangesAsync()       → Exécute les changements en BD
```

### Code-First vs Database-First

**Code-First:** Vous écrivez d'abord les classes C#, puis EF Core crée la BD

```
Vous créez Hotel.cs, Appreciation.cs
       ↓
EF Core analyse les classes
       ↓
EF Core génère automatiquement les migrations
       ↓
Les migrations créent les tables SQL
```

**C'est ce que nous avons fait dans ce TP.**

### Migrations - Qu'est-ce que c'est ?

Une **migration** est un **fichier C# qui décrit les changements à la base de données**.

**Exemple:**

```csharp
// Migration: 20260404164751_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Crée la table Hotels
        migrationBuilder.CreateTable(
            name: "Hotels",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nom = table.Column<string>(maxLength: 20, nullable: false),
                Etoiles = table.Column<int>(nullable: false),
                Ville = table.Column<string>(nullable: false),
                SiteWeb = table.Column<string>(nullable: false),
                Tel = table.Column<string>(nullable: true),
                Pays = table.Column<string>(nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Hotels", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Annule la création
        migrationBuilder.DropTable(name: "Hotels");
    }
}
```

**Vocabulaire:**

| Terme | Explication |
|-------|-------------|
| **Up()** | Les changements à appliquer (CREATE TABLE, ADD COLUMN, etc.) |
| **Down()** | Annuler les changements (DROP TABLE, DELETE COLUMN, etc.) |
| **Annotation("SqlServer:Identity", "1, 1")** | Auto-incrémentation: 1, 2, 3... |
| **maxLength: 20** | Colonne VARCHAR(20) - max 20 caractères |
| **nullable: false** | NOT NULL - obligatoire |
| **nullable: true** | NULL autorisé - optionnel |
| **PrimaryKey** | Clé primaire: l'Id unique |

### Étapes des migrations que nous avons faites

```
Étape 1: InitialCreate
         Crée les tables Hotels et Appreciations

Étape 2: ajoutTel
         (Vide - Tel était déjà défini dans Hotel.cs)

Étape 3: AjoutNote
         (Vide - Note était déjà défini dans Appreciation.cs)

Étape 4: AjoutPaysHotel
         (Vide - Pays était déjà défini dans Hotel.cs)
```

**Commandes terminal utilisées:**

```powershell
# Créer une migration
dotnet-ef migrations add InitialCreate

# Appliquer les migrations à la BD
dotnet-ef database update

# Voir toutes les migrations appliquées
dotnet-ef migrations list
```

---

## ⚙️ Configuration <a name="configuration"></a>

### Program.cs - Point d'entrée

C'est le **point d'entrée** de l'application. C'est là qu'on **configure tout**.

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==== ÉTAPE 1: Ajouter les services à la conteneur ====
builder.Services.AddControllersWithViews();

// ==== ÉTAPE 2: Configurer Entity Framework Core ====
var connectionString = builder.Configuration
    .GetConnectionString("HotellerieConnection");

builder.Services.AddDbContext<HotellerieDbContext>(options =>
    options.UseSqlServer(connectionString));

// Construire l'application
var app = builder.Build();

// ==== ÉTAPE 3: Configurer le pipeline HTTP ====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// ==== ÉTAPE 4: Configurer le Routing ====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Hotels}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```

**Vocabulaire:**

| Terme | Explication |
|-------|-------------|
| **Dependency Injection (DI)** | `AddDbContext` = enregistrer DbContext dans le conteneur. Les controllers reçoivent automatiquement une instance |
| **Services Container** | `builder.Services` = conteneur qui gère les objets et leurs dépendances |
| **Configuration** | `builder.Configuration` = lire les paramètres depuis appsettings.json |
| **Pipeline HTTP** | L'ordre dans lequel les middlewares traitent les requêtes |
| **Routing Pattern** | `{controller=Hotels}/{action=Index}/{id?}` = URL format avec paramètres |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "HotellerieConnection": 
      "Server=(localdb)\\mssqllocaldb;Database=HotellerieDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Explication de la chaîne de connexion:**

| Partie | Explication |
|--------|-------------|
| `Server=(localdb)\mssqllocaldb` | Le serveur = LocalDB (instance SQL Server locale) |
| `Database=HotellerieDb` | Le nom de la BD = HotellerieDb |
| `Trusted_Connection=True` | Authentification Windows (pas de mot de passe) |
| `MultipleActiveResultSets=true` | Permet plusieurs requêtes simultanées |

---

## 🎮 Les Contrôleurs <a name="les-contrôleurs"></a>

### Qu'est-ce qu'un Controller ?

Un **Controller** est une **classe C# qui traite les requêtes HTTP**.

**Quand l'utilisateur clique sur un lien ou soumet un formulaire → le Controller le gère.**

### Classe HotelsController.cs

```csharp
public class HotelsController : Controller
{
    private readonly HotellerieDbContext _context;

    // ==== INJECTION DE DÉPENDANCE ====
    public HotelsController(HotellerieDbContext context)
    {
        _context = context;
    }

    // ============================================
    // 1. INDEX - Afficher la liste des hôtels
    // ============================================
    // GET /Hotels/Index
    public async Task<IActionResult> Index()
    {
        var hotels = await _context.Hotels.ToListAsync();
        return View(hotels);
    }

    // ============================================
    // 2. DETAILS - Afficher les détails d'un hôtel
    // ============================================
    // GET /Hotels/Details/5 (5 = l'Id)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var hotel = await _context.Hotels
            .Include(h => h.Appreciations)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (hotel == null) return NotFound();
        return View(hotel);
    }

    // ============================================
    // 3. CREATE GET - Afficher le formulaire
    // ============================================
    // GET /Hotels/Create
    public IActionResult Create()
    {
        return View();
    }

    // ============================================
    // 4. CREATE POST - Traiter le formulaire soumis
    // ============================================
    // POST /Hotels/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Hotel hotel)
    {
        if (ModelState.IsValid)
        {
            _context.Add(hotel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(hotel);
    }

    // ============================================
    // 5. EDIT GET - Afficher le formulaire d'édition
    // ============================================
    // GET /Hotels/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null) return NotFound();
        return View(hotel);
    }

    // ============================================
    // 6. EDIT POST - Traiter les modifications
    // ============================================
    // POST /Hotels/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Hotel hotel)
    {
        if (id != hotel.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(hotel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HotelExists(hotel.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(hotel);
    }

    // ============================================
    // 7. DELETE GET - Demander la confirmation
    // ============================================
    // GET /Hotels/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);
        if (hotel == null) return NotFound();
        return View(hotel);
    }

    // ============================================
    // 8. DELETE POST - Supprimer
    // ============================================
    // POST /Hotels/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel != null)
        {
            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // ==== MÉTHODE UTILITAIRE ====
    private bool HotelExists(int id)
    {
        return _context.Hotels.Any(e => e.Id == id);
    }
}
```

**Vocabulaire important:**

| Terme | Explication |
|-------|-------------|
| **public class HotelsController : Controller** | Hérite de Controller = accès aux méthodes MVC |
| **private readonly** | Variable privée (invisible de l'extérieur) et immuable |
| **Injection de Dépendance (DI)** | Constructor reçoit DbContext automatiquement du framework |
| **async/await** | Asynchrone = ne bloque pas le thread. Améliore la performance |
| **Task<IActionResult>** | Action retourne une Task qui produit un IActionResult |
| **IActionResult** | Interface = résultat d'une action (View, Redirect, NotFound, etc.) |
| **[HttpPost]** | Accepte SEULEMENT les requêtes POST (formulaires) |
| **[HttpGet]** | Par défaut - accepte GET (clics sur liens) |
| **[ValidateAntiForgeryToken]** | Sécurité: vérifie un token CSRF dans le formulaire |
| **ModelState** | Dictionnaire contenant les erreurs de validation |
| **ToListAsync()** | Récupère tous les enregistrements (SELECT *) |
| **FirstOrDefaultAsync()** | Récupère LE PREMIER enregistrement |
| **FindAsync(id)** | Cherche par clé primaire (fastest) |
| **SaveChangesAsync()** | Exécute les changements en BD (INSERT, UPDATE, DELETE) |

### Actions GET vs POST

```
GET  = Récupérer des données (cliquer un lien)
POST = Envoyer des données (soumettre un formulaire)

Pattern dans ce TP:
Create GET  → Afficher formulaire vide
Create POST → Traiter les données du formulaire

Edit GET    → Afficher formulaire pré-rempli
Edit POST   → Traiter les modifications

Delete GET  → Demander confirmation
Delete POST → Exécuter la suppression
```

---

## 🎨 Les Vues <a name="les-vues"></a>

### Qu'est-ce qu'une vue ?

Une **vue** est un **fichier .cshtml** = **HTML + Razor syntax**.

**Razor syntax** = Code C# à l'intérieur du HTML

```html
<!-- HTML normal -->
<h1>Hotels</h1>

<!-- Razor: @ = exécute du code C# -->
<p>Total: @Model.Count()</p>

@foreach (var item in Model)
{
    <tr>
        <td>@item.Nom</td>
        <td>@item.Etoiles / 5</td>
    </tr>
}
```

### Exemple: Hotels/Index.cshtml

```razor
@model IEnumerable<Hotellerie_X.Models.HotellerieModel.Hotel>

@{
    ViewData["Title"] = "Hotels";
}

<div class="container mt-4">
    <h1>@ViewData["Title"]</h1>

    <a asp-action="Create" class="btn btn-primary">Créer un nouvel hôtel</a>

    @if (Model.Any())
    {
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Nom</th>
                    <th>Étoiles</th>
                    <th>Ville</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Model)
                {
                    <tr>
                        <td>@Html.DisplayFor(m => item.Nom)</td>
                        <td>@Html.DisplayFor(m => item.Etoiles)</td>
                        <td>@Html.DisplayFor(m => item.Ville)</td>
                        <td>
                            <a asp-action="Details" asp-route-id="@item.Id">Détails</a>
                            <a asp-action="Edit" asp-route-id="@item.Id">Éditer</a>
                            <a asp-action="Delete" asp-route-id="@item.Id">Supprimer</a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }
    else
    {
        <p>Aucun hôtel.</p>
    }
</div>
```

**Vocabulaire:**

| Terme | Explication |
|-------|-------------|
| **@model** | Déclare le type du Model passé par le Controller |
| **IEnumerable<T>** | Collection d'objets (List, Array, etc.) |
| **@{...}** | Bloc de code C# pur |
| **ViewData["Title"]** | Dictionnaire pour passer des données à la Layout |
| **@foreach** | Boucle sur chaque item du Model |
| **@Html.DisplayFor()** | Affiche la valeur en HTML échappé (sécurité) |
| **asp-action** | Tag Helper: génère automatiquement l'URL /Controller/Action |
| **asp-route-id** | Paramètre d'URL: /Hotels/Details/5 |

### Exemple: Hotels/Create.cshtml

```razor
@model Hotellerie_X.Models.HotellerieModel.Hotel

@{
    ViewData["Title"] = "Create";
}

<h1>Créer un nouvel hôtel</h1>

<form asp-action="Create" method="post">
    @if (ViewData.ModelState.IsValid == false)
    {
        <div asp-validation-summary="All" class="text-danger"></div>
    }

    <div class="mb-3">
        <label asp-for="Nom" class="form-label"></label>
        <input asp-for="Nom" class="form-control" />
        <span asp-validation-for="Nom" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="Etoiles" class="form-label"></label>
        <input asp-for="Etoiles" type="number" min="1" max="5" class="form-control" />
        <span asp-validation-for="Etoiles" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Créer</button>
</form>
```

**Comment ça marche:**

```
1. Utilisateur remplit le formulaire
2. Soumet (POST)
3. asp-action="Create" → POST /Hotels/Create
4. Controller vérifie ModelState (validations)
5. Si valide: sauvegarder en BD → rediriger
6. Si invalide: réafficher le formulaire avec les erreurs
```

**Erreurs affichées:**

```
Si [Required] et vide → "Le nom est obligatoire."
Si [StringLength(20, Min=3)] et "AB" → "entre 3 et 20 caractères"
Si [Range(1,5)] et "10" → "entre 1 et 5"
```

---

## 🔐 Sécurité <a name="sécurité"></a>

### CSRF Token

**CSRF = Cross-Site Request Forgery**

```html
<form asp-action="Create" method="post">
    <!-- Le framework ajoute automatiquement le token CSRF -->
</form>
```

**Pourquoi?**

```
Attaque CSRF:
1. Utilisateur se connecte à votre site
2. Il visite un site malveillant
3. Le site malveillant soumet un formulaire à votre site
4. Votre serveur fait confiance car l'utilisateur est connecté
5. Perte de données

Protection CSRF:
- Générer un token aléatoire
- Mettre le token dans le formulaire
- Vérifier le token côté serveur
- Si absent ou invalide: rejeter
```

### XSS Protection

**XSS = Cross-Site Scripting**

```csharp
// DANGEREUX:
@Model.Nom  // Si Nom = "<script>alert('hack')</script>"
            // Le script s'exécute!

// SÛR:
@Html.DisplayFor(m => m.Nom)  // Échappe le HTML
```

---

## 🔗 Relation One-to-Many <a name="relations-bd"></a>

### Concept

```
Un Hôtel ──→ Plusieurs Appréciations

Hotel 1
  ├─ Appreciation 1
  ├─ Appreciation 2
  └─ Appreciation 3

Hotel 2
  ├─ Appreciation 4
  └─ Appreciation 5
```

### Implémentation

**Dans Hotel.cs:**
```csharp
public virtual ICollection<Appreciation>? Appreciations { get; set; }
```

**Dans Appreciation.cs:**
```csharp
public int HotelId { get; set; }  // Foreign Key
public virtual Hotel? Hotel { get; set; }  // Navigation
```

**Dans la migration:**
```csharp
table.ForeignKey(
    name: "FK_Appreciations_Hotels_HotelId",
    column: x => x.HotelId,
    principalTable: "Hotels",
    principalColumn: "Id",
    onDelete: ReferentialAction.Cascade  // Si on supprime l'hôtel,
                                          // supprimer aussi les appréciations
);
```

**Vocabulaire:**

| Terme | Explication |
|-------|-------------|
| **One-to-Many** | 1 hôtel, plusieurs appréciations |
| **Foreign Key** | Colonne qui référence la clé primaire d'une autre table |
| **Cascade Delete** | Supprimer l'hôtel = supprimer aussi ses appréciations |
| **Navigation Property** | `Hotel` property = accéder à l'objet lié sans requête SQL supplémentaire |
| **Include()** | Charger les données liées en même temps (Eager Loading) |

---

## 📋 Étapes Résumées <a name="étapes-résumées"></a>

### Étape 1: Créer les Modèles
```
Créer Hotel.cs avec propriétés et validations
Créer Appreciation.cs avec FK vers Hotel
Créer HotellerieDbContext
```

### Étape 2: Configurer la BD
```
Ajouter la chaîne de connexion dans appsettings.json
Enregistrer DbContext dans Program.cs
Installer LocalDB
```

### Étape 3: Créer les Migrations
```
dotnet-ef migrations add InitialCreate
dotnet-ef database update
```

### Étape 4: Créer les Controllers
```
HotelsController avec 8 actions (Index, Details, Create, Edit, Delete)
AppreciationsController avec même pattern
Chaque action gère GET et POST
```

### Étape 5: Créer les Vues
```
Pour Hotels: Index, Create, Edit, Delete, Details
Pour Appreciations: Index, Create, Edit, Delete, Details
Utiliser Razor syntax, Tag Helpers, Bootstrap CSS
```

### Étape 6: Configurer la Navigation
```
Ajouter les liens dans _Layout.cshtml
Modifier la route par défaut dans Program.cs
```

### Étape 7: Tester et Déboguer
```
Compiler le projet (run_build)
Relancer l'application (F5)
Tester CRUD opérations
```

---

## 🎓 Concepts Clés <a name="concepts-clés"></a>

### Pattern MVC
- Séparation des responsabilités
- Model = données + logique
- View = affichage
- Controller = traitement requêtes

### Entity Framework Core
- ORM = traduit objets C# en SQL
- Code-First = classes d'abord, BD après
- DbContext = session de travail avec BD
- Migrations = suivi des changements BD

### Validation des données
- Data Annotations = [Required], [Range], [StringLength]
- Client-side = HTML5 (navigateur)
- Server-side = C# (serveur) - IMPORTANT!

### Sécurité
- CSRF Token = protection formulaires
- HTML Escaping = protection XSS
- SQL Injection = pas de risk avec EF Core (paramètres)

### Async/Await
- Non-bloquant = mieux pour performance
- ToListAsync() vs ToList()
- SaveChangesAsync() vs SaveChanges()

### Routing
- Pattern: {controller=Hotels}/{action=Index}/{id?}
- URL = /Hotels/Index/5
- Tag Helpers génèrent automatiquement

---

## ❓ Questions du Professeur <a name="questions-du-professeur"></a>

### Q1: Pourquoi utiliser Entity Framework Core ?

**Réponse:**
C'est un ORM qui traduit automatiquement les requêtes C# en SQL. C'est plus rapide à développer et plus sûr que du SQL brut (pas de SQL injection). Exemple:

```csharp
_context.Hotels.Where(h => h.Etoiles >= 4).ToListAsync()
// Est automatiquement traduit en:
// SELECT * FROM Hotels WHERE Etoiles >= 4;
```

### Q2: Quelle est la différence entre Code-First et Database-First ?

**Réponse:**
- **Code-First:** On écrit les classes C# d'abord, EF Core génère automatiquement les migrations et crée la BD
- **Database-First:** On crée la BD d'abord, puis EF Core génère les classes C# à partir de la BD

Nous avons utilisé Code-First car c'est plus flexible pour le développement.

### Q3: À quoi servent les migrations ?

**Réponse:**
Les migrations tracent les changements à la BD. Elles permettent de:
- Versioner la BD (comme Git pour la base de données)
- Collaborer en équipe (chacun crée une migration)
- Revenir en arrière si besoin (dotnet-ef database update -Migration PreviousMigration)
- Déployer en production en confiance

### Q4: Pourquoi async/await ?

**Réponse:**
Pour ne pas bloquer le thread. Quand on attend la BD, le serveur peut traiter d'autres requêtes. Ça améliore la performance sous charge.

```csharp
// Sans async - bloque le thread
var hotels = _context.Hotels.ToList();

// Avec async - ne bloque pas
var hotels = await _context.Hotels.ToListAsync();
```

### Q5: Qu'est-ce qu'une Foreign Key ?

**Réponse:**
Une colonne qui référence la clé primaire d'une autre table. Elle crée une relation et assure l'intégrité referentielle.

```csharp
// Appreciation.HotelId référence Hotel.Id
public int HotelId { get; set; }  // Foreign Key
public virtual Hotel? Hotel { get; set; }  // Navigation

// Si on essaie de créer une Appreciation avec HotelId = 999
// (et aucun Hotel n'a Id=999), la BD rejette
```

### Q6: Comment fonctionne la validation ?

**Réponse:**
Les Data Annotations ([Required], [Range]) décrivent les règles. ASP.NET valide:
1. **Côté client** (HTML5) - rapide mais on ne peut pas faire confiance
2. **Côté serveur** (C#) - obligatoire pour la sécurité

```csharp
[Required(ErrorMessage = "...")]
[Range(1, 5)]
public int Etoiles { get; set; }

// Formulaire HTML5 valide côté client
// Mais un attaquant peut contourner avec un proxy
// Donc le serveur DOIT toujours valider:
if (ModelState.IsValid)
{
    // Sûr, les données sont validées
}
```

### Q7: Pourquoi le Token CSRF ?

**Réponse:**
Protection contre les attaques CSRF. Un attaquant ne peut pas soumettre un formulaire à votre site sans connaître le token. Le token est unique par utilisateur et par session.

```html
<form asp-action="Create" method="post">
    <!-- Le framework ajoute automatiquement:
         <input name="__RequestVerificationToken" value="...random..." />
    -->
</form>

// Côté serveur:
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Hotel hotel)
{
    // Vérifie le token avant de continuer
}
```

### Q8: Qu'est-ce qu'une Navigation Property ?

**Réponse:**
Une propriété qui représente une relation. `Hotel.Appreciations` = liste des appréciations d'un hôtel. Permet d'accéder aux données liées facilement sans écrire de requête SQL.

```csharp
// Sans navigation property:
var hotel = _context.Hotels.FirstOrDefault(h => h.Id == 1);
var appreciations = _context.Appreciations
    .Where(a => a.HotelId == hotel.Id)
    .ToList();

// Avec navigation property:
var hotel = _context.Hotels
    .Include(h => h.Appreciations)
    .FirstOrDefault(h => h.Id == 1);
var appreciations = hotel.Appreciations;  // Déjà chargées!
```

### Q9: Qu'est-ce que la Dependency Injection ?

**Réponse:**
Un pattern qui permet à un objet de recevoir ses dépendances au lieu de les créer lui-même.

```csharp
// SANS DI - mauvais:
public class HotelsController : Controller
{
    public HotelsController()
    {
        _context = new HotellerieDbContext();  // Crée lui-même
    }
}

// AVEC DI - bon:
public class HotelsController : Controller
{
    public HotelsController(HotellerieDbContext context)
    {
        _context = context;  // Reçoit une instance
    }
}
// Le framework crée et passe l'instance automatiquement
```

Avantages:
- Testable (on peut passer un mock)
- Flexible (on peut changer l'implémentation)
- Loose coupling (pas de dépendance directe)

### Q10: Qu'est-ce que le Routing ?

**Réponse:**
Le système qui mappe les URLs aux Controllers et Actions.

```
URL: https://localhost:7073/Hotels/Index/5
↓
Pattern: {controller}/{action}/{id}
↓
Controller: Hotels
Action: Index
Id: 5
↓
Appelle: HotelsController.Index(5)
```

Configuration dans Program.cs:
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Hotels}/{action=Index}/{id?}");
    //         ↑ défaut   ↑ défaut   ↑ optionnel
```

---

## 🎓 Conclusion

Vous avez implémenté une **application Web MVC complète** avec:

✅ **Architecture MVC** - Séparation des responsabilités  
✅ **ORM (EF Core)** - Gestion BD sans SQL brut  
✅ **CRUD complet** - Create, Read, Update, Delete  
✅ **Validations** - Data Annotations  
✅ **Relations BD** - One-to-Many  
✅ **Sécurité** - CSRF Token, HTML Escaping  
✅ **Async** - Meilleure performance  
✅ **Bootstrap CSS** - Interface responsive  

Cela montre que vous maîtrisez les **concepts fondamentaux du développement web avec ASP.NET Core**.

**Bonne chance pour la présentation!** 🎓

---

**Auteur:** GitHub Copilot  
**Date:** 2026  
**Framework:** ASP.NET Core MVC (.NET 10)  
**Version:** 1.0
