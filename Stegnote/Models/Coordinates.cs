using System;
using System.Drawing;

namespace Stegnote.Models
{
    public struct Coordinates
    {
        public int X { get; }
        public int Y { get; }
        public Color Color { get; }

        public Coordinates(int x, int y, Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}
