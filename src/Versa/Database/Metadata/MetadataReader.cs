using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;

namespace Versa.Database.Metadata;

internal class MetadataReader
{
    private readonly ILogger _logger;

    public MetadataReader(ILogger logger)
    {
        _logger = logger;
    }

    internal List<SchemaInfo> ReadTargetDatabaseMetadata(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);

            var databaseInfo = connection.Query<DatabaseInfo>(DatabaseInfo.Query());

            return databaseInfo
                .GroupBy(x => x.Schema)
                .Select(bySchema => new SchemaInfo(bySchema.Key)
                {
                    Tables = bySchema
                        .GroupBy(x => x.TableName)
                        .Select(byTable => new TableInfo(byTable.Key)
                        {
                            Columns = byTable
                                .Select(c => new ColumnInfo
                                {
                                    Name = c.ColumnName,
                                    DataType = c.DataType,
                                    IsNullable = c.IsNullable == "YES",
                                    Position = int.Parse(c.Position),
                                    CharacterMaxLength = c.CharacterMaxLength is null ? null : int.Parse(c.CharacterMaxLength)
                                })
                        })
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Metadata retrieval from the target database failed.");
            return [];
        }
    }
}
