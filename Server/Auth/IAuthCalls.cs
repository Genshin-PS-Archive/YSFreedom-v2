using System;
using System.Threading.Tasks;

namespace YSFreedom.Server.Auth
{
    public interface IAuthCalls
    {
        public Task<MHYAccount> Login(string username, string password);
        public Task<MHYAccount> GetAccountByUID(int uid);
        public Task SetAccountMeta(int uid, string subkey, object value);
        public Task<T> GetAccountMeta<T>(int uid, string subkey);
        public bool ExistsMeta(int uid, string subkey);
        public void DeleteMeta(int uid, string suffix);
    }
}