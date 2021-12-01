using System;

namespace Models
{
    public class LogDetail
    {
        public int ID { get; set; }
        public Guid SessionID { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
