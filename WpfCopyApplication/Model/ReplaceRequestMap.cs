using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyApplication.Model
{
    internal class ReplaceRequestMap : EntityTypeConfiguration<ReplaceRequest>
    {
        internal ReplaceRequestMap()
        {
            ToTable("Replace request");
            HasKey(x => new { x.Id});
        }
    }
}
