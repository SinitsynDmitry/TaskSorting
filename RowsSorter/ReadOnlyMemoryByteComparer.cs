using static System.Runtime.InteropServices.JavaScript.JSType;
namespace RowsSorter
{
    internal class ReadOnlyMemoryByteComparer : IComparer<ReadOnlyMemory<byte>>
    {
        private readonly byte _separator;

        public ReadOnlyMemoryByteComparer(byte separator = (byte)'.')
        {
            _separator = separator;
        }

        public int Compare(ReadOnlyMemory<byte> array1, ReadOnlyMemory<byte> array2)
        {
            // Quick comparison of whole arrays first
            int fullCompare = array1.Span.SequenceCompareTo(array2.Span);
            if (fullCompare == 0)
            {
                return 0;
            }

            var span1 = array1.Span;
            var span2 = array2.Span;

            int array1SeparatorIndex = span1.IndexOf(_separator);
            int array2SeparatorIndex = span2.IndexOf(_separator);

            // Compare second segments
            ReadOnlySpan<byte> second1 = array1SeparatorIndex >= 0
                ? span1.Slice(array1SeparatorIndex + 1)
                : ReadOnlySpan<byte>.Empty;
            ReadOnlySpan<byte> second2 = array2SeparatorIndex >= 0
                ? span2.Slice(array2SeparatorIndex + 1)
                : ReadOnlySpan<byte>.Empty;

            int secondCompare = second1.SequenceCompareTo(second2);
            if (secondCompare != 0)
            {
                return secondCompare;
            }

            // If second parts are equal, use the full compare result
            return fullCompare;
        }
    }
}