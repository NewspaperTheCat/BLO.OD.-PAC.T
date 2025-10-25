using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class Maxwell : MonoBehaviour
{
    Vector2Int dimensions =  SpreadSheet.inst.GetArrayDimensions();
    List<string> flavorText;
    List<string> changeColor;
    List<string> changeText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void summonMaxwell(string option)
    {
        int x = Random.Range(0, dimensions.x+1);
        int y = Random.Range(0, dimensions.y+1);
        if (option == "color")
        {
            //change color of random cell
            SpreadSheet.inst.GetCellAt(new Vector2Int(x, y)).SetBgColor(Color.red);

        }

        else 
        {
            //change text of random cell
            SpreadSheet.inst.GetCellAt(x, y).SetContent("Hello");
        }
    }
}
