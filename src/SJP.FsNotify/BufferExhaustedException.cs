using System;

namespace SJP.FsNotify
{
    public sealed class BufferExhaustedException : Exception
    {
        public BufferExhaustedException(string message, int capacity) : base(message)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }
    }
}
