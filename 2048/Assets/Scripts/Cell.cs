using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Tile tile;

    public bool hasTile()
    {
        return tile != null ? true : false;
    }
}
