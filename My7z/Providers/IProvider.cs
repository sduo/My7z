using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My7z.Providers
{
    public enum ProviderType : byte
    {
        Unknow = 0,
        SQLite = 1,
        MySQL = 2,
        MSSQL = 3,
        API = 255
    }

    public enum EmptyPasswordRule : byte
    {
        None = 0,
        First = 1,
        Last = 2
    }

    public interface IProvider
    {
        Task<string> GetPasswordByHash(string hash, string algorithm);
        Task<List<string>> GetPasswordList();

        Task<List<string>> BuildPasswordList(EmptyPasswordRule rule);        

        Task<int> CreateOrUpdateHash(string hash, string password, string algorithm);

        Task<int> CreateOrUpdatePassword(string password);
    }

    
}
