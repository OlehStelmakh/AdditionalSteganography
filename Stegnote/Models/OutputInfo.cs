using System.Collections.Generic;

namespace Stegnote.Models
{
    public class OutputInfo
    {
        public static Coordinates first { get; set; }

        public static int offset { get; set; }

        public static Dictionary<char, List<string>> symbolsAndHashes { get; set; }

        public OutputInfo()
        {
        }
    }
}
