@echo off

REM --------------------------------------------------
REM Monster Trading Cards Game
REM --------------------------------------------------
title Monster Trading Cards Game
echo CURL Testing for Monster Trading Cards Game
echo.

REM --------------------------------------------------
echo 1) Create Users (Registration)
REM Create User
curl -i -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -i -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}"
echo.
curl -i -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"admin\",    \"Password\":\"istrator\"}"
echo.

pause

echo should fail:
curl -i -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -i -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"different\"}"
echo. 
echo.

pause

REM --------------------------------------------------
echo 2) Login Users
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}"
echo.
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"admin\",    \"Password\":\"istrator\"}"
echo.

pause

echo should fail:
curl -i -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"different\"}"
echo.
echo.

pause

REM --------------------------------------------------
echo 3) create packages (done by "admin") - edited
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":1, \"Name\":\"WaterGoblin\", \"region\": 0, \"Damage\": 10, \"card_type\":\"Champion\"}, {\"Id\":2, \"Name\":\"Dragon\", \"region\": 1, \"Damage\": 50, \"card_type\":\"Champion\"}, {\"Id\":3, \"Name\":\"Knight\", \"region\": 2, \"Damage\": 30, \"card_type\":\"Champion\"}, {\"Id\":4, \"Name\":\"Ork\", \"region\": 0, \"Damage\": 45, \"card_type\":\"Champion\"}, {\"Id\":5, \"Name\":\"FireSpell\", \"region\": 1, \"Damage\": 25, \"card_type\":\"Spell\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":6, \"Name\":\"WaterGoblin\", \"region\": 0, \"Damage\": 9, \"card_type\":\"Champion\"}, {\"Id\":7, \"Name\":\"Dragon\", \"region\": 1, \"Damage\": 55, \"card_type\":\"Champion\"}, {\"Id\":8, \"Name\":\"WaterSpell\", \"region\": 2, \"Damage\": 21, \"card_type\":\"Spell\"}, {\"Id\":9, \"Name\":\"Ork\", \"region\": 0, \"Damage\": 55, \"card_type\":\"Champion\"}, {\"Id\":10, \"Name\":\"WaterSpell\", \"region\": 1, \"Damage\": 23, \"card_type\":\"Spell\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":11, \"Name\":\"WaterGoblin\", \"region\": 0, \"Damage\": 11, \"card_type\":\"Champion\"}, {\"Id\":12, \"Name\":\"Dragon\", \"region\": 1, \"Damage\": 70, \"card_type\":\"Champion\"}, {\"Id\":13, \"Name\":\"WaterSpell\", \"region\": 2, \"Damage\": 22, \"card_type\":\"Spell\"}, {\"Id\":14, \"Name\":\"Ork\", \"region\": 0, \"Damage\": 40, \"card_type\":\"Champion\"}, {\"Id\":15, \"Name\":\"RegularSpell\", \"region\": 1, \"Damage\": 28, \"card_type\":\"Spell\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":16, \"Name\":\"WaterGoblin\", \"region\": 0, \"Damage\": 10, \"card_type\":\"Champion\"}, {\"Id\":17, \"Name\":\"Dragon\", \"region\": 1, \"Damage\": 50, \"card_type\":\"Champion\"}, {\"Id\":18, \"Name\":\"WaterSpell\", \"region\": 2, \"Damage\": 20, \"card_type\":\"Spell\"}, {\"Id\":19, \"Name\":\"Ork\", \"region\": 0, \"Damage\": 45, \"card_type\":\"Champion\"}, {\"Id\":20, \"Name\":\"WaterSpell\", \"region\": 1, \"Damage\": 25, \"card_type\":\"Spell\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":21, \"Name\":\"WaterGoblin\", \"region\": 0, \"Damage\": 9, \"card_type\":\"Champion\"}, {\"Id\":22, \"Name\":\"Dragon\", \"region\": 1, \"Damage\": 55, \"card_type\":\"Champion\"}, {\"Id\":23, \"Name\":\"WaterSpell\", \"region\": 2, \"Damage\": 21, \"card_type\":\"Spell\"}, {\"Id\":24, \"Name\":\"Ork\", \"region\": 0, \"Damage\": 55, \"card_type\":\"Champion\"}, {\"Id\":25, \"Name\":\"FireSpell\", \"region\": 1, \"Damage\": 23, \"card_type\":\"Spell\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":26, \"Name\":\"WaterGoblin\", \"region\": 0, \"Damage\": 11, \"card_type\":\"Champion\"}, {\"Id\":27, \"Name\":\"Dragon\", \"region\": 1, \"Damage\": 70, \"card_type\":\"Champion\"}, {\"Id\":28, \"Name\":\"WaterSpell\", \"region\": 2, \"Damage\": 22, \"card_type\":\"Spell\"}, {\"Id\":29, \"Name\":\"Ork\", \"region\": 0, \"Damage\": 40, \"card_type\":\"Champion\"}, {\"Id\":30, \"Name\":\"RegularSpell\", \"region\": 1, \"Damage\": 28, \"card_type\":\"Spell\"}]"
echo.
echo.

pause


REM --------------------------------------------------
echo 4) acquire packages kienboec
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
echo should fail (no money):
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
echo.

pause

REM --------------------------------------------------
echo 5) acquire packages altenhof
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo should fail (no package):
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo.

pause

REM --------------------------------------------------
echo 6) add new packages
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":26, \"Name\":\"WaterGoblin\", \"Damage\": 10.0, \"card_type\":\"Champion\"}, {\"Id\":27, \"Name\":\"RegularSpell\", \"Damage\": 50.0, \"card_type\":\"Spell\"}, {\"Id\":28, \"Name\":\"Knight\", \"Damage\": 20.0, \"card_type\":\"Champion\"}, {\"Id\":29, \"Name\":\"RegularSpell\", \"Damage\": 45.0, \"card_type\":\"Spell\"}, {\"Id\":30, \"Name\":\"FireElf\", \"Damage\": 25.0, \"card_type\":\"Champion\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":31, \"Name\":\"WaterGoblin\", \"Damage\": 9.0, \"card_type\":\"Champion\"}, {\"Id\":32, \"Name\":\"FireSpell\", \"Damage\": 55.0, \"card_type\":\"Spell\"}, {\"Id\":33, \"Name\":\"Knight\", \"Damage\": 21.0, \"card_type\":\"Champion\"}, {\"Id\":34, \"Name\":\"FireSpell\", \"Damage\": 55.0, \"card_type\":\"Spell\"}, {\"Id\":35, \"Name\":\"FireElf\", \"Damage\": 23.0, \"card_type\":\"Champion\"}]"
echo.
curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":36, \"Name\":\"WaterGoblin\", \"Damage\": 11.0, \"card_type\":\"Champion\"}, {\"Id\":37, \"Name\":\"Dragon\", \"Damage\": 70.0, \"card_type\":\"Champion\"}, {\"Id\":38, \"Name\":\"Knight\", \"Damage\": 22.0, \"card_type\":\"Champion\"}, {\"Id\":39, \"Name\":\"WaterSpell\", \"Damage\": 40.0, \"card_type\":\"Spell\"}, {\"Id\":40, \"Name\":\"FireElf\", \"Damage\": 28.0, \"card_type\":\"Champion\"}]"
echo.
echo.

pause

REM --------------------------------------------------
echo 7) acquire newly created packages altenhof
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo should fail (no money):
curl -i -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo.

pause

REM --------------------------------------------------
echo 8) show all acquired cards kienboec
curl -i -X GET http://localhost:10001/cards --header "Authorization: Bearer kienboec-mtcgToken"
echo should fail (no token)
curl -i -X GET http://localhost:10001/cards 
echo.
echo.

pause

REM --------------------------------------------------
echo 9) show all acquired cards altenhof
curl -i -X GET http://localhost:10001/cards --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 10) show unconfigured deck
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 11) configure deck - edited
curl -i -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d "[1, 2, 3, 4]"
echo.
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer kienboec-mtcgToken"
echo.

curl -i -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "[36, 37, 38, 39]"
echo.
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

echo should fail and show original from before:
curl -i -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "[1, 2, 3, 4]"
echo.
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.
echo should fail ... only 3 cards set
curl -i -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "[36, 37, 38]"
echo.

pause

REM --------------------------------------------------
echo 12) show configured deck 
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -i -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 13) show configured deck different representation
echo kienboec
curl -i -X GET http://localhost:10001/deck?format=plain --header "Authorization: Bearer kienboec-mtcgToken"
echo.
echo.
echo altenhof
curl -i -X GET http://localhost:10001/deck?format=plain --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 14) edit user data
echo.
curl -i -X GET http://localhost:10001/users/kienboec --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -i -X GET http://localhost:10001/users/altenhof --header "Authorization: Bearer altenhof-mtcgToken"
echo.
curl -i -X PUT http://localhost:10001/users/kienboec --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d "{\"Username\": \"Kienboeck\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
echo.
curl -i -X PUT http://localhost:10001/users/altenhof --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "{\"Username\": \"Altenhofer\", \"Bio\": \"me codin...\",  \"Image\": \":-D\"}"
echo.
curl -i -X GET http://localhost:10001/users/Kienboeck --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -i -X GET http://localhost:10001/users/Altenhofer --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

echo should fail:
curl -i -X GET http://localhost:10001/users/Altenhofer --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -i -X GET http://localhost:10001/users/Kienboeck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
curl -i -X PUT http://localhost:10001/users/Kienboeck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "{\"Name\": \"Hoax\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
echo.
curl -i -X PUT http://localhost:10001/users/Altenhofer --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d "{\"Name\": \"Hoax\", \"Bio\": \"me codin...\",  \"Image\": \":-D\"}"
echo.
curl -i -X GET http://localhost:10001/users/someGuy  --header "Authorization: Bearer kienboec-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 15) stats
curl -i -X GET http://localhost:10001/stats --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -i -X GET http://localhost:10001/stats --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 16) scoreboard
curl -i -X GET http://localhost:10001/scoreboard --header "Authorization: Bearer kienboec-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 17) battle - edit (scoreboard)
start /b "kienboec battle" curl -i -X POST http://localhost:10001/battles --header "Authorization: Bearer kienboec-mtcgToken"

curl -i -X GET http://localhost:10001/scoreboard --header "Authorization: Bearer kienboec-mtcgToken"

::start /b "altenhof battle" curl -i -X POST http://localhost:10001/battles --header "Authorization: Bearer altenhof-mtcgToken"
::ping localhost -n 10 >NUL 2>NUL

pause

REM --------------------------------------------------
echo 18) Stats 
echo kienboec
curl -i -X GET http://localhost:10001/stats --header "Authorization: Bearer kienboec-mtcgToken"
echo.
echo altenhof
curl -i -X GET http://localhost:10001/stats --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo 19) scoreboard
curl -i -X GET http://localhost:10001/scoreboard --header "Authorization: Bearer kienboec-mtcgToken"
echo.
echo.

pause

REM --------------------------------------------------
echo end...

REM this is approx a sleep 
ping localhost -n 100 >NUL 2>NUL
@echo on
