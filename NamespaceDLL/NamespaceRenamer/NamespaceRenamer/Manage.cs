using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NamespaceRenamer.Model;

namespace NamespaceRenamer
{
    public class Manage
    {
        
        public Renamer rename = new Renamer();

        public void Start(string configPath)
        {
            rename.ConfigList.LoadBase(configPath);
            rename = new Renamer(new ReplaceContext(rename.ConfigList.DbPath));
            configPath = rename.ConfigList.Load(configPath);
            
            foreach (var item in rename.ConfigList.projectsList)
            {
                rename.Process(item);
            }
        }

    }
}
