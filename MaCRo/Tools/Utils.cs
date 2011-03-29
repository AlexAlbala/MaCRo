using System;
using Microsoft.SPOT;

namespace MaCRo.Tools
{
    public enum Mode
    {
        SearchingForWall,
        FollowWall
    }

    public enum Orientation
    {
        HorizontalPos,
        VerticalPos,
        HorizontalNeg,
        VerticalNeg
    }

    public class Position
    {
        private int _x;
        private int _y;
        public int x { get { return _x; } set { _x = value; } }
        public int y { get { return _y; } set { _y = value; } }
    }
}
