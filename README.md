# FTP Bulk Insert Application

Application ASP.NET Core pour automatiser le téléchargement de fichiers CSV depuis un serveur FTP et effectuer des BULK INSERT dans SQL Server.

## 📋 Fonctionnalités

- ✅ **Interface web de gestion** pour upload manuel et visualisation des logs
- ✅ **Création automatique de tables** avec détection intelligente des types de colonnes
- ✅ Téléchargement automatique de fichiers CSV depuis un serveur FTP
- ✅ Upload manuel de fichiers CSV via l'interface web
- ✅ Vidage automatique des tables avant insertion
- ✅ Bulk insert optimisé dans SQL Server
- ✅ Logging détaillé dans la table `UploadLog`
- ✅ Visualisation en temps réel de l'historique des imports
- ✅ API REST pour déclenchement via webhook (n8n, Zapier, etc.)
- ✅ Authentification par clé API pour l'API
- ✅ Option de suppression des fichiers FTP après import

## 🏗️ Architecture

```
FTP Server (fichiers CSV)
    ↓
Application ASP.NET
    ↓
SQL Server (BULK INSERT)
    ↓
Table UploadLog (historique)
```

## 📦 Prérequis

- .NET 6.0 ou supérieur
- SQL Server (compatible avec SmarterASP)
- Accès FTP pour les fichiers CSV
- SmarterASP ou tout hébergeur ASP.NET

## 🔧 Installation

### 1. Créer la table UploadLog

Exécutez le script SQL suivant dans votre base de données :

```sql
-- Voir le fichier SQL/CreateUploadLogTable.sql
```

### 2. Créer les tables de destination

Vous avez deux options :

#### Option A : Création automatique (recommandé pour dev/test) 🤖

Activez `AutoCreateTables` dans `appsettings.json` :

```json
{
  "AutoCreateTables": true,
  "SampleRowsForTypeDetection": 100
}
```

L'application analysera automatiquement vos fichiers CSV et créera les tables avec les types de colonnes appropriés.

**📚 Documentation complète :** [AUTO_CREATE_TABLES.md](AUTO_CREATE_TABLES.md)

#### Option B : Création manuelle (recommandé pour production)

Pour chaque fichier CSV, créez une table correspondante. Le nom de la table doit correspondre au nom du fichier (sans l'extension .csv).

**Exemple :**
- Fichier : `Clients.csv`
- Table : `Clients`

```sql
CREATE TABLE [Clients] (
    [Id] INT,
    [Nom] NVARCHAR(255),
    [Email] NVARCHAR(255),
    -- ... autres colonnes selon votre CSV
)
```

**IMPORTANT :** L'ordre des colonnes dans la table doit correspondre à l'ordre des colonnes dans le fichier CSV.

### 3. Configuration de l'application

Modifiez le fichier `appsettings.json` avec vos paramètres :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql.smarterasp.net;Database=DB_XXXXXX;User Id=DB_XXXXXX_user;Password=VotreMotDePasse;TrustServerCertificate=True;"
  },
  "FtpSettings": {
    "Server": "ftp://ftp.smarterasp.net",
    "Username": "votre_username_ftp",
    "Password": "votre_password_ftp",
    "Path": "/csv"
  },
  "TempFolder": "D:\\Web\\temp\\FtpBulkInsert",
  "ApiKey": "votre_cle_api_secrete_123",
  "AutoCreateTables": true,
  "SampleRowsForTypeDetection": 100
}
```

**Notes pour SmarterASP :**
- Le `TempFolder` doit pointer vers un dossier accessible sur votre hébergement (généralement `D:\\Web\\temp`)
- Utilisez les informations de connexion fournies par SmarterASP

### 4. Déploiement sur SmarterASP

1. **Compiler l'application :**
   ```bash
   dotnet publish -c Release
   ```

2. **Uploader les fichiers :**
   - Via FTP, uploadez le contenu du dossier `bin/Release/net6.0/publish/` vers votre site SmarterASP
   - Assurez-vous que le fichier `appsettings.json` contient les bonnes informations

3. **Configuration IIS :**
   - SmarterASP configure automatiquement IIS
   - Vérifiez que l'application utilise .NET 6.0

## 🚀 Utilisation

### Interface web de gestion

**Accéder à l'interface :** `https://votresite.com/manage`

L'interface web vous permet de :

#### 📤 Upload manuel de fichiers CSV
1. Cliquez sur "Sélectionner des fichiers CSV"
2. Choisissez un ou plusieurs fichiers CSV
3. Cliquez sur "Uploader et importer"
4. Les fichiers seront automatiquement importés dans les tables correspondantes

#### 🔄 Déclencher l'import FTP manuellement
1. Cochez "Supprimer les fichiers du FTP après import" si souhaité
2. Cliquez sur "Lancer l'import FTP"
3. L'application récupérera tous les fichiers CSV du serveur FTP et les importera

#### 📋 Consulter l'historique
- L'historique des imports s'affiche automatiquement en bas de la page
- Cliquez sur "Rafraîchir" pour actualiser les logs
- Vous pouvez voir pour chaque import :
  - Date et heure
  - Nom du fichier et de la table
  - Nombre de lignes insérées
  - Durée de l'opération
  - Statut (succès/échec)

### API REST (pour automatisation)

L'application expose également une API REST pour l'automatisation via n8n ou autres outils.

### Endpoints disponibles

#### 1. Traiter les fichiers FTP

**POST** `/api/bulkinsert/process`

Headers :
```
X-API-Key: votre_cle_api_secrete_123
```

Query Parameters :
- `deleteAfterImport` (boolean, optionnel) : Supprimer les fichiers du FTP après import (défaut: false)

**Exemple avec curl :**
```bash
curl -X POST "https://votresite.com/api/bulkinsert/process?deleteAfterImport=true" \
     -H "X-API-Key: votre_cle_api_secrete_123"
```

**Réponse :**
```json
{
  "success": true,
  "results": [
    {
      "tableName": "Clients",
      "fileName": "Clients.csv",
      "success": true,
      "rowsInserted": 1500,
      "errorMessage": null,
      "duration": "00:00:05.1234567"
    }
  ],
  "totalFilesProcessed": 1,
  "totalRowsInserted": 1500
}
```

#### 2. Vérifier l'état du service

**GET** `/api/bulkinsert/health`

```bash
curl https://votresite.com/api/bulkinsert/health
```

### Configuration dans n8n

1. **Créer un workflow n8n**
2. **Ajouter un nœud "Schedule Trigger"**
   - Cron Expression: `0 6 * * *` (tous les jours à 6h)
3. **Ajouter un nœud "HTTP Request"**
   - Method: POST
   - URL: `https://votresite.com/api/bulkinsert/process?deleteAfterImport=true`
   - Headers:
     - Name: `X-API-Key`
     - Value: `votre_cle_api_secrete_123`

## 📊 Workflow typique

1. **Déposer les fichiers CSV sur le FTP**
   - Les fichiers doivent avoir le format : `NomTable.csv`
   - Exemple : `Clients.csv`, `Commandes.csv`, `Produits.csv`

2. **Déclencher l'API** (via n8n ou manuellement)
   - L'application liste tous les fichiers `.csv` sur le FTP

3. **Pour chaque fichier :**
   - Téléchargement du fichier en local
   - Vidage de la table correspondante (`TRUNCATE TABLE`)
   - Bulk insert des données
   - Enregistrement dans `UploadLog`
   - Suppression du fichier temporaire
   - (Optionnel) Suppression du fichier FTP

4. **Consulter les logs**
   - Vérifier la table `UploadLog` pour voir l'historique

## 📝 Format des fichiers CSV

Les fichiers CSV doivent respecter le format suivant :

```csv
Colonne1,Colonne2,Colonne3
valeur1,valeur2,valeur3
valeur4,valeur5,valeur6
```

**Important :**
- La première ligne contient les en-têtes (ignorée lors de l'import)
- Séparateur : virgule (`,`)
- Encodage : UTF-8
- Terminateur de ligne : `\n` ou `\r\n`

## 🔍 Consultation des logs

Requête SQL pour consulter l'historique :

```sql
SELECT TOP 50
    Id,
    TableName,
    FileName,
    UploadDate,
    RowsInserted,
    Success,
    ErrorMessage,
    DurationMs
FROM UploadLog
ORDER BY UploadDate DESC
```

## 🛠️ Dépannage

### Erreur : "La table XXX n'existe pas"
- Vérifiez que vous avez créé la table dans SQL Server
- Le nom de la table doit correspondre exactement au nom du fichier (sans .csv)

### Erreur : "BULK INSERT failed"
- Vérifiez que l'ordre des colonnes correspond
- Vérifiez le format du CSV (séparateurs, encodage)
- Vérifiez que le serveur SQL a accès au fichier temporaire

### Erreur FTP : "Unable to connect"
- Vérifiez les credentials FTP
- Vérifiez que le chemin FTP existe
- Vérifiez les règles de pare-feu

### Erreur : "Unauthorized"
- Vérifiez que vous passez la bonne clé API dans le header `X-API-Key`

## 🔐 Sécurité

- ✅ Authentification par clé API
- ✅ Connexion SQL Server sécurisée
- ✅ Nettoyage automatique des fichiers temporaires
- ✅ Protection contre l'injection SQL (paramètres sécurisés)
- ⚠️ Stockez la clé API en sécurité (ne la commitez jamais)
- ⚠️ Utilisez HTTPS pour les appels API

## 📄 Licence

Ce projet est un outil interne pour l'automatisation des imports de données.

## 🤝 Support

Pour toute question ou problème, consultez les logs de l'application et la table `UploadLog`.
