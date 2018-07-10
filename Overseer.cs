using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Overseer : MonoBehaviour {

    #region game variables

    public bool gameStarted = false; // Holds true if the game has started or not.

    public PlayGrid mainGrid; // Grid object that holds play tiles.

    [SerializeField]
    public Player[] players = new Player[2]; // Players active.

    private Player currentTurn; // Current player turn.

    private int currentPlayerIndex; // Index into player array showing which player is current selecting a tile.

    public Image[] playerUISymbols = new Image[2]; // Player chosen symbols.

    public Text turnDisplay; // Text displaying current player turn.

    public RectTransform EndGamePanel; // Panel displaying end game options (Restart & Return to menu)

    private Canvas gameCanvas; // The canvas parent of our game. The Overseer should not be a child of this.

    private RectTransform canvasRectTransform; // Rect transform of our game canvas. Used for calculations of image positions.

    // List used to store moves made throughout game.
    private List<PlayerMoveEntry> movesMade;

    [HideInInspector]
    public bool runningDebugTest = false; // Are we currently doing any debug testing coroutines? If so, wait until the first coroutine is done to start another.

    public bool debugMode = false;
    [SerializeField]
    private Text debugText;
    #endregion

    #region static references and variables

    // Overseer Singleton.
    private static  Overseer instance;

    public static Overseer Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<Overseer>();

            return instance;
        }
    }

    public enum GameState
    {
        Main_Menu,
        Game,
        End_Game,
        Debug
    }

    private static GameState gameState; // current state of the game

    /// <summary>
    /// Change the state of the game. Used when switching back and forth between the menu and game, or playing the game and ending.
    /// </summary>
    public static void ChangeGameState(GameState newState)
    {
        gameState = newState;
    }

    private static int GameGridSize = 3; // Size of the Game Grid. Default = 3.

    /// <summary>
    /// Change the grid size to be used in the actual game. Called from the main menu, and should be either 3 or 4.
    /// </summary>
    public static void ChangeGridSize(int newGridSize)
    {
        if (newGridSize == 3 || newGridSize == 4)
            GameGridSize = newGridSize;
    }

    public static int GetGridSize()
    {
        return Overseer.GameGridSize;
    }

    private static Sprite[] GamePlayerSymbols = new Sprite[2]; // Player Symbols to be used in the game.

    /// <summary>
    /// Set the player symbols to use in the actual game. This method should be called from the main menu.
    /// </summary>
    public static void SetPlayerSymbols(Sprite[] newSymbols)
    {
        for(int i = 0; i < 2; i++)
        {
            if(newSymbols[i] != null)
            {
                GamePlayerSymbols[i] = newSymbols[i];
            }
        }
    }

    #endregion

    #region monobehavior methods
    // Use this for initialization
    void Start () {
        if (SceneManager.GetActiveScene().name == "TicTacToe-Menu")
            gameState = GameState.Main_Menu;
        else if (SceneManager.GetActiveScene().name == "TicTacToe-Game")
            gameState = GameState.Game;

        StartGame();
        if (FindObjectOfType<Canvas>())
        {
            gameCanvas = FindObjectOfType<Canvas>(); // We need to find and initialize the game canvas for image position calculations.
            canvasRectTransform = gameCanvas.gameObject.GetComponent<RectTransform>();
        }
	}

    #endregion

    #region game methods

    /// <summary>
    /// Begin game session.
    /// </summary>
    private void StartGame()
    {
        gameStarted = true;

        movesMade = new List<PlayerMoveEntry>(); // Restart bookkeeping list.

        mainGrid.InitializeGrid(Overseer.GameGridSize);
        mainGrid.InitializeTiles();
        
        // Set player symbols.
        if (playerUISymbols[0] != null)
            playerUISymbols[0].sprite = Overseer.GamePlayerSymbols[0];
        if (playerUISymbols[1] != null)
            playerUISymbols[1].sprite = Overseer.GamePlayerSymbols[1];

        players[0].playerSymbol = Overseer.GamePlayerSymbols[0]; 
        players[1].playerSymbol = Overseer.GamePlayerSymbols[1];

        // Choose a random player to go first.
        currentPlayerIndex = (int)Random.Range(0, 2);
        currentTurn = players[currentPlayerIndex];
        ChangeTurnDisplay();
        // Stop displaying end game panel and display current turn.
        EndGamePanel.gameObject.SetActive(false);
        turnDisplay.gameObject.SetActive(true);

        // Initialize game state
        gameState = GameState.Game;

        if (debugMode == true)
        {
            debugText.gameObject.SetActive(true);
        }
        else
            debugText.gameObject.SetActive(false);

    }

    /// <summary>
    /// Clear the play grid, reinitialize tiles, and begin the game again.
    /// </summary>
    public void RestartGame()
    {
        mainGrid.ClearGrid();
        StartGame();
    }

    /// <summary>
    /// Switch players.
    /// </summary>
    public void ChangePlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        currentTurn = players[currentPlayerIndex];
        ChangeTurnDisplay();
    }

    /// <summary>
    /// Return the player whos turn it is.
    /// </summary>
    public Player GetCurrentPlayer()
    {
        return currentTurn;
    }

    /// <summary>
    /// Announce the winner. If playerWhoWon is null, then it is a draw.
    /// </summary>
    public void AnnounceWinner(Player playerWhoWon)
    {
        if(playerWhoWon != null)
        {
            EndGamePanel.GetComponentInChildren<Text>().text = playerWhoWon.gameObject.name.ToString() + " Wins!";
        }
        else
        {
            EndGamePanel.GetComponentInChildren<Text>().text = "Draw!";
        }
        GameOver();
    }

    /// <summary>
    /// End the game session.
    /// </summary>
    private void GameOver()
    {
        if (!debugMode)
        {
            gameStarted = false;
            turnDisplay.gameObject.SetActive(false);
            EndGamePanel.gameObject.SetActive(true);
            gameState = GameState.End_Game;
        }
        else
        {
            turnDisplay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Switch the display to the side of the current player.
    /// </summary>
    private void ChangeTurnDisplay()
    {
        turnDisplay.transform.position = new Vector2(playerUISymbols[currentPlayerIndex].transform.position.x, playerUISymbols[currentPlayerIndex].transform.position.y + 100);
    }

    /// <summary>
    /// Add move entry struct to moves list for bookkeeping.
    /// </summary>
    public void AddToMovesList(PlayerMoveEntry newMove)
    {
        movesMade.Add(newMove);
    }

    /// <summary>
    /// Exit scene and return to main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("TicTacToe-Menu");
    }

    #endregion

    #region bookkeeping structs

    // Bookkeeping entry. Saves necessary information.
    public struct PlayerMoveEntry
    {
        int moveNumber; // Move number
        Player playerWhoMoved;
        int rowMoved;
        int columnMoved; 
        bool winningMove; // Did this move win the game.
        Sprite playerSymbol; // Sprite the player was using.

        public PlayerMoveEntry(int movenumber = 0, Player player = null, int row = 0, int column = 0, bool winner = false, Sprite symbol = null)
        {
            playerWhoMoved = player;
            rowMoved = row;
            columnMoved = column;
            winningMove = winner;
            playerSymbol = symbol;
            moveNumber = movenumber;
        }

    }

    #endregion

    #region debug methods

    /// <summary>
    /// Start Debug mode.
    /// </summary>
    public void EnterDebugMode()
    {
        Debug.Log("Entering Debug Mode");
        if(debugText != null)
        {
            if (canvasRectTransform != null)
            {
                debugText.rectTransform.anchoredPosition = new Vector2(-canvasRectTransform.rect.width / 2f + 50, -canvasRectTransform.rect.height / 2f); // Set the debug mode text to the bottom-left corner of the screen.
            }
            debugText.gameObject.SetActive(true);
        }
        debugMode = true;
        // Clear the grid and start a new game.
        mainGrid.ClearGrid();
        StartGame();
        currentTurn = null; // We control the current turn of the player using debug.
        currentPlayerIndex = 0; 
        gameState = GameState.Debug; // Update the state.
    }

    /// <summary>
    /// Exit Debug Mode
    /// </summary>
    public void ExitDebugMode() 
    {
        Debug.Log("Exiting Debug Mode");
        debugMode = false;
        if (debugText != null)
            debugText.gameObject.SetActive(false); // Deactivate the debug mode text.
        mainGrid.ClearGrid();
        StartGame();
    }

    /// <summary>
    /// Test the selected row in debug.
    /// </summary>
    public IEnumerator StartRowTest(int playerIndex, int startingRow)
    {
        if (!runningDebugTest)
        {
            RestartGame(); // Restart the game board.
            runningDebugTest = true; // In the debug editor, we do not want to start a test while another test is active.
            currentTurn = players[playerIndex]; // The first player to select a tile will be the player specified in the debug editor.
            currentPlayerIndex = playerIndex;
            Debug.Log("Starting Row Test");
            int currentColumn = 0; // Current column in the row we are testing. We want to insert a tile here.
            while(mainGrid.GetMoveNumber() < GameGridSize * 2 - 1) // After (GameGridSize * 2) - 1, the game should be over.
            {
                if(currentTurn == players[playerIndex]) // If the current player selected is the player we want to win, we should place a tile in the row we specified.
                {
                    mainGrid.Debug_InsertTile(startingRow, currentColumn++);
                }
                else                                    // If it is the other players turn, select a random tile to set. Make sure the tile is not in the row we specified.
                {
                    int randomRow = (int)Random.Range(0, GameGridSize);
                    int randomColumn = (int)Random.Range(0, GameGridSize);

                    while (randomRow == startingRow || mainGrid.IsTileInitialized(randomRow, randomColumn)) // If we happen to randomly choose the row we specified, find another row to select.
                    {
                        randomRow = (int)Random.Range(0, GameGridSize - 1);
                        yield return new WaitForEndOfFrame();                                               // Yield the CoRoutine so we don't hold up the rest of the game.
                    }
                    
                    mainGrid.Debug_InsertTile(randomRow, randomColumn);                                     // Finally, insert the tile we found.
                }
                yield return new WaitForSeconds(.5f);
            }
            Debug.Log("Test Ended");
            runningDebugTest = false;
        }
        yield return null;
    }

    /// <summary>
    /// Test the selected column in debug.
    /// </summary>
    public IEnumerator StartColumnTest(int playerIndex, int startingColumn)
    {
        if (!runningDebugTest)
        {
            RestartGame();
            runningDebugTest = true;
            currentTurn = players[playerIndex];
            currentPlayerIndex = playerIndex;
            Debug.Log("Starting Column Test");
            int currentRow = 0; // Current column in the row we are testing. We want to insert a tile here.
            while (mainGrid.GetMoveNumber() < GameGridSize * 2 - 1)
            {
                if (currentTurn == players[playerIndex])
                {
                    mainGrid.Debug_InsertTile(currentRow++, startingColumn);
                }
                else
                {
                    int randomRow = (int)Random.Range(0, GameGridSize);
                    int randomColumn = (int)Random.Range(0, GameGridSize);

                    while (randomColumn == startingColumn || mainGrid.IsTileInitialized(randomRow, randomColumn))
                    {
                        randomColumn = (int)Random.Range(0, GameGridSize - 1);
                        yield return new WaitForEndOfFrame();
                    }

                    mainGrid.Debug_InsertTile(randomRow, randomColumn);
                }
                yield return new WaitForSeconds(.5f);
            }
            Debug.Log("Test Ended");
            runningDebugTest = false;
        }
        yield return null;
    }

    /// <summary>
    /// Test the diagonal of the game grid in debug.
    /// </summary>
    public IEnumerator StartDiagonalTest(int playerIndex)
    {
        if (!runningDebugTest)
        {
            RestartGame();
            runningDebugTest = true;
            currentTurn = players[playerIndex];
            currentPlayerIndex = playerIndex;
            Debug.Log("Starting Diagonal Test");
            int currentIndex = 0;
            while (mainGrid.GetMoveNumber() < GameGridSize * 2 - 1)
            {
                if (currentTurn == players[playerIndex])
                {
                    mainGrid.Debug_InsertTile(currentIndex, currentIndex++);
                }
                else
                {
                    int randomRow = (int)Random.Range(0, GameGridSize);
                    int randomColumn = (int)Random.Range(0, GameGridSize);

                    while (randomColumn == randomRow || mainGrid.IsTileInitialized(randomRow, randomColumn)) // The randomly selected grid index conflicts when the row equals the column.
                    {
                        randomColumn = (int)Random.Range(0, GameGridSize - 1);
                        yield return new WaitForEndOfFrame();
                    }

                    mainGrid.Debug_InsertTile(randomRow, randomColumn);
                }
                yield return new WaitForSeconds(.5f);
            }
            Debug.Log("Test Ended");
            runningDebugTest = false;
        }
        yield return null;
    }

    public IEnumerator StartSecondDiagonalTest(int playerIndex)
    {
        if (!runningDebugTest)
        {
            RestartGame();
            runningDebugTest = true;
            currentTurn = players[playerIndex];
            currentPlayerIndex = playerIndex;
            Debug.Log("Starting Second Diagonal Test");
            int currentIndex = GameGridSize - 1;
            while (mainGrid.GetMoveNumber() < GameGridSize * 2 - 1)
            {
                if (currentTurn == players[playerIndex])
                {
                    mainGrid.Debug_InsertTile(GameGridSize - currentIndex - 1, currentIndex--);
                }
                else
                {
                    int randomRow = (int)Random.Range(0, GameGridSize);
                    int randomColumn = (int)Random.Range(0, GameGridSize);

                    while ((randomColumn + randomRow == GameGridSize - 1) || mainGrid.IsTileInitialized(randomRow, randomColumn)) // The randomly selected grid index conflicts when the row equals the column.
                    {
                        randomColumn = (int)Random.Range(0, GameGridSize - 1);
                        yield return new WaitForEndOfFrame();
                    }

                    mainGrid.Debug_InsertTile(randomRow, randomColumn);
                }
                yield return new WaitForSeconds(.5f);
            }
            Debug.Log("Test Ended");
            runningDebugTest = false;
        }
        yield return null;
    }

    /// <summary>
    /// Test a draw on the game grid in debug.
    /// </summary>
    public IEnumerator StartDrawTest()
    {
        if (!runningDebugTest)
        {
            RestartGame(); // Reset the grid and game tiles.
            runningDebugTest = true;
            currentPlayerIndex = (int)Random.Range(0, 2); // Choose a random player.
            currentTurn = players[currentPlayerIndex];
            Debug.Log("Starting Draw Test");
            int direction = 1;
            int columnOffset = 0; // What column do we want to start from.
            // For even size grids (even # x even #) we start at the outside and make our way inwards as we move down every row. EX: 4 x 4: col 1, col 4, col 2, col 3
            // For odd size grids, the column offset increments by 1 every row. 
            // We also want to switch the direction in which we initialize the tiles. This way, in combination with the changing column offset, we do not get a winner.
            // For a draw, we can technically place the tiles anywhere. To Do: Make sure we don't get an actual winner when doing this.
            for (int i = 0; i < GameGridSize; i++)
            {
                for (int k = 0; k < GameGridSize; k++)
                {
                    int index = (k * direction) + columnOffset; // If direction = -1, we are moving from right to left. We also add the column offset to our index.
                    if (index < 0)
                    {
                        index += GameGridSize; // If we are going from right to left and get a negative number, we need to wrap around by adding the grid size.
                    }
                    mainGrid.Debug_InsertTile(i, (Mathf.Abs(index % GameGridSize))); // Initialize the tile.
                    yield return new WaitForSeconds(.5f); // Yield the Coroutine so we don't hang the rest of the game.
                }
                direction = direction * -1; // Alternate the direction.
                if (GameGridSize % 2 == 0) // If we have an even size grid, alternate ends of the grid and offsetting by 1. Example above.
                    columnOffset = (columnOffset + (GameGridSize - 1 - i)) % GameGridSize;
                else                       // If we have an odd size grid, add 1 to the column offset to start at the next column.
                    columnOffset += 1;
            }
            // Complete testing and return.
            Debug.Log("Testing complete");
            runningDebugTest = false;
        }
        yield return null;
    }

    #endregion
}
