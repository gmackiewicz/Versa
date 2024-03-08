using System.Collections.Generic;

namespace Versa.Database.Metadata;

internal class TableInfo
{
    public string Name { get; set; }
    public IEnumerable<ColumnInfo> Columns { get; set; }

    public TableInfo(string name)
    {
        Name = name;
    }
}
