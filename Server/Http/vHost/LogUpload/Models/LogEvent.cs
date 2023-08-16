using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Http.vHost.LogUpload.Models
{
    public class LogEvent
    {
        public class Uploadcontent
        {
            public int error_code { get; set; }
            public string message { get; set; }
            public int user_id { get; set; }
            public int time { get; set; }
            public string stackTrace { get; set; }
            public int exceptionSerialNum { get; set; }
            public string frame { get; set; }
            public string deviceModel { get; set; }
            public string deviceName { get; set; }
            public string operatingSystem { get; set; }
            public string userName { get; set; }
            public string version { get; set; }
            public string guid { get; set; }
            public string errorCode { get; set; }
            public bool isRelease { get; set; }
            public string serverName { get; set; }
            public string projectNick { get; set; }
            public string userNick { get; set; }
            public string logType { get; set; }
            public string subErrorCode { get; set; }
            public string cpuInfo { get; set; }
            public string gpuInfo { get; set; }
            public string memoryInfo { get; set; }
            public string clientIp { get; set; }
            public string errorLevel { get; set; }
            public string errorCategory { get; set; }
            public string notifyUser { get; set; }
        }
        public int applicationId { get; set; }
        public string applicationName { get; set; }
        public string msgID { get; set; }
        public string eventTime { get; set; }
        public int eventId { get; set; }
        public string eventName { get; set; }
        public Uploadcontent uploadContent { get; set; }
    }
}