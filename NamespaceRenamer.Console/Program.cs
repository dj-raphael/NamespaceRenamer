using System;
using System.Linq;
using System.Threading.Tasks;
using NamespaceRenamer.Core;

namespace NamespaceRenamer.Console
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
               System.Console.WriteLine(pathConfig);
            }
            catch (Exception xmlException)
            {
                System.Console.WriteLine(xmlException);
            }

            if (ConfigList.projectsList.Count != 0)
            {
                Manage.OnAdd2 += WriteLog;

                System.Console.WriteLine();
                System.Console.WriteLine("=====================================");

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
                System.Console.WriteLine(!args.Any()
                    ? "Please write argument: path to config file."
                    : "Config file doesn't contain data of replacing projects. Please choose correct config file.");

                return -1;
            }

            System.Console.WriteLine("=====================================");
            
            System.Console.WriteLine("Renaming completed. " +  Manage.rename.ConflictList.Count(x => x.Merge == true) + " conflicts occured.");

            var count = 0;

            foreach (var conflict in  Manage.rename.ConflictList.Where(x => x.Merge == true))
            {
                System.Console.WriteLine(count + " " + conflict.Message);
                count++;
            }
            
            var q = System.Console.ReadLine();

            return 0;
       }


        private static void WriteLog(Conflict e)
        {
           System.Console.WriteLine(e.Message);           
        }
    }
}
