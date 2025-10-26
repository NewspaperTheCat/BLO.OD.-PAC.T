using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using ModelLevelJSON;

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
        string path = Path.Combine(Application.streamingAssetsPath, "1");
    }
}
