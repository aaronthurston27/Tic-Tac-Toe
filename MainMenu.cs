using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    #region main variables

    public List<Sprite> availableSymbols = new List<Sprite>(); // List of available symbol sprites.

    public Image player1Sprite; // Chosen sprite of player 1.

    public Image player2Sprite; // Chosen sprite of player 2.

    private int chosenGridSize = 3; // Current grid size.

    public Button gridSizeButton; // Button showing the grid size.

    #endregion

    #region monobehavior methods

    // Use this for initialization
    void Start () {

        Overseer.ChangeGameState(Overseer.GameState.Main_Menu);
        if (player1Sprite != null)
            player1Sprite.sprite = availableSymbols[(int)Random.Range(0, availableSymbols.Count)];
        if (player2Sprite != null)
            player2Sprite.sprite = availableSymbols[(int)Random.Range(0, availableSymbols.Count)];

        ChangePlayer1Image();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    #endregion

    #region methods

    /// <summary>
    /// Switch the selected sprite of Player 1.
    /// </summary>
    public void ChangePlayer1Image()
    {
        int index = 0;
        if(player1Sprite != null)
        {
            index = availableSymbols.IndexOf(player1Sprite.sprite); // Current position in list of player 1.
            player1Sprite.sprite = availableSymbols[(index + 1) % availableSymbols.Count]; // Increment index.
            if (player1Sprite.sprite == player2Sprite.sprite) // If we have the same symbol as player 2, we should skip the next symbol.
                player1Sprite.sprite = availableSymbols[(index + 2) % availableSymbols.Count];
        }
    }


    /// <summary>
    /// Switch the selected sprite of Player 2.
    /// </summary>
    public void ChangePlayer2Image()
    {
        int index = 0;
        if (player1Sprite != null)
        {
            index = availableSymbols.IndexOf(player2Sprite.sprite);
            player2Sprite.sprite = availableSymbols[(index + 1) % availableSymbols.Capacity];
            if (player2Sprite.sprite == player1Sprite.sprite)
                player2Sprite.sprite = availableSymbols[(index + 2) % availableSymbols.Capacity];
        }
    }

    /// <summary>
    /// Change grid size of main menu button and game.
    /// </summary>
    public void ChangeGridSize()
    {
        
        if (chosenGridSize == 3)
            chosenGridSize = 4;
        else
            chosenGridSize = 3;
        if(gridSizeButton != null)
            gridSizeButton.GetComponentInChildren<Text>().text = chosenGridSize + "X" + chosenGridSize; // Change display text of grid size button.

    }

    /// <summary>
    /// Start game from main menu. Load game scene.
    /// </summary>
    public void StartGame()
    {
        Overseer.ChangeGridSize(chosenGridSize); // Set the grid size of the grid in the game scene.
        Overseer.SetPlayerSymbols(new Sprite[2] { player1Sprite.sprite, player2Sprite.sprite }); // Based on the symbols chosen by the player in the menu, save and send to the Overseer for use in the game scene.
        UnityEngine.SceneManagement.SceneManager.LoadScene("TicTacToe-Game"); // Load the game scene.
    }

    #endregion
}
