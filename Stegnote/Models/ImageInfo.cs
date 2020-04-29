using System;
using System.Collections.Generic;
using System.Drawing;
using Color = System.Drawing.Color;

namespace Stegnote.Models
{
    public class ImageInfo
    {
        public ImageInfo(Image imageSource, Bitmap bitmap, string path)
        {
            ImageSource = imageSource;
            Bitmap = bitmap;
            Height = bitmap.Height;
            Width = bitmap.Width;
            Path = path;
            CashedBitmap = (Bitmap)bitmap.Clone();
        }
        public Image ImageSource { set; get; }
        public Bitmap Bitmap { set; get; }
        public int Height { set; get; }
        public int Width { set; get; }
        public Color[,] Pixels { set; get; }
        public string Path { set; get; }
        public Bitmap CashedBitmap { set; get; }

    }
}
    /*public class Pixel : IComparable
    {
        public string Color { get; set; }
        public int Amount { get; set; }
        public List<Coordinates> Coordinates { get; set; }

        public Pixel(string color)
        {
            Color = color;
        }

        public Pixel(string color, int amount, List<Coordinates> coordinates) : this(color)
        {
            Amount = amount;
            Coordinates = coordinates;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) { return 1; }
            Pixel pixel = obj as Pixel;
            if (pixel != null)
            {
                return this.Color.CompareTo(pixel.Color);
            }
            else
            {
                throw new ArgumentException("Object is not pixel");
            }
        }
    }*/

    /*public struct Coordinates
    {
        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;
    }*/
//}