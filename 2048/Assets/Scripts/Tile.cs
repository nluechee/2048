using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public Vector2 currentCoord;
    public Vector2 previousCoord;
    public bool merge;
    public Text numberText;
    public int value;
    
    void Awake()
    {
        initValue();
        numberText.text = value.ToString();
    }


    // Update is called once per frame
    void Update()
    {
    }

    public Tween move(float distance, Direction inputDir)
    {
        Tween tileTween;
        switch (inputDir)
        {
            case Direction.up:
            case Direction.down:
                tileTween = transform.DOMoveY(transform.position.y + distance,0.1f, true);
                return tileTween;

            case Direction.left:
            case Direction.right:
                tileTween = transform.DOMoveX(transform.position.x + distance, 0.1f, true);
                return tileTween;
            default:
                return null;
        }
    }

    private void initValue()
    {
        float randomizer = Random.Range(0f, 1f);
        if (randomizer > 0.9f)
        {
            value = 4;
        }
        else
        {
            value = 2;
        }
        
    }

    public void destroyTile()
    {
        Destroy(gameObject);
    }

    public void updateValue()
    {
        value *= 2;
        numberText.text = value.ToString();
    }
}
