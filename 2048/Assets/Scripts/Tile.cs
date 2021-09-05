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
    public bool destroy;

    public Image tileColor;
    public Text numberText;
    public int value;
    [SerializeField] Color[] colorScale;
    
    void Awake()
    {
        initValue();
        numberText.text = value.ToString();
        updateColor();
    }

    public void punch()
    {
        transform.DOPunchScale(new Vector3(0.2f,0.2f,0.2f), 0.1f, 0, 0f);
        return;
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

    public void updateValue()
    {
        value *= 2;
        numberText.text = value.ToString();
    }

    public void updateColor()
    {
        int colorIndex = (int)Mathf.Clamp((Mathf.Log(value, 2) - 1), 0, 14);
        tileColor.DOColor(colorScale[colorIndex],0);
        if (colorIndex > 1)
        {
            numberText.DOColor(new Color(255, 255, 255), 0);
        }
    }
}
