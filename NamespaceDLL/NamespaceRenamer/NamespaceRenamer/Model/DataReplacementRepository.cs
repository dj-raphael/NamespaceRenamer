using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NamespaceRenamer.Model
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

        public bool IsDbEmpty()
        {
            return _context.DataReplacements.Any();
        }

        public async Task<DataReplacement> GetFileByTargetDirectory(string fileName)
        {
            return await _context.DataReplacements.FirstOrDefaultAsync(x => x.PathTargetDirectory == fileName);
        }

        public void RemoveByHash(string hash, string path)
        {
            _context.DataReplacements.RemoveRange(_context.DataReplacements.Where(x => x.Hash == hash && x.Path == path));
        }

        public bool ConsistRecords(string Path)
        {
            var consist = (bool)_context.DataReplacements.Any(x => x.PathTargetDirectory.Contains(Path));

            return consist;
        }

        public void AddDataReplace(FileInfo file, string targetPath, string sourceHash, FileInfo destFile, string destHash)
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

            if (_context.DataReplacements.FirstOrDefault(x => x.Path == file.FullName && x.PathTargetDirectory == targetPath) == null)
            {
                _context.DataReplacements.Add(insertFile);
                _context.SaveChangesAsync();
            }
        }

        public string TargetFileBySource(string sourceFileName)
        {
            var fTarget = _context.DataReplacements.First(x => x.Path == sourceFileName);

            return fTarget != null ? fTarget.PathTargetDirectory : null;
        }
    }
}
