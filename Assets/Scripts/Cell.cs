using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// more organized data structure of a cell
// spreadsheet contains a 2d array of these

public class Cell : MonoBehaviour
{

    // Static to be one universal value
    static Color highlight = new Color(0, 200, 255);
    bool isHighlighted;

    // Lazy game jam coding practice: public parameters
    // cells are organized by (Row, Col) which is spatially (y, x)
    public int row;
    public int col;

    // private because must change stuff on set
    [SerializeField] String content;
    [SerializeField] TMP_Text contentDisplay;
    private Color bgColor;
    [SerializeField] Image background;

    // default constructor
    public void SetValues(int row, int col)
    {
        name = "(" + row + ", " + col + ")";
        this.row = row;
        this.col = col;
        SetContent("");
        SetBgColor(Color.white);
    }

    // specified constructor
    public void SetValues(int row, int col, String content, Color bgColor)
    {
        name = "(" + row + ", " + col + ")";
        this.row = row;
        this.col = col;
        SetContent(content);
        SetBgColor(bgColor);
    }

    public String GetContent() { return content; }
    public void SetContent(String content) { this.content = content; contentDisplay.text = content;
        int maxwellOdds = UnityEngine.Random.Range(0, 5);
        if (maxwellOdds == 0)
        {
            Maxwell.inst.summonMaxwell("text");
        }
    }
    public Color GetBgColor() { return bgColor; }
    public void SetBgColor(Color bgColor) { this.bgColor = bgColor; if (isHighlighted) background.color = bgColor * highlight; else background.color = bgColor;
        int maxwellOdds = UnityEngine.Random.Range(0, 5);
        if (maxwellOdds == 0)
        {
            Maxwell.inst.summonMaxwell("color");
        }


    }

    public void SetHighlight(bool setTo) { isHighlighted = setTo; SetBgColor(bgColor); }

    public void SetSize(Vector2 size)
    {
        float cellMargin = 0;
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = size - Vector2.one * cellMargin;
    }
}
