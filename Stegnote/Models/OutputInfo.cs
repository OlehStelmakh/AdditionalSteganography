using System.Collections.Generic;

namespace Stegnote.Models
{
    public class OutputInfo
    {
        public Coordinates FirstCoordinates { get; set; }

        public int Offset { get; set; }

        public Dictionary<char, List<string>> SymbolsAndHashes { get; set; }

        public Dictionary<char, List<string>> NoiseSymbols { get; set; }

        public OutputInfo(Coordinates first, int offset, Dictionary<char, List<string>> symbolsAndHashes,
            Dictionary<char, List<string>> noiseSymbols)
        {
            FirstCoordinates = first;
            Offset = offset;
            SymbolsAndHashes = symbolsAndHashes;
            NoiseSymbols = noiseSymbols;
        }
    }
}
