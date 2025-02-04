using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RowsSorter.Interfaces;

internal interface IStreamLineReader
{
    bool ReadLine(Stream reader, Stream lineBuffer);
    Task<bool> ReadLineAsync(Stream readerStream, MemoryStream lineBuffer);
}
