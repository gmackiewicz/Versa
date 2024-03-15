using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;
using Versa.Database.Metadata;

namespace Versa.Database;

internal class VersaDatabaseService
{
    private readonly VersaDbConfiguration _dbConfiguration;
    private readonly ILogger _logger;

    public VersaDatabaseService(
        VersaDbConfiguration dbConfiguration,
        ILogger logger)
    {
        _dbConfiguration = dbConfiguration;
        _logger = logger;
    }

    internal void EnsureDatabaseCreated()
    {
        try
        {
            using var connection = new SqlConnection(_dbConfiguration.ConnectionString);

            connection.Execute(InitScript());
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Init failed.");
        }
    }

    internal void UpdateSavedMetadata(List<SchemaInfo> schemaInfos)
    {
        _logger.Information("Metadata update started.");
        using var connection = new SqlConnection(_dbConfiguration.ConnectionString);

        connection.Execute(
            "DELETE FROM SchemaInfo WHERE [Schema] NOT IN @Schemas",
            new { Schemas = schemaInfos.Select(s => s.Schema) });

        var schemas = connection.Query<SchemaInfo>("SELECT * FROM SchemaInfo");

        foreach (var schema in schemaInfos)
        {
            var schemaFromDb = schemas.FirstOrDefault(s => s.Schema == schema.Schema);
            if (schemaFromDb is null)
            {
                AddSchemaInfo(connection, schema);
            }
            else
            {
                schema.Id = schemaFromDb.Id;
                UpdateSchemaInfo(connection, schema);
            }
        }
        _logger.Information("Metadata update finished.");
    }

    private static void AddSchemaInfo(SqlConnection connection, SchemaInfo schema)
    {
        var schemaId = connection.QuerySingle<int>(SchemaInfo.InsertSql(), schema);

        foreach (var table in schema.Tables)
        {
            table.SchemaId = schemaId;
            var tableId = connection.QuerySingle<int>(TableInfo.InsertSql(), table);

            foreach (var column in table.Columns)
            {
                column.TableId = tableId;
                connection.Execute(ColumnInfo.InsertSql(), column);
            }
        }
    }

    private void UpdateSchemaInfo(SqlConnection connection, SchemaInfo schema)
    {
        connection.Execute(
            "DELETE FROM TableInfo WHERE SchemaId = @SchemaId AND [Name] NOT IN @Tables",
            new { SchemaId = schema.Id, Tables = schema.Tables.Select(s => s.Name) });

        var tables = connection.Query<TableInfo>(
            "SELECT * FROM TableInfo WHERE SchemaId = @schemaId",
            new { schemaId = schema.Id });

        foreach (var table in schema.Tables)
        {
            var tableFromDb = tables.FirstOrDefault(t => t.Name == table.Name);
            if (tableFromDb is null)
            {
                AddTableInfo(connection, table);
            }
            else
            {
                table.Id = tableFromDb.Id;
                UpdateTableInfo(connection, table);
            }
        }

        connection.Execute(
            "UPDATE SchemaInfo SET VerifiedOn = GETUTCDATE() WHERE Id = @SchemaId",
            new { SchemaId = schema.Id });
    }

    private void AddTableInfo(SqlConnection connection, TableInfo table)
    {
        var tableId = connection.QuerySingle<int>(TableInfo.InsertSql(), table);

        foreach (var column in table.Columns)
        {
            column.TableId = tableId;
            connection.Execute(ColumnInfo.InsertSql(), column);
        }
    }

    private void UpdateTableInfo(SqlConnection connection, TableInfo table)
    {
        connection.Execute(
            "DELETE FROM ColumnInfo WHERE TableId = @TableId AND [Name] NOT IN @Columns",
            new { TableId = table.Id, Columns = table.Columns.Select(s => s.Name) });

        var columns = connection.Query<ColumnInfo>(
            "SELECT * FROM ColumnInfo WHERE TableId = @tableId",
            new { tableId = table.Id });

        foreach (var column in table.Columns)
        {
            var columnFromDb = columns.FirstOrDefault(c => c.Name == column.Name);
            if (columnFromDb is not null)
            {
                continue;
            }

            column.TableId = table.Id;
            connection.Execute(ColumnInfo.InsertSql(), column);
        }
    }

    private string InitScript() =>
        @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'Version')
BEGIN
    CREATE TABLE [dbo].[Version](
        [Version] [int] NOT NULL,
        [InstalledOn] [datetime] NOT NULL,
        CONSTRAINT [PK_Version] PRIMARY KEY CLUSTERED ([Version] ASC)
    );
END
ELSE
    PRINT 'Table [Version] exists.'


DECLARE @CurrentVersion INT;
SELECT @CurrentVersion = MAX([Version]) FROM [Version]

IF (@CurrentVersion >= 1) RETURN;


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'SchemaInfo')
BEGIN
    CREATE TABLE [dbo].[SchemaInfo](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Schema] [nvarchar](50) NOT NULL,
        [VerifiedOn] [datetime] NOT NULL,
        CONSTRAINT [PK_SchemaInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
ELSE
    PRINT 'Table [SchemaInfo] exists.'


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'TableInfo')
BEGIN
    CREATE TABLE [dbo].[TableInfo](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SchemaId] [int] NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        CONSTRAINT [PK_TableInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    ALTER TABLE [dbo].[TableInfo] WITH CHECK ADD CONSTRAINT [FK_TableInfo_SchemaInfo] FOREIGN KEY([SchemaId]) REFERENCES [dbo].[SchemaInfo] ([Id]) ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[TableInfo] CHECK CONSTRAINT [FK_TableInfo_SchemaInfo];
END
ELSE
    PRINT 'Table [TableInfo] exists.'


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'ColumnInfo')
BEGIN
    CREATE TABLE [dbo].[ColumnInfo](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TableId] [int] NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [DataType] [nvarchar](50) NOT NULL,
        [IsNullable] [bit] NOT NULL,
        [Position] [int] NOT NULL,
        [CharacterMaxLength] [int] NULL,
        CONSTRAINT [PK_ColumnInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_TableInfo] FOREIGN KEY([TableId])    REFERENCES [dbo].[TableInfo] ([Id]) ON DELETE CASCADE;
    ALTER TABLE [dbo].[ColumnInfo] CHECK CONSTRAINT [FK_ColumnInfo_TableInfo];
END
ELSE
    PRINT 'Table [ColumnInfo] exists.'


INSERT INTO [dbo].[Version] ([Version], [InstalledOn]) VALUES (1, GETUTCDATE());

PRINT 'Done.'";
}
