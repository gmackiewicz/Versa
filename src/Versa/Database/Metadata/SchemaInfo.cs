using System.Collections.Generic;

namespace Versa.Database.Metadata;

internal class SchemaInfo
{
    public string Schema { get; set; }
    public IEnumerable<TableInfo> Tables { get; set; }

    public SchemaInfo(string schema)
    {
        Schema = schema;
    }
}
