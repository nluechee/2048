using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Tile tile;
    public Tile tempTile;

    public bool hasTile()
    {
       return tile != null ? true : false;
    }

    public bool hasTempTile()
    {
        return tempTile != null ? true : false;
    }
}

