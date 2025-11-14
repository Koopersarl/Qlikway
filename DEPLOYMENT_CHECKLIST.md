# ✅ Checklist de déploiement SmarterASP

Utilisez cette checklist pour vous assurer que votre déploiement est complet.

---

## 📋 AVANT LA COMPILATION

### Informations à récupérer

- [ ] Informations SQL Server (depuis panneau SmarterASP → Databases)
  - [ ] Server: `sql___.smarterasp.net`
  - [ ] Database: `DB______`
  - [ ] Username: `DB______user`
  - [ ] Password: `**********`

- [ ] Informations FTP (depuis panneau SmarterASP → FTP)
  - [ ] FTP Server: `ftp___.smarterasp.net`
  - [ ] Username: `__________`
  - [ ] Password: `**********`

- [ ] Clé API sécurisée générée (min. 20 caractères)
  - [ ] Clé API: `______________________`

---

## 🔧 CONFIGURATION

### appsettings.json

- [ ] Fichier `appsettings.json` ouvert
- [ ] ConnectionStrings → DefaultConnection configuré
  - [ ] Server remplacé
  - [ ] Database remplacé
  - [ ] User Id remplacé
  - [ ] Password remplacé
  - [ ] `TrustServerCertificate=True` présent
- [ ] FtpSettings → Server configuré
- [ ] FtpSettings → Username configuré
- [ ] FtpSettings → Password configuré
- [ ] FtpSettings → Path = `/csv`
- [ ] TempFolder = `D:\\Web\\temp\\FtpBulkInsert`
- [ ] ApiKey changée (différente de `YOUR_SECRET_API_KEY_HERE`)
- [ ] AutoCreateTables configuré (`true` ou `false`)
- [ ] SampleRowsForTypeDetection = `100`

---

## 🔨 COMPILATION

### Exécuter le script de compilation

**Windows :**
- [ ] Ouvrir PowerShell dans le dossier du projet
- [ ] Exécuter: `.\Scripts\compile-and-prepare.ps1`
- [ ] ✅ "Compilation réussie!" affiché
- [ ] ⚠️  Aucun avertissement de configuration

**Mac/Linux :**
- [ ] Ouvrir Terminal dans le dossier du projet
- [ ] Rendre le script exécutable: `chmod +x Scripts/compile-and-prepare.sh`
- [ ] Exécuter: `./Scripts/compile-and-prepare.sh`
- [ ] ✅ "Compilation réussie!" affiché
- [ ] ⚠️  Aucun avertissement de configuration

### Vérifier le dossier publish

- [ ] Dossier `publish` créé
- [ ] Fichier `publish/FtpBulkInsert.dll` présent
- [ ] Fichier `publish/appsettings.json` présent avec VOS paramètres
- [ ] Fichier `publish/web.config` présent
- [ ] Dossier `publish/wwwroot/` présent (avec css/ et js/)
- [ ] Environ 50-100 fichiers au total

---

## 🗄️ BASE DE DONNÉES

### Créer la table UploadLog

- [ ] Connecté au Query Editor SQL (panneau SmarterASP)
- [ ] Script `SQL/CreateUploadLogTable.sql` copié
- [ ] Script exécuté avec succès
- [ ] Message "Table UploadLog créée avec succès" affiché
- [ ] Vérification: `SELECT * FROM UploadLog` ne retourne pas d'erreur

### Créer les tables de données (si AutoCreateTables = false)

- [ ] Liste des tables nécessaires identifiée
- [ ] Scripts SQL de création préparés
- [ ] Chaque table créée avec succès
- [ ] Ordre des colonnes = ordre dans les CSV

---

## 📁 FTP

### Configuration du serveur FTP

- [ ] Connecté au FTP (FileZilla ou File Manager SmarterASP)
- [ ] Dossier `/csv` créé à la racine
- [ ] Permissions du dossier vérifiées (lecture/écriture)
- [ ] Fichiers CSV de test uploadés dans `/csv`

---

## 📤 UPLOAD DE L'APPLICATION

### Upload vers SmarterASP

- [ ] Connecté au FTP SmarterASP
- [ ] Navigué vers `/wwwroot`
- [ ] Ancien contenu de `/wwwroot` supprimé (si applicable)
- [ ] TOUT le contenu de `publish/` uploadé vers `/wwwroot`
- [ ] Upload terminé à 100%
- [ ] Vérification: `FtpBulkInsert.dll` présent dans `/wwwroot`
- [ ] Vérification: `appsettings.json` présent dans `/wwwroot`
- [ ] Vérification: `web.config` présent dans `/wwwroot`
- [ ] Vérification: Dossier `wwwroot/` présent dans `/wwwroot`

---

## ⚙️ CONFIGURATION IIS

### Paramètres SmarterASP

- [ ] Connecté au panneau de contrôle SmarterASP
- [ ] IIS Settings → ASP.NET Version = `.NET 6.0` (ou supérieur)
- [ ] Paramètres sauvegardés
- [ ] Dossier temporaire créé: `D:\Web\temp\FtpBulkInsert`

---

## 🧪 TESTS

### Test 1: Health Check

- [ ] Navigateur ouvert
- [ ] URL testée: `https://votre-site.smarterasp.net/api/bulkinsert/health`
- [ ] Réponse JSON reçue:
  ```json
  {
    "status": "healthy",
    "timestamp": "...",
    "version": "1.0.0"
  }
  ```

### Test 2: Interface Web

- [ ] URL testée: `https://votre-site.smarterasp.net/manage`
- [ ] Interface de gestion affichée correctement
- [ ] Section "Upload manuel" visible
- [ ] Section "Déclencher l'import FTP" visible
- [ ] Section "Historique des imports" visible

### Test 3: Upload Manuel

- [ ] Fichier CSV de test préparé (2-3 lignes)
- [ ] Fichier uploadé via l'interface `/manage`
- [ ] Bouton "Uploader et importer" cliqué
- [ ] Message de succès affiché
- [ ] Nombre de lignes insérées correct
- [ ] Aucune erreur affichée

### Test 4: Vérification des Logs

- [ ] Query Editor SQL ouvert
- [ ] Requête exécutée: `SELECT TOP 10 * FROM UploadLog ORDER BY UploadDate DESC`
- [ ] Import de test visible dans les logs
- [ ] Colonne `Success` = 1 (true)
- [ ] Colonne `RowsInserted` correcte

### Test 5: Import FTP (si configuré)

- [ ] Fichiers CSV placés dans `/csv` sur le FTP
- [ ] Bouton "Lancer l'import FTP" cliqué dans `/manage`
- [ ] Message de succès affiché
- [ ] Tous les fichiers traités
- [ ] Logs mis à jour dans `UploadLog`

---

## 🤖 n8n (OPTIONNEL)

### Configuration du workflow

- [ ] n8n ouvert
- [ ] Nouveau workflow créé
- [ ] Nœud Schedule Trigger ajouté
  - [ ] Cron Expression: `0 6 * * *`
- [ ] Nœud HTTP Request ajouté
  - [ ] Method: POST
  - [ ] URL: `https://votre-site.smarterasp.net/api/bulkinsert/process?deleteAfterImport=false`
  - [ ] Header X-API-Key configuré avec votre clé
- [ ] (Optionnel) Nœud Send Email ajouté
- [ ] Workflow testé manuellement
- [ ] Test réussi
- [ ] Workflow activé

---

## 📊 SURVEILLANCE

### Vérifications post-déploiement

- [ ] **Jour 1:** Vérifier que l'application répond
- [ ] **Jour 1:** Vérifier les logs d'erreur IIS (panneau SmarterASP)
- [ ] **Jour 2:** Vérifier que les imports automatiques fonctionnent
- [ ] **Semaine 1:** Consulter `UploadLog` régulièrement
- [ ] **Semaine 1:** Vérifier l'utilisation du TempFolder
- [ ] **Mensuel:** Nettoyer les vieux logs si nécessaire

---

## 📝 DOCUMENTATION

### Documenter votre configuration

- [ ] Sauvegarder une copie de `appsettings.json` (sans mots de passe) localement
- [ ] Noter l'URL de l'application: `https://____________.smarterasp.net`
- [ ] Noter la clé API utilisée (dans un gestionnaire de mots de passe)
- [ ] Documenter la structure des fichiers CSV attendus
- [ ] Créer un document listant les tables et leur correspondance avec les CSV

---

## ✅ VALIDATION FINALE

- [ ] ✅ Health check fonctionnel
- [ ] ✅ Interface web accessible
- [ ] ✅ Upload manuel testé et fonctionnel
- [ ] ✅ Logs enregistrés correctement
- [ ] ✅ Import FTP testé (si applicable)
- [ ] ✅ n8n configuré et testé (si applicable)
- [ ] ✅ Aucune erreur dans les logs IIS
- [ ] ✅ Documentation complète

---

## 🎉 DÉPLOIEMENT TERMINÉ !

Félicitations ! Votre application FTP Bulk Insert est maintenant en production.

**Prochaines étapes :**
1. Commencez à utiliser l'application pour vos imports réels
2. Surveillez les logs pendant la première semaine
3. Optimisez les performances si nécessaire
4. Créez des sauvegardes régulières de votre base de données

**En cas de problème :**
- Consultez `PRODUCTION_DEPLOYMENT_GUIDE.md` section "Dépannage"
- Vérifiez les logs IIS dans le panneau SmarterASP
- Consultez les logs d'erreur dans la Console du navigateur (F12)
- Vérifiez que `appsettings.json` est correct

**Support :**
- Documentation complète dans `README.md`
- Interface web : `WEB_INTERFACE_GUIDE.md`
- Création auto de tables : `AUTO_CREATE_TABLES.md`
- Support SmarterASP : https://www.smarterasp.net/support
