using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Versa.Database.Metadata;

internal class MetadataReader
{
    internal List<SchemaInfo> ParseDatabaseMetadata(string connectionString)
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
                                    IsNullable = c.IsNullable,
                                    Position = c.Position,
                                    CharacterMaxLength = c.CharacterMaxLength
                                })
                        })
                })
                .ToList();
        }
        catch (Exception ex)
        {
            // TODO: logger
            return [];
        }
    }
}
