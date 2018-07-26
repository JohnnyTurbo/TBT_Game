# Alpha Test - Update 1 - Patch Notes - 7/14/18

## Main Menu Improvements
1. Back button brings user to the previous menu page
2. Auto login on app launch if user has "stay signed in" check marked
3. Load screen is shown in more areas to avoid the app appearing stuck
4. Users can submit feedback via a feedback form accessible from the user's homepage
5. Fixed an issue where the error text box covered a portion of the screen making the user unable to click buttons on the lower portion of the screen. Notably the "clear" button on the draft team page.

## Game Scene Improvements
1. Back button returns player's to their home screen.
2. "Player 1" and "Player 2" text are displayed at the bottom of the screen to let the player know what team they are on for that specific game. They are colored to match the jerseys of the team they represent.
3. Optomizations to reduce battery and data usage
    - Removed dynamic lighting from the scene
    - Used a flat color for the background
    - When waiting for your turn, the game no longer makes frequent requests to the server to see if your opponent has made their move. Instead only one request is made once the device receives an FCM message indicating it is your turn.
4. Fixed a bug that caused the loading screen to block the bottom two rows of the playing field eventhough the loading screen was not visible.
