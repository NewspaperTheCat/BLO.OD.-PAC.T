using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Selector : MonoBehaviour
{
    // serialized so can be viewed
    [SerializeField] int row = 0; 
    [SerializeField] int col = 0;


    void Start()
    {
        MoveSelected(0, 0);
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
    
    // deals with update world position as well
    void MoveSelected(int dr, int dc) { SetSelected(dr + row, dc + col); }
    void SetSelected(int r, int c)
    {
        Vector2Int dim = SpreadSheet.inst.GetArrayDimensions();
        row = (r + dim.x) % dim.x;
        col = (c + dim.y) % dim.y;

        transform.position = SpreadSheet.inst.SheetToWorld(new Vector2(row, col));
    }
}
