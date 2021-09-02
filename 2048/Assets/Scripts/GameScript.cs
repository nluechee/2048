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

        if (Input.GetKeyDown(KeyCode.UpArrow) && !inputLocked)
        {
            // move up
            inputLocked = true;
            activeTiles = activeTiles.OrderBy(t => t.currentCoord.y).ThenBy(t => t.currentCoord.x).ToList();
            foreach (Tile t in activeTiles)
            {
                StartCoroutine(moveTile(t, Direction.up));
            }
            yield return new WaitForSeconds(0.11f);
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
                StartCoroutine(moveTile(t, Direction.down));
            }
            yield return new WaitForSeconds(0.11f);
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
                StartCoroutine(moveTile(t, Direction.left));
            }
            yield return new WaitForSeconds(0.11f);
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
                StartCoroutine(moveTile(t, Direction.right));
            }
            yield return new WaitForSeconds(0.11f);
            // spawn a new tile
            SpawnTile();
            inputLocked = false;
        }
        else
        {
            yield return null;
        }
    }

    private IEnumerator moveTile(Tile t, Direction inputDir)
    {
        int maxCoord=0, xPrime=0, yPrime=0;
        float directionalMove=0f, coordToCompare=0f;

        switch (inputDir)
        {
            case Direction.up:
                maxCoord = 0;
                xPrime = 0;
                yPrime = -1;
                coordToCompare = t.currentCoord.y;
                directionalMove = +120f;
                break;

            case Direction.down:
                maxCoord = 3;
                xPrime = 0;
                yPrime = 1;
                coordToCompare = t.currentCoord.y;
                directionalMove = -120f;
                break;

            case Direction.left:
                maxCoord = 0;
                xPrime = -1;
                yPrime = 0;
                coordToCompare = t.currentCoord.x;
                directionalMove = -120f;
                break;

            case Direction.right:
                maxCoord = 3;
                xPrime = 1;
                yPrime = 0;
                coordToCompare = t.currentCoord.x;
                directionalMove = +120f;
                break;
        }

        bool merge = false;
        float moveAmt = 0;
        Vector2 futureCoord = t.currentCoord;
        while (coordToCompare != maxCoord)
        {
            bool emptyNeighbour = true;
            foreach (Tile otherTile in activeTiles)
            {
                if (otherTile.currentCoord == new Vector2(futureCoord.x + xPrime, futureCoord.y + yPrime))
                {
                    // there is a neighbouring tile
                    emptyNeighbour = false;
                    if (otherTile.value == t.value)
                    {
                        // the neighbour can merge
                        moveAmt += directionalMove;
                        futureCoord.x += xPrime;
                        futureCoord.y += yPrime;
                        coordToCompare += xPrime + yPrime;
                        otherTile.updateValue();
                        merge = true;
                        break;
                    }
                    else
                    {
                        // the neighbour can't merge
                        break;
                    }
                }
            }
            if (emptyNeighbour) // keep moving as long as the neighbouring space is empty
            {
                moveAmt += directionalMove;
                futureCoord.x += xPrime;
                futureCoord.y += yPrime;
                coordToCompare += xPrime + yPrime;
            }
            else
            {
                // stop moving since the neighbour can't merge
                break;
            }
        }
        if (moveAmt != 0)
        {
            Tween tileTween = t.move(moveAmt, inputDir);
            if (tileTween != null)
            {
                yield return tileTween.WaitForCompletion();
                if (merge)
                {
                    activeTiles.Remove(t);
                    t.destroyTile();
                }
                else
                {
                    t.currentCoord = futureCoord;
                }
            }
            else
            {
                yield return null;
            }
        }
        else
        {
            yield return null;
        }
    }
    
}
