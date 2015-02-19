using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using NamespaceRenamer.Model;

namespace NamespaceRenamer
{
    public class Manage
    {
        public Renamer rename = new Renamer();

        public delegate void MethodContainer(Conflict e);
        public event MethodContainer OnAdd2 = delegate { };
        public bool IsSwitchedScroll;

        public async Task Start(string configPath)
        {
            rename.ConfigList.LoadBase(configPath);
            rename = new Renamer(new ReplaceContext(rename.ConfigList.DbPath));

            rename.OnAdd += AddConflict;
            configPath = rename.ConfigList.Load(configPath);

            foreach (var item in rename.ConfigList.projectsList)
            {
                await rename.Process(item);
            }
        }

        public void AddConflict(Conflict conflict)
        {
            OnAdd2(conflict);

        }

    }
}