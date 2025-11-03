# Guide de déploiement sur SmarterASP

## Étape 1 : Préparation de la base de données

### 1.1 Créer la table UploadLog

Connectez-vous à votre base de données SQL Server via le panneau de contrôle SmarterASP et exécutez :

```sql
-- Voir SQL/CreateUploadLogTable.sql
```

### 1.2 Créer vos tables de données

Pour chaque fichier CSV que vous allez importer, créez une table correspondante.

**Exemple pour Clients.csv :**

```sql
CREATE TABLE [Clients] (
    [ClientID] INT PRIMARY KEY,
    [Nom] NVARCHAR(255),
    [Prenom] NVARCHAR(255),
    [Email] NVARCHAR(255),
    [Telephone] NVARCHAR(50),
    [DateInscription] DATE
)
```

**⚠️ IMPORTANT :**
- Le nom de la table doit correspondre au nom du fichier CSV (sans l'extension)
- L'ordre des colonnes dans la table SQL doit correspondre EXACTEMENT à l'ordre dans le CSV

## Étape 2 : Configuration de l'application

### 2.1 Modifier appsettings.json

Avant de compiler, modifiez `appsettings.json` avec vos vraies informations :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql.smarterasp.net;Database=DB_123456;User Id=DB_123456_user;Password=VotreMotDePasse;TrustServerCertificate=True;"
  },
  "FtpSettings": {
    "Server": "ftp://ftp123.smarterasp.net",
    "Username": "votre-username-ftp",
    "Password": "votre-password-ftp",
    "Path": "/csv"
  },
  "TempFolder": "D:\\Web\\temp\\FtpBulkInsert",
  "ApiKey": "GenerezUneCleSecuriseIci123!@#"
}
```

**Obtenir vos informations SmarterASP :**
- Connexion SQL : Panneau de contrôle → Database → Details
- FTP : Panneau de contrôle → FTP Details
- Le `TempFolder` doit être dans le dossier de votre site (généralement `D:\\Web\\...`)

### 2.2 Générer une clé API sécurisée

Générez une clé API forte, par exemple :
```
VotreNom_2024_SecretKey_Abc123XyZ!
```

## Étape 3 : Compilation

### 3.1 Compiler l'application

```bash
dotnet restore
dotnet publish -c Release -o publish
```

Le dossier `publish` contiendra tous les fichiers à déployer.

## Étape 4 : Upload vers SmarterASP

### 4.1 Via FTP

1. Connectez-vous à votre FTP SmarterASP
2. Naviguez vers votre dossier racine (généralement `/wwwroot`)
3. Uploadez TOUT le contenu du dossier `publish`
4. Vérifiez que `web.config` est bien présent

### 4.2 Structure finale sur le serveur

```
/wwwroot/
    ├── FtpBulkInsert.dll
    ├── FtpBulkInsert.deps.json
    ├── FtpBulkInsert.runtimeconfig.json
    ├── appsettings.json
    ├── web.config
    ├── Microsoft.Data.SqlClient.dll
    └── ... (autres DLLs)
```

## Étape 5 : Configuration du dossier FTP

### 5.1 Créer le dossier CSV

Via FTP, créez un dossier `/csv` à la racine de votre FTP (ou utilisez le chemin configuré dans `appsettings.json`)

```
/
├── wwwroot/
└── csv/         ← Vos fichiers CSV ici
    ├── Clients.csv
    ├── Commandes.csv
    └── Produits.csv
```

## Étape 6 : Test de l'application

### 6.1 Vérifier que l'application fonctionne

```bash
curl https://votre-site.smarterasp.net/api/bulkinsert/health
```

Réponse attendue :
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00",
  "version": "1.0.0"
}
```

### 6.2 Test avec un fichier

1. Uploadez un fichier CSV de test sur votre FTP
2. Lancez l'import :

```bash
curl -X POST "https://votre-site.smarterasp.net/api/bulkinsert/process" \
     -H "X-API-Key: VotreCleAPI"
```

### 6.3 Vérifier les logs

```sql
SELECT * FROM UploadLog ORDER BY UploadDate DESC
```

## Étape 7 : Configuration de n8n

### 7.1 Créer un nouveau workflow

1. Ouvrez n8n
2. Créez un nouveau workflow

### 7.2 Ajouter le trigger

1. Ajoutez un nœud "Schedule Trigger"
2. Configuration :
   - Trigger Times: `Cron`
   - Cron Expression: `0 6 * * *` (6h du matin tous les jours)

### 7.3 Ajouter l'appel HTTP

1. Ajoutez un nœud "HTTP Request"
2. Configuration :
   - Method: `POST`
   - URL: `https://votre-site.smarterasp.net/api/bulkinsert/process?deleteAfterImport=false`
   - Authentication: `None`
   - Headers:
     ```
     Name: X-API-Key
     Value: VotreCleAPI
     ```

### 7.4 (Optionnel) Ajouter des notifications

Ajoutez un nœud "Send Email" ou "Slack" pour recevoir une notification après chaque import.

## Étape 8 : Routine quotidienne

### 8.1 Workflow typique

1. **Chaque nuit** : Vos systèmes génèrent les fichiers CSV
2. **Upload automatique** : Les fichiers sont uploadés sur le FTP
3. **6h du matin** : n8n déclenche l'API
4. **Processus :**
   - Téléchargement des CSV
   - Vidage des tables
   - Import des données
   - Logging dans UploadLog
5. **Vérification** : Consulter la table UploadLog

### 8.2 Monitoring

Créez une vue SQL pour suivre les imports :

```sql
CREATE VIEW vw_ImportSummary AS
SELECT
    CAST(UploadDate AS DATE) as Date,
    TableName,
    COUNT(*) as NombreImports,
    SUM(RowsInserted) as TotalLignes,
    SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) as Succes,
    SUM(CASE WHEN Success = 0 THEN 1 ELSE 0 END) as Echecs
FROM UploadLog
GROUP BY CAST(UploadDate AS DATE), TableName
```

## Troubleshooting SmarterASP

### Erreur : "HTTP Error 500.0 - ANCM In-Process Handler Load Failure"

- Vérifiez que vous utilisez .NET 6.0
- Dans le panneau SmarterASP, allez à "ASP.NET Configuration" et sélectionnez ".NET 6.0"

### Erreur : "Access denied" sur le fichier temporaire

- Le dossier `TempFolder` doit être dans votre espace web
- Changez le chemin vers : `D:\\Web\\yourusername\\temp\\FtpBulkInsert`

### Erreur FTP : "530 User cannot log in"

- Vérifiez vos credentials FTP dans le panneau SmarterASP
- Testez la connexion avec FileZilla

### Erreur SQL : "Login failed"

- Vérifiez les informations de connexion SQL
- Ajoutez `TrustServerCertificate=True` dans la chaîne de connexion

## Support

Pour le support SmarterASP : https://www.smarterasp.net/support
