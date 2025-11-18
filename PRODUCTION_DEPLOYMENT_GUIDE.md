# 🚀 Guide de déploiement en production sur SmarterASP

Ce guide vous accompagne étape par étape pour déployer l'application FTP Bulk Insert sur SmarterASP.

---

## 📋 Prérequis

Avant de commencer, assurez-vous d'avoir :

- ✅ Un compte SmarterASP actif
- ✅ Accès à votre base de données SQL Server (informations dans le panneau SmarterASP)
- ✅ Accès FTP à votre serveur (informations dans le panneau SmarterASP)
- ✅ .NET SDK 6.0 ou supérieur installé sur votre machine de développement
- ✅ Un client FTP (FileZilla recommandé) ou accès au File Manager de SmarterASP

---

## 🔧 ÉTAPE 1 : Configuration de la base de données

### 1.1 Récupérer les informations de connexion SQL Server

1. Connectez-vous à votre compte SmarterASP
2. Allez dans **Control Panel** → **Databases** → **SQL Server**
3. Notez les informations suivantes :
   ```
   Server: sql.smarterasp.net (ou sql123.smarterasp.net)
   Database: DB_123456_votredb
   Username: DB_123456_user
   Password: [votre mot de passe]
   ```

### 1.2 Créer la table UploadLog

1. Dans le panneau SmarterASP, allez dans **Databases** → **SQL Server** → **Manage**
2. Ouvrez le **Query Editor**
3. Copiez et exécutez le script suivant :

```sql
-- Création de la table UploadLog
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

    CREATE NONCLUSTERED INDEX [IX_UploadLog_UploadDate] ON [dbo].[UploadLog] ([UploadDate] DESC)
    CREATE NONCLUSTERED INDEX [IX_UploadLog_TableName] ON [dbo].[UploadLog] ([TableName] ASC)

    PRINT 'Table UploadLog créée avec succès'
END
ELSE
BEGIN
    PRINT 'La table UploadLog existe déjà'
END
GO
```

4. Vérifiez que le message "Table UploadLog créée avec succès" s'affiche

### 1.3 Créer vos tables de données (optionnel)

**Si `AutoCreateTables = false` :**

Créez vos tables manuellement pour chaque fichier CSV que vous allez importer.

**Exemple pour Clients.csv :**
```sql
CREATE TABLE [Clients] (
    [ClientID] INT PRIMARY KEY,
    [Nom] NVARCHAR(100) NOT NULL,
    [Prenom] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [Telephone] NVARCHAR(50),
    [DateInscription] DATE
)
```

**Si `AutoCreateTables = true` :**

Vous pouvez sauter cette étape, les tables seront créées automatiquement.

---

## 🔧 ÉTAPE 2 : Configuration FTP

### 2.1 Récupérer les informations FTP

1. Dans le panneau SmarterASP, allez dans **FTP**
2. Notez les informations :
   ```
   FTP Server: ftp123.smarterasp.net
   Username: votre-username-ftp
   Password: [votre mot de passe FTP]
   ```

### 2.2 Créer le dossier pour les fichiers CSV

1. Connectez-vous à votre FTP avec FileZilla ou le File Manager
2. À la racine de votre FTP, créez un dossier nommé `csv`
3. Structure attendue :
   ```
   /
   ├── wwwroot/          (votre site web)
   └── csv/              (vos fichiers CSV)
       ├── Clients.csv
       ├── Commandes.csv
       └── ...
   ```

---

## 🔧 ÉTAPE 3 : Configuration de l'application

### 3.1 Modifier appsettings.json

⚠️ **FICHIER CRITIQUE : `appsettings.json`**

C'est dans ce fichier que vous allez mettre **TOUS vos paramètres de connexion**.

Sur votre machine de développement, ouvrez le fichier `appsettings.json` et remplacez les valeurs :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql123.smarterasp.net;Database=DB_123456_votredb;User Id=DB_123456_user;Password=VOTRE_MOT_DE_PASSE_SQL;TrustServerCertificate=True;Encrypt=True;"
  },
  "FtpSettings": {
    "Server": "ftp://ftp123.smarterasp.net",
    "Username": "votre-username-ftp",
    "Password": "VOTRE_MOT_DE_PASSE_FTP",
    "Path": "/csv"
  },
  "TempFolder": "temp",
  "ApiKey": "CHANGEZ_CETTE_CLE_PAR_QUELQUE_CHOSE_DE_SECURISE_123!",
  "AutoCreateTables": true,
  "SampleRowsForTypeDetection": 100
}
```

### 3.2 Paramètres à configurer

| Paramètre | Description | Exemple |
|-----------|-------------|---------|
| **Server** | Serveur SQL fourni par SmarterASP | `sql123.smarterasp.net` |
| **Database** | Nom de votre base de données | `DB_123456_votredb` |
| **User Id** | Utilisateur SQL | `DB_123456_user` |
| **Password** (SQL) | Mot de passe SQL | `VotreMotDePasseSQL!` |
| **FTP Server** | Serveur FTP | `ftp://ftp123.smarterasp.net` |
| **FTP Username** | Utilisateur FTP | `votre-username-ftp` |
| **FTP Password** | Mot de passe FTP | `VotreMotDePasseFTP!` |
| **TempFolder** | Dossier temporaire | `temp` (recommandé pour hébergement mutualisé) |
| **ApiKey** | Clé secrète pour l'API | `MonSuperSecret2024!` |
| **AutoCreateTables** | Création auto des tables | `true` (dev) ou `false` (prod) |

### 3.3 Notes importantes

- ⚠️ **IMPORTANT pour hébergement mutualisé SmarterASP** : Utilisez `"TempFolder": "temp"` (chemin relatif)
  - L'application créera automatiquement un dossier `temp` dans `/wwwroot`
  - Ce dossier est protégé contre l'accès HTTP via `web.config`
  - **NE PAS utiliser** `D:\\Web\\temp\\` sur un hébergement mutualisé (accès refusé)
  - 📚 **Pour plus de détails, consultez :** `SMARTERASP_TEMPFOLDER_FIX.md`
- **ApiKey** : Générez une clé forte, par exemple avec : https://passwordsgenerator.net/
- **AutoCreateTables** :
  - `true` : Les tables sont créées automatiquement (pratique pour débuter)
  - `false` : Vous devez créer les tables manuellement (recommandé en production)

---

## 🔧 ÉTAPE 4 : Compilation de l'application

### 4.1 Ouvrir un terminal

Sur votre machine de développement :

**Windows :**
- Ouvrez PowerShell ou Command Prompt
- Naviguez vers le dossier du projet :
  ```bash
  cd C:\chemin\vers\Qlikway
  ```

**Mac/Linux :**
- Ouvrez Terminal
- Naviguez vers le dossier du projet :
  ```bash
  cd /chemin/vers/Qlikway
  ```

### 4.2 Compiler l'application

Exécutez la commande suivante :

```bash
dotnet publish -c Release -o ./publish
```

**Explication :**
- `-c Release` : Compile en mode Release (optimisé pour la production)
- `-o ./publish` : Place les fichiers compilés dans le dossier `publish`

### 4.3 Vérifier la compilation

Vous devriez voir un message de succès :
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Le dossier `publish` doit contenir environ 50-100 fichiers :
```
publish/
├── FtpBulkInsert.dll
├── FtpBulkInsert.deps.json
├── FtpBulkInsert.runtimeconfig.json
├── appsettings.json          ← VOS PARAMÈTRES SONT ICI
├── appsettings.Development.json
├── web.config
├── wwwroot/
│   ├── css/
│   └── js/
├── Microsoft.Data.SqlClient.dll
├── ... (autres DLLs)
└── ... (autres fichiers)
```

⚠️ **IMPORTANT : Vérifiez que `appsettings.json` contient bien vos paramètres de production !**

---

## 🔧 ÉTAPE 5 : Upload vers SmarterASP

### 5.1 Se connecter au FTP

**Avec FileZilla :**
1. Ouvrez FileZilla
2. Entrez vos informations FTP :
   - Hôte : `ftp123.smarterasp.net`
   - Nom d'utilisateur : votre username FTP
   - Mot de passe : votre mot de passe FTP
   - Port : `21`
3. Cliquez sur **Connexion rapide**

**Avec le File Manager SmarterASP :**
1. Dans le panneau de contrôle, allez dans **File Manager**

### 5.2 Naviguer vers le dossier wwwroot

1. Dans la partie droite (serveur distant), naviguez vers `/wwwroot`
2. **⚠️ IMPORTANT** : Supprimez tout le contenu existant de `/wwwroot` (sauf si vous avez d'autres applications)

### 5.3 Uploader tous les fichiers

1. Dans la partie gauche (local), naviguez vers votre dossier `publish`
2. Sélectionnez **TOUS les fichiers et dossiers** dans `publish`
3. Glissez-déposez vers `/wwwroot` sur le serveur
4. Attendez la fin de l'upload (peut prendre 5-15 minutes selon votre connexion)

### 5.4 Vérifier la structure finale

Sur le serveur, dans `/wwwroot`, vous devez avoir :

```
/wwwroot/
├── FtpBulkInsert.dll
├── FtpBulkInsert.deps.json
├── FtpBulkInsert.runtimeconfig.json
├── appsettings.json              ← VOS PARAMÈTRES
├── appsettings.Development.json
├── web.config
├── Pages/
├── wwwroot/
│   ├── css/
│   │   └── manage.css
│   └── js/
│       └── manage.js
├── Microsoft.Data.SqlClient.dll
└── ... (tous les autres fichiers)
```

---

## 🔧 ÉTAPE 6 : Configuration IIS sur SmarterASP

### 6.1 Définir la version .NET

1. Dans le panneau SmarterASP, allez dans **Control Panel** → **IIS Settings**
2. Trouvez l'option **ASP.NET Version** ou **.NET Core Runtime**
3. Sélectionnez **.NET 6.0** (ou .NET 7.0/8.0 si disponible)
4. Cliquez sur **Save** ou **Apply**

### 6.2 Vérifier les permissions du dossier temporaire

1. Allez dans **File Manager**
2. Créez le dossier `D:\Web\temp` s'il n'existe pas
3. Créez le sous-dossier `D:\Web\temp\FtpBulkInsert`
4. Vérifiez les permissions (lecture/écriture nécessaires)

---

## 🔧 ÉTAPE 7 : Test de l'application

### 7.1 Tester le health check

Ouvrez votre navigateur et accédez à :

```
https://votre-site.smarterasp.net/api/bulkinsert/health
```

**Résultat attendu :**
```json
{
  "status": "healthy",
  "timestamp": "2024-11-14T10:30:00",
  "version": "1.0.0"
}
```

**Si vous avez une erreur :**
- Erreur 404 : L'application n'est pas déployée correctement
- Erreur 500 : Problème de configuration (vérifier appsettings.json)
- Erreur 502/503 : IIS n'a pas démarré l'application

### 7.2 Accéder à l'interface web

```
https://votre-site.smarterasp.net/manage
```

Vous devriez voir l'interface de gestion avec :
- Section d'upload manuel
- Section de déclenchement FTP
- Historique des imports

### 7.3 Tester un upload manuel

1. Préparez un fichier CSV de test simple :
   ```csv
   Id,Nom
   1,Test1
   2,Test2
   ```

2. Dans l'interface `/manage` :
   - Cliquez sur "Sélectionner des fichiers CSV"
   - Choisissez votre fichier de test
   - Cliquez sur "Uploader et importer"

3. Vérifiez le résultat :
   - ✅ Succès : La table a été créée et les données importées
   - ❌ Échec : Lisez le message d'erreur

### 7.4 Vérifier les logs

1. Retournez dans le Query Editor SQL
2. Exécutez :
   ```sql
   SELECT TOP 10 * FROM UploadLog ORDER BY UploadDate DESC
   ```
3. Vous devriez voir votre import de test

---

## 🔧 ÉTAPE 8 : Configuration de n8n (optionnel)

Si vous souhaitez automatiser les imports avec n8n :

### 8.1 Créer le workflow n8n

1. Ouvrez n8n
2. Créez un nouveau workflow
3. Ajoutez un nœud **Schedule Trigger**
   - Cron Expression : `0 6 * * *` (6h du matin tous les jours)

4. Ajoutez un nœud **HTTP Request**
   - Method : `POST`
   - URL : `https://votre-site.smarterasp.net/api/bulkinsert/process?deleteAfterImport=false`
   - Authentication : None
   - Headers :
     ```
     Name: X-API-Key
     Value: [Votre clé API depuis appsettings.json]
     ```

5. (Optionnel) Ajoutez un nœud **Send Email** pour recevoir une notification

6. Activez le workflow

### 8.2 Tester le workflow

1. Dans n8n, cliquez sur **Execute Workflow**
2. Vérifiez que l'API répond correctement
3. Consultez `/manage` pour voir le résultat

---

## ✅ CHECKLIST DE VÉRIFICATION

Avant de considérer le déploiement comme terminé, vérifiez :

### Base de données
- [ ] Table `UploadLog` créée
- [ ] Tables de données créées (si `AutoCreateTables = false`)
- [ ] Connexion SQL testée

### Configuration FTP
- [ ] Dossier `/csv` créé
- [ ] Fichiers CSV de test uploadés
- [ ] Accès FTP vérifié

### Configuration de l'application
- [ ] `appsettings.json` configuré avec les bons paramètres
- [ ] Clé API changée (sécurisée)
- [ ] TempFolder accessible

### Déploiement
- [ ] Compilation réussie (`dotnet publish`)
- [ ] Tous les fichiers uploadés dans `/wwwroot`
- [ ] .NET 6.0 configuré dans IIS
- [ ] Dossier temporaire créé

### Tests
- [ ] Health check fonctionne (`/api/bulkinsert/health`)
- [ ] Interface web accessible (`/manage`)
- [ ] Upload manuel fonctionne
- [ ] Logs enregistrés dans `UploadLog`
- [ ] Import FTP fonctionne (si configuré)

### n8n (optionnel)
- [ ] Workflow créé
- [ ] Clé API configurée
- [ ] Test manuel réussi
- [ ] Schedule activé

---

## 🐛 Dépannage

### Erreur : "HTTP Error 500.30 - ANCM In-Process Start Failure"

**Cause :** La version .NET n'est pas correctement configurée

**Solution :**
1. Vérifiez que .NET 6.0 est sélectionné dans IIS Settings
2. Redémarrez le site dans le panneau SmarterASP

### Erreur : "Cannot open database"

**Cause :** Mauvais paramètres de connexion SQL

**Solution :**
1. Vérifiez `appsettings.json` (Server, Database, User Id, Password)
2. Testez la connexion depuis le Query Editor SQL
3. Assurez-vous que `TrustServerCertificate=True` est présent

### Erreur : "Access to the path is denied" (TempFolder)

**Cause :** Permissions insuffisantes sur le dossier temporaire

**Solution :**
1. Changez le TempFolder vers : `D:\\Web\\VOTRE_USERNAME\\temp\\FtpBulkInsert`
2. Créez le dossier via le File Manager
3. Recompilez et redéployez

### Erreur : "Unable to connect to FTP"

**Cause :** Mauvais paramètres FTP

**Solution :**
1. Testez la connexion FTP avec FileZilla
2. Vérifiez `appsettings.json` (Server, Username, Password)
3. Assurez-vous que le chemin `/csv` existe

### L'interface web ne se charge pas

**Cause :** Fichiers statiques manquants

**Solution :**
1. Vérifiez que le dossier `wwwroot` avec `css/` et `js/` est bien uploadé
2. Vérifiez que `web.config` est présent
3. Rechargez la page en vidant le cache (Ctrl+F5)

### Les logs ne s'affichent pas dans l'interface

**Cause :** Table `UploadLog` manquante ou mal configurée

**Solution :**
1. Exécutez le script SQL de création de `UploadLog`
2. Vérifiez que la connexion SQL fonctionne
3. Regardez les erreurs dans l'onglet Console du navigateur (F12)

---

## 📞 Support

**Documentation :**
- [README.md](README.md) - Documentation complète
- [WEB_INTERFACE_GUIDE.md](WEB_INTERFACE_GUIDE.md) - Guide de l'interface
- [AUTO_CREATE_TABLES.md](AUTO_CREATE_TABLES.md) - Création automatique de tables

**SmarterASP :**
- Support : https://www.smarterasp.net/support
- Documentation : https://www.smarterasp.net/kb

**Logs applicatifs :**
- Dans le panneau SmarterASP : **Control Panel** → **Error Logs**
- Consultez les logs IIS pour identifier les erreurs

---

## 🎉 Félicitations !

Votre application FTP Bulk Insert est maintenant en production sur SmarterASP !

**Prochaines étapes :**
1. Uploadez vos fichiers CSV réels sur le FTP
2. Testez les imports via l'interface web
3. Configurez n8n pour l'automatisation
4. Surveillez les logs régulièrement
5. Créez des sauvegardes de votre base de données

**Utilisation quotidienne :**
- Accès à l'interface : `https://votre-site.smarterasp.net/manage`
- Les fichiers CSV déposés sur FTP seront importés automatiquement (si n8n configuré)
- En cas de problème, utilisez l'interface web pour relancer manuellement
