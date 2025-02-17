using System;
using System.Linq;

namespace StepAPIService
{
    public class ProgramOptions
    {
        public string InputFile { get; set; } = string.Empty;
        public string NamedPipe { get; set; } = string.Empty;

        public ProgramOptions(string inputFile)
        {
            InputFile = inputFile;
            NamedPipe = RandomString;
        }

        private static readonly Random random = new();
        const int stringLength = 10;
        const string stringSpace = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static string RandomString =>
            new string(Enumerable.Repeat(stringSpace, stringLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());

    }
}
