using System;
using System.Collections.Generic;

namespace Versa.Database.Metadata;

internal class SchemaInfo
{
    public int Id { get; set; }
    public string Schema { get; set; }
    public DateTime VerifiedOn { get; set; }
    public IEnumerable<TableInfo> Tables { get; set; }

    public SchemaInfo()
    {
    }

    public SchemaInfo(string schema)
    {
        Schema = schema;
        VerifiedOn = DateTime.UtcNow;
    }
}
