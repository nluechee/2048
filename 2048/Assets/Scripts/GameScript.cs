using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum Direction
{
    up, down, left, right
}

public class GameScript : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject cellPrefab;
    public GameObject cellGrid;
    public GameObject tileLayer;
    public List<Tile> activeTiles;
    public Cell[,] grid;
    public int score = 0;
    public Text scoreText;
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Text playAgainButton;
    private bool inputLocked;
    private bool nothingMoved;

    // Start is called before the first frame update
    void Start()
    {
        // initialize the grid
        grid = new Cell[4, 4];
        StartCoroutine(GenerateGrid());
        inputLocked = false;
        nothingMoved = false;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(moveTiles());
    }

    private void updateScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();
    }

    private void SpawnTile(bool start = false)
    {
        if (nothingMoved)
        {
           return;  // prevent spawning a new tile if an input was made but no tiles moved. Track this in animateTiles()
        }
        if (activeTiles.Count == 16)
        {
            return;
        }
        Vector2 coords;
        int randomX, randomY;
        bool tileExists;
        do
        {
            // get a random location
            randomX = Random.Range(0, 4);
            randomY = Random.Range(0, 4);
            coords = new Vector2(randomX,randomY);
            tileExists = grid[randomX, randomY].hasTile() ? true : false;
        }
        while (tileExists);
        GameObject newTile = Instantiate(tilePrefab, grid[randomX,randomY].transform);
        Tile tile = newTile.GetComponent<Tile>();
        tile.currentCoord = coords;
        tile.punch();
        newTile.transform.SetParent(tileLayer.transform);
        grid[randomX, randomY].tile = tile;
        activeTiles.Add(tile);
        if (start)
        {
            tile.value = 2;
            tile.numberText.text = "2";
            tile.updateColor();
        }
        return;
    }
  
    private IEnumerator GenerateGrid()
    {
        for (int y = 0; y<4; y++)
        {
            for (int x=0; x<4; x++)
            {
                GameObject newCell = Instantiate(cellPrefab, cellGrid.transform);
                Cell cell = newCell.GetComponent<Cell>();
                grid[x, y] = cell;
            }
        }
        yield return new WaitForEndOfFrame();
        // spawn in 2 tiles
        SpawnTile(true);
        SpawnTile(true);
    }

    private IEnumerator moveTiles()
    {
        // sort the activeTiles based off the direction of input
        if (Input.GetKeyDown(KeyCode.UpArrow) && !inputLocked)
        {
            // move up
            inputLocked = true;
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.y).ThenBy(t => t.currentCoord.x).ToList();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.up);
            }
            Sequence tileMovementSequence = animateTiles();
            // wait for all animations to complete
            yield return tileMovementSequence.WaitForCompletion();
            // merge any tiles that need merging
            mergeTiles();
            // spawn a new tile 
            SpawnTile();
            inputLocked = false;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && !inputLocked)
        {
            // move down
            inputLocked = true;
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.y).ThenBy(t => t.currentCoord.x).ToList();
            activeTiles.Reverse();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.down);
            }
            Sequence tileMovementSequence = animateTiles();
            // wait for all animations to complete
            yield return tileMovementSequence.WaitForCompletion();
            // merge any tiles that need merging
            mergeTiles();
            // spawn a new tile 
            SpawnTile();
            inputLocked = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && !inputLocked)
        {
            // move left
            inputLocked = true;
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.x).ThenBy(t => t.currentCoord.y).ToList();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.left);
            }
            Sequence tileMovementSequence = animateTiles();
            // wait for all animations to complete
            yield return tileMovementSequence.WaitForCompletion();
            // merge any tiles that need merging
            mergeTiles();
            // spawn a new tile 
            SpawnTile();
            inputLocked = false;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && !inputLocked)
        {
            // move right
            inputLocked = true;
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.x).ThenBy(t => t.currentCoord.y).ToList();
            activeTiles.Reverse();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.right);
            }
            Sequence tileMovementSequence = animateTiles();
            // wait for all animations to complete
            yield return tileMovementSequence.WaitForCompletion();
            // merge any tiles that need merging
            mergeTiles();
            // spawn a new tile 
            SpawnTile();
            inputLocked = false;
        }

        if (activeTiles.Count == 16)
        {
            if (lostGame())
            {
                Debug.Log("Game Over");
                StartCoroutine(gameOver());
            }
        }
    }

    private void updateTileCoords(Tile t, Direction inputDir)
    {
        int maxCoord=0, xPrime=0, yPrime=0;
        float coordToCompare=0f;

        switch (inputDir)
        {
            case Direction.up:
                maxCoord = 0;
                xPrime = 0;
                yPrime = -1;
                coordToCompare = t.currentCoord.y;
                break;

            case Direction.down:
                maxCoord = 3;
                xPrime = 0;
                yPrime = 1;
                coordToCompare = t.currentCoord.y;
                break;

            case Direction.left:
                maxCoord = 0;
                xPrime = -1;
                yPrime = 0;
                coordToCompare = t.currentCoord.x;
                break;

            case Direction.right:
                maxCoord = 3;
                xPrime = 1;
                yPrime = 0;
                coordToCompare = t.currentCoord.x;
                break;
        }

        t.previousCoord = t.currentCoord;
        while (coordToCompare != maxCoord) // if the tile is at a max/min coord break immediately
        {
            bool emptyNeighbour = true;
            foreach (Tile otherTile in activeTiles)
            {
                if (otherTile.currentCoord == new Vector2(t.currentCoord.x + xPrime, t.currentCoord.y + yPrime))
                {
                    // there is a neighbouring tile
                    emptyNeighbour = false;
                    if (otherTile.value != t.value)
                    {
                        // the neighbour can't  merge
                        break;
                    }
                    else
                    {
                        // the neighbour can merge
                        grid[(int)t.currentCoord.x, (int)t.currentCoord.y].tile = null;  // make the tile at this cell null
                        t.currentCoord.x += xPrime; 
                        t.currentCoord.y += yPrime;  // shift the current coords to the neighbouring cell 
                        coordToCompare += xPrime + yPrime;
                        otherTile.updateValue();        // double the value of the other tile
                        updateScore(otherTile.value);   // add this to the score
                        otherTile.updateColor();        // update the colour of the other tile
                        t.merge = true;                 // mark this tile for mergeing
                    }                                   // i.e. get destroyed after animation in mergeTile()
                }
            }
            if (emptyNeighbour) // keep moving as long as the neighbouring space is empty
            {
                grid[(int)t.currentCoord.x, (int)t.currentCoord.y].tile = null;  // make the tile at this cell null
                t.currentCoord.x += xPrime;
                t.currentCoord.y += yPrime;  // shift the current coords to the neighbouring cell
                grid[(int)t.currentCoord.x, (int)t.currentCoord.y].tile = t; // make the neighbouring cell hold this tile
                coordToCompare += xPrime + yPrime;
            }
            else
            {
                // stop moving since the neighbour can't merge
                break;
            }
        }
    }
    
    private Sequence animateTiles()
    {
        Sequence tileAnimations = DOTween.Sequence();
        nothingMoved = true; // see SpawnTile()
        foreach (Tile t in activeTiles)
        {
            if (t.previousCoord != t.currentCoord)
            {
                int x = (int)t.currentCoord.x;
                int y = (int)t.currentCoord.y;
                Tween tileTween = t.transform.DOMove(grid[x, y].transform.position, 0.1f);
                tileAnimations.Insert(0, tileTween);
                nothingMoved = false; // see SpawnTile(). Allow a spawn since at least one tile moved
            }
        }
        return tileAnimations;
    }

    private void mergeTiles()
    {
        for(int i = activeTiles.Count-1; i>=0; i--)
        {
            if (activeTiles[i].merge == true)
            {
                Destroy(activeTiles[i].gameObject);
                activeTiles.Remove(activeTiles[i]);
            }
        }
    }

    private bool lostGame()
    {
        int lastValue = 0;
        for (int y = 0; y < 4; y++) // check all the rows
        {
            for (int x = 0; x < 4; x++)
            {
                if (!grid[x, y].hasTile())
                {
                    return false;
                }
                if (x == 0)
                {
                    lastValue = grid[x, y].tile.value;
                }
                else
                {
                    if (grid[x, y].tile.value == lastValue)
                    {
                        return false;
                    }
                    else
                    {
                        lastValue = grid[x, y].tile.value;
                    }
                }
            }
        }
        for (int x = 0; x< 4; x++) // check all the columns
        {
            for (int y = 0; y < 4; y++)
            {
                if (y == 0)
                {
                    lastValue = grid[x, y].tile.value;
                }
                else
                {
                    if (grid[x, y].tile.value == lastValue)
                    {
                        return false;
                    }
                    else
                    {
                        lastValue = grid[x, y].tile.value;
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator gameOver()
    {
        if (!gameOverPanel.activeInHierarchy)
        {
            gameOverPanel.SetActive(true);
            CanvasGroup gameOverCanvas = gameOverPanel.GetComponent<CanvasGroup>();
            gameOverCanvas.interactable = false;
            yield return new WaitForSeconds(1);
            Tween gameOverTween = gameOverText.DOFade(1f, 1f);
            yield return gameOverTween.WaitForCompletion();
            Tween playAgainTween = playAgainButton.DOFade(1f, 0.2f);
            yield return playAgainTween.WaitForCompletion();
            gameOverCanvas.interactable = true;
        }
    }
}
