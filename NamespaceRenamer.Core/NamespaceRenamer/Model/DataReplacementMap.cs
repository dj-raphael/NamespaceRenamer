using System.Data.Entity.ModelConfiguration;

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
