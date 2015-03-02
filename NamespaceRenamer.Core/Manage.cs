using System.Threading.Tasks;
using NamespaceRenamer.Core.Model;

namespace NamespaceRenamer.Core
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
//            foreach (var item in rename.ConfigList.projectsList)
//            {
//                var allConfigFilesList = rename.updateListOfFiles;
//                rename.updateListOfFiles.Clear();
//
//                await rename.FillingList(item.SourceDirectory, rename.ConfigList.needUpdateList);
//                var configFilesList = rename.updateListOfFiles;
//
//                rename.RenameRootNamespaceAndAssemblyNameInCsproj(item.SourceNamespace, item.TargetNamespace);
//
//                rename.updateListOfFiles = allConfigFilesList;
//                rename.updateListOfFiles.AddRange(configFilesList);
//            }

//            rename.RenameProjectReferencesInCsproj();


            foreach (var item in rename.ConfigList.projectsList)
            {
                await rename.FillingListOfSln(item.SourceDirectory, rename.ConfigList.needUpdateList, item);
            }
            
            foreach (var item in rename.ConfigList.projectsList)
            {
                await rename.Process(item);
            }

            await rename.SaveUpdateListOfCsproj();
            await rename.SaveUpdateListOfSln();

        }

        public void AddConflict(Conflict conflict)
        {
            OnAdd2(conflict);
        }

    }
}