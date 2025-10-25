using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.Burst.Intrinsics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Selector : MonoBehaviour
{
    // serialized so can be viewed
    [SerializeField] Vector2Int selected;

    [SerializeField] Vector2Int pivotStart;
    [SerializeField] Vector2Int pivotEnd;

    void Start()
    {
        Reset();
    }

    public void Reset() {
        pivotStart = Vector2Int.zero;
        pivotEnd = Vector2Int.zero;
        SetSelected(0, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { MoveSelected(0, -1); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { MoveSelected(0, 1); }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { MoveSelected(1, 0); }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { MoveSelected(-1, 0); }

        // DELETE THIS when this component gets fleshed out more
        // Debug.Log(new Vector2(row, col) + " --> " + SpreadSheet.inst.WorldToSheet(transform.position));
    }

    bool IsShiftPressed() { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); }
    bool IsCTRLPressed() { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); }

    // deals with update world position as well
    void MoveSelected(int dr, int dc)
    {
        if (Input.GetKey(KeyCode.LeftControl)) {
            // if control then move via continuity
            SetSelected(GetNextContinuityEnd(selected, new Vector2Int(dr, dc)));
        } else {
            SetSelected(dr + selected.x, dc + selected.y);
        }
    }
    
    Vector2Int GetNextContinuityEnd(Vector2Int start, Vector2Int dir)
    {
        bool continuityBlank = SpreadSheet.inst.GetCellAt(start).GetContent() == "";

        Vector2Int nullEnd = new Vector2Int(-1, -1);
        Vector2Int foundEnd = nullEnd;
        while (foundEnd == nullEnd)
        {
            Vector2Int next = start + dir;
            if (!SpreadSheet.inst.InBounds(next)) { foundEnd = start; continue; } // exits when reach edge

            bool nextContinuity = SpreadSheet.inst.GetCellAt(next).GetContent() == "";
            if (nextContinuity != continuityBlank) { foundEnd = next; continue; }

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
        transform.position = (Vector3)SpreadSheet.inst.SheetToWorld(selected) + Vector3.back;

        if (IsShiftPressed()) {
            if (nr < pivotStart.x) pivotStart.x = nr;
            else if (nc < pivotStart.y) pivotStart.y = nc;
            else if (nr > pivotEnd.x) pivotEnd.x = nr;
            else if (nc > pivotEnd.y) pivotEnd.y = nc;
            
            // update highlights
            for (int rh = pivotStart.x; rh <= pivotEnd.x; rh++)
            {
                for (int ch = pivotStart.y; ch <= pivotEnd.y; ch++)
                {
                    SpreadSheet.inst.GetCellAt(rh, ch).SetHighlight(true);
                }
            }
            SpreadSheet.inst.GetCellAt(nr, nc).SetHighlight(false);
        } else {
            // set any previously highlighted to false
            for (int rh = pivotStart.x; rh <= pivotEnd.x; rh++)
            {
                for (int ch = pivotStart.y; ch <= pivotEnd.y; ch++)
                {
                    if (SpreadSheet.inst.GetCellAt(rh, ch) != null)
                        SpreadSheet.inst.GetCellAt(rh, ch).SetHighlight(false); // flags an error on start up for no apparent reason?? works fine
                }
            }

            pivotStart = selected;
            pivotEnd = selected;
        }
    }
}
