using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayTile : PlayGrid {

    #region main variables

    public Player associatedPlayer; // The player who has clicked this tile. If null, then the tile is uninitialized.

    private Sprite symbol; // Sprite symbol of the tile.

    public Image spriteImage;  // UI image

    private int row; // Row in the play grid.

    private int column; // Column in the play grid.

    private Button tileButton; // UI Button element. When clicked, initializes the tile.

    #endregion

    #region methods

    /// <summary>
    /// Initialize tile by saving its row and column in the play grid (for searching grid for matches)., and setting variables.
    /// </summary>
    public void InitializeTile(int gridRow, int gridColumn)
    {
        row = gridRow;
        column = gridColumn;
        associatedPlayer = null;
        if (tileButton == null)
            tileButton = GetComponent<Button>();
        tileButton = GetComponent<Button>();
        tileButton.onClick.AddListener(OnButtonPress);
        if (Overseer.Instance.debugMode) // We don't want to interact with tiles during debug mode, so set this to false.
            tileButton.interactable = false;
    }

    /// <summary>
    /// When we click the tile, set it to the current player's symbol. We also want to check for a winner, update the tiles set, and add to our bookkeeping list.
    /// </summary>
    public void OnButtonPress()
    {
        if (Overseer.Instance.gameStarted) //  Only initialize a tile if the game has started.
        {
            tileButton.interactable = false; // We don't want to be able to click this tile again.
            spriteImage = GetComponent<Image>();
            associatedPlayer = Overseer.Instance.GetCurrentPlayer(); // Set the unitialized tile to the current players symbol (whichever player's turn it is).
            spriteImage.sprite = associatedPlayer.playerSymbol;
            bool winner = Overseer.Instance.mainGrid.checkForWinner(this.row, this.column, this.associatedPlayer); // Check for a winner on the game board.
            Overseer.Instance.ChangePlayer(); // Switch players.
            if (Overseer.Instance.gameStarted) // If someone wins during the final move, it isnt a draw. So we dont want to call this method.
            {
                Overseer.Instance.mainGrid.UpdateTilesInitialized();
            }
            Overseer.Instance.AddToMovesList(new Overseer.PlayerMoveEntry(Overseer.Instance.mainGrid.GetMoveNumber(), associatedPlayer, row, column, winner, associatedPlayer.playerSymbol)); // Add to bookkeeping list.
        }
    }
    #endregion

}
