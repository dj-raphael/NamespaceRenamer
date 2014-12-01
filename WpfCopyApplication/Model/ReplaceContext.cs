using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyApplication.Model
{
    public class ReplaceContext: DbContext
    {
        public ReplaceContext() : base("DefaultConnection") { }
        public DbSet<DataReplacement> DataReplacements { get; set; }

        protected override void OnModelCreating(DbModelBuilder dbModelBuilder)
        {
            dbModelBuilder.Configurations.Add(new DataReplacementMap());
        }
    }
}
