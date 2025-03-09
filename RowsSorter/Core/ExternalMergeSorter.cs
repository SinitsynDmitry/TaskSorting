using RowsSorter.Common.Pipeline;
using RowsSorter.MainPipelineSteps.ChunkMerge;
using RowsSorter.MainPipelineSteps.ChunkMerge.Mergers;
using RowsSorter.MainPipelineSteps.ChunkProcessing;
using RowsSorter.MainPipelineSteps.ChunkProcessing.Steps;
using RowsSorter.MainPipelineSteps.Context;
using RowsSorter.MainPipelineSteps.FileChunkMarking;
using RowsSorter.Shared;


namespace RowsSorter.Core;
public class ExternalMergeSorter : IExternalMergeSorter
{
    private readonly IPipeline<FileProcessingContext> _filePipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMergeSorter"/> class.
    /// </summary>
    public ExternalMergeSorter()
    {
        var byteLineComparer = new ByteLineComparer(46);
        var fileStreamProvider = new FileStreamProvider();

        _filePipeline = new ProcessingPipeline<FileProcessingContext>()
        .AddStep(new FileChunkMarkingStep(new FileChunkMarker(fileStreamProvider,(byte)'\n')))
        .AddStep(new ChunkProcessingStep(new ChunkReader(fileStreamProvider), new LineSorter(byteLineComparer), new ChunkWriter(fileStreamProvider)))
        .AddStep(new ChunkMergeStep(new ChunkFoldingMerger(new ChunkSimpleMerger(new ChunkDataComparer(byteLineComparer), fileStreamProvider))));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMergeSorter"/> class.
    /// </summary>
    /// <param name="fileSplitter">The file splitter.</param>
    /// <param name="chunkMerger">The chunk merger.</param>
    //public ExternalMergeSorter(IPipeline<FileProcessingContext, List<string>> filePipeline)
    //{

    //    ArgumentNullException.ThrowIfNull(filePipeline);

    //    _filePipeline = filePipeline;
    //}

    /// <summary>
    /// Sorts the large file.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="baseChunkSize">The base chunk size.</param>
    public void SortLargeFile(string inputFile, string outputFile, int baseChunkSize)
    {
        var tempOutputDir = Path.Combine(Path.GetDirectoryName(outputFile)!, "temp");
        if (!Directory.Exists(tempOutputDir))
        {
            Directory.CreateDirectory(tempOutputDir);
        }
        var context = new FileProcessingContext(inputFile, outputFile, baseChunkSize, tempOutputDir);

       _filePipeline.Execute(context);
    }

    /// <summary>
    /// Sorts the large file async.
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <param name="outputFile">The output file.</param>
    /// <param name="baseChunkSize">The base chunk size.</param>
    /// <returns>A ValueTask.</returns>
    public async ValueTask SortLargeFileAsync(string inputFile, string outputFile, int baseChunkSize)
    {
        var tempOutputDir = Path.Combine(Path.GetDirectoryName(outputFile)!, "temp");
        if (!Directory.Exists(tempOutputDir))
        {
            Directory.CreateDirectory(tempOutputDir);
        }
        var context = new FileProcessingContext(inputFile, outputFile, baseChunkSize, tempOutputDir);
        await _filePipeline.ExecuteAsync(context);
    }
}
