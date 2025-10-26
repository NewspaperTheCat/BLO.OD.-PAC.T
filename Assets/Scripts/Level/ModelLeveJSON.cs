using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ModelLevelJSON
{
    public class LevelJSON
    {
        public int level { get; set;}
        public int[] boardSize { get; set;}
        public CellJSON[] cells { get; set;}
    }

    public class CellJSON
    {
        public int row { get; set;}
        public int column { get; set;}
        public string value { get; set;}
        public string file_color {  get; set;}
    }
}
