# ⚠️ Configuration du TempFolder pour SmarterASP (Hébergement Mutualisé)

## 🔍 Le problème

Sur un **hébergement mutualisé** SmarterASP, vous n'avez **pas accès** au chemin `D:\Web\temp\`. Ce chemin est réservé aux VPS ou serveurs dédiés.

## ✅ La solution pour l'hébergement mutualisé

### Structure typique SmarterASP mutualisé

```
D:\Web\VOTRE_USERNAME\
├── wwwroot\              (votre site web - accessible via HTTP)
│   └── (fichiers de l'application)
├── temp\                 (dossier temporaire - NON accessible via HTTP)
├── logs\                 (logs - NON accessible via HTTP)
└── (autres dossiers)
```

### Option 1 : Utiliser wwwroot/temp (RECOMMANDÉ pour SmarterASP mutualisé)

**Configuration dans `appsettings.json` :**

```json
{
  "TempFolder": "temp"
}
```

**Ou chemin relatif :**
```json
{
  "TempFolder": "./temp"
}
```

L'application créera automatiquement un dossier `temp` dans le dossier de l'application (`/wwwroot`).

**Structure résultante :**
```
/wwwroot/
├── FtpBulkInsert.dll
├── appsettings.json
├── web.config
├── temp/                  ← Dossier créé automatiquement
│   ├── Clients.csv
│   └── Commandes.csv
└── ...
```

### Option 2 : Utiliser le dossier temporaire Windows

**Configuration dans `appsettings.json` :**

```json
{
  "TempFolder": ""
}
```

Avec une chaîne vide, l'application utilisera `Path.GetTempPath()` qui pointe vers le dossier temporaire Windows du serveur.

Sur SmarterASP, cela pointera généralement vers :
- `C:\Windows\Temp\` ou
- `C:\Users\[service_account]\AppData\Local\Temp\`

⚠️ **Attention :** Ce dossier est partagé par tous les processus et peut poser des problèmes de permissions.

## 🔧 Comment trouver le bon chemin sur SmarterASP ?

### Méthode 1 : Via le File Manager SmarterASP

1. Connectez-vous au panneau de contrôle SmarterASP
2. Allez dans **File Manager**
3. Observez la barre d'adresse, vous verrez quelque chose comme :
   ```
   D:\Web\username123\wwwroot
   ```
4. Votre username est donc `username123`

### Méthode 2 : Créer une page de test

Créez un fichier `test-path.html` dans `/wwwroot` :

```html
<!DOCTYPE html>
<html>
<body>
    <h1>Test du chemin</h1>
    <p>Chemin actuel : <%=Server.MapPath("~")%></p>
</body>
</html>
```

Accédez à `https://votre-site.smarterasp.net/test-path.html` pour voir le chemin complet.

### Méthode 3 : Via l'API de diagnostic (à ajouter temporairement)

Ajoutez cet endpoint temporaire dans votre application :

```csharp
[HttpGet("diagnostic/paths")]
public IActionResult GetPaths()
{
    return Ok(new
    {
        CurrentDirectory = Directory.GetCurrentDirectory(),
        TempPath = Path.GetTempPath(),
        BaseDirectory = AppDomain.CurrentDomain.BaseDirectory
    });
}
```

Accédez à `/api/bulkinsert/diagnostic/paths` après déploiement.

## 📝 Configuration recommandée pour SmarterASP mutualisé

### appsettings.json

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
    "DefaultConnection": "Server=sql123.smarterasp.net;Database=DB_123456_votredb;User Id=DB_123456_user;Password=VOTRE_PASSWORD;TrustServerCertificate=True;Encrypt=True;"
  },
  "FtpSettings": {
    "Server": "ftp://ftp123.smarterasp.net",
    "Username": "votre-username-ftp",
    "Password": "VOTRE_PASSWORD_FTP",
    "Path": "/csv"
  },
  "TempFolder": "temp",
  "ApiKey": "VOTRE_CLE_SECURISEE_123!",
  "AutoCreateTables": true,
  "SampleRowsForTypeDetection": 100
}
```

**Point clé :** `"TempFolder": "temp"` (chemin relatif)

## ⚙️ Modification du code pour supporter les chemins relatifs

Le code actuel dans `FtpService.cs` et `UploadController.cs` devrait déjà gérer cela, mais vérifiez :

```csharp
_tempFolder = configuration["TempFolder"] ?? Path.Combine(Path.GetTempPath(), "FtpBulkInsert");

// Créer le dossier temporaire s'il n'existe pas
if (!Directory.Exists(_tempFolder))
{
    Directory.CreateDirectory(_tempFolder);
}
```

Ce code créera automatiquement le dossier s'il n'existe pas.

## 🧪 Test après déploiement

### Test 1 : Vérifier que le dossier est créé

1. Uploadez un fichier CSV via l'interface `/manage`
2. Via le File Manager SmarterASP, vérifiez que le dossier `/wwwroot/temp` a été créé
3. Vérifiez que le fichier CSV y a été copié temporairement
4. Après l'import, le fichier doit être supprimé automatiquement

### Test 2 : Permissions d'écriture

Si vous obtenez une erreur "Access denied", cela signifie que l'application n'a pas les permissions d'écriture.

**Solutions :**

1. **Via le panneau SmarterASP :**
   - Allez dans **File Manager**
   - Créez manuellement le dossier `temp` dans `/wwwroot`
   - Les permissions devraient être correctes par défaut

2. **Utiliser un chemin absolu (si vous connaissez votre username) :**
   ```json
   {
     "TempFolder": "D:\\Web\\VOTRE_USERNAME\\temp"
   }
   ```

3. **Contactez le support SmarterASP** pour vérifier les permissions

## 🔐 Sécurité : Bloquer l'accès HTTP au dossier temp

**IMPORTANT :** Le dossier `temp` dans `/wwwroot` sera accessible via HTTP par défaut.

### Protection via web.config

Ajoutez cette section dans votre `web.config` :

```xml
<configuration>
  <!-- ... configuration existante ... -->

  <location path="temp">
    <system.webServer>
      <security>
        <authorization>
          <remove users="*" roles="" verbs="" />
          <add accessType="Deny" users="*" />
        </authorization>
      </security>
    </system.webServer>
  </location>
</configuration>
```

Ou créez un fichier `wwwroot/temp/web.config` :

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <security>
      <authorization>
        <remove users="*" roles="" verbs="" />
        <add accessType="Deny" users="*" />
      </authorization>
    </security>
  </system.webServer>
</configuration>
```

Cela bloquera l'accès HTTP à `https://votre-site.com/temp/`.

## 📊 Tableau comparatif des options

| Option | Chemin | Avantages | Inconvénients |
|--------|--------|-----------|---------------|
| **Chemin relatif "temp"** | `/wwwroot/temp/` | ✅ Simple<br>✅ Fonctionne partout<br>✅ Auto-création | ⚠️ Accessible HTTP (si non protégé)<br>⚠️ Dans wwwroot |
| **Chemin vide ""** | Temp Windows | ✅ Non accessible HTTP<br>✅ Pas de configuration | ⚠️ Partagé<br>⚠️ Peut avoir des problèmes de permissions |
| **Chemin absolu avec username** | `D:\Web\username\temp` | ✅ Hors de wwwroot<br>✅ Non accessible HTTP | ⚠️ Faut connaître username<br>⚠️ Peut nécessiter création manuelle |

## ✅ Recommandation finale pour SmarterASP mutualisé

**Configuration recommandée :**

```json
{
  "TempFolder": "temp"
}
```

**Avec protection dans web.config :**

```xml
<location path="temp">
  <system.webServer>
    <security>
      <authorization>
        <add accessType="Deny" users="*" />
      </authorization>
    </security>
  </system.webServer>
</location>
```

**Pourquoi ?**
- ✅ Fonctionne immédiatement sans configuration complexe
- ✅ Création automatique du dossier
- ✅ Permissions correctes par défaut
- ✅ Protégé via web.config
- ✅ Nettoyage automatique des fichiers après traitement

## 🆘 Dépannage

### Erreur : "Access to the path is denied"

**Solution 1 :** Utilisez un chemin relatif simple
```json
{ "TempFolder": "temp" }
```

**Solution 2 :** Créez manuellement le dossier via File Manager

**Solution 3 :** Contactez le support SmarterASP pour les permissions

### Les fichiers ne sont pas supprimés du dossier temp

**C'est normal si :**
- Il y a eu une erreur pendant le traitement
- L'application a crashé

**Solution :**
- Ajoutez un job de nettoyage périodique (via n8n)
- Ou nettoyez manuellement via File Manager

### Erreur : "Could not find a part of the path"

**Cause :** Le chemin spécifié n'existe pas et ne peut pas être créé

**Solution :**
- Utilisez un chemin relatif : `"temp"`
- Ou créez le dossier manuellement avant le déploiement

## 📝 Mise à jour des fichiers de configuration

Mettez à jour votre `appsettings.json` avant de compiler :

```json
{
  "TempFolder": "temp"
}
```

Puis recompilez :
```bash
.\Scripts\compile-and-prepare.ps1
```

Et redéployez le contenu du dossier `publish`.
