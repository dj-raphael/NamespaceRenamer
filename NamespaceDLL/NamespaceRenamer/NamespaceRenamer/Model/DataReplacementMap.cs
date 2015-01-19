using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamespaceRenamer.Model
{
    internal class DataReplacementMap: EntityTypeConfiguration<DataReplacement>
    {
        internal DataReplacementMap()
        {
            ToTable("Data replacement");
            HasKey(x => new{x.Path, x.PathTargetDirectory});
        }
    }
}
