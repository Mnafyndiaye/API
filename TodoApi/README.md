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
- MongoDB local (ou Docker)

## Lancement en local (sans Docker)
1. Demarrer MongoDB local sur `mongodb://localhost:27017`.
2. Se placer dans le dossier du projet.
3. Executer:
   - `dotnet restore`
   - `dotnet run --launch-profile https`
4. Tester les endpoints via le fichier `TodoApi.http`.

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
