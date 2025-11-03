-- Script de création des tables d'exemple
-- Utilisez ce script comme modèle pour créer vos propres tables

-- Table Clients (correspondant à Clients.csv)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Clients](
        [ClientID] [int] NOT NULL,
        [Nom] [nvarchar](255) NOT NULL,
        [Prenom] [nvarchar](255) NOT NULL,
        [Email] [nvarchar](255) NOT NULL,
        [Telephone] [nvarchar](50) NULL,
        [DateInscription] [date] NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED ([ClientID] ASC)
    ) ON [PRIMARY]

    PRINT 'Table Clients créée avec succès'
END
ELSE
BEGIN
    PRINT 'La table Clients existe déjà'
END
GO

-- Table Commandes (exemple)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Commandes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Commandes](
        [CommandeID] [int] NOT NULL,
        [ClientID] [int] NOT NULL,
        [DateCommande] [date] NOT NULL,
        [Montant] [decimal](10, 2) NOT NULL,
        [Statut] [nvarchar](50) NULL,
        CONSTRAINT [PK_Commandes] PRIMARY KEY CLUSTERED ([CommandeID] ASC)
    ) ON [PRIMARY]

    PRINT 'Table Commandes créée avec succès'
END
ELSE
BEGIN
    PRINT 'La table Commandes existe déjà'
END
GO

-- Table Produits (exemple)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Produits]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Produits](
        [ProduitID] [int] NOT NULL,
        [Nom] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [Prix] [decimal](10, 2) NOT NULL,
        [Stock] [int] NOT NULL,
        [Categorie] [nvarchar](100) NULL,
        CONSTRAINT [PK_Produits] PRIMARY KEY CLUSTERED ([ProduitID] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    PRINT 'Table Produits créée avec succès'
END
ELSE
BEGIN
    PRINT 'La table Produits existe déjà'
END
GO

PRINT 'Toutes les tables ont été créées avec succès !'
