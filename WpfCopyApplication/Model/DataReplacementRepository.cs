using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfCopyApplication.Model;

namespace WpfCopyApplication.Model
{
    public class DataReplacementRepository 
    {
        public  DataReplacement GetFileByPaths(string file, string destFile)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                return context.DataReplacements.FirstOrDefault(x => x.Path == file && x.PathTargetDirectory == destFile);
            }            
        }

        public bool IsExist(string fileName)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                var foundFile = context.DataReplacements.FirstOrDefaultAsync(x => x.PathTargetDirectory == fileName);
                return foundFile != null;
            }          
        }

        public bool IsDbEmpty()
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                return context.DataReplacements.Any();            
            }
        }

        public async Task<DataReplacement> GetFileByTargetDirectory(string fileName)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                return await context.DataReplacements.FirstOrDefaultAsync(x => x.PathTargetDirectory == fileName);
            }           
        }

        public void RemoveByHash(string hash)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                context.DataReplacements.Remove(context.DataReplacements.Find(hash));
            }
        }

        public bool ConsistRecords(string Path)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                var consist = (bool)context.DataReplacements.Any(x => x.PathTargetDirectory.Contains(Path));
                return consist;
            }           
        }

        public void AddDataReplace(FileInfo file, string targetPath, string sourceHash, FileInfo destFile, string destHash)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                var insertFile = new DataReplacement
                {
                    Date = file.LastWriteTime.Ticks,
                    Path = file.FullName,
                    Size = file.Length,
                    Hash = sourceHash,
                    PathTargetDirectory = targetPath,
                    DateTarget = destFile.LastWriteTime.Ticks,
                    SizeTarget = destFile.Length,
                    HashTarget = destHash
                };

                if (context.DataReplacements.FirstOrDefault(x => x.Path == file.FullName && x.PathTargetDirectory == targetPath) == null)
                {
                    context.DataReplacements.Add(insertFile);
                    context.SaveChangesAsync();
                }
            }            
        }

        public void AddHistory(ReplaceRequest item)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                context.ReplaceRequests.Add(item);
                context.SaveChanges();
            }
        }

        public string TargetFileBySource(string sourceFileName)
        {
            using (ReplaceContext context = new ReplaceContext())
            {
                var fTarget = context.DataReplacements.First(x => x.Path == sourceFileName);
                return fTarget != null ? fTarget.PathTargetDirectory : null;
            }
            
        }
    }
}
