using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;


public class Maxwell : MonoBehaviour
{
    Vector2Int dimensions;
    [SerializeField] private TextMeshProUGUI dialogueBox;
    public SpriteRenderer textBubble;
    Boolean textBubbleActive = false;
    public SpriteRenderer maxwellPfp;
    List<String> flavorText = new List<String>();
    [SerializeField] TextAsset flavor;
    List<String> changeColor = new List<String>();
    [SerializeField] TextAsset color;
    List<String> changeText = new List<String>();
    [SerializeField] TextAsset text;
    List<String> changeSolution = new List<String>();
    [SerializeField] TextAsset solution;

    // Singleton Design Pattern (to a degree)
    public static Maxwell inst;
    void OnEnable()
    {
        // ensures there is only ever one Maxwell
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

    // Start is called before the first frame update
    void Start()
    {
        dimensions = SpreadSheet.inst.GetSheetDimensions();
        textBubble.gameObject.SetActive(false);
        maxwellPfp.gameObject.SetActive(false);

        readText(flavor, "f");
         readText(color, "c");
         readText(text, "t");
         readText(solution, "s");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void readText([SerializeField] TextAsset file, String name)
    {
        var splitFile = new string[] {"\r\n", "\r", "\n"};
        var Lines = file.text.Split(splitFile, System.StringSplitOptions.RemoveEmptyEntries);
        if (name == "f")
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i].Split(splitFile, System.StringSplitOptions.None);
                flavorText.Add(line[0]);
            }
        }

        else if (name == "c")
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i].Split(splitFile, System.StringSplitOptions.None);
                changeColor.Add(line[0]);
            }
        }

        else if (name == "t")
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i].Split(splitFile, System.StringSplitOptions.None);
                changeText.Add(line[0]);
            }
        }

        else
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i].Split(splitFile, System.StringSplitOptions.None);
                changeSolution.Add(line[0]);
            }
        }

    }

    public void changeLevel()
    {
        dialogueBox.text = (changeSolution[UnityEngine.Random.Range(0, changeSolution.Count)]);
    }

    public void summonMaxwell( int levelIndex)
    {
        int maxwellOdds = UnityEngine.Random.Range(0, 5);
        int x = UnityEngine.Random.Range(0,SpreadSheet.inst.GetSheetDimensions().x);
        int y = UnityEngine.Random.Range(0, SpreadSheet.inst.GetSheetDimensions().y);
        if (maxwellOdds == 0 && levelIndex != 0 && levelIndex != 8)
        {
            int option = UnityEngine.Random.Range(0, 2);

            if (option == 0)
            {
                //change color of random cell
                SpreadSheet.inst.GetCellAt(new Vector2Int(x, y)).SetBgColor(Color.red);
                if (!textBubbleActive)
                {
                    textBubbleActive = true;
                    textBubble.gameObject.SetActive(true);
                    maxwellPfp.gameObject.SetActive(true);
                }
                dialogueBox.text = (changeColor[UnityEngine.Random.Range(0, changeColor.Count)]);

            }

            else
            {
                //change text of random cell
                SpreadSheet.inst.GetCellAt(x, y).SetContent("Maxwell");
                if (!textBubbleActive)
                {
                    textBubbleActive = true;
                    textBubble.gameObject.SetActive(true);
                    maxwellPfp.gameObject.SetActive(true);
                }
                dialogueBox.text = (changeText[UnityEngine.Random.Range(0, changeColor.Count)]);
            }
        }

        else if (maxwellOdds >=3)
        {
            if (!textBubbleActive)
            {
                textBubbleActive = true;
                textBubble.GameObject().SetActive(true);
                maxwellPfp.GameObject().SetActive(true);
            }
            dialogueBox.text = (flavorText[UnityEngine.Random.Range(0, flavorText.Count)]);
        }


    }
}
