# MonsterTradingCardsGame

Tobias Kastl, Semester 3 / A1, if22b007

# Git-Repo



# Docker-Setup fuer Datenbank

docker run --name SWE_MTCG -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=MTCG -p 5432:5432 -v pgdata:/var/lib/postgresql/data postgres

# Edits in Curl

- IDs were changed to regular Integers
- "Name" was changed to "Username"
- Curl 3, 6: Added "region" and "card_type" to every Json-Package
- Curl 11: IDs
- Curl 14: Updated path to new Usernames (was "kienboec", is now "Kienboeck", since Username was updated)
- Curl 20, 21: Removed since Trading-Feature was not implemented    