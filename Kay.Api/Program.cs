using System.Text.Json.Serialization;
using Kay.Api.Hubs;
using Kay.Api.Middleware;
using Kay.Api.Models;
using Kay.Api.Persistance;
using Kay.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Les énumérations (ex. EtatPartie) circulent sous forme de texte ("EnCours") et non de nombre.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    // Requête mal formée (JSON invalide, champ manquant) : même format { erreur } que les erreurs métier.
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = _ =>
        new BadRequestObjectResult(new { erreur = "Requête invalide : vérifiez les données envoyées." }));
builder.Services.AddSignalR()
    .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Strategy : chaque variante de règles est une implémentation de IRegles, déclarée ici et nulle part ailleurs.
// Ajouter une variante = une nouvelle classe + une ligne ; PartieService choisit selon la configuration de la partie.
builder.Services.AddSingleton<IRegles, ReglesKay>();
builder.Services.AddSingleton<IRegles, ReglesKayCouperLaTete>();
builder.Services.AddSingleton<PartieService>();

// Persistance du tableau d'honneur : fichier SQLite à côté de l'API (créé au premier démarrage).
string fichierBase = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Persistance:FichierBaseDeDonnees"] ?? "kay.db");
builder.Services.AddDbContextFactory<KayDbContext>(o => o.UseSqlite($"Data Source={fichierBase}"));
builder.Services.AddSingleton<IDepotResultats, DepotResultatsEf>();
builder.Services.AddSingleton<TableauHonneurService>();
builder.Services.AddExceptionHandler<GestionnaireErreurs>();
builder.Services.AddProblemDetails();

// En développement, le proxy Vite rend les appels « même origine » : CORS ne sert que si le client
// est servi depuis une autre origine (configurée dans appsettings, section Cors:OriginesAutorisees).
string[] origines = builder.Configuration.GetSection("Cors:OriginesAutorisees").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(origines)
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

// Schéma créé s'il n'existe pas. Un seul modèle stable : EnsureCreated suffit (des migrations
// EF Core seraient nécessaires si le schéma devait évoluer avec des données existantes).
using (KayDbContext contexte = app.Services.GetRequiredService<IDbContextFactory<KayDbContext>>().CreateDbContext())
    contexte.Database.EnsureCreated();

app.UseExceptionHandler();

// En développement, la redirection HTTPS casse les requêtes préliminaires CORS et la négociation SignalR.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.MapHub<PartieHub>("/hubs/partie");
app.Run();
