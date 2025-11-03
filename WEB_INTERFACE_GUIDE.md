# 🌐 Guide de l'interface web

## Accès à l'interface

URL : `https://votre-site.com/manage` ou simplement `https://votre-site.com/`

L'interface de gestion est accessible sans authentification (pas de clé API requise), ce qui la rend pratique pour une utilisation manuelle rapide.

## 📖 Vue d'ensemble

L'interface est divisée en 3 sections principales :

### 1. 📤 Upload manuel de fichiers CSV

**Cas d'usage :**
- Importer rapidement des fichiers CSV sans passer par le FTP
- Tester l'import d'un fichier spécifique
- Import d'urgence en cas de problème avec le FTP

**Comment utiliser :**
1. Cliquez sur le champ "Sélectionner des fichiers CSV"
2. Sélectionnez un ou plusieurs fichiers `.csv` depuis votre ordinateur
3. Cliquez sur "Uploader et importer"
4. L'application va automatiquement :
   - Uploader les fichiers
   - Vider les tables correspondantes
   - Effectuer le BULK INSERT
   - Afficher le résultat

**Résultat :**
- ✅ Succès : Affiche le nombre de lignes insérées pour chaque fichier
- ❌ Échec : Affiche l'erreur détaillée pour chaque fichier
- L'historique est automatiquement rafraîchi

### 2. 🔄 Déclencher l'import FTP

**Cas d'usage :**
- Le déclencheur n8n ne fonctionne pas
- Vous voulez lancer l'import manuellement à une heure spécifique
- Test de l'import FTP

**Comment utiliser :**
1. (Optionnel) Cochez "Supprimer les fichiers du FTP après import"
2. Cliquez sur "Lancer l'import FTP"
3. L'application va :
   - Se connecter au serveur FTP
   - Lister tous les fichiers `.csv`
   - Les télécharger un par un
   - Effectuer le BULK INSERT pour chaque fichier
   - (Si coché) Supprimer les fichiers du FTP

**Options :**
- **Supprimer les fichiers du FTP** : Recommandé si vous ne voulez pas réimporter les mêmes données
- **Ne pas supprimer** : Utile pour les tests ou si vous voulez conserver les fichiers sources

### 3. 📋 Historique des imports

**Affichage automatique :**
L'historique se charge automatiquement à l'ouverture de la page et se rafraîchit après chaque opération.

**Informations affichées :**
- **Date** : Date et heure de l'import
- **Fichier** : Nom du fichier CSV importé
- **Table** : Nom de la table SQL Server
- **Lignes** : Nombre de lignes insérées
- **Durée** : Temps d'exécution de l'opération
- **Statut** :
  - 🟢 **Succès** : Import réussi
  - 🔴 **Échec** : Erreur lors de l'import (avec message d'erreur)

**Actions :**
- Cliquez sur "🔄 Rafraîchir" pour actualiser l'historique

## 🎨 Fonctionnalités de l'interface

### Interface responsive
- S'adapte automatiquement aux téléphones, tablettes et ordinateurs
- Design moderne avec dégradés et animations

### Feedback en temps réel
- Spinners pendant le traitement
- Messages de succès/erreur avec détails
- Mise à jour automatique de l'historique

### Gestion des erreurs
- Affichage clair des erreurs pour chaque fichier
- Messages d'erreur détaillés pour faciliter le débogage

## 🔧 Cas d'utilisation pratiques

### Scénario 1 : Import d'urgence
```
Problème : n8n ne fonctionne pas et vous avez besoin d'importer les données maintenant
Solution :
1. Accédez à /manage
2. Cliquez sur "Lancer l'import FTP"
3. Vérifiez l'historique pour confirmer le succès
```

### Scénario 2 : Test d'un nouveau fichier
```
Besoin : Tester un nouveau fichier CSV avant de l'ajouter au processus automatique
Solution :
1. Accédez à /manage
2. Utilisez "Upload manuel"
3. Sélectionnez votre fichier de test
4. Vérifiez le résultat immédiatement
```

### Scénario 3 : Corriger une erreur
```
Problème : Un fichier a échoué dans l'import automatique
Solution :
1. Consultez l'historique pour identifier l'erreur
2. Corrigez le fichier CSV
3. Utilisez "Upload manuel" pour réimporter le fichier corrigé
```

### Scénario 4 : Import partiel
```
Besoin : Importer seulement certains fichiers parmi ceux disponibles sur FTP
Solution :
1. Téléchargez les fichiers spécifiques depuis le FTP
2. Utilisez "Upload manuel" pour les importer un par un
3. Vérifiez l'historique après chaque import
```

## 🚨 Points importants

### Sécurité
- ⚠️ L'interface web ne nécessite pas d'authentification
- Recommandation : Restreindre l'accès à `/manage` via les règles IIS si nécessaire
- L'API REST reste protégée par la clé API

### Limitations
- Upload manuel : Limité par la taille maximale de fichier configurée dans IIS (par défaut 500MB)
- Les mêmes règles s'appliquent que pour l'import FTP :
  - Le nom du fichier doit correspondre au nom de la table
  - L'ordre des colonnes doit correspondre
  - La première ligne (en-têtes) est ignorée

### Performance
- Les fichiers sont traités séquentiellement, pas en parallèle
- Pour de gros volumes, préférez l'import FTP automatique via n8n

## 📱 Navigation

- **Page d'accueil** : `/` → Redirige vers `/manage`
- **Interface de gestion** : `/manage`
- **Documentation API** : `/swagger` (en développement uniquement)
- **Health check** : `/api/bulkinsert/health`

## 💡 Conseils

1. **Gardez un onglet ouvert** sur `/manage` pendant vos imports automatiques pour surveiller l'activité
2. **Rafraîchissez régulièrement** l'historique pour voir les nouveaux imports
3. **Notez les erreurs** affichées dans l'historique pour corriger vos fichiers CSV
4. **Testez d'abord** avec "Upload manuel" avant d'ajouter de nouveaux fichiers au FTP
5. **Vérifiez toujours** l'historique après un import pour confirmer le succès

## 🆘 Dépannage

### L'interface ne se charge pas
- Vérifiez que l'application est déployée correctement
- Vérifiez les logs IIS
- Testez le health check : `/api/bulkinsert/health`

### "Aucun log disponible"
- La table `UploadLog` n'existe peut-être pas
- Vérifiez que vous avez exécuté le script `SQL/CreateUploadLogTable.sql`
- Vérifiez la connexion à la base de données

### Erreur lors de l'upload
- Vérifiez que le fichier est bien au format CSV
- Vérifiez que la table correspondante existe
- Consultez le message d'erreur détaillé dans le résultat

### Import FTP ne trouve aucun fichier
- Vérifiez les paramètres FTP dans `appsettings.json`
- Vérifiez que le dossier FTP contient bien des fichiers `.csv`
- Testez la connexion FTP avec un client FTP (FileZilla)

## 🔗 Liens utiles

- [README.md](README.md) - Documentation complète
- [DEPLOYMENT.md](DEPLOYMENT.md) - Guide de déploiement
- [SQL/CreateUploadLogTable.sql](SQL/CreateUploadLogTable.sql) - Script de création de table
