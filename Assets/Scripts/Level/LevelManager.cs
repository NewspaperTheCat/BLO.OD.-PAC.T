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
        string path = Path.Combine(Application.dataPath, "Levels", $"brilliantLevel{level}.json");

        if(!File.Exists(path))
        {
            Debug.LogError("Level JSON File not found at: " + path);
            return;
        }

        //Read JSON
        string json = File.ReadAllText(path);

        //Deserialize to class
        LevelJSON levelData = JsonConvert.DeserializeObject<LevelJSON>(json);

        level = levelData.level;
        //Debug.Log("Board size array: " + string.Join(", ", levelData.boardSize));

        SpreadSheet.inst.CreateGrid(levelData.boardSize[0], levelData.boardSize[1]);

    }
}
