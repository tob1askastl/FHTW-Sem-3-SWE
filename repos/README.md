# --- MonsterTradingCardsGame

Tobias Kastl, Semester 3 / A1, if22b007

# --- Git-Repo

https://github.com/tob1askastl/FHTW-Sem-3-SWE
https://github.com/tob1askastl/FHTW-Sem-3-SWE/tree/main/repos/MTCG (Project-Directory)

# --- Information ueber Location der Dateien

README, Protokoll und Curls befinden sich im repos-Verzeichnis; Projekt im Ordner "MTCG"

# --- Docker-Setup fuer Datenbank

docker run --name SWE_MTCG -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=MTCG -p 5432:5432 -v pgdata:/var/lib/postgresql/data postgres

# --- Befehle fuer Docker-CLI

# Log in
docker exec -it MTCG psql -U postgres
# DB auswaehlen
\c MTCG_DB
# Alle Tabellen der DB anzeigen
\dt
# Details fuer Tabelle
\d mtcg_users

# --- Edits im Curl

- IDs were changed to regular Integers
- "Name" was changed to "Username"
- Curl 3, 6: Added "region" and "card_type" to every Json-Package
- Curl 11: IDs
- Curl 14: Updated path to new Usernames (was "kienboec", is now "Kienboeck", since Username was updated)
- Curl 20, 21: Removed since Trading-Feature was not implemented    