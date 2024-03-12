﻿namespace Versa.Database.Metadata;

internal class ColumnInfo
{
    public int Id { get; set; }
    public int TableId { get; set; }

    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
    public int Position { get; set; }
    public int? CharacterMaxLength { get; set; }
}
