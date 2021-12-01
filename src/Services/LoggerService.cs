using Common.Enums;
using Common.Interfaces;
using Models;
using Persistence;
using System;

namespace Services
{

    public interface ILoggerService
    {
        void Log(LogTypes logType, string message, Guid sessionId);
    } 
    public class LoggerService : ILoggerService
    {
        public LoggerService()
        {
        }

        /// <summary>
        /// Logs details
        /// <paramref name="logType"/>
        /// <paramref name="message"/>
        /// <paramref name="sessionId"/>
        /// </summary>
        public void Log(LogTypes logType, string message, Guid sessionId)
        {
            LogRepository logRepository = new LogRepository();
            ILogRepository _logRepository = logRepository;

            var log = new LogDetail()
            {
                SessionID = sessionId,
                LogType = logType.ToString(),
                Message = message
            };

            _logRepository.GenarateLog(log);
        }
    }
}
