using System;
using Microsoft.SPOT;
using MaCRo.Config;
using MaCRo.Tools;

namespace MaCRo.Core
{
    /*public class Cell
    {
        bool _mapped;
        bool _object;

        public bool mapped { get { return _mapped; } set { _mapped = value; } }
        public bool isObject { get { return _object;}set{_object = value;}}
    }*/

    public class MapManager
    {
        private bool[][] mapped;
        private bool[][] isObject;
        private float[][] measure_mm;


        public MapManager()
        {
            /*
            int cells = GlobalVal.initialMapSize_cm / GlobalVal.cellSize_cm;

            measure_mm = new float[cells][];
            mapped = new bool[cells][];
            isObject = new bool[cells][];

            for (int i = 0; i < cells; i++)
            {
                measure_mm[i] = new float[cells];
                mapped[i] = new bool[cells];
                isObject[i] = new bool[cells];
                for (int j = 0; j < cells; j++)
                {
                    mapped[i][j] = false;
                    isObject[i][j] = false;
                    measure_mm[i][j] = float.MaxValue;
                    //map[i][j] = new Cell();
                    //map[i][j].mapped = false;
                    //map[i][j].isObject = false;
                }
            }
             */
        }

        public void SetObject(Position position)
        {
            //map[position.x][position.y].isObject = true;
        }

        public bool isMapped(Position position)
        {
            //return map[position.x][position.y].mapped;
            return false;
        }

        public void SetMapped(Position position)
        {
            //map[position.x][position.y].mapped = true;
        }
    }
}
