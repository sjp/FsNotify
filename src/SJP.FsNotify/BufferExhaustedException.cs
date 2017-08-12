using System;

namespace SJP.FsNotify
{
    public sealed class BufferExhaustedException : Exception
    {
        public BufferExhaustedException(string message, int capacity) : base(message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            Capacity = capacity;
        }

        public int Capacity { get; }
    }
}
