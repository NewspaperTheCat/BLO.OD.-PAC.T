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
    private bool levelComplete = false;
    public static LevelManager Instance { get; private set; }

    enum RequirementType { HoverOver, NoCell, Replace, AnswerKey };

    private Color nullColor;
    [SerializeField] bool displayDebug;

    private abstract class Requirement
    {
        public RequirementType type;
        public bool done = false;
    }

    private class HoverOver : Requirement
    {
        public KeyCell criteria;

        public HoverOver(String targetContent, Color targetColor) {
            type = RequirementType.HoverOver;
            criteria = new KeyCell(targetContent, targetColor);    
        }
    }

    private class NoCell : Requirement
    {
        public KeyCell refuted;

        public NoCell(String targetContent, Color targetColor) {
            type = RequirementType.NoCell;
            refuted = new KeyCell(targetContent, targetColor);
        }
    }

    private class Replace : Requirement
    {
        // does not use key cell to make selector implementation easier
        public String sourceContent;
        public Color sourceColor;
        public String targetContent;
        public Color targetColor;

        public Replace(String sourceContent, Color sourceColor, String targetContent, Color targetColor) {
            type = RequirementType.Replace;
            if (sourceContent == "Null") sourceContent = null;
            if (targetContent == "Null") targetContent = null;
            this.sourceContent = sourceContent; this.sourceColor = sourceColor;
            this.targetColor = targetColor; this.targetContent = targetContent;
        }
    }

    private class KeyCell {
        public Vector2Int rc; public String content; public Color bgColor;

        public KeyCell(String content, Color bgColor)
        {
            if (content == "Null") content = null;
            this.rc = new Vector2Int(-1, -1); this.content = content; this.bgColor = bgColor;
        }
        public KeyCell(Vector2Int rc, String content, Color bgColor)
        {
            if (content == "Null") content = null;
            this.rc = rc; this.content = content; this.bgColor = bgColor;
        }

        public bool Accepts(Cell real)
        {
            return (content == null || content == real.GetContent()) && (bgColor == Color.white || bgColor == real.GetBgColor());
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

        ColorUtility.TryParseHtmlString("#FFFFFF", out nullColor);
    }

    void OnDisable()
    {
        // when disabled (destoryed or scene change) removes this reference from static value
        Instance = null;
    }

    void Start()
    {
        if (level == 0) NewDay();
    }

    private void setLevel()
    {
        // reset level complete
        levelComplete = false;

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

        // patchwork explain objective
        Debug.Log(levelData.description);

        level = levelData.level;
        //if (displayDebug) Debug.Log("Board size array: " + string.Join(", ", levelData.boardSize))

        SpreadSheet.inst.CreateGrid(levelData.boardSize[0], levelData.boardSize[1]);


        for (int i = 0; i < levelData.cells.Length; i++)
        {
            Cell currentCell = SpreadSheet.inst.GetCellAt(levelData.cells[i].row - 1, levelData.cells[i].column - 1);

            ColorUtility.TryParseHtmlString($"#{levelData.cells[i].file_color.Substring(2)}", out Color currentColor);

            currentCell.SetBgColor(currentColor);

            // if (displayDebug) Debug.Log(currentColor);

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
        if (levelData.requirements.answerKey.regionStart[0] != -1) // if this is breaking then JSON answerkey doesn't have cells listed 
        {
            AnswerKey ak = new AnswerKey(
                new Vector2Int(levelData.requirements.answerKey.regionStart[0] - 1, levelData.requirements.answerKey.regionStart[1] - 1),
                new Vector2Int(levelData.requirements.answerKey.regionEnd[0] - 1, levelData.requirements.answerKey.regionEnd[1] - 1));

            // add all cells
            for (int i = 0; i < levelData.requirements.answerKey.cells.Length; i++)
            {
                CellJSON cj = levelData.requirements.answerKey.cells[i];
                Color targetColor = nullColor;
                if (cj.file_color != "Null")
                    ColorUtility.TryParseHtmlString($"#{cj.file_color.Substring(2)}", out targetColor);
                ak.cells.Add(new KeyCell(new Vector2Int(cj.row - 1, cj.column -1), cj.value, targetColor));
            }
            requirements.Add(ak);
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
        level++;
        Debug.Log(level);

        if (level > levelAmountsPerDay[GameManager.inst.day])
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

        if (!levelComplete)
        {
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
                else
                {
                    if (displayDebug) Debug.Log($"Win, level: {level}");
                }
            }
            if (allDone) CompleteLevel();
        }
        else // if the level was complete, we don't need to check for answers, instead enable next level and that sequence
        {
            ProgressCompleteAnimation();

            // allow the player to continue
            if (Input.GetKeyDown(KeyCode.Period) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                AudioManager.inst.PlayRandomKeyPress();
                NextLevel();
            }
        }
    }

    private void CheckHoverOver(HoverOver horeq) {
        Cell cur = Selector.inst.GetHover();
        horeq.done = horeq.criteria.Accepts(cur);
        if (displayDebug && horeq.done) Debug.Log("Hover");
    }

    private void CheckNoCell(NoCell noCell) {
        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        for (int r = 0; r < dim.x; r++)
        {
            for (int c = 0; c < dim.y; c++)
            {
                Cell cell = SpreadSheet.inst.GetCellAt(r, c);
                if (noCell.refuted.Accepts(cell))
                {
                    // any infraction then break
                    noCell.done = false;
                    return;
                }
            }
        }
        // if made it through all cells then success
        if (displayDebug) Debug.Log("No cell");
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
                    && (rreq.sourceContent == null || rreq.sourceContent == sourceContent)
                    && (rreq.targetColor == nullColor || rreq.targetColor == targetColor)
                    && (rreq.targetContent == null || rreq.targetContent == targetContent))
                {
                    if (displayDebug) Debug.Log("Replace");
                    rreq.done = true; // can only be set forwards
                }
            }
        }
    }

    private void CheckAnswerKey(AnswerKey akreq)
    {

        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        //if (displayDebug) Debug.Log(akreq.regionStart + " to " + akreq.regionEnd);

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
                    Vector2Int pos = key.rc + start;


                    if (!SpreadSheet.inst.InBounds(pos))
                    {
                        if (displayDebug) Debug.Log(start + " lead to out of bound");
                        thisStartHasIt = false;
                        break;
                    }

                    Cell real = SpreadSheet.inst.GetCellAt(pos.x, pos.y);
                    if (!key.Accepts(real))
                    {
                        if (displayDebug) Debug.Log(start + " checking at " + pos + " did not have desired results\nFound " + real.GetContent() + ", " + real.GetBgColor() + "\nExpected " + key.content + ", " + key.bgColor);
                        // if not equal to any key
                        thisStartHasIt = false;
                        break;
                    }
                }
                if (thisStartHasIt)
                {
                    if (displayDebug) Debug.Log("Answer Key");
                    akreq.done = true;
                    return;
                }
            }
        }

        // got to end and didn't find it
        akreq.done = false;
    }

    // complete animation parameters
    float ringSpeed = 12.5f;
    float ringThickness = 1.25f;
    private float timeSinceComplete = 0;

    private void CompleteLevel()
    {
        Debug.Log("Level Completed!");
        AudioManager.inst.PlayRandomSuccess();
        levelComplete = true;
        timeSinceComplete = 0;
    }
    
    // handles complete animation
    public void ProgressCompleteAnimation()
    {
        timeSinceComplete += Time.deltaTime * ringSpeed;
        float outer = timeSinceComplete / 2.0f;
        float inner = outer - ringThickness;
        Vector2Int dim = SpreadSheet.inst.GetSheetDimensions();
        Vector2Int center = dim / 2 - Vector2Int.one;

        if (inner > dim.magnitude + 2) return;

        for (int r = 0; r < dim.x; r++)
        {
            for (int c = 0; c < dim.y; c++)
            {
                Cell selected = Selector.inst.GetHover();
                Vector2Int sPos = new Vector2Int(selected.row - 1, selected.col - 1);

                Vector2Int curPos = new Vector2Int(r, c);
                float dis = Vector2Int.Distance(sPos, curPos);
                Cell cur = SpreadSheet.inst.GetCellAt(curPos);

                Debug.Log(curPos + " and dim " + (dim / 2));

                if (outer > dis)
                {
                    if (dis > inner)
                    {
                        cur.SetContent("");
                        cur.SetBgColor(Color.red);
                    }
                    else
                    {
                        // bad manual check for finish text
                        if (curPos == center)
                        {
                            cur.SetContent("Level");
                            cur.SetBgColor(Color.yellow);
                        }
                        else if (curPos == center + new Vector2Int(0, 1))
                        {
                            cur.SetContent("Complete");
                            cur.SetBgColor(Color.yellow);
                        }
                        else if (curPos == center + new Vector2Int(1, 0))
                        {
                            cur.SetContent("Press");
                            cur.SetBgColor(Color.yellow);
                        }
                        else if (curPos == center + Vector2Int.one)
                        {
                            cur.SetContent("CTRL >");
                            cur.SetBgColor(Color.yellow);
                        }
                        else
                        {
                            cur.SetContent("");
                            cur.SetBgColor(Color.white);
                        }
                    }
                }
            }
        }
    }
}

