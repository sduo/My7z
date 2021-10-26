using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My7z.Providers
{
    public class SQLite : DatabaseProvider
    {
        protected override string Sql_GetPasswordByHash => "SELECT \"password\" FROM \"hash\" WHERE \"hash\"=@hash AND \"algorithm\"=@algorithm;";
        protected override string Sql_GetPasswordList => "SELECT \"password\" FROM \"password\" ORDER BY \"hit\" DESC;";
        protected override string Sql_CreateOrUpdateHash => "INSERT INTO \"hash\" (\"algorithm\",\"hash\",\"password\",\"hit\") VALUES (@algorithm,@hash,@password,@hit) ON CONFLICT(\"algorithm\",\"hash\") DO UPDATE SET \"password\"=@password, \"hit\" = \"hit\" + 1;";
        protected override string Sql_CreateOrUpdatePassword => "INSERT INTO \"password\" (\"password\", \"hit\") VALUES(@password, @hit) ON CONFLICT(\"password\") DO UPDATE SET \"hit\" = \"hit\" + 1;";

        protected string Sql_CreateHashTable => "CREATE TABLE IF NOT EXISTS \"hash\" (\"algorithm\" TEXT NOT NULL,\"hash\" TEXT NOT NULL,\"password\" TEXT,\"hit\" INTEGER DEFAULT 1, PRIMARY KEY(\"algorithm\",\"hash\"));";
        protected string Sql_CreatePasswordTable => "CREATE TABLE IF NOT EXISTS \"password\" (\"password\" TEXT NOT NULL,\"hit\" INTEGER DEFAULT 1, PRIMARY KEY(\"password\"));";

        public SQLite(IConfiguration configuration):base(configuration)
        {
            string db = Configuration.GetValue(nameof(db), $"{nameof(My7z)}.sqlite3");
            if (!Path.IsPathFullyQualified(db))
            {
                db = Path.Combine(AppContext.BaseDirectory, db);
            }
            if (string.IsNullOrEmpty(db))
            {
                throw new ApplicationException(nameof(db));
            }
            CreateConnection = () => new SqliteConnection($"Data Source={db};");
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
