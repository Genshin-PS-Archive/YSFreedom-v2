using YSFreedom.Common.HttpApi.vHost.TakumiApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp;
using YSFreedom.Common.Protocol;

namespace YSFreedom.Server.HttpApi.vHost.TakumiApi
{
    internal class api_os_takumi : BaseController
    {
        public api_os_takumi()
        {
            _handlers.Add("/account/risky/api/check", Check);
        }

        private async Task Check(IHttpContext context, Func<Task> nextHandler)
        {
            CheckModel model = new CheckModel
            {
                data = new CheckModel.Data
                {
                    id = "06611ed14c3131a676b19c0d34c0644b",
                    action = "ACTION_NONE",
                    geetest = null
                },

                message = "OK",
                retcode = (Int32)Retcode.RetSucc
            };

            context.Response = GetJsonResponse(context.Request, model);
            return;
        }
    }
}
