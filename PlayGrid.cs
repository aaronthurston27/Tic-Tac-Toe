using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayGrid : MonoBehaviour
{
    #region main variables

    private int gridSize = 3; // size of grid

    private PlayTile[,] Grid; // Multi-Dimensional array representing play grid and all of it's tiles.

    private RectTransform Board; // The canvas where the board is. Used for drawing the grid.

    [SerializeField]
    private PlayTile TilePrefab; // Prefab used to instantiate a tile.

    private float interval; // Interval between tiles and grid lines.
    private float startingPoint; // Top left edge of board.
    [SerializeField]
    private int TilesInitialized; // Number of tiles chosen (set) by our players. If this number equals gridsize*gridsize, then the game is over.
    #endregion

    // Call these methods when starting or restarting the game.
    #region initialization methods

    /// <summary>
    /// Create a new play grid, add tile prefabs, and draw tiles and grid lines.
    /// </summary>
    public void InitializeGrid(int newGridSize)
    {
        gridSize = newGridSize;

        if (gridSize != 3 && gridSize != 4) // Make sure we don't set an invalid grid Size.
            gridSize = 3;

        Grid = new PlayTile[gridSize, gridSize]; // Make a new grid consisting of PlayTiles.
        Board = GetComponentInParent<RectTransform>();
        startingPoint = 0;

        if (gridSize == 4)
            startingPoint = (Board.rect.width / gridSize);
        else if (gridSize == 3)
            startingPoint = Board.rect.width / (2 * (gridSize));

        interval = ((gridSize % 2) * startingPoint) + startingPoint;

        for (int i = 0; i < gridSize - 1; i++)
        {
            // Create a horizontal line to draw.
            GameObject gridHorizontal = new GameObject();
            RectTransform rect = gridHorizontal.AddComponent<RectTransform>();
            gridHorizontal.AddComponent<Image>();
            gridHorizontal.transform.SetParent(GameObject.Find("Grid Lines").transform, true);
            rect.sizeDelta = new Vector2(Board.rect.width, 2); // This line needs to be as wide as the board, and pretty thin.
            rect.anchoredPosition = new Vector2(0, startingPoint - (interval * i)); // Math. We want evenly spaced horizontal lines.
            gridHorizontal.gameObject.name = "Horizontal" + (i+1);

            //c Create a vertical line to draw.
            GameObject gridVertical = new GameObject();
            RectTransform rect2 = gridVertical.AddComponent<RectTransform>();
            gridVertical.AddComponent<Image>();
            gridVertical.transform.SetParent(GameObject.Find("Grid Lines").transform, true);
            rect2.sizeDelta = new Vector2(2, Board.rect.height);
            rect2.anchoredPosition = new Vector2(-(startingPoint) + (interval * i), 0);
            gridVertical.gameObject.name = "Vertical " + (i+1);

        }

    }

    /// <summary>
    /// Instantiate tile prefabs and add to play grid.
    /// </summary>
    public void InitializeTiles()
    {

        for (int i = 0; i < gridSize; i++)
        {
            for (int k = 0; k < gridSize; k++)
            {
                PlayTile Pieces = Instantiate(TilePrefab);
                Pieces.InitializeTile(i, k); // Initializethe tile
                Pieces.transform.SetParent(GameObject.Find("Pieces").transform, true);
                Vector2 tilePosition = new Vector2();
                tilePosition.x = (-Board.rect.width / 2) + ((interval / 2) + (interval * k)); // Space the tile between the grid lines and edge of board.
                tilePosition.y = (Board.rect.height / 2) - ((interval / 2) + (interval * i)); 
                Pieces.GetComponent<RectTransform>().anchoredPosition = tilePosition;
                Grid[i, k] = Pieces; // Make sure we save the new tile in the grid.
            }
        }

    }

    /// <summary>
    /// Clear the grid game objects. This is useful for restarting the game. Future Optimization: Don't destroy prefabs, just go back to uninialized state.
    /// </summary>
    public void ClearGrid()
    {
        for(int i = 0; i < gridSize; i++)
        {
            for(int k = 0; k < gridSize; k++)
            {
                if (Grid != null && Grid[i, k] != null)
                    Destroy(Grid[i, k].gameObject);
            }
        }
        TilesInitialized = 0;
    }

    #endregion

    // Methods to check for winners and update which tiles have been initialized.
    #region play tile methods
    /// <summary>
    /// Check the board for any matching 3 adjacent player tiles.
    /// </summary>
    public bool checkForWinner(int row, int column, Player playerToCheck)
    {
        bool match = true;
        for (int i = 0; i < gridSize; i++) // Check current row for match
        {
            if (Grid[row, i].associatedPlayer == null)
            {
                match = false;
                break;
            }
            else if (Grid[row, i].associatedPlayer != playerToCheck)
            {
                match = false;
                break;
            }
        }

        if(match)
        {
            Overseer.Instance.AnnounceWinner(Overseer.Instance.GetCurrentPlayer());
            return true;
        }

        match = true;

        for (int i = 0; i < gridSize; i++) // Check current column for match
        {
            if (Grid[i, column].associatedPlayer == null)
            {
                match = false;
                break;
            }
            else if (Grid[i,column].associatedPlayer != playerToCheck)
            {
                match = false;
                break;
            }
        } 

        if (match)
        {
            Overseer.Instance.AnnounceWinner(Overseer.Instance.GetCurrentPlayer());
            return true;
        }

        match = true;

        for (int i = 0; i < gridSize; i++) // Check Diagonally (0,0 to gridSize - 1,gridSize - 1)
        {
            if (Grid[i, i].associatedPlayer == null)
            {
                match = false;
                break;
            }
            else if (Grid[i, i].associatedPlayer != playerToCheck)
            {
                match = false;
                break;
            }
        }

        if (match)
        {
            Overseer.Instance.AnnounceWinner(Overseer.Instance.GetCurrentPlayer());
            return true;
        }

        match = true;

        for (int i = 0; i < gridSize; i++) // Check Diagonally in the other direction (gridSize - 1,  0) to (0, gridSize - 1) 
        {
            if (Grid[gridSize - 1 - i, i].associatedPlayer == null)
            {
                match = false;
                break;
            }
            else if (Grid[gridSize - 1 - i, i].associatedPlayer != playerToCheck)
            {
                match = false;
                break;
            }

        }

        if (match)
        {
            Overseer.Instance.AnnounceWinner(Overseer.Instance.GetCurrentPlayer());
            return true;
        }

        return false;

    }

    /// <summary>
    /// Update the number of tiles that have been set. If this number is equal to the square of the grid size, then the game is over.
    /// </summary>
    public void UpdateTilesInitialized()
    {
        ++TilesInitialized;
        if (TilesInitialized >= (gridSize * gridSize))
        {
            Overseer.Instance.AnnounceWinner(null);
        }
    }

    /// <summary>
    /// Return the move number of the tile placed. Used for book keeping.
    /// </summary>
    public int GetMoveNumber()
    {
        return TilesInitialized;
    }

    #endregion

    // Methods used primarily in debug.
    #region debug methods & CoRoutines
    /// <summary>
    /// Manually insert a tile into the game board without clicking.
    /// </summary>
    public void Debug_InsertTile(int row, int column)
    {
        Grid[row, column].OnButtonPress(); // OnButtonPress() initializes the tile. I can change this to a method with a more descriptive name.
    }

    /// <summary>
    /// Return where the tile has been initialized or not.
    /// </summary>
    public bool IsTileInitialized(int row, int column)
    {
        return Grid[row, column].associatedPlayer != null;
    }

    #endregion

}


