using System;
using System.Drawing;

namespace Stegnote.Models
{
    public struct Pixel
    {
        public int X { get; }
        public int Y { get; }
        public Color Color { get; }

        public Pixel(int x, int y, Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }
}
