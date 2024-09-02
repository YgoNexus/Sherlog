using System.IO;

namespace Sherlog.Appenders
{
    public class FileWriterAppender
    {
        readonly object _lock = new object();
        readonly string _filePath;

        public FileWriterAppender(string filePath) => _filePath = filePath;

        public void WriteLine(Logger logger, LogLevel logLevel, string message)
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath))
                {
                    File.Create(_filePath).Dispose();
                }
                using StreamWriter writer = new StreamWriter(_filePath, true);
                writer.WriteLine(message);
            }
        }

        public void ClearFile()
        {
            lock (_lock)
            {
                using StreamWriter writer = new StreamWriter(_filePath, false);
                writer.Write(string.Empty);
            }
        }
    }
}
