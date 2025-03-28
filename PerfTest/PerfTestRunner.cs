namespace ReportLab.PerfTest
{
    public class PerfTestRunner
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Bulk Insert Performance Tests");
            Console.WriteLine("=============================");
            
            int recordCount = 10000; // Default value
            
            // Check if a record count was specified as a command line argument
            if (args.Length > 0 && int.TryParse(args[0], out int count))
            {
                recordCount = count;
            }
            
            // Create and run the tests
            var tests = new BulkInsertTests();
            await tests.RunWeatherForecastTestTableTests(recordCount);
            // await tests.RunWeatherForecastTests(recordCount);
            // await tests.RunAllTests(recordCount);
            
            Console.WriteLine("Performance tests completed.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
