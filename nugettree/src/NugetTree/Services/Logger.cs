
namespace NugetTree.Services
{
    using log4net;
    using NuGet.Common;
    using System;
    using System.Threading.Tasks;

    public class Logger : ILogger
    {
        private ILog _ILog { get; set; }

        public Logger()
        {
            _ILog = LogManager.GetLogger(typeof(Logger));
        }
        public void Log(LogLevel level, string data)
        {
            Console.WriteLine(data);
            _ILog.Info(data);
        }

        public void Log(ILogMessage message)
        {
            Console.WriteLine(message);
            _ILog.Info(message.Message);
        }

        public Task LogAsync(LogLevel level, string data)
        {
            Console.WriteLine(data);
            _ILog.Info(data);
            return null;
        }

        public Task LogAsync(ILogMessage message)
        {
            Console.WriteLine(message);
            _ILog.Info(message.Message);
            return null;
        }

        public void LogDebug(string data)
        {
            Console.WriteLine(data);
            _ILog.Debug(data);

        }

        public void LogError(string data)
        {
            Console.WriteLine(data);
            _ILog.Error(data);
        }

        public void LogInformation(string data)
        {
            _ILog.Info(data);
        }

        public void LogInformationSummary(string data)
        {
            Console.WriteLine(data);
            _ILog.Info(data);
        }

        public void LogMinimal(string data)
        {
            Console.WriteLine(data);
            _ILog.Info(data);
        }

        public void LogVerbose(string data)
        {
            Console.WriteLine(data);
            _ILog.Debug(data);
        }

        public void LogWarning(string data)
        {
            Console.WriteLine(data);
            _ILog.Warn(data);
        }
    }
}
