using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My7z.Providers
{
    public class MySQL : DatabaseProvider
    {
        protected override string Sql_GetPasswordByHash => "SELECT `password` FROM `hash` WHERE `hash`=@hash AND `algorithm`=@algorithm";

        protected override string Sql_GetPasswordList => "SELECT `password` FROM `password` ORDER BY `hit` DESC";        

        protected override string Sql_CreateOrUpdateHash => "INSERT `hash` (`algorithm`,`hash`,`password`,`hit`) VALUES (@algorithm,@hash,@password,@hit) ON DUPLICATE KEY UPDATE `password`=@password,`hit`=`hit`+1;";

        protected override string Sql_CreateOrUpdatePassword => "INSERT `password` (`password`,`hit`) VALUES (@password,@hit) ON DUPLICATE KEY UPDATE `hit`=`hit`+1;";

        protected string Sql_CreateHashTable => "CREATE TABLE IF NOT EXISTS `hash` (`algorithm` varchar(32) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,`hash` varchar(256) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,`password` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,`hit` bigint UNSIGNED NOT NULL DEFAULT 1,PRIMARY KEY(`algorithm`,`hash`)) ENGINE=InnoDB DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_unicode_ci;";
        protected string Sql_CreatePasswordTable => "CREATE TABLE IF NOT EXISTS `password` (`password` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL, `hit` bigint UNSIGNED NOT NULL DEFAULT 1,PRIMARY KEY(`password`)) ENGINE=InnoDB DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_unicode_ci;";

        public MySQL(IConfiguration configuration):base(configuration)
        {
            string db = Configuration.GetValue<string>(nameof(db));
            if (string.IsNullOrEmpty(db))
            {
                throw new ApplicationException(nameof(db));
            }
            CreateConnection = () => new MySqlConnection(db);
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
