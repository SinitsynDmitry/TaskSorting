using Moq;
using RowsSorter.Common.Pipeline;

namespace RowsSorterTests.Common.Pipeline;

[TestFixture]
public class ProcessingPipelineTests
{
    private ProcessingPipeline<object> _pipeline;
    private Mock<IProcessingStep<object>> _step1;
    private Mock<IProcessingStep<object>> _step2;
    private object _context;

    [SetUp]
    public void Setup()
    {
        _pipeline = new ProcessingPipeline<object>();
        _step1 = new Mock<IProcessingStep<object>>();
        _step2 = new Mock<IProcessingStep<object>>();
        _context = new object();
    }

    [Test]
    public void AddStep_ShouldIncreaseStepCount()
    {
        _pipeline.AddStep(_step1.Object);
        _pipeline.AddStep(_step2.Object);

        Assert.That(() => _pipeline.Execute(_context), Throws.Nothing);
    }

    [Test]
    public void Execute_ShouldRunAllSteps()
    {
        _pipeline.AddStep(_step1.Object);
        _pipeline.AddStep(_step2.Object);

        _pipeline.Execute(_context);

        _step1.Verify(s => s.Process(_context), Times.Once);
        _step2.Verify(s => s.Process(_context), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldRunAllStepsAsync()
    {
        _pipeline.AddStep(_step1.Object);
        _pipeline.AddStep(_step2.Object);

        await _pipeline.ExecuteAsync(_context);

        _step1.Verify(s => s.ProcessAsync(_context), Times.Once);
        _step2.Verify(s => s.ProcessAsync(_context), Times.Once);
    }
}
