@echo off
REM MTCG API Test Script for Windows
setlocal EnableDelayedExpansion

echo MTCG Testing Script
echo ===================

REM Wait for server to be ready
timeout /t 1

REM Store the base URL
set BASE_URL=http://localhost:10001

REM ##################
REM # User Management
REM ##################

echo.
echo 1) Register Users
echo Registering User1...
curl -X POST %BASE_URL%/users -H "Content-Type: application/json" -d "{\"Username\":\"user1\", \"Password\":\"password123\"}"
echo.
echo Registering User2...
curl -X POST %BASE_URL%/users -H "Content-Type: application/json" -d "{\"Username\":\"user2\", \"Password\":\"password123\"}"
echo.

echo.
echo 2) Login Users
echo Logging in User1:
curl -X POST %BASE_URL%/sessions -H "Content-Type: application/json" -d "{\"Username\":\"user1\", \"Password\":\"password123\"}"
echo.
echo Logging in User2:
curl -X POST %BASE_URL%/sessions -H "Content-Type: application/json" -d "{\"Username\":\"user2\", \"Password\":\"password123\"}"
echo.

REM Prompt for tokens
echo.
echo Please enter the tokens from the login responses above:
set /p USER1_TOKEN="Enter User1 token: "
set /p USER2_TOKEN="Enter User2 token: "

echo.
echo Testing with tokens:
echo User1 Token: %USER1_TOKEN%
echo User2 Token: %USER2_TOKEN%
echo.

:MENU
echo.
echo Choose a test to run (or 0 to exit):
echo ================================
echo 1. Get User Data (User1)
echo 2. Get User Data (User2)
echo 3. Update User Data (User1)
echo 4. Update User Data (User2)
echo 5. Buy Card Package (User1)
echo 6. Buy Card Package (User2)
echo 7. Show Cards (User1)
echo 8. Show Cards (User2)
echo 9. Show Deck (User1)
echo 10. Show Deck (User2)
echo 11. Configure Deck (User1)
echo 12. Configure Deck (User2)
echo 13. Show Trading Deals
echo 14. Create Trading Deal (User1)
echo 15. Create Trading Deal (User2)
echo 16. Execute Trade (User1)
echo 17. Execute Trade (User2)
echo 18. Enter Battle (User1)
echo 19. Enter Battle (User2)
echo 20. Show Stats (User1)
echo 21. Show Stats (User2)
echo 22. Show Scoreboard
echo 0. Exit
echo.

set /p CHOICE="Enter your choice (0-22): "

if "%CHOICE%"=="0" goto END
if "%CHOICE%"=="1" goto GET_USER1_DATA
if "%CHOICE%"=="2" goto GET_USER2_DATA
if "%CHOICE%"=="3" goto UPDATE_USER1_DATA
if "%CHOICE%"=="4" goto UPDATE_USER2_DATA
if "%CHOICE%"=="5" goto BUY_PACKAGE_USER1
if "%CHOICE%"=="6" goto BUY_PACKAGE_USER2
if "%CHOICE%"=="7" goto SHOW_CARDS_USER1
if "%CHOICE%"=="8" goto SHOW_CARDS_USER2
if "%CHOICE%"=="9" goto SHOW_DECK_USER1
if "%CHOICE%"=="10" goto SHOW_DECK_USER2
if "%CHOICE%"=="11" goto CONFIGURE_DECK_USER1
if "%CHOICE%"=="12" goto CONFIGURE_DECK_USER2
if "%CHOICE%"=="13" goto SHOW_TRADES
if "%CHOICE%"=="14" goto CREATE_TRADE_USER1
if "%CHOICE%"=="15" goto CREATE_TRADE_USER2
if "%CHOICE%"=="16" goto EXECUTE_TRADE_USER1
if "%CHOICE%"=="17" goto EXECUTE_TRADE_USER2
if "%CHOICE%"=="18" goto BATTLE_USER1
if "%CHOICE%"=="19" goto BATTLE_USER2
if "%CHOICE%"=="20" goto SHOW_STATS_USER1
if "%CHOICE%"=="21" goto SHOW_STATS_USER2
if "%CHOICE%"=="22" goto SHOW_SCOREBOARD

echo Invalid choice!
goto MENU

:GET_USER1_DATA
echo.
echo Getting User1 data...
curl -X GET %BASE_URL%/users/user1 -H "Authorization: %USER1_TOKEN%"
goto MENU

:GET_USER2_DATA
echo.
echo Getting User2 data...
curl -X GET %BASE_URL%/users/user2 -H "Authorization: %USER2_TOKEN%"
goto MENU

:UPDATE_USER1_DATA
echo.
echo Updating User1 data...
curl -X PUT %BASE_URL%/users/user1 -H "Authorization: %USER1_TOKEN%" -H "Content-Type: application/json" -d "{\"Bio\":\"Player 1 bio\", \"Image\":\":-)\"}"
goto MENU

:UPDATE_USER2_DATA
echo.
echo Updating User2 data...
curl -X PUT %BASE_URL%/users/user2 -H "Authorization: %USER2_TOKEN%" -H "Content-Type: application/json" -d "{\"Bio\":\"Player 2 bio\", \"Image\":\":-D\"}"
goto MENU

:BUY_PACKAGE_USER1
echo.
echo User1 buying card package...
curl -X POST %BASE_URL%/cards/packages -H "Authorization: %USER1_TOKEN%"
goto MENU

:BUY_PACKAGE_USER2
echo.
echo User2 buying card package...
curl -X POST %BASE_URL%/cards/packages -H "Authorization: %USER2_TOKEN%"
goto MENU

:SHOW_CARDS_USER1
echo.
echo Showing User1's cards...
curl -X GET %BASE_URL%/cards -H "Authorization: %USER1_TOKEN%"
goto MENU

:SHOW_CARDS_USER2
echo.
echo Showing User2's cards...
curl -X GET %BASE_URL%/cards -H "Authorization: %USER2_TOKEN%"
goto MENU

:SHOW_DECK_USER1
echo.
echo Showing User1's deck...
curl -X GET %BASE_URL%/deck -H "Authorization: %USER1_TOKEN%"
goto MENU

:SHOW_DECK_USER2
echo.
echo Showing User2's deck...
curl -X GET %BASE_URL%/deck -H "Authorization: %USER2_TOKEN%"
goto MENU

:CONFIGURE_DECK_USER1
echo.
echo Enter four card IDs for User1's deck (separated by spaces):
set /p CARD_IDS="Card IDs: "
echo Configuring User1's deck...
curl -X PUT %BASE_URL%/deck -H "Authorization: %USER1_TOKEN%" -H "Content-Type: application/json" -d "[%CARD_IDS%]"
goto MENU

:CONFIGURE_DECK_USER2
echo.
echo Enter four card IDs for User2's deck (separated by spaces):
set /p CARD_IDS="Card IDs: "
echo Configuring User2's deck...
curl -X PUT %BASE_URL%/deck -H "Authorization: %USER2_TOKEN%" -H "Content-Type: application/json" -d "[%CARD_IDS%]"
goto MENU

:SHOW_TRADES
echo.
echo Showing all trading deals...
curl -X GET %BASE_URL%/tradings -H "Authorization: %USER1_TOKEN%"
goto MENU

:CREATE_TRADE_USER1
echo.
echo Enter trade details for User1:
set /p CARD_ID="Card ID to trade: "
set /p MIN_DAMAGE="Minimum damage required: "
echo Creating trading deal...
curl -X POST %BASE_URL%/tradings -H "Authorization: %USER1_TOKEN%" -H "Content-Type: application/json" -d "{\"CardId\": %CARD_ID%, \"RequiredType\": \"monster\", \"MinimumDamage\": %MIN_DAMAGE%}"
goto MENU

:CREATE_TRADE_USER2
echo.
echo Enter trade details for User2:
set /p CARD_ID="Card ID to trade: "
set /p MIN_DAMAGE="Minimum damage required: "
echo Creating trading deal...
curl -X POST %BASE_URL%/tradings -H "Authorization: %USER2_TOKEN%" -H "Content-Type: application/json" -d "{\"CardId\": %CARD_ID%, \"RequiredType\": \"spell\", \"MinimumDamage\": %MIN_DAMAGE%}"
goto MENU

:EXECUTE_TRADE_USER1
echo.
set /p TRADE_ID="Enter trading ID: "
set /p CARD_ID="Enter card ID to trade: "
echo User1 executing trade...
curl -X POST %BASE_URL%/tradings/%TRADE_ID% -H "Authorization: %USER1_TOKEN%" -H "Content-Type: application/json" -d "{\"CardId\": %CARD_ID%}"
goto MENU

:EXECUTE_TRADE_USER2
echo.
set /p TRADE_ID="Enter trading ID: "
set /p CARD_ID="Enter card ID to trade: "
echo User2 executing trade...
curl -X POST %BASE_URL%/tradings/%TRADE_ID% -H "Authorization: %USER2_TOKEN%" -H "Content-Type: application/json" -d "{\"CardId\": %CARD_ID%}"
goto MENU

:BATTLE_USER1
echo.
echo User1 entering battle...
curl -X POST %BASE_URL%/battles -H "Authorization: %USER1_TOKEN%"
goto MENU

:BATTLE_USER2
echo.
echo User2 entering battle...
curl -X POST %BASE_URL%/battles -H "Authorization: %USER2_TOKEN%"
goto MENU

:SHOW_STATS_USER1
echo.
echo Showing User1's stats...
curl -X GET %BASE_URL%/stats -H "Authorization: %USER1_TOKEN%"
goto MENU

:SHOW_STATS_USER2
echo.
echo Showing User2's stats...
curl -X GET %BASE_URL%/stats -H "Authorization: %USER2_TOKEN%"
goto MENU

:SHOW_SCOREBOARD
echo.
echo Showing scoreboard...
curl -X GET %BASE_URL%/score -H "Authorization: %USER1_TOKEN%"
goto MENU

echo.
echo Testing completed!
pause
endlocal
