using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Stegnote.Models;
using Color = System.Drawing.Color;

namespace Stegnote
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
            Coordinates firstCoordinates = getFirstRandomCoordinates(downloadedImage);
            string textBlock = Message.ToLower();
            int offsetCount = CalcuteOffset(firstCoordinates);
            OutputInfo.offset = offsetCount;
            var symbolsAndCoordinates = GetAllColors(textBlock, firstCoordinates, downloadedImage);
            CreateOutputInfo(symbolsAndCoordinates);
            Console.WriteLine(); //TODO continue 
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

        private Dictionary<char, List<Coordinates>> GetAllColors(string text, Coordinates firstCoordinates, ImageInfo imageInfo)
        {
            Dictionary<char, List<Coordinates>> keyValues = new Dictionary<char, List<Coordinates>>();

            Coordinates previousCoordinates = firstCoordinates;
            for (int i = 0; i< text.Length; i++)
            {
                char symbol = text[i];
                previousCoordinates = GetNextCoordinates(previousCoordinates, imageInfo);
                if (keyValues.ContainsKey(symbol))
                {
                    keyValues[symbol].Add(previousCoordinates);
                }
                else
                {
                    keyValues.Add(symbol, new List<Coordinates>() { previousCoordinates });
                }
            }
            return keyValues;
        }

        private void CreateOutputInfo(Dictionary<char, List<Coordinates>> symbolsAndColors)
        {

            Dictionary<char, List<string>> symbolsAndHashes = new Dictionary<char, List<string>>();

            foreach(var symbolWithColors in symbolsAndColors)
            {
                symbolsAndHashes.Add(symbolWithColors.Key, new List<string>());
                foreach(var colorCoord in symbolWithColors.Value)
                {
                    string hash = GenerateUniqueValue256(colorCoord);
                    symbolsAndHashes[symbolWithColors.Key].Add(hash);
                }
            }

            OutputInfo.symbolsAndHashes = symbolsAndHashes;
        }
        

        private string GenerateUniqueValue256(Coordinates coordinates)
        {
            string input = coordinates.X.ToString() + coordinates.Color.Name + coordinates.Y.ToString();

            SHA256 shaHash = SHA256.Create();
            byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        private bool checkIfColorExist()
        {
            return true; //TODO add to function that needed it
        }


        private Coordinates GetNextCoordinates(Coordinates previousCoordinates, ImageInfo imageInfo)
        {
            int maxX = imageInfo.Width;
            int maxY = imageInfo.Height;

            string joinedInfo =
                Convert.ToString(previousCoordinates.X, 2) +
                Convert.ToString(previousCoordinates.Y, 2) +
                Convert.ToString(Convert.ToInt64(previousCoordinates.Color.Name, 16), 2);
            int length = joinedInfo.Length;
            string twoBytesX = string.Empty;
            string twoBytesY = string.Empty;

            int capacity = 16;

            if (length >= capacity*2)
            {
                twoBytesX = joinedInfo.Substring(0, capacity);
                twoBytesY = joinedInfo.Substring(length - capacity, capacity);
            }
            else
            {
                twoBytesX = joinedInfo.Substring(0, length / 2);
                twoBytesY = joinedInfo.Substring(length / 2, length - length / 2);
            }

            int l1 = Convert.ToInt32(twoBytesX.ToString(), 2);
            int l2 = Convert.ToInt32(twoBytesY.ToString(), 2);

            int X = Convert.ToInt32(l1 | l2);
            int Y = Convert.ToInt32(l1 ^ l2);

            while (X > maxX)
            {
                X -= maxX;
            }

            while (Y > maxY)
            {
                Y -= maxY;
            }

            return new Coordinates(X, Y, imageInfo.Bitmap.GetPixel(X, Y));

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