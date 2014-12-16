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
        public ReplaceContext() : base(AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin\\Debug\\")) + "App_Data\\data.sdf") { }
        public DbSet<DataReplacement> DataReplacements { get; set; }
        public DbSet<ReplaceRequest> ReplaceRequests { get; set; }

        protected override void OnModelCreating(DbModelBuilder dbModelBuilder)
        {
            dbModelBuilder.Configurations.Add(new DataReplacementMap());
            dbModelBuilder.Configurations.Add(new ReplaceRequestMap());
        }
    }
}
