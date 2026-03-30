# Todo API - TP ASP.NET Core + MongoDB

## Objectif couvert
- API ASP.NET Core basee sur un controleur.
- Persistance MongoDB.
- Securisation JWT + roles (`admin`, `user`).
- Conteneurisation Docker (alternative a Azure demandee dans le TP).

## Fonctionnalites de securite
- Authentification JWT via `POST /api/auth/login`.
- Lecture des todos (`GET`) accessible a tout utilisateur authentifie.
- Ecriture (`POST`, `PUT`, `DELETE`) reservee au role `admin`.

## Comptes de test
- Utilisateur normal:
  - username: `user`
  - password: `User123!`
- Administrateur:
  - username: `admin`
  - password: `Admin123!`

## Prerequis
- .NET SDK 10
- Docker Desktop (recommande pour MongoDB)

## Utilisation rapide (recommandee)
1. Ouvrir un terminal dans le dossier du projet.
2. Executer:
  - `docker compose up -d mongo`
  - `dotnet run --launch-profile http`
3. Ouvrir `TodoApi.http` et lancer les requetes.

API locale:
- `http://localhost:5158`

## Utilisation locale detaillee
1. Demarrer MongoDB:
  - Option Docker: `docker compose up -d mongo`
  - Option locale: service MongoDB sur `mongodb://localhost:27017`
2. Restaurer et lancer l'API:
  - `dotnet restore`
  - `dotnet run --launch-profile http`
3. Tester l'authentification:
  - `POST /api/auth/login` avec `user/User123!`
  - `POST /api/auth/login` avec `admin/Admin123!`
4. Tester les autorisations:
  - `GET /api/todoitems` avec token `user` -> autorise
  - `POST/PUT/DELETE /api/todoitems` avec token `user` -> refuse (403)
  - `POST/PUT/DELETE /api/todoitems` avec token `admin` -> autorise

## Demo automatique (prof)
Pour executer la demonstration complete en une commande:
- `powershell -ExecutionPolicy Bypass -File .\demo-prof.ps1`

Le script:
- demarre MongoDB si necessaire,
- verifie le login `user` et `admin`,
- verifie les protections (401/403/201/200),
- affiche un resume final.

## Lancement avec Docker
Depuis le dossier du projet, executer:
- `docker compose up --build`

API disponible sur:
- `http://localhost:8080`

## Endpoints principaux
- `POST /api/auth/login`
- `GET /api/todoitems` (authentifie)
- `GET /api/todoitems/{id}` (authentifie)
- `POST /api/todoitems` (admin)
- `PUT /api/todoitems/{id}` (admin)
- `DELETE /api/todoitems/{id}` (admin)

## Livrables pour la remise
1. Depot GitHub public contenant tout le code, y compris Dockerfile et compose.
2. Ce README avec les etapes d'execution.
3. Preuve de reussite du cours "Securisez votre application .NET" a joindre dans votre document de remise (txt/pdf).
