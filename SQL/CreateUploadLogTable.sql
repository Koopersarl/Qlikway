-- Script de création de la table UploadLog
-- Exécutez ce script dans votre base de données SQL Server avant d'utiliser l'application

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UploadLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UploadLog](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [TableName] [nvarchar](255) NOT NULL,
        [FileName] [nvarchar](500) NOT NULL,
        [UploadDate] [datetime2](7) NOT NULL,
        [RowsInserted] [int] NOT NULL,
        [Success] [bit] NOT NULL,
        [ErrorMessage] [nvarchar](max) NULL,
        [DurationMs] [bigint] NOT NULL,
        CONSTRAINT [PK_UploadLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    -- Index pour les recherches par date
    CREATE NONCLUSTERED INDEX [IX_UploadLog_UploadDate] ON [dbo].[UploadLog]
    (
        [UploadDate] DESC
    )

    -- Index pour les recherches par table
    CREATE NONCLUSTERED INDEX [IX_UploadLog_TableName] ON [dbo].[UploadLog]
    (
        [TableName] ASC
    )

    PRINT 'Table UploadLog créée avec succès'
END
ELSE
BEGIN
    PRINT 'La table UploadLog existe déjà'
END
GO
