using System;
using NLog;
using ILogger = PingDomIntegration.Logging.Interfaces.ILogger;

namespace PingDomIntegration.Logging.Services
{
    /// <summary>
    /// Log implementation uses nlog
    /// </summary>
    /// <example>
    /*   
    private static ILogger logger = LoggingService.GetLoggingService();
    public static void Main(string[] args)
    {
        logger.Info("Program startup");
        
        logger.Info("Program End");
    }
    */
    /// </example>
    public class LoggingService : Logger, Interfaces.ILogger
    {
        public static ILogger GetLoggingService(string loggerName = "")
        {
            if (string.IsNullOrEmpty(loggerName))
                loggerName =AppDomain.CurrentDomain.FriendlyName;

            var logger = (ILogger)LogManager.GetLogger(loggerName, typeof(LoggingService));
            
            return logger;
        }

        public new void Debug(Exception exception, string format, params object[] args)
        {
            if (!IsDebugEnabled) return;
            var logEvent = GetLogEvent(LogLevel.Debug, exception, format, args);
            Log(typeof(LoggingService), logEvent);
        }

        public new void Error(Exception exception, string format, params object[] args)
        {
            if (!IsErrorEnabled) return;
            var logEvent = GetLogEvent(LogLevel.Error, exception, format, args);
            Log(typeof(LoggingService), logEvent);
        }

        public new void Fatal(Exception exception, string format, params object[] args)
        {
            if (!IsFatalEnabled) return;
            var logEvent = GetLogEvent(LogLevel.Fatal, exception, format, args);
            Log(typeof(LoggingService), logEvent);
        }

        public new void Info(Exception exception, string format, params object[] args)
        {
            if (!IsInfoEnabled) return;
            var logEvent = GetLogEvent(LogLevel.Info, exception, format, args);
            Log(typeof(LoggingService), logEvent);
        }

        public new void Trace(Exception exception, string format, params object[] args)
        {
            if (!IsTraceEnabled) return;
            var logEvent = GetLogEvent(LogLevel.Trace, exception, format, args);
            Log(typeof(LoggingService), logEvent);
        }

        public new void Warn(Exception exception, string format, params object[] args)
        {
            if (!IsWarnEnabled) return;
            var logEvent = GetLogEvent(LogLevel.Warn, exception, format, args);
            Log(typeof(LoggingService), logEvent);
        }

        public void Debug(Exception exception)
        {
            Debug(exception, string.Empty);
        }

        public void Error(Exception exception)
        {
            Error(exception, string.Empty);
        }

        public void Fatal(Exception exception)
        {
            Fatal(exception, string.Empty);
        }

        public void Info(Exception exception)
        {
            Info(exception, string.Empty);
        }

        public void Trace(Exception exception)
        {
            Trace(exception, string.Empty);
        }

        public void Warn(Exception exception)
        {
            Warn(exception, string.Empty);
        }

        private LogEventInfo GetLogEvent(LogLevel level, Exception exception, string format, object[] args)
        {
            var assemblyProp = string.Empty;
            var classProp = string.Empty;
            var methodProp = string.Empty;
            var messageProp = string.Empty;
            var innerMessageProp = string.Empty;
            
            var logEvent = new LogEventInfo
                (level, Name, string.Format(format, args));

            if (exception != null)
            {
                assemblyProp = exception.Source;
                if (exception.TargetSite.DeclaringType != null) classProp = exception.TargetSite.DeclaringType.FullName;
                methodProp = exception.TargetSite.Name;
                messageProp = exception.Message;

                if (exception.InnerException != null)
                {
                    innerMessageProp = exception.InnerException.Message;
                }
            }

            logEvent.Properties["error-source"] = assemblyProp;
            logEvent.Properties["error-class"] = classProp;
            logEvent.Properties["error-method"] = methodProp;
            logEvent.Properties["error-message"] = messageProp;
            logEvent.Properties["inner-error-message"] = innerMessageProp;

            return logEvent;
        }
    }
}
