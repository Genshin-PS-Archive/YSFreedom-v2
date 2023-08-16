using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YSFreedom.Server.Database;
using Serilog;
using Newtonsoft.Json;

namespace YSFreedom.Server.Auth
{
    public class MHYAccMeta
    {
        [Key]
        public string GlobalID { get; set; }

        public string Value { get; set; }

        public void SetJSONValue(object obj)
        {
            Value = JsonConvert.SerializeObject(obj);
        }
        public T GetJSONValue<T>()
        {
            return JsonConvert.DeserializeObject<T>(Value);
        }
    }
}