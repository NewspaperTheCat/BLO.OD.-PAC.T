using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using ModelLevelJSON;
using System.IO;
using Newtonsoft.Json;
using System;

public class LevelManager : MonoBehaviour
{

    [SerializeField] private int level;
    public static LevelManager Instance { get; private set; }

    enum RequirementType { HoverOver, NoCell, Replace, AnswerKey };

    private Color nullColor = new Color(1, 1, 1); // barely not black

    private abstract class Requirement
    {
        public RequirementType type;
        public bool done = false;
    }

    private class HoverOver : Requirement
    {
        public String targetContent;
        public Color targetColor;

        public HoverOver(String targetContent, Color targetColor) { type = RequirementType.HoverOver; this.targetColor = targetColor; this.targetContent = targetContent; }
    }

    private class NoCell : Requirement
    {
        public String targetContent;
        public Color targetColor;

        public NoCell(String targetContent, Color targetColor) { type = RequirementType.NoCell; this.targetColor = targetColor; this.targetContent = targetContent; }
    }

    private class Replace : Requirement
    {
        public String sourceContent;
        public Color sourceColor;
        public String targetContent;
        public Color targetColor;

        public Replace(String sourceContent, Color sourceColor, String targetContent, Color targetColor) {
            type = RequirementType.Replace;
            this.sourceContent = sourceContent; this.sourceColor = sourceColor;
            this.targetColor = targetColor; this.targetContent = targetContent;
        }
    }

    private class KeyCell {
        public Vector2Int rc; public String content; public Color bgColor;
        public KeyCell(Vector2Int rc, String content, Color bgColor)
        {
            this.rc = rc; this.content = content; this.bgColor = bgColor;
        }
    }
        
    private class AnswerKey : Requirement
    {
        public Vector2Int regionStart;
        public Vector2Int regionEnd;
        public List<KeyCell> cells = new List<KeyCell>();

        public AnswerKey(Vector2Int rs, Vector2Int re)
        {
            type = RequirementType.AnswerKey;
            regionStart = rs; regionEnd = re;
            // cells defined later
        }
    }

    private List<Requirement> requirements = new List<Requirement>();


    void Awake()
    {
        //Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDisable()
    {
        // when disabled (destoryed or scene change) removes this reference from static value
        Instance = null;
    }

    void Start()
    {
        NewDay();
    }

    private void setLevel()
    {
        string path = Path.Combine(Application.dataPath, "Resources/Levels", $"brilliantLevel{level}.json");

        if (!File.Exists(path))
        {
            Debug.LogError("Level JSON File not found at: " + path);
            return;
        }

        //Read JSON
        string json = File.ReadAllText(path);

        //Deserialize to class
        LevelJSON levelData = JsonConvert.DeserializeObject<LevelJSON>(json);

        level = levelData.level;
        //Debug.Log("Board size array: " + string.Join(", ", levelData.boardSize))

        SpreadSheet.inst.CreateGrid(levelData.boardSize[0], levelData.boardSize[1]);


        for (int i = 0; i < levelData.cells.Length; i++)
        {
            Cell currentCell = SpreadSheet.inst.GetCellAt(levelData.cells[i].row - 1, levelData.cells[i].column - 1);

            ColorUtility.TryParseHtmlString($"#{levelData.cells[i].file_color.Substring(2)}", out Color currentColor);

            currentCell.SetBgColor(currentColor);

            // Debug.Log(currentColor);

            if (levelData.cells[i].value != null)
            {
                currentCell.SetContent(levelData.cells[i].value);
            }
        }

        // format this level's requirements
        requirements = new List<Requirement>();
        for (int i = 0; i < levelData.requirements.hoverover.Length; i++)
        {
            ColorUtility.TryParseHtmlString($"#{levelData.requirements.hoverover[i][1].Substring(2)}", out Color currentColor);
            requirements.Add(new HoverOver(levelData.requirements.hoverover[i][0], currentColor));
        }
        for (int i = 0; i < levelData.requirements.nocell.Length; i++)
        {
            ColorUtility.TryParseHtmlString($"#{levelData.requirements.nocell[i][1].Substring(2)}", out Color currentColor);
            requirements.Add(new NoCell(levelData.requirements.nocell[i][0], currentColor));
        }
        for (int i = 0; i < levelData.requirements.replace.Length; i++)
        {
            Color sourceColor = nullColor;
            if (levelData.requirements.replace[i][1] != "Null")
                ColorUtility.TryParseHtmlString($"#{levelData.requirements.replace[i][3].Substring(2)}", out sourceColor);
            Color targetColor = nullColor;
            if (levelData.requirements.replace[i][3] != "Null")
                ColorUtility.TryParseHtmlString($"#{levelData.requirements.replace[i][1].Substring(2)}", out targetColor);
            requirements.Add(new Replace(levelData.requirements.hoverover[i][2], sourceColor, levelData.requirements.hoverover[i][0], targetColor));
        }
        if (false && levelData.requirements.answerKey.regionStart[0] != -1)
        {
            AnswerKey ak = new AnswerKey(
                new Vector2Int(levelData.requirements.answerKey.regionStart[0], levelData.requirements.answerKey.regionStart[1]),
                new Vector2Int(levelData.requirements.answerKey.regionEnd[0], levelData.requirements.answerKey.regionEnd[1]));

            // add all cells
            for (int i = 0; i < levelData.requirements.answerKey.cells.Length; i++)
            {
                CellJSON cj = levelData.requirements.answerKey.cells[i];
                Color targetColor = nullColor;
                if (cj.file_color != "Null")
                    ColorUtility.TryParseHtmlString($"#{cj.file_color.Substring(2)}", out targetColor);
                ak.cells.Add(new KeyCell(new Vector2Int(cj.row, cj.column), cj.value, targetColor));
            }
        }
    }

    // ===============================================
    // Level navigation logic

    int[] levelAmountsPerDay = { 9, 13, 17, 21, 25 };

    // called externally
    private void NewDay()
    {
        level = 1;
        setLevel();
    }

    private void NextLevel()
    {
        AudioManager.inst.PlayRandomSuccess();
        level++;

        if (level > levelAmountsPerDay[GameManager.inst.day - 1])
        {
            GameManager.inst.DayComplete();
        }
        else
        {
            setLevel();
        }
    }

    // ===============================================

    private void Update()
    {
        // reset option
        if (Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            AudioManager.inst.PlayRandomKeyPress();
            setLevel();
        }

        // End Requirement Checks
        bool allDone = true;
        for (int i = 0; i < requirements.Count; i++)
        {
            switch (requirements[i].type)
            {
                case RequirementType.HoverOver:
                    HoverOver horeq = (HoverOver)requirements[i];
                    CheckHoverOver(horeq);
                    break;
                case RequirementType.NoCell:
                    NoCell ncreq = (NoCell)requirements[i];
                    CheckNoCell(ncreq);
                    break;
                case RequirementType.AnswerKey:
                    AnswerKey akreq = (AnswerKey)requirements[i];
                    CheckAnswerKey(akreq);
                    break;
            }
            // if a singler missing requirement then exit
            if (!requirements[i].done) { allDone = false; break; }
        }


        // // allow the player to continue
        if (Input.GetKeyDown(KeyCode.Period) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            AudioManager.inst.PlayRandomKeyPress();
            // if (allDone) NextLevel(); // ideally show some indicator
            NextLevel();
        }
        // indicator cut for time, just boots you to next goal
    }

    private void CheckHoverOver(HoverOver horeq) {
        Cell cur = Selector.inst.GetHover();
        horeq.done = (horeq.targetContent == null || horeq.targetContent == cur.GetContent())
                    &&
                    (horeq.targetColor == nullColor || horeq.targetColor == cur.GetBgColor());
    }

    private void CheckNoCell(NoCell noCell) {
        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        for (int r = 0; r < dim.x; r++)
        {
            for (int c = 0; c < dim.y; c++)
            {
                Cell cell = SpreadSheet.inst.GetCellAt(r, c);
                if ((noCell.targetContent == "Null" || noCell.targetContent == cell.GetContent())
                    && (noCell.targetColor == nullColor || noCell.targetColor == cell.GetBgColor()))
                {
                    // any infraction then break
                    noCell.done = false;
                    return;
                }
            }
        }
        // if made it through all cells then success
        noCell.done = true;
    }

    // public because called externally by Selector when pasted over something
    public void CheckReplace(Color sourceColor, String sourceContent, Color targetColor, String targetContent)
    {
        for (int i = 0; i < requirements.Count; i++)
        {
            if (requirements[i].type == RequirementType.Replace)
            {
                Replace rreq = (Replace)requirements[i];
                if ((rreq.sourceColor == nullColor || rreq.sourceColor == sourceColor)
                    && (rreq.sourceContent == "Null" || rreq.sourceContent == sourceContent)
                    && (rreq.targetColor == nullColor || rreq.targetColor == targetColor)
                    && (rreq.targetContent == "Null" || rreq.targetContent == targetContent))
                {
                    rreq.done = true; // can only be set forwards
                }
            }
        }
    }
    
    private void CheckAnswerKey(AnswerKey akreq)
    {
        for (int r = akreq.regionStart.x; r <= akreq.regionEnd.x; r++)
        {
            for (int c = akreq.regionStart.y; c <= akreq.regionEnd.y; c++)
            {
                // checks if cells are equal with this pivot
                Vector2Int start = new Vector2Int(r, c);
                bool thisStartHasIt = true;
                for (int i = 0; i < akreq.cells.Count; i++)
                {
                    KeyCell key = akreq.cells[i];
                    Cell real = SpreadSheet.inst.GetCellAt(key.rc + start);
                    if (!((key.content == "Null" || key.content == real.GetContent())
                            && (key.bgColor == nullColor || key.bgColor == real.GetBgColor())))
                    {
                        // if not equal to any key
                        thisStartHasIt = false;
                        break;
                    }
                }
                if (thisStartHasIt)
                {
                    akreq.done = true;
                    return;
                }
            }
        }

        // got to end and didn't find it
        akreq.done = false;
    }
}

