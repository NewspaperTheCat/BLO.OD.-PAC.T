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

    int ROWS = 0;
    int COLS = 0;
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
        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLS; c++)
            {
                // delete old
                Destroy(sheet[r, c].gameObject);
            }
        }

        // set to new dimensions
        ROWS = newRows;
        COLS = newCols;
        sheet = new Cell[ROWS, COLS];

        // calculate cell size
        CELL_WIDTH = screenBounds.x / (float)(COLS);
        CELL_HEIGHT = screenBounds.y / (float)(ROWS);

        Selector.inst.SetSize(new Vector2(CELL_WIDTH, CELL_HEIGHT));

        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLS; c++)
            {
                // constructs initially
                Cell cell = Instantiate(CELL_PREFAB, transform).GetComponent<Cell>();
                cell.SetValues(r, c);
                cell.SetSize(new Vector2(CELL_WIDTH, CELL_HEIGHT));
                cell.GetComponent<RectTransform>().anchoredPosition = SheetToWorld(new Vector2(r, c));
                sheet[r, c] = cell;
            }
        }

        gridInitialized = true;

        Selector.inst.Reset();

        Debug.Log(newRows + "" + newCols);
    }

    public bool InBounds(int r, int c) { return r >= 0 && c >= 0 && r < ROWS && c < COLS; }
    public bool InBounds(Vector2Int rc) { return InBounds(rc.x, rc.y); }
    public Cell GetCellAt(int row, int col) { return sheet[row, col]; }
    public Cell GetCellAt(Vector2Int rc) { return GetCellAt(rc.x, rc.y); }

    // conversion helpers
    public Vector2 SheetToWorld(Vector2 sheetPos)
    {
        Vector2 local = new Vector2((screenBounds.x - CELL_WIDTH) / (float)(COLS - 1) * sheetPos.y, (screenBounds.y - CELL_HEIGHT) / (float)(ROWS - 1) * -sheetPos.x);
        return local;
    }
    public Vector2 WorldToSheet(Vector2 worldPos)
    {
        return new Vector2(-worldPos.y * (float)(ROWS - 1) / (screenBounds.y - CELL_HEIGHT), worldPos.x * (float)(COLS - 1) / (screenBounds.x - CELL_WIDTH));
    }

    public Vector2Int GetSheetDimensions() { return new Vector2Int(ROWS, COLS); }

}
