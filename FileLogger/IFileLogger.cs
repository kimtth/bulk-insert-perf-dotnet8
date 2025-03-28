using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Formatting.Display;
using Serilog.Sinks.File;
using System;
using System.IO;
using System.Text;

namespace FileLogger
{
    /// <summary>
    /// Interface for logging operations
    /// </summary>
    public interface IFileLogger
    {
        void Information(string message);
        void Warning(string message);
        void Error(string message);
        void Error(Exception ex, string message);
        void Debug(string message);
    }

    /// <summary>
    /// Base Serilog logger implementation
    /// </summary>
    public abstract class SerilogLogger : IFileLogger
    {
        protected readonly Logger _logger;

        protected SerilogLogger(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Information(string message) => _logger.Information(message);
        public void Warning(string message) => _logger.Warning(message);
        public void Error(string message) => _logger.Error(message);
        public void Error(Exception ex, string message) => _logger.Error(ex, message);
        public void Debug(string message) => _logger.Debug(message);

        public void Dispose()
        {
            _logger.Dispose();
        }
    }

    /// <summary>
    /// Logger implementation that outputs to CSV format
    /// </summary>
    public class CsvLogger : SerilogLogger
    {
        public CsvLogger(string filePath, LogEventLevel minimumLevel = LogEventLevel.Information)
            : base(CreateCsvLogger(filePath, minimumLevel))
        {
        }

        private static Logger CreateCsvLogger(string filePath, LogEventLevel minimumLevel)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .WriteTo.File(
                    formatter: new MessageTemplateTextFormatter("{Timestamp:yyyy-MM-dd HH:mm:ss.fff},{Level},{Message:lj}{NewLine}{Exception}", null),
                    path: filePath,
                    shared: true)
                .CreateLogger();
        }
    }

    /// <summary>
    /// Logger implementation that outputs to HTML format
    /// </summary>
    public class HtmlLogger : SerilogLogger
    {
        public HtmlLogger(string filePath, LogEventLevel minimumLevel = LogEventLevel.Information)
            : base(CreateHtmlLogger(filePath, minimumLevel))
        {
        }

        private static Logger CreateHtmlLogger(string filePath, LogEventLevel minimumLevel)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .WriteTo.File(
                    formatter: new MessageTemplateTextFormatter(
                        "<tr><td>{Timestamp:yyyy-MM-dd HH:mm:ss.fff}</td><td>{Level}</td><td>{Message:lj}</td><td>{Exception}</td></tr>",
                        null),
                    path: filePath,
                    hooks: new FileLifecycleHooksImplementation(),
                    shared: false)
                .CreateLogger();
        }
    }

    public class FileLifecycleHooksImplementation : FileLifecycleHooks
    {
        public override Stream OnFileOpened(string path, Stream underlyingStream, Encoding encoding)
        {
            var writer = new StreamWriter(underlyingStream, encoding);
            writer.WriteLine("<html><head><style>table { border-collapse: collapse; width: 100%; } th, td { text-align: left; padding: 8px; } tr:nth-child(even) { background-color: #f2f2f2; } th { background-color: #4CAF50; color: white; }</style></head><body><table border='1'><tr><th>Timestamp</th><th>Level</th><th>Message</th><th>Exception</th></tr>");
            writer.Flush();
            return underlyingStream;
        }
    }

}
