namespace Versa.Database.Metadata;

internal class DatabaseInfo
{
    public string Schema { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string Position { get; set; }
    public string IsNullable { get; set; }
    public string DataType { get; set; }
    public string CharacterMaxLength { get; set; }

    public static string Query() =>
        @"SELECT
            t.TABLE_SCHEMA as 'Schema',
            t.TABLE_NAME as 'TableName',
            c.COLUMN_NAME as 'ColumnName',
            c.DATA_TYPE as 'DataType',
            c.IS_NULLABLE as 'IsNullable',
            c.ORDINAL_POSITION as 'Position',
            c.CHARACTER_MAXIMUM_LENGTH as 'CharacterMaxLength'
        FROM INFORMATION_SCHEMA.TABLES t
        LEFT JOIN INFORMATION_SCHEMA.COLUMNS c on c.TABLE_NAME = t.TABLE_NAME and c.TABLE_SCHEMA = t.TABLE_SCHEMA";
}
