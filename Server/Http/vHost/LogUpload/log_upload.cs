using Newtonsoft.Json;
using Serilog;
using Server.Http.vHost.LogUpload.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using YSFreedom.Server.HttpApi.vHost;

namespace Server.Http.vHost.LogUpload
{
    internal class log_upload : BaseController
    {
        public log_upload()
        {
            _handlers.Add("/crash/dataUpload", DataUpload);
        }

        private async Task DataUpload(IHttpContext context, Func<Task> nextHandler)
        {
            List<LogEvent> dataUpload = JsonConvert.DeserializeObject<List<LogEvent>>(Encoding.UTF8.GetString(context.Request.Post.Raw));

            foreach (var item in dataUpload)
            {
                Log.Information(item.uploadContent.message);
                Log.Verbose(item.uploadContent.stackTrace);
            }

            context.Response = GetBaseResponse(context.Request, "");
            return;
        }
    }
}
