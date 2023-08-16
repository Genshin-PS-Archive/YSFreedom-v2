using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YSFreedom.Server.Database;
using Serilog;

namespace YSFreedom.Server.Auth
{
    public class MHYAccount
    {
        [Key]
        public int UID { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string RealName { get; set; }
        public string CountryCode { get; set; }

        public string NickName { get; set; }

        private YuanShenDbContext DB = new YuanShenDbContext(new DatabaseService().config.ConnectionString);

        // TODO: relocate this to AuthService
        // Assuming `ConnectionString` is the default value is bad practice, since the user may change it
        // in a configuration file.
        internal async void CreateAccount(string email, string name, string countryCode, string password)
        {
            MHYAccount account = new MHYAccount();

            // generate UID
            try
            {
                int lastUID = await DB.Accounts.MaxAsync(u => u.UID);
                account.UID = lastUID + 1;
            }
            catch (InvalidOperationException)
            {
                account.UID = 100000000;
            }

            account.Name = account.RealName = account.NickName = name;
            account.Email = email;
            account.CountryCode = countryCode;
            account.Password = password;

            // check if account with uid or email exists, add account to DB
            bool emailPreviouslyRegistered = false;
            bool initCheck = DB.Accounts.Any(u => (u.UID == account.UID)) == false && DB.Accounts.Any(u => (u.Email == account.Email)) == false;
            if (initCheck)
                await DB.AddAsync(account);
            else if (DB.Accounts.Any(u => (u.UID == account.UID)) == true && DB.Accounts.Any(u => (u.Email == account.Email)) == false)
            {
                // UID taken, try again.
                for (int i = account.UID; DB.Accounts.Any(u => (u.UID == account.UID)) == false; i++)
                {
                    account.UID += 1;
                }
                await DB.AddAsync(account);
            }
            else
                emailPreviouslyRegistered = true;

            await DB.SaveChangesAsync();

            // double check everything is ok
            if (initCheck && (await DB.Accounts.FindAsync((int)account.UID)) != null)
                Log.Debug("New account created with UID {0}", account.UID);
            else if (emailPreviouslyRegistered)
                Log.Debug("Email is already registered to an account");
            else
                Log.Debug("Failed to create new account");
        }

        public bool VerifyPassword(string pass)
        {
            // TODO: hashing
            return Password == pass;
        }

        public void SetPassword(string pass)
        {
            Password = pass;
        }
    }
}