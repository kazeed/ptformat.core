using Microsoft.Extensions.DependencyInjection;
using Ptformat.Core.Parsers;
using Ptformat.Core.Readers;
using Ptformat.Core.Extensions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PtInfo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: PtsInfo <path_to_pts_file> <output_folder>");
                return;
            }

            var filePath = args[0];
            var outputFolder = args[1];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found at path {filePath}");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine($"Error: Output folder not found at path {outputFolder}");
                return;
            }

            try
            {
                // Read the file into a byte array
                var rawFileData = File.ReadAllBytes(filePath);

                // Setup the service provider (IoC)
                var serviceProvider = ConfigureServices();

                // Resolve the XorDecoderReader
                using var xorDecoder = new XorDecoderStream(rawFileData, serviceProvider.GetRequiredService<ILogger<XorDecoderStream>>());

                // Use XorDecoderReader to decode the provided file data
                var decodedData = xorDecoder.Decode();

                // Resolve the SessionParser and parse the session
                var sessionParser = serviceProvider.GetRequiredService<SessionParser>();
                var session = sessionParser.Parse(decodedData);

                // Serialize the session object to JSON
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var sessionJson = JsonSerializer.Serialize(session, jsonOptions);

                // Create the output file path with session name
                var outputFilePath = Path.Combine(outputFolder, $"{session.HeaderInfo.Name}.json");

                // Write the JSON to the specified output folder
                File.WriteAllText(outputFilePath, sessionJson);

                Console.WriteLine($"Session parsed and saved to {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            static IServiceProvider ConfigureServices()
            {
                var services = new ServiceCollection();

                // Register logging
                services.AddLogging(config =>
                {
                    config.AddConsole();
                    config.SetMinimumLevel(LogLevel.Information);
                });

                // Register parsers and readers
                services.AddParsers(); // Extension method to add all parsers into IoC
                services.AddSingleton<XorDecoderStream>(); // Add the XOR decoder

                // Build and return the service provider
                return services.BuildServiceProvider();
            }
        }
    }
}