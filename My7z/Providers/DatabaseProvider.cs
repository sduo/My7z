using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My7z.Providers
{
    public abstract class DatabaseProvider : IProvider
    {
        protected Func<IDbConnection> CreateConnection { get; set; }

        protected abstract string Sql_GetPasswordByHash { get; }

        protected abstract string Sql_GetPasswordList { get; }        

        protected abstract string Sql_CreateOrUpdateHash { get; }

        protected abstract string Sql_CreateOrUpdatePassword { get; }

        protected IConfiguration Configuration { get; set; }

        public DatabaseProvider(IConfiguration configuration)
        {
            Configuration = configuration;            
        }

        public async Task<string> GetPasswordByHash(string hash, string algorithm)
        {            
            try
            {
                using IDbConnection connection = CreateConnection();
                string password = await connection.QueryFirstOrDefaultAsync<string>(Sql_GetPasswordByHash, new { hash, algorithm });
                return password;
            }
            catch
            {
                return null;
            }            
        }

        public async Task<List<string>> GetPasswordList()
        {            
            List<string> list = new();
            try
            {
                using IDbConnection connection = CreateConnection();
                list.AddRange(await connection.QueryAsync<string>(Sql_GetPasswordList));
            }
            catch
            {
                list.Clear();
            }            
            return list;
        }       

        public async Task<List<string>> BuildPasswordList(EmptyPasswordRule rule)
        {
            List<string> list = await GetPasswordList();
            switch (rule)
            {
                case EmptyPasswordRule.First: { list.Insert(0, string.Empty); break; }
                case EmptyPasswordRule.Last: { list.Add(string.Empty); break; }
            }
            return list;
        }       

        public async Task<int> CreateOrUpdateHash(string hash, string password, string algorithm)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(algorithm))
            {
                return 0;
            }
            try
            {
                using IDbConnection connection = CreateConnection();
                int affected = await connection.ExecuteAsync(Sql_CreateOrUpdateHash, new { hash, password, algorithm, hit = 1 });
                return affected;
            }
            catch
            {
                return 0;
            }            
        }

        public async Task<int> CreateOrUpdatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return 0;
            }
            try
            {
                using IDbConnection connection = CreateConnection();
                int affected = await connection.ExecuteAsync(Sql_CreateOrUpdatePassword, new { password, hit = 1 });
                return affected;
            }
            catch
            {
                return 0;
            }            
        }
    }
}
