using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    Cell[,] problem;
    Cell[,] solution;
    int numCells = 0;
    int accuracy = 0;
    public Puzzle(Cell[,] p, Cell[,] s)
        { 
            problem = p;
            solution = s;
            foreach (Cell item in p)
            {
                numCells++;
            }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkPuzzle(Cell[,] problem, Cell[,] solution)
    {
        for (int i = 0; i < numCells; i++)
        {
            if (problem[x,y].GetBgColor == solution[x,y].GetBgColor && problem)
            { 
                
            }
        }
    }
}
