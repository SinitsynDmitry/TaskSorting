﻿using RowsSorter;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string filePath;
        string outputPath;
        int batchSize = 100000; // default value
                                // filePath = @"C:\test\test200M.txt";
                                // filePath = @"C:\test\test2G.txt";
                                // outputPath = @"C:\test\out2\output.txt";
        if (args.Length >= 2)
        {
            // Command line mode
            filePath = args[0];
            outputPath = args[1];
            if (args.Length >= 3 && !int.TryParse(args[2], out batchSize))
            {
                Console.WriteLine("Invalid batch size provided. Using default: 100000");
                batchSize = 100000;
            }
        }
        else
        {
            // Interactive mode
            if (args.Length > 0)
            {
                Console.WriteLine("Insufficient arguments. Usage:");
                Console.WriteLine("RowsSorter.exe <input_file> <output_file> [batch_size]");
                Console.WriteLine("Switching to interactive mode...\n");
            }

            filePath = GetValidInputFile();
            outputPath = GetValidOutputPath();
            batchSize = GetValidBatchSize();
        }

        // Validate command line arguments
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: Input file '{filePath}' does not exist.");
            return;
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            double fileSizeGb = fileInfo.Length / (1024.0 * 1024 * 1024);

            Console.WriteLine($"Start");
            Console.WriteLine($"Input file: {fileInfo.FullName}");
            Console.WriteLine($"Output file: {outputPath}");
            Console.WriteLine($"Batch size: {batchSize}");
            Console.WriteLine($"File size: {fileSizeGb:F2} GB");

            var stopwatch = Stopwatch.StartNew();
            var sorter = new ExternalMergeSorter();
            sorter.SortLargeFile(filePath, outputPath, batchSize);

            stopwatch.Stop();
            Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return;
        }
    }

    private static string GetValidInputFile()
    {
        while (true)
        {
            Console.WriteLine("Please enter the input file path:");
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Path cannot be empty. Please try again.");
                continue;
            }

            if (!File.Exists(input))
            {
                Console.WriteLine("File does not exist. Please enter a valid file path.");
                continue;
            }

            try
            {
                using (FileStream fs = File.OpenRead(input))
                {
                    return input;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot access file: {ex.Message}");
            }
        }
    }

    private static string GetValidOutputPath()
    {
        while (true)
        {
            Console.WriteLine("Please enter the output file path:");
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Path cannot be empty. Please try again.");
                continue;
            }

            try
            {
                string? directory = Path.GetDirectoryName(input);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                return input;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid output path: {ex.Message}");
            }
        }
    }

    private static int GetValidBatchSize()
    {
        while (true)
        {
            Console.WriteLine("Please enter the batch size (press Enter for default: 100000):");
            string? input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                return 100000;

            if (int.TryParse(input, out int batchSize) && batchSize > 0)
                return batchSize;

            Console.WriteLine("Invalid input. Please enter a positive number.");
        }
    }
}