# Music Streaming Data Analyzer

## Overview

This C# console application analyzes music streaming data to calculate the distribution of distinct song play counts per user for a specified target date. By default, it analyzes data for August 10, 2016, but users can specify any target date. The program processes CSV input files containing streaming logs and outputs a summary showing how many clients played a specific number of distinct songs.

## Problem Statement

Given a CSV file with music streaming logs containing:
- `PLAY_ID`: Unique 16-byte identifier (hexadecimal string)
- `SONG_ID`: Identifier of the played song
- `CLIENT_ID`: Identifier of the client
- `PLAY_TS`: Date/time when the song was played

The program calculates how many clients played each number of distinct songs on a specified target date (default: August 10, 2016).

### Example

If the data shows:
- Client 1 played 2 distinct songs
- Client 2 played 4 distinct songs  
- Client 3 played 2 distinct songs

The output would be:
```
DISTINCT_PLAY_COUNT    CLIENT_COUNT
2                      2
4                      1
```


## Project Structure

```
MusicStreamingAnalyzer/
├── MusicStreamingAnalyzer.cs    # Main application code
├── README.md                    # This documentation
└── data.csv                     # Sample input file (if available)
```

## Code Architecture

### Classes

1. **`PlayRecord`**: Data model representing a single streaming log entry
2. **`DistributionResult`**: Data model for the output results
3. **`MusicStreamingAnalyzer`**: Main class containing all processing logic

### Key Methods

- **`Main()`**: Entry point, handles command line arguments (file path and optional target date)
- **`AnalyzeDistinctSongPlays()`**: Core analysis logic that processes the data for the specified date
- **`TryParseTargetDate()`**: Parses user-specified target dates in various formats
- **`ParseCsvFile()`**: Reads and parses the input CSV file
- **`ParseCsvLine()`**: Parses individual CSV lines with proper handling of quoted fields
- **`TryParseDateTime()`**: Robust date parsing supporting multiple formats
- **`OutputResults()`**: Formats and displays results to console
- **`SaveResultsToFile()`**: Saves results to output CSV file

## How It Works

1. **Input Processing**: 
   - Reads the CSV file line by line
   - Parses each record into a `PlayRecord` object
   - Handles both comma and tab delimited formats
   - Supports various date/time formats

2. **Data Filtering**:
   - Filters records to include only plays from the specified target date (default: August 10, 2016)
   - Uses exact date matching (ignoring time component)

3. **Analysis**:
   - Groups filtered records by `CLIENT_ID`
   - Counts distinct `SONG_ID` values for each client
   - Creates distribution summary showing how many clients played each number of distinct songs

4. **Output**:
   - Displays results on console in tabular format with target date information
   - Saves results to a new CSV file with date-specific naming (e.g., "_results_20160810.csv")

## Usage

### Prerequisites
- .NET Framework 4.5 or higher, or .NET Core 2.0+
- Input CSV file with the required columns

### Compilation
```bash
# Using .NET CLI
dotnet build

# Or using Visual Studio
# Open the .cs file and build the project
```

### Running the Application
```bash
# Basic usage with default date (August 10, 2016)
MusicStreamingAnalyzer.exe input_file.csv

# Specify a custom target date
MusicStreamingAnalyzer.exe input_file.csv 15/08/2016

# Different date formats supported
MusicStreamingAnalyzer.exe streaming_data.csv 08/15/2016
MusicStreamingAnalyzer.exe streaming_data.csv 2016-08-15

# Examples
MusicStreamingAnalyzer.exe streaming_data.csv
MusicStreamingAnalyzer.exe streaming_data.csv 12/25/2016
```

### Command Line Arguments
1. **Required**: Input CSV file path
2. **Optional**: Target date in various formats:
   - `dd/MM/yyyy` (e.g., 10/08/2016)
   - `MM/dd/yyyy` (e.g., 08/10/2016) 
   - `yyyy-MM-dd` (e.g., 2016-08-10)
   - Other common formats

### Expected Output
```
Target date: 10/08/2016 (Wednesday, August 10, 2016)
Successfully parsed 150 records from 150 lines
Found 8 play records for 10/08/2016

Client Statistics:
Client 1: 2 distinct songs
Client 2: 4 distinct songs
Client 3: 2 distinct songs

=== RESULTS FOR 10/08/2016 (Wednesday, August 10, 2016) ===
DISTINCT_PLAY_COUNT     CLIENT_COUNT
-----------------------------------
2                       2
4                       1

Results also saved to: streaming_data_results_20160810.csv
```

### Usage Examples

```bash
# Analyze default date (August 10, 2016)
MusicStreamingAnalyzer.exe data.csv

# Analyze Christmas Day 2016
MusicStreamingAnalyzer.exe data.csv 25/12/2016

# Analyze New Year's Day 2017 (US date format)
MusicStreamingAnalyzer.exe data.csv 01/01/2017

# Analyze with ISO date format
MusicStreamingAnalyzer.exe data.csv 2016-12-31
```

## Input File Format

The input CSV file should have the following structure:

```csv
PLAY_ID,SONG_ID,CLIENT_ID,PLAY_TS
44BB190BC2493964E053CF0A000AB546,6164,1,09/08/2016 09:16:00
44BB190BC24A3964E053CF0A000AB546,544,3,10/08/2016 13:54:00
...
```

### Supported Date Formats
- `dd/MM/yyyy HH:mm:ss` (e.g., "10/08/2016 13:54:00")
- `dd/MM/yyyy HH:mm` (e.g., "10/08/2016 13:54")
- `dd/MM/yyyy` (e.g., "10/08/2016")
- `MM/dd/yyyy` formats
- `yyyy-MM-dd` formats
- General parsing as fallback

## Error Handling

The application includes comprehensive error handling for:
- Missing or invalid input files
- Invalid target date formats
- Malformed CSV data
- Invalid date formats in data
- Missing command line arguments
- File I/O errors

Errors are reported with descriptive messages and line numbers where applicable.

## Example Walkthrough

Given this sample data:
```csv
PLAY_ID,SONG_ID,CLIENT_ID,PLAY_TS
44BB190BC24A3964E053CF0A000AB546,544,3,10/08/2016 13:54:00
44BB190BC2563964E053CF0A000AB546,9857,1,10/08/2016 22:05:00
44BB190BC2583964E053CF0A000AB546,217,2,10/08/2016 13:20:00
44BB190BC2593964E053CF0A000AB546,3022,1,10/08/2016 17:06:00
44BB190BC25A3964E053CF0A000AB546,9857,1,10/08/2016 15:06:00
```

**Step 1**: Filter for target date (e.g., August 10, 2016 - all 5 records match)

**Step 2**: Group by CLIENT_ID and count distinct SONG_IDs:
- Client 1: Songs [9857, 3022] = 2 distinct songs
- Client 2: Songs [217] = 1 distinct song  
- Client 3: Songs [544] = 1 distinct song

**Step 3**: Create distribution:
- 1 distinct song: 2 clients (clients 2 and 3)
- 2 distinct songs: 1 client (client 1)

**Output**:
```
DISTINCT_PLAY_COUNT     CLIENT_COUNT
1                       2
2                       1
```