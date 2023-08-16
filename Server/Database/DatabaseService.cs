using System;
using System.Threading;
using System.Threading.Tasks;
using YSFreedom.Server.Database;
using YSFreedom.Server.Database.Models;
using YSFreedom.Common.Services;
using System.Collections.Generic;
using Serilog;
using System.IO;

namespace YSFreedom.Server.Database
{
    public class DatabaseService : Service
    {
        public class Config
        {
            public string ConnectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.db")}";
        }

        public Config config = new Config();

        internal YuanShenDbContext DB { get { return dbContext; } }
        internal YuanShenDbContext dbContext;

        public override Task Start()
        {
            if (task != null) return task;
            return task = Task.Run(Main);
        }

        public async void Main()
        {
            Thread.CurrentThread.Name = "DatabaseService";
            _State = ServiceState.RUNNING;
            Log.Information("Initializing...");
            Initialize();

            await dbContext.Database.EnsureCreatedAsync();

            // Test accounts
            new Auth.MHYAccount().CreateAccount("test@test.com", "PlayerTest", "**", "test");
            new Auth.MHYAccount().CreateAccount("Leonardomeitz@gmail.com", "Fujiwara", "DE", "test");
            new Auth.MHYAccount().CreateAccount("padlocks@gmail.com", "padlocks", "US", "test");

            Log.Information("Ready");
            while (_State == ServiceState.RUNNING)
                await Task.Delay(1000);

            dbContext.Dispose();
            _State = ServiceState.STOPPED;
        }

        public override void Configure(object newConfig)
        {
            config = ((IDictionary<string,object>)newConfig).ToObject<Config>();
        }

        private void Initialize()
        {
            dbContext = new YuanShenDbContext(config.ConnectionString);
        }

        public static IService Create() => new DatabaseService();
    }
}