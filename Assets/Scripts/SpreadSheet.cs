using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

    [SerializeField] int ROWS = 10;
    [SerializeField] int COLS = 5;
    Cell[,] sheet;

    // useful reference
    [SerializeField] GameObject CELL_PREFAB; // to be set in editor
    private Vector2 screenBounds; // this references the box collider to have visuals in editor when mapping the screen
    [SerializeField] float CELL_WIDTH = 1;
    [SerializeField] float CELL_HEIGHT = .5f;


    void Start()
    {
        screenBounds = GetComponent<BoxCollider2D>().size;

        sheet = new Cell[ROWS, COLS];
        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLS; c++)
            {
                // constructs initially
                Cell cell = Instantiate(CELL_PREFAB, transform).GetComponent<Cell>();
                cell.SetValues(r, c);
                cell.transform.position = SheetToWorld(new Vector2(r, c));
                sheet[r, c] = cell;
            }
        }
    }

    public Cell GetCellAt(int row, int col) { return sheet[row, col]; }
    public Cell GetCellAt(Vector2Int rc) { return GetCellAt(rc.x, rc.y); }

    // conversion helpers
    public Vector2 SheetToWorld(Vector2 sheetPos)
    {
        Vector2 local = new Vector2((screenBounds.x - CELL_WIDTH) / (float)(COLS - 1) * sheetPos.y, (screenBounds.y - CELL_HEIGHT) / (float)(ROWS - 1) * -sheetPos.x);
        return local + (Vector2)transform.position;
    }
    public Vector2 WorldToSheet(Vector2 worldPos)
    {
        worldPos -= (Vector2)transform.position;
        return new Vector2(-worldPos.y * (float)(ROWS - 1) / (screenBounds.y - CELL_HEIGHT), worldPos.x * (float)(COLS - 1) / (screenBounds.x - CELL_WIDTH));
    }

    public Vector2Int GetArrayDimensions() { return new Vector2Int(ROWS, COLS); }

}
