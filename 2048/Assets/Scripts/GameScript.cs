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
    [SerializeField] List<Cell> allCells;
    public GameObject grid;
    public GameObject tileLayer;
    public List<Tile> activeTiles;
    public int score = 0;
    private bool inputLocked;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // initialize the grid
        StartCoroutine(GenerateGrid());
        inputLocked = false;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(moveTiles());
    }
 
    public void SpawnTile()
    {
        if (activeTiles.Count == 16)
        {
            return;
        }
        Vector2 coords;
        int randomLocation;
        bool tileExists = false;
        do
        {
            // get a random location
            randomLocation = Random.Range(0, 16);
            coords = allCells[randomLocation].coords;
            foreach (Tile t in activeTiles)
            {
                if (t.currentCoord == coords)
                {
                    tileExists = true;
                    break;
                }
                else
                {
                    tileExists = false;
                }
            }
        }
        while (tileExists);
        GameObject newTile = Instantiate(tilePrefab, allCells[randomLocation].transform);
        Tile tile = newTile.GetComponent<Tile>();
        tile.currentCoord = coords;
        newTile.transform.SetParent(tileLayer.transform);
        activeTiles.Add(tile);
        return;
    }
  
    public IEnumerator GenerateGrid()
    {
        for (int i = 0; i<4; i++)
        {
            for (int j=0; j<4; j++)
            {
                GameObject newCell = Instantiate(cellPrefab, grid.transform);
                Cell cell = newCell.GetComponent<Cell>();
                cell.coords = new Vector2(j, i);
                allCells.Add(cell);
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
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.y).ThenBy(t => t.currentCoord.x).ToList();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.up);
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
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.y).ThenBy(t => t.currentCoord.x).ToList();
            activeTiles.Reverse();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.down);
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
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.x).ThenBy(t => t.currentCoord.y).ToList();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.left);
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
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.x).ThenBy(t => t.currentCoord.y).ToList();
            activeTiles.Reverse();
            foreach (Tile t in activeTiles)
            {
                updateTileCoords(t, Direction.right);
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
                        t.currentCoord.x += xPrime;
                        t.currentCoord.y += yPrime;
                        coordToCompare += xPrime + yPrime;
                        otherTile.updateValue();
                        t.merge = true;
                    }
                }
            }
            if (emptyNeighbour) // keep moving as long as the neighbouring space is empty
            {
                t.currentCoord.x += xPrime;
                t.currentCoord.y += yPrime;
                coordToCompare += xPrime + yPrime;
            }
            else
            {
                // stop moving since the neighbour can't merge
                break;
            }
        }
    }
    
    private Sequence animateTiles(Direction inputDir)
    {
        Sequence tileAnimations = DOTween.Sequence();
        float delta;
        foreach (Tile t in activeTiles)
        {
            if (t.previousCoord != t.currentCoord)
            {
                delta = (t.previousCoord.y - t.currentCoord.y) + (t.currentCoord.x - t.previousCoord.x);
                Tween tileTween = t.move(delta * 120f, inputDir);
                tileAnimations.Insert(0, tileTween);
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
}
