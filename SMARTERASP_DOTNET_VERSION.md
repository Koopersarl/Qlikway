# 🔧 Configuration de .NET sur SmarterASP - Guide complet

## 📍 Où trouver la configuration .NET sur SmarterASP

### Option 1 : Via le Control Panel (le plus courant)

1. Connectez-vous à votre compte SmarterASP
2. Allez dans **Control Panel**
3. Cherchez l'une de ces sections :
   - **Application Settings** ou
   - **ASP.NET Configuration** ou
   - **Website Settings** ou
   - **Advanced Settings**

4. Cherchez une option nommée :
   - **ASP.NET Version** ou
   - **.NET Framework Version** ou
   - **Runtime Version**

### Option 2 : Via le File Manager (web.config)

Si vous ne trouvez pas l'option dans le Control Panel, c'est peut-être géré automatiquement via `web.config`.

---

## ⚠️ Limitations importantes sur SmarterASP

### Plans d'hébergement

SmarterASP propose différents plans, et **tous ne supportent pas .NET Core/.NET 6.0+** :

| Plan | .NET Framework | .NET Core/6.0+ |
|------|---------------|----------------|
| **Basic/Starter** | ✅ 4.x | ❌ Non disponible |
| **Advanced/Plus** | ✅ 4.x | ⚠️ Peut-être |
| **Premium/Enterprise** | ✅ 4.x | ✅ Oui |

### Vérifier votre plan

1. Allez dans **Control Panel** → **Account Information** ou **Subscription**
2. Vérifiez le nom de votre plan
3. Consultez la page : https://www.smarterasp.net/hosting-plans

---

## 🔍 Comment vérifier si .NET Core est disponible

### Méthode 1 : Vérifier dans le Control Panel

Cherchez dans les paramètres une option qui mentionne :
- **.NET Core**
- **.NET 5.0** ou **6.0** ou **7.0** ou **8.0**
- **ASP.NET Core Runtime**

Si vous ne voyez que :
- **.NET Framework 4.8**
- **.NET Framework 4.7**
- **.NET Framework 2.0-4.0**

→ Votre plan **ne supporte probablement pas .NET Core**

### Méthode 2 : Contacter le support SmarterASP

Ouvrez un ticket de support et demandez :
```
Bonjour,

Je souhaite déployer une application ASP.NET Core 6.0.
Mon plan actuel supporte-t-il .NET Core / .NET 6.0 ?
Si non, quel plan dois-je choisir pour avoir ce support ?

Merci.
```

### Méthode 3 : Tester le déploiement

1. Déployez l'application telle quelle
2. Accédez à : `https://votre-site.smarterasp.net/api/bulkinsert/health`

**Si ça fonctionne :** ✅ .NET 6.0 est supporté (détection automatique)

**Si vous avez une erreur 500.x :** ❌ .NET 6.0 n'est pas supporté

---

## ✅ Solutions selon votre situation

### Situation 1 : .NET Core est disponible mais pas visible dans le Control Panel

**C'est le cas le plus fréquent sur SmarterASP Premium/Enterprise**

La version .NET est **détectée automatiquement** via le fichier `web.config` et `FtpBulkInsert.runtimeconfig.json`.

**Aucune action requise !** Déployez simplement l'application.

Le fichier `web.config` contient déjà la bonne configuration :
```xml
<aspNetCore processPath="dotnet"
            arguments=".\FtpBulkInsert.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess" />
```

### Situation 2 : .NET Core n'est pas disponible sur votre plan

**Options :**

#### Option A : Upgrader votre plan SmarterASP

1. Allez dans **Control Panel** → **Upgrade**
2. Choisissez un plan qui supporte .NET Core (généralement Premium ou supérieur)
3. Coût typique : ~15-30$/mois selon le plan

#### Option B : Migrer vers un hébergeur compatible .NET Core

Hébergeurs recommandés pour ASP.NET Core :
- **Azure App Service** (Microsoft, excellent pour .NET)
- **AWS Elastic Beanstalk**
- **DigitalOcean App Platform**
- **Heroku** (avec buildpack .NET)
- **SmarteASP plan Premium** (si upgrade possible)

#### Option C : Utiliser un hébergement Windows VPS

- **SmarterASP VPS** : https://www.smarterasp.net/vps
- **Contabo Windows VPS** : ~5-10€/mois
- **OVH Windows VPS**
- **Azure VM**

#### Option D : Recompiler l'application en .NET Framework 4.8

⚠️ **Nécessite de modifier le code** car ASP.NET Core et .NET Framework sont différents.

**Non recommandé** pour cette application car elle utilise des fonctionnalités spécifiques à .NET Core.

---

## 🔧 Configuration automatique via web.config

SmarterASP utilise le **ASP.NET Core Module (ANCM)** qui détecte automatiquement la version via :

1. **`web.config`** (déjà inclus) :
```xml
<aspNetCore processPath="dotnet"
            arguments=".\FtpBulkInsert.dll"
            hostingModel="inprocess" />
```

2. **`FtpBulkInsert.runtimeconfig.json`** (généré automatiquement à la compilation) :
```json
{
  "runtimeOptions": {
    "tfm": "net6.0",
    "framework": {
      "name": "Microsoft.AspNetCore.App",
      "version": "6.0.0"
    }
  }
}
```

Si ces fichiers sont présents (ce qui est le cas après compilation), **IIS/ANCM sélectionnera automatiquement .NET 6.0**.

---

## 🧪 Test de compatibilité

### Étape 1 : Déployez l'application

Suivez le guide normal de déploiement.

### Étape 2 : Testez le health check

```
https://votre-site.smarterasp.net/api/bulkinsert/health
```

### Résultats possibles

#### ✅ Succès - Réponse JSON
```json
{
  "status": "healthy",
  "timestamp": "2024-11-18T...",
  "version": "1.0.0"
}
```
→ **Tout fonctionne !** .NET 6.0 est supporté et actif.

#### ❌ Erreur 500.30 - ANCM In-Process Start Failure
```
HTTP Error 500.30 - ASP.NET Core app failed to start
```
→ .NET Core Runtime n'est pas installé sur le serveur.

**Solutions :**
1. Contactez le support SmarterASP
2. Ou upgradez votre plan

#### ❌ Erreur 500.31 - ANCM Failed to Find Native Dependencies
```
HTTP Error 500.31 - Failed to load ASP.NET Core runtime
```
→ Le runtime .NET 6.0 n'est pas disponible.

**Solution :** Upgrade du plan ou changement d'hébergeur.

#### ❌ Erreur 404
```
404 - File or directory not found
```
→ L'application n'est pas déployée correctement ou le routing ne fonctionne pas.

**Solution :** Vérifiez que tous les fichiers sont bien uploadés dans `/wwwroot`.

---

## 📞 Contacter le support SmarterASP

Si vous ne trouvez pas l'option, **contactez le support** :

**Via le portail :**
1. https://www.smarterasp.net/support
2. Cliquez sur **Submit a Ticket**
3. Catégorie : **Technical Support**

**Message type :**
```
Subject: .NET Core 6.0 Support - Configuration

Bonjour,

Je souhaite déployer une application ASP.NET Core 6.0 sur mon hébergement.

Questions :
1. Mon plan actuel (précisez votre plan) supporte-t-il .NET Core 6.0 ?
2. Si oui, comment configurer la version .NET dans le Control Panel ?
   Je ne trouve pas d'option pour sélectionner .NET Core.
3. Si non, quel plan recommandez-vous pour .NET Core 6.0 ?

Informations :
- Nom de domaine : votresite.com
- Plan actuel : [votre plan]
- Application : ASP.NET Core 6.0 Web API

Merci de votre aide.
```

---

## 🎯 Recommandations

### Si vous avez un plan Basic/Starter

**Vous devrez probablement upgrader** vers un plan Premium qui supporte .NET Core.

**Coût estimé :**
- Basic → Premium : +10-20$/mois
- Vérifiez les prix exacts sur : https://www.smarterasp.net/hosting-plans

### Si vous avez un plan Premium/Enterprise

**Vous avez probablement déjà le support .NET Core.**

**Pas de configuration manuelle nécessaire** - déployez simplement et testez !

### Si le support n'est pas disponible

**Alternatives rapides :**
1. **Azure App Service** :
   - Free tier disponible pour les tests
   - Excellent support .NET
   - URL : https://azure.microsoft.com/en-us/pricing/details/app-service/

2. **DigitalOcean App Platform** :
   - 5$/mois pour les apps simples
   - Support natif .NET Core
   - URL : https://www.digitalocean.com/products/app-platform

---

## ✅ Checklist de vérification

- [ ] J'ai vérifié mon plan d'hébergement actuel
- [ ] J'ai cherché "ASP.NET Configuration" dans le Control Panel
- [ ] J'ai cherché ".NET Core" ou ".NET 6.0" dans les options
- [ ] J'ai déployé l'application pour tester
- [ ] J'ai testé `/api/bulkinsert/health`
- [ ] Si erreur 500.30/500.31 : Je dois upgrader ou changer d'hébergeur
- [ ] Si succès : Tout fonctionne, pas de configuration nécessaire !

---

## 📚 Ressources SmarterASP

- **Plans d'hébergement :** https://www.smarterasp.net/hosting-plans
- **Knowledge Base :** https://www.smarterasp.net/kb
- **Support Ticket :** https://www.smarterasp.net/support
- **FAQ ASP.NET Core :** https://www.smarterasp.net/kb/search?q=asp.net+core

---

## 🔍 TL;DR (Résumé)

**Question :** Où configurer .NET 6.0 sur SmarterASP ?

**Réponse courte :**
1. Sur les plans **Premium/Enterprise**, c'est **automatique** via `web.config`
2. Sur les plans **Basic/Starter**, .NET Core **n'est pas supporté**
3. **Testez** en déployant et en accédant à `/api/bulkinsert/health`
4. Si erreur 500.30, **contactez le support** ou **upgradez votre plan**

**Pas de panique :** La plupart du temps, **aucune configuration manuelle n'est nécessaire** si votre plan supporte .NET Core !
