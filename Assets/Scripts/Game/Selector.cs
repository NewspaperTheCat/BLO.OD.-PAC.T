using System;
using UnityEngine;

// contains all player input logic

public class Selector : MonoBehaviour
{
    // Singleton Design Pattern (to a degree)
    public static Selector inst;
    void OnEnable()
    {
        // ensures there is only ever one Selector
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

    // serialized so can be viewed
    [SerializeField] Vector2Int selected;
    Vector2Int pivotStart;
    Vector2Int pivotEnd;
    RectTransform rt;

    // refernce to be set in editor
    public RectTransform copyIndicator;
    Vector2Int copyPivotStart;
    Vector2Int copyPivotEnd;
    bool toCut = false;

    void Start()
    {
        copyPivotStart = new Vector2Int(-1, -1); // null value
        rt = GetComponent<RectTransform>();
    }

    public void Reset()
    {
        // weird stuff so it doesn't try to access stuff that doesn't exist
        pivotStart = Vector2Int.one;
        pivotEnd = Vector2Int.zero;
        SetSelected(0, 0);

        copyPivotStart = new Vector2Int(-1, -1);
        copyIndicator.gameObject.SetActive(false);
    }

    public void SetSize(Vector2 cellSize)
    {
        rt.sizeDelta = cellSize;
        copyIndicator.sizeDelta = cellSize;
    }

    public Cell GetHover()
    {
        return SpreadSheet.inst.GetCellAt(selected);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) { AudioManager.inst.PlayRandomKeyPress(); }
        if (Input.GetKeyDown(KeyCode.RightControl)) { AudioManager.inst.PlayRandomKeyPress(); }
        if (Input.GetKeyDown(KeyCode.LeftShift)) { AudioManager.inst.PlayRandomKeyPress(); }
        if (Input.GetKeyDown(KeyCode.RightShift)) { AudioManager.inst.PlayRandomKeyPress(); }

        // control varient handled internally
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { MoveSelected(0, -1); AudioManager.inst.PlayRandomKeyPress(); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { MoveSelected(0, 1); AudioManager.inst.PlayRandomKeyPress(); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { MoveSelected(1, 0); AudioManager.inst.PlayRandomKeyPress(); }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { MoveSelected(-1, 0); AudioManager.inst.PlayRandomKeyPress(); }

        if (IsCTRLPressed())
        {
            if (Input.GetKeyDown(KeyCode.C)) { MarkCopy(false); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.X)) { MarkCopy(true); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.V)) { Paste(); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.A)) { SelectAll(); AudioManager.inst.PlayRandomKeyPress(); }

            // Column Operations
            if (Input.GetKeyDown(KeyCode.Space)) { HighlightColumn(); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.Minus)) { DeleteColumn(); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.Equals)) { InsertColumn(); AudioManager.inst.PlayRandomKeyPress(); }
        }

        if (IsShiftPressed())
        {
            // Row Operations
            if (Input.GetKeyDown(KeyCode.Space)) { HighlightRow(); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.Minus)) { DeleteRow(); AudioManager.inst.PlayRandomKeyPress(); }
            if (Input.GetKeyDown(KeyCode.Equals)) { InsertRow(); AudioManager.inst.PlayRandomKeyPress(); }
        }
    }

    bool IsShiftPressed() { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); }
    bool IsCTRLPressed() { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); }

    // deals with update world position as well
    void MoveSelected(int dr, int dc)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // if control then move via continuity
            SetSelected(GetNextContinuity(selected, new Vector2Int(dr, dc)));
        }
        else
        {
            SetSelected(dr + selected.x, dc + selected.y);
        }
    }

    Vector2Int GetNextContinuity(Vector2Int start, Vector2Int dir)
    {
        bool continuityBlank = SpreadSheet.inst.GetCellAt(start).GetContent() == "";

        Vector2Int nullEnd = new Vector2Int(-1, -1);
        Vector2Int foundEnd = nullEnd;
        while (foundEnd == nullEnd)
        {
            Vector2Int next = start + dir;
            if (!SpreadSheet.inst.InBounds(next)) { foundEnd = start; continue; } // exits when reach edge

            bool nextContinuity = SpreadSheet.inst.GetCellAt(next).GetContent() == "";
            if (!continuityBlank && nextContinuity) continuityBlank = true;
            if (continuityBlank && !nextContinuity) { foundEnd = next; continue; }

            // progress scanner
            start = next;
        }

        return foundEnd;
    }

    void SetSelected(Vector2Int rc) { SetSelected(rc.x, rc.y); }
    void SetSelected(int r, int c)
    {
        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        int nr = (r + dim.x) % dim.x;
        int nc = (c + dim.y) % dim.y;

        selected = new Vector2Int(nr, nc);
        rt.anchoredPosition = (Vector3)SpreadSheet.inst.SheetToWorld(selected);

        if (IsShiftPressed())
        {
            if (nr < pivotStart.x) pivotStart.x = nr;
            else if (nc < pivotStart.y) pivotStart.y = nc;
            else if (nr > pivotEnd.x) pivotEnd.x = nr;
            else if (nc > pivotEnd.y) pivotEnd.y = nc;

            // update highlights
            SetHighlights(true);
        }
        else
        {
            // set any previously highlighted to false
            SetHighlights(false);

            pivotStart = selected;
            pivotEnd = selected;
        }
    }

    private void SetHighlights(bool setTo)
    {
        // set any previously highlighted to false
        for (int rh = pivotStart.x; rh <= pivotEnd.x; rh++)
        {
            for (int ch = pivotStart.y; ch <= pivotEnd.y; ch++)
            {
                SpreadSheet.inst.GetCellAt(rh, ch).SetHighlight(setTo); // flags an error on start up for no apparent reason?? works fine
            }
        }
    }

    private void MarkCopy(bool toCut)
    {
        this.toCut = toCut;
        copyPivotStart = pivotStart;
        copyPivotEnd = pivotEnd;

        copyIndicator.gameObject.SetActive(true);
        //set size to encompass the region
        Vector2 regDim = new Vector2(copyPivotEnd.y - copyPivotStart.y + 1, copyPivotEnd.x - copyPivotStart.x + 1);
        copyIndicator.sizeDelta = rt.sizeDelta * regDim;
        //set position to middle of region
        copyIndicator.anchoredPosition = SpreadSheet.inst.SheetToWorld(copyPivotStart) + regDim * .5f;

        // if just a 1x1 copy then hide the base selector
        if (regDim.sqrMagnitude < 1.1f) rt.anchoredPosition = new Vector2(-1000, -1000); // hide it :)
    }

    private void DisableCopy()
    {
        copyPivotStart = new Vector2Int(-1, -1);
        copyIndicator.gameObject.SetActive(false);
    }

    private void Paste()
    {
        if (copyPivotStart == new Vector2Int(-1, -1)) return; // do nothing if nothing to copy

        Vector2Int areaDim = copyPivotEnd - copyPivotStart + Vector2Int.one;
        Color[,] colors = new Color[areaDim.x, areaDim.y];
        String[,] contents = new String[areaDim.x, areaDim.y];

        for (int r = copyPivotStart.x; r <= copyPivotEnd.x; r++)
        {
            for (int c = copyPivotStart.y; c <= copyPivotEnd.y; c++)
            {
                Cell source = SpreadSheet.inst.GetCellAt(r, c);

                colors[r - copyPivotStart.x, c - copyPivotStart.y] = source.GetBgColor();
                contents[r - copyPivotStart.x, c - copyPivotStart.y] = source.GetContent();

                if (toCut) // replace value
                {
                    source.SetBgColor(Color.white);

                    source.SetContent("");
                }
            }
        }

        for (int r = 0; r < areaDim.x; r++)
        {
            int dr = selected.x + r;
            for (int c = 0; c < areaDim.y; c++)
            {
                int dy = selected.y + c;

                // room for optimization
                if (!SpreadSheet.inst.InBounds(dr, dy)) continue;

                Cell destination = SpreadSheet.inst.GetCellAt(dr, dy);

                // check if this paste will complete the level
                LevelManager.Instance.CheckReplace(colors[r, c], contents[r, c], destination.GetBgColor(), destination.GetContent());

                destination.SetBgColor(colors[r, c]);
                destination.SetContent(contents[r, c]);
            }
        }

        pivotStart = selected;
        pivotEnd = selected + (copyPivotEnd - copyPivotStart);
        pivotEnd.Clamp(Vector2Int.zero, SpreadSheet.inst.GetSheetDimensions() - Vector2Int.one);
        SetHighlights(true);

        // nothing left to paste if we just cut
        if (toCut)
        {
            DisableCopy();
        }
        Maxwell.inst.summonMaxwell(4);
    }

    private void SelectAll()
    {
        pivotStart = Vector2Int.zero;
        pivotEnd = SpreadSheet.inst.GetSheetDimensions() - Vector2Int.one;
        SetHighlights(true);
    }

    private void HighlightColumn()
    {
        // override existing
        SetHighlights(false);
        // and define new
        pivotStart = new Vector2Int(0, selected.y);
        pivotEnd = new Vector2Int(SpreadSheet.inst.GetSheetDimensions().x - 1, selected.y);
        SetHighlights(true);
    }

    private void DeleteColumn()
    {
        // Lazy solution to pivots (used by excel)
        DisableCopy();

        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        for (int c = selected.y; c < dim.y; c++)
        {
            for (int r = pivotStart.x; r <= pivotEnd.x; r++)
            {
                Cell cell = SpreadSheet.inst.GetCellAt(r, c);

                Vector2Int next = new Vector2Int(r, c + 1);
                if (SpreadSheet.inst.InBounds(next))
                {
                    Cell nextCell = SpreadSheet.inst.GetCellAt(next);
                    cell.SetBgColor(nextCell.GetBgColor());
                    cell.SetContent(nextCell.GetContent());
                }
                else
                {
                    cell.SetBgColor(Color.white);
                    cell.SetContent("");
                }
            }
        }
        Maxwell.inst.summonMaxwell(4);
    }

    private void InsertColumn()
    {
         // Lazy solution to pivots (used by excel)
        DisableCopy();

        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        for (int c = dim.y - 1; c >= selected.y; c--)
        {
            for (int r = pivotStart.x; r <= pivotEnd.x; r++)
            {
                Cell cell = SpreadSheet.inst.GetCellAt(r, c);

                Vector2Int next = new Vector2Int(r, c - 1);
                if (c != selected.y)
                {
                    Cell nextCell = SpreadSheet.inst.GetCellAt(next);
                    cell.SetBgColor(nextCell.GetBgColor());
                    cell.SetContent(nextCell.GetContent());
                }
                else
                {
                    cell.SetBgColor(Color.white);
                    cell.SetContent("");
                }
            }
        }
        Maxwell.inst.summonMaxwell(4);
    }

    private void HighlightRow()
    {
        // override existing
        SetHighlights(false);
        // and define new
        pivotStart = new Vector2Int(selected.x, 0);
        pivotEnd = new Vector2Int(selected.x, SpreadSheet.inst.GetSheetDimensions().y - 1);
        SetHighlights(true);
    }

    private void DeleteRow()
    {
        // Lazy solution to pivots (used by excel)
        DisableCopy();

        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        for (int r = selected.x; r < dim.x; r++)
        {
            for (int c = pivotStart.y; c <= pivotEnd.y; c++)
            {
                Cell cell = SpreadSheet.inst.GetCellAt(r, c);

                Vector2Int next = new Vector2Int(r + 1, c);
                if (SpreadSheet.inst.InBounds(next))
                {
                    Cell nextCell = SpreadSheet.inst.GetCellAt(next);
                    cell.SetBgColor(nextCell.GetBgColor());
                    cell.SetContent(nextCell.GetContent());
                }
                else
                {
                    cell.SetBgColor(Color.white);
                    cell.SetContent("");
                }
            }
        }
        Maxwell.inst.summonMaxwell(4);
    }

    private void InsertRow()
    {
        // Lazy solution to pivots (used by excel)
        DisableCopy();

        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        for (int r = dim.x - 1; r >= selected.x; r--)
        {
            for (int c = pivotStart.y; c <= pivotEnd.y; c++)
            {
                Cell cell = SpreadSheet.inst.GetCellAt(r, c);

                Vector2Int next = new Vector2Int(r - 1, c);
                if (r != selected.x)
                {
                    Cell nextCell = SpreadSheet.inst.GetCellAt(next);
                    cell.SetBgColor(nextCell.GetBgColor());
                    cell.SetContent(nextCell.GetContent());
                }
                else
                {
                    cell.SetBgColor(Color.white);
                    cell.SetContent("");
                }
            }
        }
        Maxwell.inst.summonMaxwell( 4);
    }
}
