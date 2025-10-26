using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using ModelLevelJSON;
using System.IO;
using Newtonsoft.Json;

public class LevelManager : MonoBehaviour
{

    [SerializeField] private int level;
    public static LevelManager Instance { get; private set; }


    void Awake()
    {
        //Singleton setup
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
        
    
    void Start()
    {
        setLevel();
    }

    private void setLevel()
    {
        string path = Path.Combine(Application.dataPath, "Levels", $"brilliantLevel{level}.json");

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

            Debug.Log(currentColor);

            if (levelData.cells[i].value != null)
            {
                currentCell.SetContent(levelData.cells[i].value);
            }


        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            AudioManager.inst.PlayRandomKeyPress();
            level++;
            setLevel();
        }
        if(Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            AudioManager.inst.PlayRandomKeyPress();
            setLevel();
        }
    }
}


