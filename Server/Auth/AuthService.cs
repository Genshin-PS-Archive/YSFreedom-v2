using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;
using YSFreedom.Common.Services;
using YSFreedom.Server.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace YSFreedom.Server.Auth
{
    public class AuthService : Service, IAuthCalls
    {
        public class Config
        {
            public DatabaseService DatabaseService;
        }

        public Config config = new Config();
        private YuanShenDbContext DB { get { return config.DatabaseService.DB; } }

        public override Task Start()
        {
            if (task != null) return task;
            return task = Task.Run(Main);
        }

        public override void Configure(object newConfig)
        {
            config = ((IDictionary<string,object>)newConfig).ToObject<Config>();
        }

        public async void Main()
        {
            Thread.CurrentThread.Name = "AuthService";
            _State = ServiceState.RUNNING;
            Log.Information("Initializing...");

            Log.Information("Ready");
            while (_State == ServiceState.RUNNING)
                await Task.Delay(1000);
        }

        public static IService Create() => new AuthService();

        // AuthService public API
        public async Task<MHYAccount> Login(string email, string password)
        {
            try
            {
                var account = await DB.Accounts.FirstAsync(x => x.Email == email);
                if (password != null && !account.VerifyPassword(password))
                    throw new InvalidCredentialException("Invalid password specififed.");

                return account;
            } catch (InvalidOperationException)
            {
                throw new KeyNotFoundException("No user could be located for the specified email address.");
            }
        }

        public async Task<MHYAccount> GetAccountByUID(int uid)
        {
            var account = await DB.Accounts.FindAsync(uid);
            if (account == null)
                throw new KeyNotFoundException("No user could be located for the specified UID.");

            return account;
        }

        public async Task SetAccountMeta(int uid, string suffix, object value)
        {
            var key = uid.ToString() + "_" + suffix;
            var accMeta = new MHYAccMeta()
            {
                GlobalID = key
            };

            if (DB.AccMeta.Find(key) != null)
            {
                DB.AccMeta.Remove(DB.AccMeta.Find(key));
            }
            // TODO: use transactions
            // Normally I'd use the *Async functions, but because we aren't using transactions yet
            // I want to avoid at least some possible race conditions, which means no yielding
            accMeta.SetJSONValue(value);
            DB.AccMeta.Add(accMeta);

            await DB.SaveChangesAsync();
            return;
        }

        public async Task<T> GetAccountMeta<T>(int uid, string suffix)
        {
            var key = uid.ToString() + "_" + suffix;
            var data = DB.AccMeta.First(x => x.GlobalID == key);
            if (data == null)
                throw new KeyNotFoundException("No metadata could be located for the specified UID and suffix pair.");

            return data.GetJSONValue<T>();
        }

        public void DeleteMeta(int uid, string suffix)
        {
            var key = uid.ToString() + "_" + suffix;
            if (DB.AccMeta.Find(key) != null)
            {
                DB.AccMeta.Remove(DB.AccMeta.Find(key));
            }
        }

        public bool ExistsMeta(int uid, string suffix)
        {
            var key = uid.ToString() + "_" + suffix;
            var data = DB.AccMeta.First(x => x.GlobalID == key);
            if (data == null) { 
                return false;
            } else
            {
                return true;
            }
        }
    }
}