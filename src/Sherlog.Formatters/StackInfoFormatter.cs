using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sherlog.Formatters
{
    public class StackInfoFormatter
    {
        readonly Func<string> _timeDelegate;
        readonly string _timeFormat;
        readonly string _format;
        public StackInfoFormatter(string format = null)
        {
            if (format == null)
                // do not care about {0}, stack info contains that message
                format = $"{{0}}{Environment.NewLine}=> {{1}}";
            _format = format;
        }

        public string FormatMessage(Logger logger, LogLevel logLevel, string message)
        {
            StackTrace stackTrace = new(true);
            // filter LLog and Sherlog from stackTrace
            string filteredStackTrace = string.Join(Environment.NewLine,
                stackTrace.GetFrames()
                .Where(frame =>
                !frame.GetMethod().DeclaringType.FullName.ToUpper().Contains("LLOG", StringComparison.OrdinalIgnoreCase) &&
                !frame.GetMethod().DeclaringType.FullName.ToUpper().Contains("SHERLOG", StringComparison.OrdinalIgnoreCase))
                .Select(frame => frame.ToString()));

            var result = $"{string.Format(_format, message, filteredStackTrace + Environment.NewLine)} ";
            return result;
        }
    }
}
