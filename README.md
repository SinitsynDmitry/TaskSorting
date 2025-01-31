# Large File Sorting Utility

## Overview

This project provides two C# utilities:

1. **File Generator** - Creates a test file with randomly generated lines in the format `Number. String`.
2. **File Sorter** - Sorts a large text file based on the **String part first**, and if they match, by the **Number part**.

The solution is optimized for handling extremely large files (~100GB) efficiently.

## Input & Output Format

### Input

A text file where each line follows this format:

```
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
```

### Expected Output

Sorted file based on:

1. **String part (alphabetically)**
2. **Number part (numerically) when Strings match**

```
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
```

## Components

### 1. File Generator(FileCreator)

Generates a test file of a specified size with randomly generated lines containing:

- Random numbers
- Random string values (some repeated to test sorting stability)

### 2. File Sorter(RowsSorter)

- Efficiently sorts large files using **external merge sort** (handling large sizes efficiently without loading the entire file into memory).
- Uses a **two-step process**:
  1. **Splitting Phase** - Reads the large file in chunks, sorts them in memory, and writes sorted temporary files.
  2. **Merge Phase** - Merges the sorted chunks efficiently to produce the final sorted output.

## How to Run

### File Generator

```sh
FileCreator.exe <output_file> <rowsInFile>
```

Example:

```sh
FileCreator.exe testfile.txt 1000000000
```

(Generates a `testfile.txt` of approximately 20GB.)
rowsInFile = 1000000000 - 20Gb
rowsInFile = 100000000 - 2Gb
rowsInFile = 10000000 - 0.2Gb

### File Sorter

default value batchSize = 100000;

```sh
RowsSorter.exe <input_file> <output_file> [batch_size]
```

Example:

```sh
RowsSorter.exe C:\test\test2G.txt C:\test\out2\output.txt
```

## Performance Considerations

- **Uses streaming and chunk-based processing** to avoid memory overflows.
- **Optimized merge strategy** to minimize disk I/O.
- **Multi-threading where applicable** to speed up sorting.

## Requirements

- .NET 9

## Future Improvements

- Parallelized merging for even faster performance.
- Configurable chunk sizes for better memory tuning.


