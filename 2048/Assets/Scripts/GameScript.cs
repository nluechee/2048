using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

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
    public Cell[,] grid;
    private int activeTiles;
    public int score = 0;
    private bool inputLocked;

    // Start is called before the first frame update
    void Start()
    {
        // initialize the grid
        grid = new Cell[4, 4];
        StartCoroutine(GenerateGrid()); 
        inputLocked = false;
        activeTiles = 0;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(moveTiles());
    }
 
    public void SpawnTile()
    {
        if (activeTiles == 16)
        {
            return;
        }
        int randomX, randomY;
        bool tileExists;
        do
        {
            // get a random location
            randomX = Random.Range(0, 4);
            randomY = Random.Range(0, 4);
            tileExists = grid[randomY, randomX].hasTile() ? true : false; 
        }
        while (tileExists);
        GameObject newTile = Instantiate(tilePrefab, grid[randomY, randomX].transform);
        Tile tile = newTile.GetComponent<Tile>();
        tile.punch();
        newTile.transform.SetParent(tileLayer.transform);
        grid[randomY, randomX].tile = tile;
        activeTiles++;
        return;
    }
  
    public IEnumerator GenerateGrid()
    {
        for (int y = 0; y<4; y++)
        {
            for (int x=0; x<4; x++)
            {
                GameObject newCell = Instantiate(cellPrefab, cellGrid.transform);
                Cell cell = newCell.GetComponent<Cell>();
                grid[y, x] = cell;
            }
        }
        yield return new WaitForEndOfFrame();
        // spawn in 2 tiles
        SpawnTile();
        SpawnTile();
    }

    public IEnumerator moveTiles()
    {
        // sort the activeTiles based off the direction of input
        if (Input.GetKeyDown(KeyCode.UpArrow) && !inputLocked)
        {
            // move up
            inputLocked = true;
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Cell c = grid[y, x];
                    updateTileCoords(c, Direction.up, x, y);
                }
            }
            Sequence tileMovementSequence = animateTiles(Direction.up);
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
            for (int y = 3; y >= 0; y--)
            {
                for (int x = 0; x < 4; x++)
                {
                    Cell c = grid[y, x];
                    updateTileCoords(c, Direction.down, x, y);
                }
            }
            Sequence tileMovementSequence = animateTiles(Direction.down);
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
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Cell c = grid[y, x];
                    updateTileCoords(c, Direction.left, x, y);
                }
            }
            Sequence tileMovementSequence = animateTiles(Direction.left);
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
            for (int x = 3; x >=0; x--)
            {
                for (int y = 0; y < 4; y++)
                {
                    Cell c = grid[y, x];
                    updateTileCoords(c, Direction.right, x, y);
                }
            }
            Sequence tileMovementSequence = animateTiles(Direction.right);
            // wait for all animations to complete
            yield return tileMovementSequence.WaitForCompletion();
            // merge any tiles that need merging
            mergeTiles();
            // spawn a new tile 
            SpawnTile();
            inputLocked = false;
        }

        //if (activeTiles.Count == 16)
        //{
        //    if (lostGame())
        //    {
        //        Debug.Log("Lost");
        //    }
        //}
    }

    private void updateTileCoords(Cell c, Direction inputDir, int x, int y)
    {
        if (!c.hasTile()) // do nothing if there is no tile on the cell
        {
            return;
        }
        int maxCoord=0, xPrime=0, yPrime=0,neighbourX = 0, neighbourY = 0;
        float coordToCompare = 0f;
        Tile t = c.tile;
        switch (inputDir)
        {
            case Direction.up:
                maxCoord = 0;
                xPrime = 0;
                yPrime = -1;
                coordToCompare = y;
                neighbourX = x;
                neighbourY = y;
                break;

            case Direction.down:
                maxCoord = 3;
                xPrime = 0;
                yPrime = 1;
                coordToCompare = y;
                neighbourX = x;
                neighbourY = y;
                break;

            case Direction.left:
                maxCoord = 0;
                xPrime = -1;
                yPrime = 0;
                coordToCompare = x;
                neighbourX = x;
                neighbourY = y;
                break;

            case Direction.right:
                maxCoord = 3;
                xPrime = 1;
                yPrime = 0;
                coordToCompare = x;
                neighbourX = x;
                neighbourY = y;
                break;
        }

        t.moveVector = new Vector2(0, 0);
        while (coordToCompare != maxCoord)  // if the tile is at a max/min coord break immediately
        {
            Cell neighbourCell = grid[neighbourY + yPrime, neighbourX + xPrime];
            if (neighbourCell.hasTile())  // check for neighbouring tile in direction of move
            {
                if (neighbourCell.tile.value == t.value)
                {
                    // the values are the same and the neighbour can merge
                    // update the move vector for the tile and update its position in the grid
                    t.moveVector.x += xPrime;
                    t.moveVector.y -= yPrime;
                    t.merge = true;  // tile will be updated in mergeTiles()
                    neighbourCell.tempTile = neighbourCell.tile; // move the neighbour's tile to a temp spot
                    neighbourCell.tile = t;  // overwrite the neighbour tile with this one
                    c.tile = null;  // remove the tile from this cell
                    // change the coord to compare to the next cell 
                    coordToCompare += xPrime + yPrime;
                    neighbourX += xPrime;
                    neighbourY += yPrime;
                }
                else
                {
                    // the values are different and the neighbour can't  merge 
                    break;
                }
            }
            else  // keep moving as long as the neighbouring space is empty
            {
                // update the move vector for the tile and update its position in the grid
                t.moveVector.x += xPrime;
                t.moveVector.y -= yPrime;
                neighbourCell.tile = t; // move this tile to the neighbour
                c.tile = null;  // remove the tile from this cell
                // change the coord to compare to the next cell 
                coordToCompare += xPrime + yPrime;
                neighbourX += xPrime;
                neighbourY += yPrime;
            }
        }
    }
    
    private Sequence animateTiles(Direction inputDir)
    {
        Sequence tileAnimations = DOTween.Sequence();
        float delta;
        Cell c;
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                c = grid[y, x];
                if (c.hasTile())
                {
                    delta = c.tile.moveVector.x + c.tile.moveVector.y;
                    Tween tileTween = c.tile.move(delta * 120f, inputDir);
                    tileAnimations.Insert(0, tileTween);
                    c.tile.moveVector = new Vector2(0, 0);
                }
            }
        }
        return tileAnimations;
    }

    private void mergeTiles()
    {
        
        Cell c;
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                c = grid[y, x];
                if (c.hasTile())
                {
                    if (c.tile.merge == true)
                    {
                        // update the neighbour tile to show the merge
                        c.tile.updateValue();
                        c.tile.updateColor();
                        c.tile.merge = false;
                    }
                }

                if (c.hasTempTile())
                {
                    Destroy(c.tempTile.gameObject);
                    c.tempTile = null;
                    activeTiles--;
                }
            }
        }
    }

    //private bool lostGame()
    //{
    //    foreach (Tile t in activeTiles)
    //    {
    //        if (checkNeighbours(t))
    //        {
    //            return false;
    //        }

    //    }
    //    return true;
    //}

    //private bool checkNeighbours(Tile t)
    //{

    //    foreach (Tile otherTile in activeTiles)
    //    {
    //        if (t.currentCoord.y == otherTile.currentCoord.y && t.currentCoord.x + 1 == otherTile.currentCoord.x && t.value == otherTile.value)
    //        {
    //            return true;
    //        }
    //        else if (t.currentCoord.y == otherTile.currentCoord.y && t.currentCoord.x - 1 == otherTile.currentCoord.x && t.value == otherTile.value)
    //        {
    //            return true;
    //        }
    //        else if (t.currentCoord.x == otherTile.currentCoord.x && t.currentCoord.y + 1 == otherTile.currentCoord.y && t.value == otherTile.value)
    //        {
    //            return true;
    //        }
    //        else if (t.currentCoord.x == otherTile.currentCoord.x && t.currentCoord.y - 1 == otherTile.currentCoord.y && t.value == otherTile.value)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

}
