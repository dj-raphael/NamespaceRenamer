using System;
using System.Linq;
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
            
            if (args.Count() > 1) Console.WriteLine("Too many arguments was specified. Please write only 1 argument - path of a config file. ");
            else pathConfig = args.Any() ? args[0] : Environment.CurrentDirectory + "\\config.xml";

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

                Task.WaitAll(Manage.Start(pathConfig));
                
                var q = Console.ReadLine();
                ConfigList.Save(pathConfig);
            }
            else
            {
                Console.WriteLine(!args.Any()
                    ? "Please write argument: path to config file."
                    : "Config file doesn't contain data of replacing projects. Please choose correct config file.");
            }

            return 0;
        }


        private static void WriteLog(Conflict e)
        {
           Console.WriteLine(e.Message);           
        }
    }
}
