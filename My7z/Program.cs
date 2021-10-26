using Microsoft.Extensions.Configuration;
using My7z.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace My7z
{
    class Program
    {
        const string DefaultZip = @"7z.exe";
        const int DefaultBlock = 4 * 1024 * 1024;
        const string NoneAlgorithm = @"none";
        const int ErrorCode = -1;

        static IProvider Provider;

        static IConfiguration Configuration;

        static async Task Main(string[] args)
        {
            Console.WriteLine();

            Configuration = new ConfigurationBuilder().AddJsonFile($"{nameof(My7z)}.json", true).AddCommandLine(args).Build();

            string input = Configuration.GetValue<string>(nameof(input));
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"请指定需要解压的文件");
                Environment.Exit(ErrorCode);
                return;
            }
            input = Path.GetFullPath(input);
            Console.WriteLine($"输入文件：{input}");
            if (!File.Exists(input))
            {
                Console.WriteLine($"文件不存在");
                Environment.Exit(ErrorCode);
                return;
            }

            string output = Configuration.GetValue<string>(nameof(output), null);
            if (output==null)
            {
                output = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input));
            }
            else if(string.Equals(string.Empty,output))
            {
                output = Path.GetDirectoryName(input);
            }

            Console.WriteLine($"输出文件夹：{output}");

            ProviderType provider = Configuration.GetValue(nameof(provider), ProviderType.SQLite);
            Console.WriteLine($"解密器：{provider}");

            switch (provider)
            {
                case ProviderType.MySQL: { Provider = new MySQL(Configuration.GetSection(nameof(MySQL))); break; }
                case ProviderType.SQLite: { Provider = new SQLite(Configuration.GetSection(nameof(SQLite))); break; }
                case ProviderType.MSSQL: { Provider = new MSSQL(Configuration.GetSection(nameof(MSSQL))); break; }
                case ProviderType.API:
                default:
                    {
                        Console.WriteLine($"不被支持的解密器：{provider}");
                        Environment.Exit(ErrorCode);
                        return;
                    }
            }           

            string algorithm = Configuration.GetValue<string>(nameof(algorithm),null);
            string hash = null;
            if (!string.IsNullOrEmpty(algorithm) && !string.Equals(NoneAlgorithm,algorithm,StringComparison.OrdinalIgnoreCase))
            {   
                int block = Configuration.GetValue(nameof(block), DefaultBlock);
                hash = Hash(input, algorithm, block);                
            }

            int code = 1;

            while (true)
            {
                string password = Configuration.GetValue<string>(nameof(password), null);
                if (password != null)
                {
                    Console.WriteLine($"指定密码：{password}");
                    code = TestPassword(input, password, output);
                    if (code == 0)
                    {
                        Console.WriteLine($"解压成功：更新密码 {await Provider.CreateOrUpdatePassword(password)} 项；更新哈希 {await Provider.CreateOrUpdateHash(hash, password, algorithm)} 项；");
                        break;
                    }
                }

                password = await Provider.GetPasswordByHash(hash, algorithm);
                if (!string.IsNullOrEmpty(password))
                {
                    Console.WriteLine($"使用哈希：{hash}({algorithm})");
                    code = TestPassword(input, password, output);
                    if (code == 0)
                    {
                        Console.WriteLine($"解压成功：更新密码 {await Provider.CreateOrUpdatePassword(password)} 项；更新哈希 {await Provider.CreateOrUpdateHash(hash, password, algorithm)} 项；");
                        break;
                    }
                }

                EmptyPasswordRule rule = Configuration.GetValue(nameof(rule), EmptyPasswordRule.First);
                Console.WriteLine($"空密码策略：{rule}");

                List<string> passwords = await Provider.BuildPasswordList(rule);
                for (int i = 0; i < passwords.Count; ++i)
                {
                    password = passwords[i];
                    Console.WriteLine($"尝试密码：{(password?.Length > 0 ? password : "空密码")}");
                    code = TestPassword(input, password, output);
                    if (code == 0) 
                    {
                        Console.WriteLine($"解压成功：更新密码 {await Provider.CreateOrUpdatePassword(password)} 项；添加哈希 {await Provider.CreateOrUpdateHash(hash, password, algorithm)} 项；");
                        break;
                    }
                }
                break;
            }

            if (code == 0)
            {
                bool remove = Configuration.GetValue(nameof(remove), false);
                if (remove)
                {
                    try
                    {
                        File.Delete(input);
                        Console.WriteLine($"移除文件成功");
                    }
                    catch
                    {
                        Console.WriteLine($"移除文件失败");
                    }
                }
            }

            Environment.Exit(code);
        }        

        static int TestPassword(string input, string password,string output)
        {
            string zip = Path.GetFullPath(Configuration.GetValue("7z", DefaultZip));
            if (!File.Exists(zip))
            {
                zip = DefaultZip;
            }            
            string arguments = $" x -y -p\"{password}\" -o\"{output}\" \"{Path.GetFullPath(input)}\"";
            Console.WriteLine();
            Console.WriteLine($"{zip}{arguments}");
            using Process process = new() { StartInfo = new ProcessStartInfo(zip, arguments) };
            process.Start();
            process.WaitForExit();
            int code = process.ExitCode;
            Console.WriteLine();
            return code;
        }        

        static string Hash(string fullname, string algorithm,int block)
        {
            using HashAlgorithm alg = HashAlgorithm.Create(algorithm);
            if (alg == null)
            {
                Console.WriteLine($"不被支持的哈希计算器：{algorithm}");
                return null;
            }
            Console.WriteLine($"哈希计算器：{algorithm}（分块：{block:N0}）");
            using FileStream fs = new(fullname, FileMode.Open, FileAccess.Read);            
            byte[] hash = new byte[block];
            while (true)
            {               
                int length = fs.Read(hash, 0, block);                
                Console.Write($"{'\r'}计算哈希值：{fs.Position:N0} / {fs.Length:N0}");
                if (length == 0)
                {
                    Console.WriteLine();
                    break;
                }
                alg.TransformBlock(hash, 0, length, null, 0);
            }
            alg.TransformFinalBlock(hash, 0, 0);
            string hex = Convert.ToHexString(alg.Hash);
            Console.WriteLine($"文件哈希值：{hex}");
            return hex;
        }        
    }
}
