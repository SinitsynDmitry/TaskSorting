using RowsSorter;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string filePath = string.Empty;
        string outputPath = string.Empty;
        long batchSize = 0;
        //filePath = @"C:\test\test2G.txt";
        //filePath = @"C:\test\test20G.txt";
        //filePath = @"C:\test\test200M.txt";

        //outputPath = @"C:\test\out2\output.txt";
        //batchSize = 100000;

        while (string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("Please enter the full path to the input file:");
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
                // Test if we have read access to the file
                using (FileStream fs = File.OpenRead(input))
                {
                    filePath = input;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot access file: {ex.Message}");
                continue;
            }
        }

        var fileInfo = new FileInfo(filePath);
        double fileSizeGb = fileInfo.Length / (1024.0 * 1024 * 1024);

        Console.WriteLine($"Start");
        Console.WriteLine($"Input file: {fileInfo.FullName}");
        Console.WriteLine($"File size: {fileSizeGb:F2} GB");


        while (string.IsNullOrEmpty(outputPath))
        {
            Console.WriteLine("Please enter the full path for the output file:");
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Path cannot be empty. Please try again.");
                continue;
            }

            try
            {
                // Test if we can write to the directory
                string? directory = Path.GetDirectoryName(input);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                outputPath = input;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid output path: {ex.Message}");
                continue;
            }
        }


        while (batchSize <= 0)
        {
            Console.WriteLine("Please enter the batch size (positive number - 100000):");
            string? input = Console.ReadLine();
            if (!long.TryParse(input, out batchSize) || batchSize <= 0)
            {
                Console.WriteLine("Invalid input. Please enter a positive number.");
            }
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var sorter = new ExternalMergeSort();
            sorter.SortLargeFile(filePath, outputPath, (int)batchSize);
            //await sorter.SortLargeFileAsync(filePath, outputPath, (int)batchSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong: {ex.Message}");
            Console.ReadLine();
            return;
        }

        stopwatch.Stop();
        Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");
        Console.ReadLine();
    }
}