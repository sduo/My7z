using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My7z.Providers
{
    public class MSSQL : DatabaseProvider
    {
        protected override string Sql_GetPasswordByHash => throw new NotImplementedException();

        protected override string Sql_GetPasswordList => throw new NotImplementedException();

        protected override string Sql_CreateOrUpdateHash => throw new NotImplementedException();

        protected override string Sql_CreateOrUpdatePassword => throw new NotImplementedException();

        protected string Sql_CreateHashTable => throw new NotImplementedException();
        protected string Sql_CreatePasswordTable => throw new NotImplementedException();

        public MSSQL(IConfiguration configuration):base(configuration)
        {
            string db = Configuration.GetValue<string>(nameof(db));
            if (string.IsNullOrEmpty(db))
            {
                throw new ApplicationException(nameof(db));
            }
            CreateConnection = () => new SqlConnection(db);
            Initialize();
        }

        private void Initialize()
        {
            using IDbConnection db = CreateConnection();
            db.Execute(Sql_CreatePasswordTable);
            db.Execute(Sql_CreateHashTable);
        }
    }
}
