using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NamespaceRenamer;

namespace ConsoleRenamer
{
    class Program
    {
        public static ConfigManager ConfigList = new ConfigManager();
        
        public static Manage Manage = new Manage();
        static int Main(string[] args)
        {
            string pathConfig = "";
            
             pathConfig = args.Any() ? args[0] : Environment.CurrentDirectory + "\\config.xml";

            try
            {
               ConfigList.Load(pathConfig);
               Console.WriteLine(pathConfig);
            }
            catch (Exception xmlException)
            {
                Console.WriteLine(xmlException);
            }

            if (ConfigList.projectsList.Count != 0)
            {
                Manage.OnAdd2 += WriteLog;

                Console.WriteLine();
                Console.WriteLine("=====================================");

                try
                {
                    Task.WaitAll(Manage.Start(pathConfig));
                }
                catch (Exception)
                {
                    return -1;
                }
                
                
                ConfigList.Save(pathConfig);
            }
            else
            {
                Console.WriteLine(!args.Any()
                    ? "Please write argument: path to config file."
                    : "Config file doesn't contain data of replacing projects. Please choose correct config file.");

                return -1;
            }

            Console.WriteLine("=====================================");
            
            Console.WriteLine("Renaming completed. " +  Manage.rename.ConflictList.Count(x => x.Merge == true) + " conflicts occured.");

            var count = 0;

            foreach (var conflict in  Manage.rename.ConflictList.Where(x => x.Merge == true))
            {
                Console.WriteLine(count + " " + conflict.Message);
                count++;
            }
            
            var q = Console.ReadLine();

            return 0;
       }


        private static void WriteLog(Conflict e)
        {
           Console.WriteLine(e.Message);           
        }
    }
}
