using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfCopyApplication.Model;

namespace WpfCopyApplication.Repository
{
    public class DataReplacementRepository 
    {
        private ReplaceContext _context;
        public DataReplacementRepository(ReplaceContext context)
        {
            _context = context;
        }
        public bool NeedReplace(FileInfo file, FileInfo destFile)
        {
            var FoundFile = _context.DataReplacements.FirstOrDefault(x=>x.Path == file.FullName && x.PathTargetDirectory == destFile.FullName);

            if(FoundFile!=null)
            return !Compare(file, FoundFile.Date, FoundFile.Size, FoundFile.Hash);
            return true;
        }

        public bool NeedDelete(FileInfo file)
        {

            var FoundFile = _context.DataReplacements.FirstOrDefault(x => x.PathTargetDirectory == file.FullName);

            if (FoundFile != null)
            {

                // Just delete file, because we don't have records about this file...
                Console.WriteLine("");
                return true;
            }

            if (FoundFile.DateTarget == file.LastWriteTime.Ticks && FoundFile.Size == file.Length)
            {
                if (FoundFile.Hash == ComputeMD5Checksum(file.FullName))
                {
                    _context.DataReplacements.Remove(_context.DataReplacements.Find(FoundFile.Hash));
                    return true;
                }
                Console.WriteLine("");
                return true;
                
            }

            return false;
        }

        public void AddDataReplace(FileInfo file, string targetPath)
        {
            var insertFile = new DataReplacement();
            insertFile.Date = file.LastWriteTime.Ticks;
            insertFile.Path = file.FullName;
            insertFile.Size = file.Length;
            insertFile.Hash = ComputeMD5Checksum(file.FullName);
            insertFile.PathTargetDirectory = targetPath;
            
            _context.DataReplacements.Add(insertFile);
            _context.SaveChanges();
        }

        private static string ComputeMD5Checksum(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
        }

        static bool Compare(FileInfo comparedFile, long date, long size, string hashMd5)
        {
            if (comparedFile.LastWriteTime.Ticks == date && comparedFile.Length == size)
            {
                return true;
            }
            else
            {
                if (comparedFile.Length == size && ComputeMD5Checksum(comparedFile.FullName) == hashMd5) return true;
            }

            return false;
        }


    }
}
