# 📜 Scripts de compilation et déploiement

Ce dossier contient les scripts pour compiler et préparer l'application pour le déploiement sur SmarterASP.

---

## 🖥️ Scripts disponibles

### Windows : `compile-and-prepare.ps1`
Script PowerShell pour compiler l'application sur Windows

### Mac/Linux : `compile-and-prepare.sh`
Script Bash pour compiler l'application sur Mac ou Linux

---

## 🚀 Utilisation

### Windows (PowerShell)

1. Ouvrez PowerShell dans le dossier racine du projet
2. Exécutez le script :
   ```powershell
   .\Scripts\compile-and-prepare.ps1
   ```

**Si vous avez une erreur de politique d'exécution :**
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\Scripts\compile-and-prepare.ps1
```

### Mac/Linux (Terminal)

1. Ouvrez Terminal dans le dossier racine du projet
2. Rendez le script exécutable (première fois seulement) :
   ```bash
   chmod +x Scripts/compile-and-prepare.sh
   ```
3. Exécutez le script :
   ```bash
   ./Scripts/compile-and-prepare.sh
   ```

---

## 📦 Ce que fait le script

1. **Nettoyage**
   - Supprime les anciennes compilations (`publish`, `bin`, `obj`)

2. **Compilation**
   - Compile l'application en mode Release
   - Place les fichiers dans le dossier `publish`

3. **Vérifications**
   - Vérifie que `appsettings.json` est présent
   - Vérifie que les paramètres sont configurés
   - Affiche des avertissements si des valeurs par défaut sont détectées

4. **Instructions**
   - Affiche les prochaines étapes pour le déploiement

---

## ✅ Résultat attendu

Après l'exécution du script, vous devriez voir :

```
========================================
  Compilation pour SmarterASP
========================================

🧹 Nettoyage des anciennes compilations...

🔨 Compilation de l'application en mode Release...
[...sortie de dotnet publish...]

✅ Compilation réussie!

📦 X fichiers générés dans le dossier 'publish'

📄 Fichiers principaux:
   - FtpBulkInsert.dll
   - Microsoft.Data.SqlClient.dll
   - ...

⚠️  IMPORTANT: Vérification de appsettings.json

✅ Configuration semble correcte

========================================
  Prochaines étapes
========================================

1. Vérifiez/Modifiez les paramètres dans:
   publish/appsettings.json

2. Uploadez TOUT le contenu du dossier 'publish' vers:
   /wwwroot sur votre serveur SmarterASP via FTP

3. Testez votre application:
   https://votre-site.smarterasp.net/api/bulkinsert/health

4. Accédez à l'interface de gestion:
   https://votre-site.smarterasp.net/manage

🎉 Bon déploiement!
```

---

## ⚠️ Avertissements courants

### ❌ Connexion SQL Server non configurée

Cela signifie que `appsettings.json` contient encore les valeurs par défaut comme `YOUR_SQL_SERVER`.

**Solution :** Éditez `appsettings.json` avant de compiler

### ❌ Serveur FTP non configuré

Les paramètres FTP dans `appsettings.json` n'ont pas été modifiés.

**Solution :** Éditez `appsettings.json` avant de compiler

### ⚠️ Clé API non changée

La clé API par défaut `YOUR_SECRET_API_KEY_HERE` est toujours présente.

**Solution :** Générez une clé API sécurisée et mettez-la dans `appsettings.json`

---

## 📁 Structure du dossier publish

Après compilation, le dossier `publish` contient :

```
publish/
├── FtpBulkInsert.dll                    (application principale)
├── appsettings.json                     (⚠️ CONTIENT VOS PARAMÈTRES)
├── appsettings.Development.json
├── web.config                           (configuration IIS)
├── Pages/                               (pages Razor)
├── wwwroot/                             (fichiers statiques)
│   ├── css/
│   │   └── manage.css
│   └── js/
│       └── manage.js
├── Microsoft.Data.SqlClient.dll         (dépendance SQL)
├── ... (autres DLLs et fichiers)
└── ... (~50-100 fichiers au total)
```

**⚠️ IMPORTANT :** Uploadez **TOUT** le contenu de ce dossier vers `/wwwroot` sur SmarterASP

---

## 🔄 Modification de la configuration après compilation

Si vous devez modifier `appsettings.json` après la compilation :

1. Éditez directement `publish/appsettings.json`
2. Pas besoin de recompiler
3. Uploadez le fichier modifié vers SmarterASP

**OU**

1. Modifiez `appsettings.json` à la racine du projet
2. Relancez le script de compilation
3. Uploadez tout le dossier `publish`

---

## 🆘 Dépannage

### "dotnet: command not found"

**Cause :** .NET SDK n'est pas installé

**Solution :**
- Téléchargez et installez .NET SDK 6.0 : https://dotnet.microsoft.com/download
- Redémarrez votre terminal
- Réessayez

### "FtpBulkInsert.csproj non trouvé"

**Cause :** Le script n'est pas exécuté depuis le bon dossier

**Solution :**
- Naviguez vers le dossier racine du projet (celui qui contient `FtpBulkInsert.csproj`)
- Réexécutez le script depuis ce dossier

### Erreur de compilation

**Cause :** Problème dans le code source ou dépendances manquantes

**Solution :**
- Lisez le message d'erreur complet
- Vérifiez que tous les fichiers sont présents
- Essayez : `dotnet restore` puis relancez le script

### Le dossier publish est vide

**Cause :** La compilation a échoué silencieusement

**Solution :**
- Exécutez manuellement : `dotnet publish -c Release -o ./publish`
- Vérifiez les erreurs affichées

---

## 📚 Documentation complémentaire

- **Guide complet de déploiement :** `../PRODUCTION_DEPLOYMENT_GUIDE.md`
- **Checklist de déploiement :** `../DEPLOYMENT_CHECKLIST.md`
- **Documentation générale :** `../README.md`

---

## 💡 Conseils

1. **Vérifiez toujours** `appsettings.json` avant de compiler
2. **Testez localement** avant de déployer en production
3. **Gardez une copie** de `appsettings.json` (sans les mots de passe) pour référence
4. **Documentez vos paramètres** pour faciliter les futurs déploiements
5. **Créez un backup** de votre configuration avant chaque mise à jour

---

## 🔐 Sécurité

⚠️ **NE COMMITEZ JAMAIS `appsettings.json` avec vos vrais mots de passe dans Git !**

- `appsettings.json` est déjà dans `.gitignore`
- Utilisez `appsettings.Production.json.TEMPLATE` comme modèle
- Gardez vos mots de passe dans un gestionnaire de mots de passe sécurisé
