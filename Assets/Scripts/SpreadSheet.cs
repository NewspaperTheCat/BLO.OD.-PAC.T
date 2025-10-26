using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadSheet : MonoBehaviour
{
    // Singleton Design Pattern (to a degree)
    public static SpreadSheet inst;
    void OnEnable()
    {
        // ensures there is only ever one spreadsheet
        if (inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnDisable()
    {
        // when disabled (destoryed or scene change) removes this reference from static value
        inst = null;
    }

    int ROWS = -1;
    int COLS = -1;
    Cell[,] sheet;

    // useful reference
    [SerializeField] GameObject CELL_PREFAB; // to be set in editor
    RectTransform rt;
    private Vector2 screenBounds; // derived from RectTransform
    float CELL_WIDTH = 1; // get set in code, assumes cells are 1x1
    float CELL_HEIGHT = 1;

    private bool gridInitialized = false;
    public bool IsGridInitialized() { return gridInitialized; }

    public void CreateGrid(int newRows, int newCols)
    {
        if (rt == null)
        {
            rt = GetComponent<RectTransform>();
            Rect screen = rt.rect;
            screenBounds = new Vector2(screen.width, screen.height);
        }

        // delete any existing
        for (int r = 0; r < ROWS + 1; r++)
        {
            for (int c = 0; c < COLS + 1; c++)
            {
                // delete old
                Destroy(sheet[r, c].gameObject);
            }
        }

        // set to new dimensions
        ROWS = newRows;
        COLS = newCols;
        sheet = new Cell[ROWS + 1, COLS + 1]; // going to secretly hide another row and column that are the col and row names (1, 2, 3...) & (A, B, C...)

        // calculate cell size
        CELL_WIDTH = screenBounds.x / (float)(COLS + 1);
        CELL_HEIGHT = screenBounds.y / (float)(ROWS + 1);

        Selector.inst.SetSize(new Vector2(CELL_WIDTH, CELL_HEIGHT));

        for (int r = 0; r < ROWS + 1; r++)
        {
            for (int c = 0; c < COLS + 1; c++)
            {
                // constructs initially
                Cell cell = Instantiate(CELL_PREFAB, transform).GetComponent<Cell>();
                cell.SetValues(r, c);
                cell.SetSize(new Vector2(CELL_WIDTH, CELL_HEIGHT));
                cell.GetComponent<RectTransform>().anchoredPosition = SheetToWorld(new Vector2(r - 1, c - 1));
                sheet[r, c] = cell;
            }
        }

        // hide top left corner
        sheet[0, 0].gameObject.SetActive(false);
        // assign row titles
        for (int r = 1; r < ROWS + 1; r++) {
            Debug.Log(r + ", " + ROWS);
            sheet[r, 0].SetBgColor(new Color(225, 225, 235));
            sheet[r, 0].SetContent(r + "");
        }
        // assign column titles
        char[] charMapping = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        for (int c = 1; c < COLS + 1; c++) {
            sheet[0, c].SetBgColor(new Color(225, 225, 235));
            sheet[0, c].SetContent(charMapping[c - 1] + "");
        }

        gridInitialized = true;

        Selector.inst.Reset();
    }

    public bool InBounds(int r, int c) { return r >= 0 && c >= 0 && r < ROWS && c < COLS; }
    public bool InBounds(Vector2Int rc) { return InBounds(rc.x, rc.y); }
    public Cell GetCellAt(int row, int col) { return sheet[row + 1, col + 1]; }
    public Cell GetCellAt(Vector2Int rc) { return GetCellAt(rc.x, rc.y); }

    // conversion helpers
    public Vector2 SheetToWorld(Vector2 sheetPos)
    {
        Vector2 local = new Vector2((screenBounds.x - CELL_WIDTH) / (float)(COLS) * (sheetPos.y + 1), (screenBounds.y - CELL_HEIGHT) / (float)(ROWS) * -(sheetPos.x + 1));
        return local;
    }
    // public Vector2 WorldToSheet(Vector2 worldPos)
    // {
    //     return new Vector2(-worldPos.y * (float)(ROWS - 1) / (screenBounds.y - CELL_HEIGHT), worldPos.x * (float)(COLS - 1) / (screenBounds.x - CELL_WIDTH));
    // }

    public Vector2Int GetSheetDimensions() { return new Vector2Int(ROWS, COLS); }

}
