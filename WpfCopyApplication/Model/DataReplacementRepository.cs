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
        public bool NeedReplace(FileInfo file)
        {
            var FoundFile = _context.DataReplacements.FirstOrDefault(x=>x.Path == file.FullName);
            if(FoundFile!=null)
            return !Compare(file, FoundFile.Date, FoundFile.Size, FoundFile.Hash);
            return true;
        }

        public void AddDataReplace(FileInfo file, string targetPath)
        {
            var insertFile = new DataReplacement();
            insertFile.Date = file.LastWriteTime;
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

        static bool Compare(FileInfo comparedFile, DateTime date, long size, string hashMd5)
        {
            if (comparedFile.LastWriteTime == date && comparedFile.Length == size)
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
