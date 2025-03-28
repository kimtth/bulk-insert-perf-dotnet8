using System;

namespace FileLogger
{
    public class FilerLoggerMain
    {
        private readonly IFileLogger _logger;

        public FilerLoggerMain(IFileLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Refactored method to log at all levels
        public void LogAllLevels()
        {
            _logger.Debug("This is a trace message"); // Changed from LogTrace to Debug
            _logger.Debug("This is a debug message");
            _logger.Information("This is an information message");
            _logger.Warning("This is a warning message");
            _logger.Error("This is an error message");
            _logger.Error("This is a critical message"); // Changed from LogCritical to Error
        }

        public static void Main(string[] args)
        {
            var csv_log = new FilerLoggerMain(new CsvLogger("log.csv"));
            csv_log.LogAllLevels();

            var html_log = new FilerLoggerMain(new HtmlLogger("log.html"));
            html_log.LogAllLevels();
        }
    }
}
