using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        public  DataReplacement GetFileByPaths(string file, string destFile)
        {
            return _context.DataReplacements.FirstOrDefault(x => x.Path == file && x.PathTargetDirectory == destFile); 
        }

        public bool IsExist(string fileName)
        {
            var foundFile = _context.DataReplacements.FirstOrDefaultAsync(x => x.PathTargetDirectory == fileName);
            return foundFile != null;
        }

        public async Task<DataReplacement> GetFileByTargetDirectory(string fileName)
        {
            return await _context.DataReplacements.FirstOrDefaultAsync(x => x.PathTargetDirectory == fileName);
        }

        public void RemoveByHash(string hash)
        {
            _context.DataReplacements.Remove(_context.DataReplacements.Find(hash));
        }

        public bool ConsistRecords(string Path)
        {
            var consist = (bool) _context.DataReplacements.Any(x => x.PathTargetDirectory.Contains(Path));

            return consist;
        }

        public void AddDataReplace(FileInfo file, string targetPath, string hash)
        {
            var insertFile = new DataReplacement
            {
                Date = file.LastWriteTime.Ticks,
                Path = file.FullName,
                Size = file.Length,
                Hash = hash,
                PathTargetDirectory = targetPath
            };

            if (_context.DataReplacements.FirstOrDefault(x => x.Path == file.FullName && x.PathTargetDirectory == targetPath) == null)
            {
                _context.DataReplacements.Add(insertFile);
                _context.SaveChangesAsync();
            }
        }
    }
}
