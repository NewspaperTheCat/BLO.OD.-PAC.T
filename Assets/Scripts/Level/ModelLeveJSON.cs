using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ModelLevelJSON
{
    public class LevelJSON
    {
        public int level { get; set; }
        public string description { get; set; }
        public RequirementsJSON requirements { get; set; }

        public int[] boardSize { get; set; }
        public CellJSON[] cells { get; set; }
    }

    public class CellJSON
    {
        public int row { get; set; }
        public int column { get; set; }
        public string value { get; set; }
        public string file_color { get; set; }
    }

    public class RequirementsJSON
    {
        public string[][] hoverover { get; set; } // { ["Value", "Color"], ["Value", "Color"], ["Value", "Color"], ... }
        public string[][] nocolor { get; set; } // { ["Value", "Color"], ["Value", "Color"], ["Value", "Color"], ... }
        public string[][] replace { get; set; } // { ["Value", "Color", "Value", "Color"], ["Value", "Color", "Value", "Color"], ... }        
        public AnswerKeyJSON answerKey;
    }

    public class AnswerKeyJSON
    {
        public int[] regionStart;
        public int[] regionEnd;
        public CellJSON[] cells;
    }
}