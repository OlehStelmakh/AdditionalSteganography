using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using Stegnote.Models;
using Color = System.Drawing.Color;

namespace WpfApp1
{
    public class MainCalculations
    {

        public Bitmap bm { set; get; }

        public ImageInfo downloadedImage { set; get; }

        public MessageInfo messageInfo { set; get; }

        public MainCalculations()
        {

        }

        public void ExecuteOpenFileDialog()
        {
            string path = "/Users/olehstelmakh/Desktop/nature.jpg";
            Image image = Image.FromFile(path);
            Bitmap bitmap = new Bitmap(image);
            downloadedImage = new ImageInfo(image, bitmap, path);
            downloadedImage.Pixels = BitmapToArray2D(downloadedImage.Bitmap);
        }

        private Coordinates getFirstRandomCoordinates(ImageInfo image)
        {
            Random random = new Random();
            int x = random.Next(image.Width);
            int y = random.Next(image.Height);
            Color color = image.Bitmap.GetPixel(x, y);
            Coordinates coordinates = new Coordinates(x, y, color);
            OutputInfo.first = coordinates;
            
            return coordinates;
        }
        
        public void ExecuteEncrypt()
        {
            Coordinates coordinates = getFirstRandomCoordinates(downloadedImage);
            string textBlock = Message.ToLower();
            int offsetCount = CalcuteOffset(coordinates);
            OutputInfo.offset = offsetCount;
        }

        public void ExecuteDecrypt()
        {
            
        }

        

        private string _message = "Hello world Lorem sipao hdbae kakebc sdahrak";

        public string Message
        {
            get => _message;
            set => _message = value.ToString();
        }

        private int CalcuteOffset(Coordinates firstCoordinates)
        {
            int offsetCount = 0;
            for (int i = 0; i < firstCoordinates.X; i++)
            {
                for (int j = 0; j < firstCoordinates.Y; j++)
                {
                    if (downloadedImage.Pixels[i, j].Name == firstCoordinates.Color.Name)
                    {
                        offsetCount++;
                    }
                }
            }
            return offsetCount;
        }
        
        private Color[,] BitmapToArray2D(Bitmap image)
        {
            Color[,] array2D = new Color[image.Width, image.Height];
 
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

            unsafe
            {
                byte* address = (byte*)bitmapData.Scan0;

                int paddingOffset = bitmapData.Stride - (image.Width * 4);

                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        byte[] temp = new byte[4];
                        temp[0] = address[0];
                        temp[1] = address[1];
                        temp[2] = address[2];
                        temp[3] = address[3];
                        Color color = Color.FromArgb(temp[3], temp[2], temp[1], temp[0]);
                        
                        array2D[i, j] = color;
                        address += 4;
                    }

                    address += paddingOffset;
                }
            }
            image.UnlockBits(bitmapData);

            return array2D;
        }

        public static Bitmap Array2DToBitmap(int[,] integers)
        {
            int width = integers.GetLength(0);
            int height = integers.GetLength(1);

            int stride = width * 4;

            Bitmap bitmap = null;

            unsafe
            {
                fixed (int* intPtr = &integers[0, 0])
                {
                    bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppRgb, new IntPtr(intPtr));
                }
            }

            return bitmap;
        }

    }

    public class Program
    {
        static public void Main()
        {
            MainCalculations main = new MainCalculations();
            main.ExecuteOpenFileDialog();
            main.ExecuteEncrypt();
        }
    }

}