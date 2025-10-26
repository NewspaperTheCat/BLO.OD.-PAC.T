using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton Design Pattern (to a degree)
    public static GameManager inst;
    void OnEnable()
    {
        // ensures there is only ever one Game Manager
        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int day = 0;
    Boolean puzzleSolved;
    int puzzleIndex = 0;
    Boolean stopwatchActive;
    public float currentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        stopwatchActive = true;
        puzzleSolved = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (stopwatchActive)
        {
            currentTime += Time.deltaTime;
            //Debug.Log(time.ToString(@"mm\:ss\:fff"));
        }

        TimeSpan time = TimeSpan.FromSeconds(currentTime);
    }
}
