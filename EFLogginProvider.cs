using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace eftest
{
    public class EFLoggingProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //nada
        }

        private class MyLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                lock (Console.Out) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(formatter(state, exception));
                    Console.ResetColor();
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}
