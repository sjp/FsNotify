using System;

namespace SJP.FsNotify
{
    public class BufferExhaustedException : Exception
    {
        public BufferExhaustedException(string message, int capacity) : base(message)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }
    }
}
