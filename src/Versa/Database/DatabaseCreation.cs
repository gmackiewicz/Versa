using System;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Versa.Database;

internal class DatabaseCreation
{
    internal void EnsureDatabaseCreated(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);

            connection.Execute(@"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'Version')
BEGIN
    CREATE TABLE [dbo].[Version](
        [Version] [int] NOT NULL,
        [InstalledOn] [datetime] NOT NULL,
        CONSTRAINT [PK_Version] PRIMARY KEY CLUSTERED ([Version] ASC)
    );
END
ELSE
    PRINT 'Table [Version] exists.'


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'SchemaInfo')
BEGIN
    CREATE TABLE [dbo].[SchemaInfo](
        [Id] [int] NOT NULL,
        [Schema] [nvarchar](50) NOT NULL,
        CONSTRAINT [PK_SchemaInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
ELSE
    PRINT 'Table [SchemaInfo] exists.'


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'TableInfo')
BEGIN
    CREATE TABLE [dbo].[TableInfo](
        [Id] [int] NOT NULL,
        [SchemaId] [int] NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        CONSTRAINT [PK_TableInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    ALTER TABLE [dbo].[TableInfo] WITH CHECK ADD CONSTRAINT [FK_TableInfo_SchemaInfo] FOREIGN KEY([SchemaId]) REFERENCES [dbo].[SchemaInfo] ([Id]);
    
    ALTER TABLE [dbo].[TableInfo] CHECK CONSTRAINT [FK_TableInfo_SchemaInfo];
END
ELSE
    PRINT 'Table [TableInfo] exists.'


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_NAME] = 'ColumnInfo')
BEGIN
    CREATE TABLE [dbo].[ColumnInfo](
        [Id] [int] NOT NULL,
        [TableId] [int] NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [DataType] [nvarchar](50) NOT NULL,
        [IsNullable] [bit] NOT NULL,
        [Position] [int] NOT NULL,
        [CharacterMaxLength] [int] NULL,
        CONSTRAINT [PK_ColumnInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    ALTER TABLE [dbo].[ColumnInfo]  WITH CHECK ADD  CONSTRAINT [FK_ColumnInfo_TableInfo] FOREIGN KEY([TableId])    REFERENCES [dbo].[TableInfo] ([Id]);
    ALTER TABLE [dbo].[ColumnInfo] CHECK CONSTRAINT [FK_ColumnInfo_TableInfo];
END
ELSE
    PRINT 'Table [ColumnInfo] exists.'


INSERT INTO [dbo].[Version] ([Version], [InstalledOn]) VALUES (1, GETUTCDATE());

PRINT 'Done.'");
        }
        catch (Exception ex)
        {
            // TODO: logger
        }
    }
}
