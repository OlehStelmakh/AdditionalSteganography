using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Stegnote.Models;
using Color = System.Drawing.Color;
using System.IO;

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

        private Pixel getFirstRandomCoordinates(ImageInfo image)
        {
            Random random = new Random();
            //int x = random.Next(image.Width);
            //int y = random.Next(image.Height);
            int x = 0;
            int y = 0;
            Color color = downloadedImage.Pixels[y, x];
            Pixel coordinates = new Pixel(x, y, color);
            return coordinates;
        }
        
        public void ExecuteEncrypt()
        {
            Pixel firstCoordinates = getFirstRandomCoordinates(downloadedImage);
            string textBlock = Message.ToLower();
            int offsetCount = CalcuteOffset(firstCoordinates);
            var symbolsAndCoordinates = GetAllColors(textBlock, firstCoordinates, downloadedImage);
            var symbolsAndHashes = CreateOutputHashesInfo(symbolsAndCoordinates);
            var noise = GenerateSymbolNoise(symbolsAndCoordinates);
            OutputInfo outputInfo = new OutputInfo(firstCoordinates, offsetCount,
                symbolsAndHashes, noise, textBlock.Length);
            string outputData = CreateOutputString(outputInfo);
            byte[] dataForSaving = RijndaelAlgorithm.Encrypt(outputData);
            SaveOutput(dataForSaving);
         

            ExecuteDecrypt();
        }

        public void ExecuteDecrypt()
        {
            string path = "/Users/olehstelmakh/Desktop/output.txt";
            byte[] inputData = File.ReadAllBytes(path);
            string decriptedData = RijndaelAlgorithm.Decrypt(inputData);

            Parser parseData = new Parser(decriptedData);
            Color firstColor = parseData.GetColorFromString();
            int offsetOfFirstColor = parseData.GetOffsetOfcolor();
            var symbolsAndHashes = parseData.GetInfoAboutAllSymbols();
            int lengthOfText = parseData.GetLengthOfText();
            ParsedData parsedData = new ParsedData(firstColor, offsetOfFirstColor,
                symbolsAndHashes, lengthOfText);

            Pixel firstValue = GetCoordinatesOfFirst(parsedData.FirstColor, parsedData.Offset,
                downloadedImage.Bitmap);


            Console.WriteLine();
        }

        private Pixel GetCoordinatesOfFirst(Color color, int offset, Bitmap image)
        {
            int count = 1;
            Pixel pixel = new Pixel();

            for (int i=0; i< image.Height; i++)
            {
                for (int j=0; j< image.Width; j++)
                {
                    if (color.A == downloadedImage.Pixels[i, j].A &&
                        color.R == downloadedImage.Pixels[i, j].R &&
                        color.G == downloadedImage.Pixels[i, j].G &&
                        color.B == downloadedImage.Pixels[i, j].B)
                    {
                        if (offset == count)
                        {
                            return new Pixel(j, i, color);
                        }
                        count++;
                    }
                }
            }

            return pixel;
        }

        private void DecrypteText(ParsedData parsedData)
        {

        }

        private void SaveOutput(byte[] data)
        {
            string path = "/Users/olehstelmakh/Desktop/output.txt";
            File.WriteAllBytes(path, data);
        }

        private string CreateOutputString(OutputInfo outputInfo)
        {
            StringBuilder stringBuilder = new StringBuilder(10000);
            var commonInfo = new Dictionary<char, List<string>>(outputInfo.NoiseSymbols);
            foreach (var instance in outputInfo.SymbolsAndHashes)
            {
                commonInfo.Add(instance.Key, instance.Value);
            }
            commonInfo = commonInfo.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

            stringBuilder.Append(outputInfo.FirstCoordinates.Color.Name);
            stringBuilder.Append(outputInfo.Offset.ToString() + "\n");
            foreach (var info in commonInfo)
            {
                stringBuilder.Append(info.Key + " ");
                for (int i = 0; i < info.Value.Count; i++)
                {
                    stringBuilder.Append(info.Value[i] + " ");
                }
                stringBuilder.Append("\n");
            }
            stringBuilder.Append(outputInfo.LengthOfText.ToString() + "\n");
            return stringBuilder.ToString();
        }

        private string _message = "Hello world Lorem sipao hdbae kakebc sdahrak";

        public string Message
        {
            get => _message;
            set => _message = value.ToString();
        }

        private Dictionary<char, List<Pixel>> GetAllColors(string text, Pixel firstCoordinates, ImageInfo imageInfo)
        {
            Dictionary<char, List<Pixel>> keyValues = new Dictionary<char, List<Pixel>>();

            Pixel previousCoordinates = firstCoordinates;
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
                    keyValues.Add(symbol, new List<Pixel>() { previousCoordinates });
                }
            }
            return keyValues;
        }

        private Dictionary<char, List<string>> CreateOutputHashesInfo(Dictionary<char, List<Pixel>> symbolsAndColors)
        {

            Dictionary<char, List<string>> symbolsAndHashes = new Dictionary<char, List<string>>();

            foreach(var symbolWithColors in symbolsAndColors)
            {
                symbolsAndHashes.Add(symbolWithColors.Key, new List<string>());
                foreach(var colorCoord in symbolWithColors.Value)
                {
                    string input = colorCoord.X.ToString() + colorCoord.Color.Name + colorCoord.Y.ToString();
                    string hash = GenerateUniqueValue256(input);
                    symbolsAndHashes[symbolWithColors.Key].Add(hash);
                }
            }

            return symbolsAndHashes;
        }

        private Dictionary<char, List<string>> GenerateSymbolNoise(Dictionary<char, List<Pixel>> symbolsAndColors) 
        {
            Dictionary<char, List<string>> noiseSymbolsAndHashes = new Dictionary<char, List<string>>();

            HashSet<char> existingSymbols = new HashSet<char>(symbolsAndColors.Select(x => x.Key));
            int maxAmountOfColors= symbolsAndColors.Max(x => x.Value.Count) + 2;
            HashSet<char> ASCIIchars = new HashSet<char>();
            for (int i = 33; i < 128; i++)
            {
                ASCIIchars.Add((char)i);
            }
            ASCIIchars.ExceptWith(existingSymbols);
            Random random = new Random();
            foreach (var symbol in ASCIIchars)
            {
                noiseSymbolsAndHashes.Add(symbol, new List<string>());
                int hashesAmount = random.Next(1, maxAmountOfColors);
                for (int i = 0; i < hashesAmount; i++)
                {
                    string input = Convert.ToString(i, 2) + new string(symbol, 8) + Convert.ToString(i, 2);
                    string hash = GenerateUniqueValue256(input);
                    noiseSymbolsAndHashes[symbol].Add(hash);
                }
            }

            return noiseSymbolsAndHashes;
        }

        private string GenerateUniqueValue256(string input)
        {
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


        private Pixel GetNextCoordinates(Pixel previousCoordinates, ImageInfo imageInfo)
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

            return new Pixel(X, Y, imageInfo.Bitmap.GetPixel(X, Y));

        }

        private int CalcuteOffset(Pixel firstCoordinates)
        {
            int offsetCount = 0;
            for (int i = 0; i <= firstCoordinates.Y; i++)
            {
                for (int j = 0; j <= firstCoordinates.X; j++)
                {
                    if (downloadedImage.Pixels[i, j].A == firstCoordinates.Color.A &&
                        downloadedImage.Pixels[i, j].R == firstCoordinates.Color.R &&
                        downloadedImage.Pixels[i, j].G == firstCoordinates.Color.G &&
                        downloadedImage.Pixels[i, j].B == firstCoordinates.Color.B)
                    {

                        offsetCount++;
                    }
                }
            }
            return offsetCount;
        }
        
        private Color[,] BitmapToArray2D(Bitmap image)
        {
            Color[,] array2D = new Color[image.Height, image.Width];
 
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

            unsafe
            {
                byte* address = (byte*)bitmapData.Scan0;

                int paddingOffset = bitmapData.Stride - (image.Width * 4);

                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
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