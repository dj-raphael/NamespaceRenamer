using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyApplication
{
    public class ReplaceNamespace
    {
        public ReplaceNamespace()
        {
            
        }

         WpfCopyApplication.PageAppearanceSection messageFromConfiguration = WpfCopyApplication.PageAppearanceSection.GetConfiguration();

        public void CopyFile(string sourceDir, string backupDir)
        {
            try
            {
                string[] txtList = Directory.GetFiles(sourceDir);

                // Copy text files.
                foreach (string f in txtList)
                {
                    
                    // Remove path from the file name.
                    string fName = f.Substring(sourceDir.Length + 1);

                    try
                    {
                        // Will not overwrite if the destination file already exists.
                        File.Copy(Path.Combine(sourceDir, fName), Path.Combine(backupDir, fName));
                    }

                    // Catch exception if the file was already copied.
                    catch (IOException copyError)
                    {
                        Console.WriteLine(copyError.Message);
                    }
                }

                // Delete source files that were copied.
//                foreach (string f in txtList)
//                {
//                    File.Delete(f);
//                }
            }

            catch (DirectoryNotFoundException dirNotFound)
            {
                Console.WriteLine(dirNotFound.Message);
            }
        }

        public void ReplacePartOfFile(string sourceDir, string oldNamespace, string newNamespace)
        {
            String strFile = File.ReadAllText(sourceDir);

            strFile = strFile.Replace(oldNamespace, newNamespace);

            File.WriteAllText(sourceDir, strFile);
        }
    }
}
