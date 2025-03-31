// This is v2 of appdump, which is implemented as a producer-consumer pattern where threads add log entries to a thread-safe queue instead of writing directly.

// The cancellation task is a background task that runs continuously to process log entries from the queue and write them to the file. It uses CancellationToken to allow for graceful shutdown when the logger is disposed.

// The Flush() method forces all buffered data to be written to the underlying storage immediately. The Dispose() method implements the IDisposable pattern.

using System.Collections.Concurrent;

namespace AppDumpV2
{
    public class AppLogger : IDisposable {
        private readonly StreamWriter writer;
        private readonly ConcurrentQueue<string> logQueue;
        private readonly int bufferSize;
        private readonly Task processingTask;
        private readonly CancellationTokenSource cancellationToken;
        private bool disposed = false;

        public AppLogger(string filepath, int bufferSize = -1) {
            writer = new StreamWriter(filepath, append: true);
            logQueue = new ConcurrentQueue<string>();
            this.bufferSize = bufferSize;

            // TODO: Check the init value of buffersize
            if (bufferSize == 0) {
                writer.AutoFlush = true;
            } else {
                writer.AutoFlush = false;
            }

            cancellationToken = new CancellationTokenSource();

            // Start background task to process log entries
            processingTask = Task.Run(() => ProcessLogQueue(cancellationToken.Token), cancellationToken.Token);
        }

        ~AppLogger() {
            Dispose(false);
        }

        public void Log(long timestamp, string eventType, List<string> args) {
            if (disposed) throw new ObjectDisposedException(nameof(AppLogger));

            // Format the log entry
            string logEntry = $"{timestamp},{eventType},{string.Join(",", args)}";

            logQueue.Enqueue(logEntry);
        }

        public void Flush() {
            if (disposed) throw new ObjectDisposedException(nameof(AppLogger));

            // Process all queued items immediately
            ProcessQueuedItems();

            lock (writer) {
                writer.Flush();
            }

        }

        private void ProcessLogQueue(CancellationToken token) {
            while (!token.IsCancellationRequested || !logQueue.IsEmpty) {
                try {
                    ProcessQueuedItems();
                    Thread.Sleep(50);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error in log processing: {ex.Message}");
                }
            }
        }

        // Process queued items in a batch. It is called by the background task
        // and the Flush() method.

        private void ProcessQueuedItems() {
            int batchSize = 100;
            List<string> batch = new List<string>(batchSize);

            while (logQueue.TryDequeue(out string? logEntry)) {
                batch.Add(logEntry);

                if (batch.Count >= batchSize) {
                    WriteBatch(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0) {
                WriteBatch(batch);
            }
        }

        private void WriteBatch(List<string> batch) {
            lock (writer) {
                foreach (var logEntry in batch) {
                    writer.WriteLine(logEntry);
                }

                if (bufferSize > 0 && writer.BaseStream.Length >= bufferSize) {
                    writer.Flush();
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    // Signal the processing task to complete
                    cancellationToken.Cancel();

                    try {
                        // Wait for the processing task to complete
                        processingTask.Wait(TimeSpan.FromSeconds(2));
                    }
                    catch (AggregateException) {}

                    // Process remaining items
                    ProcessQueuedItems();

                    lock (writer) {
                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }
                    cancellationToken.Dispose();
                }
                disposed = true;
            }
        }
    }
}
