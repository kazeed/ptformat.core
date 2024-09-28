using System.IO;
using System;

namespace Ptformat.Core.Librarya
{
    public class PtUnxor
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ptunxor <inputfile> <outputfile>");
                return;
            }

            string inputFileName = args[0];
            string outputFileName = args[1];

            try
            {
                using (var inputFileStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read))
                using (var outputFileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    ApplyXorDecryption(inputFileStream, outputFileStream);
                }

                Console.WriteLine($"Decrypted file written to: {outputFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ApplyXorDecryption(FileStream inputFileStream, FileStream outputFileStream)
        {
            const byte xorKey = 0xFF; // Assuming XOR mask/key is 0xFF, adapt if necessary

            byte[] buffer = new byte[4096]; // Read in 4KB chunks
            int bytesRead;

            while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Apply XOR to each byte
                for (int i = 0; i < bytesRead; i++)
                {
                    buffer[i] ^= xorKey;
                }

                // Write the decrypted buffer to the output stream
                outputFileStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}