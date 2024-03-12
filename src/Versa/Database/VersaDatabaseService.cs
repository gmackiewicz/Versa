using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Versa.Database.Metadata;

namespace Versa.Database;

internal class VersaDatabaseService
{
    private readonly VersaDbConfiguration _dbConfiguration;

    public VersaDatabaseService(VersaDbConfiguration dbConfiguration)
    {
        _dbConfiguration = dbConfiguration;
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
            // TODO: logger
        }
    }

    internal void UpdateSavedMetadata(List<SchemaInfo> schemaInfos)
    {
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
                SaveSchemaInfo(connection, schema);
            }
            else
            {
                // TODO: update schema info
            }
        }
    }

    private static void SaveSchemaInfo(SqlConnection connection, SchemaInfo schema)
    {
        var schemaId = connection.QuerySingle<int>(
            "INSERT INTO SchemaInfo ([Schema], VerifiedOn) " +
            "OUTPUT INSERTED.Id " +
            "VALUES (@Schema, @VerifiedOn)",
            schema);

        foreach (var table in schema.Tables)
        {
            table.SchemaId = schemaId;

            var tableId = connection.QuerySingle<int>(
                "INSERT INTO TableInfo ([SchemaId], [Name]) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@SchemaId, @Name)",
                table);

            foreach (var column in table.Columns)
            {
                column.TableId = tableId;
                connection.Execute(
                    "INSERT INTO ColumnInfo ([TableId], [Name], DataType, IsNullable, Position, CharacterMaxLength)" +
                    "VALUES (@TableId, @Name, @DataType, @IsNullable, @Position, @CharacterMaxLength)",
                    column);
            }
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
