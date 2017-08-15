using System;

namespace SJP.FsNotify
{
    /// <summary>
    /// An exception which should be thrown when an internal buffer has been exhausted.
    /// </summary>
    public sealed class BufferExhaustedException : Exception
    {
        /// <summary>
        /// Initialized a new instance of <see cref="BufferExhaustedException"/>, with information about the buffer exhaustion, and the capacity of the buffer when that occured.
        /// </summary>
        /// <param name="message">A description of the error.</param>
        /// <param name="capacity">The number of file system events to stored in the buffer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <b>null</b>, empty, or whitespace.</exception>
        public BufferExhaustedException(string message, int capacity) : base(message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            Capacity = capacity;
        }

        /// <summary>
        /// The number of file system events that were stored in the buffer before being exhausted.
        /// </summary>
        public int Capacity { get; }
    }
}
