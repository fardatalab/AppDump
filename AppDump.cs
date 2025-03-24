namespace AppDump;

public class AppLogger {
    private StreamWriter writer;
    private object lockObj;
    private int bufferSize;

    public AppLogger(string filepath, int bufferSize = -1) {
        // Initialize the StreamWriter
        writer = new StreamWriter(filepath, append: true);
        lockObj = new object();

        // Set the buffer size in terms of bytes if specified (by default the buffer size is 4 KB)
        this.bufferSize = bufferSize;
        if (bufferSize == 0) {
            writer.AutoFlush = true;
        } else {
            writer.AutoFlush = false;
        }
    }

    ~AppLogger() {
        // Ensure thread safety
        lock (lockObj) {
            // Close the StreamWriter
            writer.Close();
            // Dispose the StreamWriter
            writer.Dispose();
        }

        // Flush the buffer to ensure that all data is written to file
        Flush();
    }

    public void Log(long timestamp, string eventType, List<string> args) {
        // Ensure thread safety
        lock (lockObj) {
            // Write the log entry to the file
            writer.WriteLine($"{timestamp},{eventType},{string.Join(",", args)}");

            if (bufferSize > 0 && writer.BaseStream.Length >= bufferSize) {
                // Flush the buffer if the buffer size exceeds the specified size
                writer.Flush();
            }
        }
    }

    public void Flush() {
        // Ensure thread safety
        lock (lockObj) {
            // Flush the buffer
            writer.Flush();
        }
    }
}
