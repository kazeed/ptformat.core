using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptformat.Core.Parsers
{
    public static class ParserUtils
    {
        private static readonly string[] InvalidNames = { ".grp", "Audio Files", "Fade Files" };
        private static readonly string[] ValidWavTypes = { "WAVE", "EVAW", "AIFF", "FFIA" };
        
        /// <summary>
        /// Determines if a WAV name or type is invalid based on the file version and expected formats.
        /// </summary>
        public static bool IsInvalidWavNameOrType(string wavName, string wavType) =>
            InvalidNames.Any(invalid => wavName.Contains(invalid)) ||
            (!string.IsNullOrEmpty(wavType) && !ValidWavTypes.Contains(wavType)) ||
            !(wavName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || wavName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase));
    }
}
