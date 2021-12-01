using Common.Interfaces;
using Models;
using Microsoft.Data.SqlClient;
using Common;

namespace Persistence
{
    public class LogRepository : BaseRepository, ILogRepository
    {
        public LogRepository()
        {

        }

        /// <summary>
        /// Genarate Log
        /// </summary>
        /// <param name="log"></param>
        public void GenarateLog(LogDetail log)
        {
            var parameters = new SqlParameter[]
            {
                 new SqlParameter("@SessionID", log.SessionID),
                 new SqlParameter("@LogType", log.LogType),
                 new SqlParameter("@LogMessage", log.Message)
            };

            Exec(Constants.spGenerateAEExRateSyncLog, parameters);
        }
    }
}
