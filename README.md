# Projet C# - Bibliotheque et Meteo

Ce depot contient 2 applications WinForms conformes au sujet:

- `LibraryManagerApp` : gestion d'une bibliotheque avec MySQL.
- `WeatherApp` : application meteo avec API OpenWeatherMap et favoris en JSON.

## 1) Prerequis

- Windows + .NET SDK (le projet cible `net10.0-windows`)
- MySQL Server
- Une cle API OpenWeatherMap ([OpenWeather](https://openweathermap.org/api))

## 2) Initialiser la base de donnees (Bibliotheque)

1. Ouvrir MySQL (Workbench, CLI, etc.).
2. Executer le script `LibraryManagerApp/database.sql`.
3. Modifier la connexion dans `LibraryManagerApp/appsettings.json` :

```json
"LibraryDb": "server=localhost;port=3306;database=library_db;uid=root;pwd=your_password;"
```

## 3) Configurer OpenWeather (Meteo)

1. Ouvrir `WeatherApp/appsettings.json`.
2. Remplacer `PUT_YOUR_OPENWEATHER_API_KEY_HERE` par ta cle API.

## 4) Lancer les applications

Depuis la racine du projet :

```bash
dotnet build CSharpProjet.slnx
dotnet run --project LibraryManagerApp/LibraryManagerApp.csproj
dotnet run --project WeatherApp/WeatherApp.csproj
```

## 5) Fonctionnalites implantees

### Bibliotheque
- Ajouter, modifier, supprimer des livres.
- Afficher la liste complete des livres dans un `DataGridView` stylise.
- Rechercher par titre, auteur, genre, ISBN.
- Colonne d'actions rapides `...` pour modifier/supprimer/copier ISBN.
- Panneau visuel avec apercu de couverture du livre selectionne.
- Recuperation de cover par ISBN via Open Library (sans API key) :
  - URL: `https://covers.openlibrary.org/b/isbn/{ISBN}-L.jpg?default=false`
  - Fallback automatique vers une couverture generee localement si indisponible.
- Mise a jour asynchrone des covers avec annulation des anciennes requetes pour eviter les images qui se melangent.
- Validation des champs + gestion des erreurs.
- Demarrage robuste: l'application peut s'ouvrir meme si MySQL est indisponible, avec message utilisateur.
- Correctif de stabilite du layout (splitter) pour eviter les erreurs au lancement.

### Meteo
- Recherche par ville via bouton ou touche Entree.
- Appel OpenWeatherMap (previsions multi-jours).
- Interface retravaillee (zones recherche, resume meteo, tableau previsions, favoris).
- Affichage temperature, nuages, humidite, vent, description, icone.
- Ajout/suppression de favoris.
- Sauvegarde des favoris en `favorites.json`.
- Boutons redimensionnes/stylises pour une meilleure lisibilite.
- Mode sombre retire pour stabilite.

## 6) Notes importantes

- L'application Bibliotheque utilise MySQL en local; verifier que le serveur est demarre.
- Les covers Open Library necessitent une connexion Internet.
- Si une cover n'existe pas pour un ISBN, un visuel local est affiche automatiquement.

## 7) Preparation pour envoi (sans secrets)

- `LibraryManagerApp/appsettings.json` ne contient pas de vrai mot de passe (placeholder).
- `WeatherApp/appsettings.json` ne contient pas de vraie API key (placeholder).
- Les dossiers `bin/` et `obj/` sont ignores via `.gitignore`.
- Avant execution locale, remplace les placeholders par tes vraies valeurs en local uniquement.
