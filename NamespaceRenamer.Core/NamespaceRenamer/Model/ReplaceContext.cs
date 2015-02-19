﻿using System.Data.Entity;

namespace NamespaceRenamer.Model
{
    public class ReplaceContext: DbContext
    {
        //        public ReplaceContext()
        //            : base(AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin\\Debug\\")) + "App_Data\\data.sdf"){ }

        public ReplaceContext( ) { }
        public ReplaceContext(string connectionString) : base(connectionString)  { }
        
        public DbSet<DataReplacement> DataReplacements { get; set; }

    }
}
