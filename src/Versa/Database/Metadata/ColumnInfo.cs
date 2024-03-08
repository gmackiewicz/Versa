namespace Versa.Database.Metadata;

internal class ColumnInfo
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public string IsNullable { get; set; }
    public string Position { get; set; }
    public string CharacterMaxLength { get; set; }
}
