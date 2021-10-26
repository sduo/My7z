using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My7z.Providers
{
    public class API : IProvider
    {
        public Task<List<string>> BuildPasswordList(EmptyPasswordRule rule)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateOrUpdateHash(string hash, string password, string algorithm)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateOrUpdatePassword(string password)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordByHash(string hash, string algorithm)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetPasswordList()
        {
            throw new NotImplementedException();
        }
    }
}
