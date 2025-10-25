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
    
    // deals with update world position as well
    void MoveSelected(int dr, int dc) {
        SetSelected(dr + selected.x, dc + selected.y);
    }
    void SetSelected(int r, int c)
    {
        Vector2Int dim = SpreadSheet.inst.GetArrayDimensions();
        Vector2Int init = new Vector2Int(selected.x, selected.y);
        int nr = (r + dim.x) % dim.x;
        int nc = (c + dim.y) % dim.y;

        selected = new Vector2Int(nr, nc);
        transform.position = SpreadSheet.inst.SheetToWorld(selected);

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
                    SpreadSheet.inst.GetCellAt(rh, ch).SetHighlight(false); // flags an error on start up for no apparent reason?? works fine
                }
            }

            pivotStart = selected;
            pivotEnd = selected;
        }
    }
}
