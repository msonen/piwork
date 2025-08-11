using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MusicStreamingAnalyzer
{
    /// <summary>
    /// Represents a single play record from the music streaming portal
    /// </summary>
    public struct PlayRecord
    {
        public string PlayId { get; set; }
        public string SongId { get; set; }
        public string ClientId { get; set; }
        public DateTime PlayTimestamp { get; set; }
    }

    /// <summary>
    /// Represents the output result showing distribution of distinct song play counts per user
    /// </summary>
    public class DistributionResult
    {
        public int DistinctPlayCount { get; set; }
        public int ClientCount { get; set; }
    }

    /// <summary>
    /// Main analyzer class that processes music streaming data
    /// </summary>
    public class MusicStreamingAnalyzer
    {
        private const string DEFAULT_TARGET_DATE = "10/08/2016"; // August 10, 2016
        
        /// <summary>
        /// Main entry point of the application
        /// </summary>
        /// <param name="args">Command line arguments - expects input file path as first argument, optional target date as second argument</param>
        public static void Main(string[] args)
        {
            try
            {
                // Check if input file path is provided
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: MusicStreamingAnalyzer.exe <input_csv_file_path> [target_date]");
                    Console.WriteLine("Examples:");
                    Console.WriteLine("  MusicStreamingAnalyzer.exe data.csv");
                    Console.WriteLine("  MusicStreamingAnalyzer.exe data.csv 15/08/2016");
                    Console.WriteLine("  MusicStreamingAnalyzer.exe data.csv \"08/15/2016\"");
                    Console.WriteLine($"\nDefault target date: {DEFAULT_TARGET_DATE} (August 10, 2016)");
                    Console.WriteLine("\nSupported date formats: dd/MM/yyyy, MM/dd/yyyy, yyyy-MM-dd");
                    return;
                }

                string inputFilePath = args[0];
                string targetDateString = args.Length > 1 ? args[1] : DEFAULT_TARGET_DATE;
                
                // Validate input file exists
                if (!File.Exists(inputFilePath))
                {
                    Console.WriteLine($"Error: Input file '{inputFilePath}' does not exist.");
                    return;
                }

                // Validate and parse target date
                DateTime targetDate;
                if (!TryParseTargetDate(targetDateString, out targetDate))
                {
                    Console.WriteLine($"Error: Invalid target date format '{targetDateString}'.");
                    Console.WriteLine("Supported formats: dd/MM/yyyy, MM/dd/yyyy, yyyy-MM-dd");
                    Console.WriteLine("Examples: 10/08/2016, 08/10/2016, 2016-08-10");
                    return;
                }

                Console.WriteLine($"Target date: {targetDate:dd/MM/yyyy} ({targetDate:dddd, MMMM dd, yyyy})");

                // Create analyzer instance and process the file
                var analyzer = new MusicStreamingAnalyzer();
                var results = analyzer.AnalyzeDistinctSongPlays(inputFilePath, targetDate);
                
                // Output results to console
                analyzer.OutputResults(results, targetDate);
                
                // Also save results to a CSV file
                string outputFilePath = Path.ChangeExtension(inputFilePath, $"_results_{targetDate:yyyyMMdd}.csv");
                analyzer.SaveResultsToFile(results, outputFilePath, targetDate);
                Console.WriteLine($"\nResults also saved to: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to parse the target date string using common date formats
        /// </summary>
        /// <param name="dateString">Date string to parse</param>
        /// <param name="result">Parsed DateTime result</param>
        /// <returns>True if parsing successful, false otherwise</returns>
        private static bool TryParseTargetDate(string dateString, out DateTime result)
        {
            // Common date formats for target date input
            string[] targetDateFormats = {
                "dd/MM/yyyy",
                "MM/dd/yyyy", 
                "yyyy-MM-dd",
                "dd-MM-yyyy",
                "MM-dd-yyyy",
                "yyyy/MM/dd"
            };

            foreach (var format in targetDateFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return true;
                }
            }

            // Try general parsing as fallback
            return DateTime.TryParse(dateString, out result);
        }

        /// <summary>
        /// Analyzes the input CSV file and calculates distribution of distinct song plays per user
        /// for the specified target date
        /// </summary>
        /// <param name="filePath">Path to the input CSV file</param>
        /// <param name="targetDate">Target date to analyze (default: August 10, 2016)</param>
        /// <returns>List of DistributionResult objects</returns>
        public List<DistributionResult> AnalyzeDistinctSongPlays(string filePath, DateTime targetDate)
        {
            // Parse the input file and get all play records
            var playRecords = ParseCsvFile(filePath);
            
            // Filter records for the target date
            var filteredRecords = playRecords
                .Where(record => record.PlayTimestamp.Date == targetDate.Date)
                .ToList();

            Console.WriteLine($"Found {filteredRecords.Count} play records for {targetDate:dd/MM/yyyy}");

            // Group by client and count distinct songs played by each client
            var clientDistinctSongCounts = filteredRecords
                .GroupBy(record => record.ClientId)
                .Select(group => new
                {
                    ClientId = group.Key,
                    DistinctSongCount = group.Select(r => r.SongId).Distinct().Count()
                })
                .ToList();

            // Log individual client statistics for debugging
            Console.WriteLine("\nClient Statistics:");
            if (!clientDistinctSongCounts.Any())
            {
                Console.WriteLine($"No clients found with play records on {targetDate:dd/MM/yyyy}");
            }

            var distribution = clientDistinctSongCounts
                .GroupBy(client => client.DistinctSongCount)
                .Select(group => new DistributionResult
                {
                    DistinctPlayCount = group.Key,
                    ClientCount = group.Count()
                })
                .OrderBy(result => result.DistinctPlayCount)
                .ToList();

            return distribution;
        }

        /// <summary>
        /// Parses the CSV file and converts it into a list of PlayRecord objects
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>List of PlayRecord objects</returns>
        private List<PlayRecord> ParseCsvFile(string filePath)
        {
            var records = new List<PlayRecord>();
            var lines = File.ReadAllLines(filePath);

            // Skip header row (assuming first row contains column names)
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    records.Add(ParseCsvLine(line));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to parse line {i + 1}: {line}. Error: {ex.Message}");
                }
            }

            Console.WriteLine($"Successfully parsed {records.Count} records from {lines.Length - 1} lines");
            return records;
        }

        /// <summary>
        /// Parses a single CSV line into a PlayRecord object
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>PlayRecord object or null if parsing fails</returns>
        private static PlayRecord ParseCsvLine(string line)
        {
            // Handle both comma and tab delimited formats
            var parts = line.Contains('\t') ? 
                line.Split('\t') : 
                SplitCsvLine(line);

            if (parts.Length < 4)
            {
                throw new ArgumentException($"Invalid CSV format: expected 4 columns, found {parts.Length}");
            }

            var playId = parts[0].Trim();
            var songId = parts[1].Trim();
            var clientId = parts[2].Trim();
            var playTsString = parts[3].Trim();

            // Parse the timestamp - handle various date formats
            DateTime playTimestamp;
            if (!TryParseDateTime(playTsString, out playTimestamp))
            {
                throw new ArgumentException($"Invalid date format: {playTsString}");
            }

            return new PlayRecord
            {
                PlayId = playId,
                SongId = songId,
                ClientId = clientId,
                PlayTimestamp = playTimestamp
            };
        }

        /// <summary>
        /// Splits a CSV line handling quoted fields and commas within quotes
        /// </summary>
        /// <param name="line">CSV line to split</param>
        /// <returns>Array of field values</returns>
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            
            result.Add(current.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// Attempts to parse a datetime string using multiple common formats
        /// </summary>
        /// <param name="dateString">Date string to parse</param>
        /// <param name="result">Parsed DateTime result</param>
        /// <returns>True if parsing successful, false otherwise</returns>
        private static bool TryParseDateTime(string dateString, out DateTime result)
        {
            // Common date formats to try
            string[] dateFormats = {
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd"
            };

            foreach (var format in dateFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return true;
                }
            }

            // Try general parsing as fallback
            return DateTime.TryParse(dateString, out result);
        }

        /// <summary>
        /// Outputs the results to console in tabular format
        /// </summary>
        /// <param name="results">List of distribution results</param>
        /// <param name="targetDate">The target date that was analyzed</param>
        public void OutputResults(List<DistributionResult> results, DateTime targetDate)
        {
            Console.WriteLine($"\n=== RESULTS FOR {targetDate:dd/MM/yyyy} ({targetDate:dddd, MMMM dd, yyyy}) ===");
            
            if (results.Any())
            {
                Console.WriteLine("DISTINCT_PLAY_COUNT\tCLIENT_COUNT");
                Console.WriteLine("-----------------------------------");
                
                foreach (var result in results)
                {
                    Console.WriteLine($"{result.DistinctPlayCount}\t\t\t{result.ClientCount}");
                }
            }
            else
            {
                Console.WriteLine("No data found for the specified date.");
            }
        }

        /// <summary>
        /// Saves the results to a CSV file
        /// </summary>
        /// <param name="results">List of distribution results</param>
        /// <param name="outputPath">Path for the output CSV file</param>
        /// <param name="targetDate">The target date that was analyzed</param>
        public void SaveResultsToFile(List<DistributionResult> results, string outputPath, DateTime targetDate)
        {
            using (var writer = new StreamWriter(outputPath))
            {
                // Write header with target date information
                writer.WriteLine($"# Music Streaming Analysis Results for {targetDate:dd/MM/yyyy} ({targetDate:dddd, MMMM dd, yyyy})");
                writer.WriteLine($"# Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine("DISTINCT_PLAY_COUNT,CLIENT_COUNT");
                
                // Write data rows
                foreach (var result in results)
                {
                    writer.WriteLine($"{result.DistinctPlayCount},{result.ClientCount}");
                }

                // Add summary if no data found
                if (!results.Any())
                {
                    writer.WriteLine("# No data found for the specified date");
                }
            }
        }
    }
}