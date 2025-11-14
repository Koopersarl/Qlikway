# 🤖 Création automatique de tables

## Vue d'ensemble

L'application peut **détecter automatiquement les types de colonnes** en analysant vos fichiers CSV et créer les tables SQL Server correspondantes si elles n'existent pas.

## 🔧 Configuration

Dans `appsettings.json` :

```json
{
  "AutoCreateTables": true,
  "SampleRowsForTypeDetection": 100
}
```

### Paramètres

- **`AutoCreateTables`** (boolean)
  - `true` : Crée automatiquement les tables manquantes
  - `false` : Lève une erreur si la table n'existe pas (comportement classique)
  - **Défaut** : `false`

- **`SampleRowsForTypeDetection`** (integer)
  - Nombre de lignes à analyser pour détecter les types de colonnes
  - Plus le nombre est élevé, plus la détection est précise
  - **Recommandé** : 100-200 lignes
  - **Défaut** : 100

## 🔍 Détection des types

L'algorithme analyse un échantillon de lignes et détecte intelligemment les types suivants :

### Types détectés

| Type détecté | Type SQL Server | Critères de détection |
|-------------|-----------------|----------------------|
| **Boolean** | `BIT` | Valeurs : true/false, 1/0, yes/no, oui/non (≥80% des valeurs) |
| **Integer** | `TINYINT`, `SMALLINT`, `INT`, `BIGINT` | Nombres entiers (≥90% des valeurs), taille adaptée à la valeur max |
| **Decimal** | `DECIMAL(precision, scale)` | Nombres décimaux (≥90% des valeurs), précision calculée |
| **Date** | `DATE` | Dates sans heure (≥80% des valeurs) |
| **DateTime** | `DATETIME2` | Dates avec heure (≥80% des valeurs) |
| **String** | `NVARCHAR(50)` à `NVARCHAR(MAX)` | Tout le reste, taille adaptée à la longueur max |

### Formats de dates supportés

- `yyyy-MM-dd`
- `yyyy-MM-dd HH:mm:ss`
- `dd/MM/yyyy`
- `MM/dd/yyyy`
- `yyyy/MM/dd`
- Et autres formats via `DateTime.TryParse()`

### Tailles de VARCHAR automatiques

| Longueur max | Type SQL |
|--------------|----------|
| ≤ 50 | `NVARCHAR(50)` |
| ≤ 100 | `NVARCHAR(100)` |
| ≤ 255 | `NVARCHAR(255)` |
| ≤ 500 | `NVARCHAR(500)` |
| ≤ 1000 | `NVARCHAR(1000)` |
| ≤ 4000 | `NVARCHAR(4000)` |
| > 4000 | `NVARCHAR(MAX)` |

### Précision DECIMAL

La précision et l'échelle sont calculées automatiquement :
- **Précision** : Nombre total de chiffres (max 38)
- **Échelle** : Nombre de chiffres après la virgule
- **Défaut** : `DECIMAL(18, 2)` si non détecté

### Colonnes NULL

- Si une colonne contient des valeurs vides dans l'échantillon → `NULL`
- Sinon → `NOT NULL`

## 📝 Exemple pratique

### Fichier CSV : `Clients.csv`

```csv
ClientID,Nom,Prenom,Email,DateInscription,Actif,Solde
1,Dupont,Jean,jean.dupont@email.com,2024-01-15,true,1500.50
2,Martin,Marie,marie.martin@email.com,2024-01-16,true,2300.00
3,Bernard,Pierre,pierre.bernard@email.com,2024-01-17,false,0
```

### Table créée automatiquement

```sql
CREATE TABLE [Clients] (
    [ClientID] INT NOT NULL,
    [Nom] NVARCHAR(100) NOT NULL,
    [Prenom] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [DateInscription] DATE NOT NULL,
    [Actif] BIT NOT NULL,
    [Solde] DECIMAL(18,2) NOT NULL
)
```

### Logs de détection

```
[Information] Analyse de la structure du fichier CSV: /temp/Clients.csv
[Information] Colonnes détectées: ClientID, Nom, Prenom, Email, DateInscription, Actif, Solde
[Information] Analyse de 100 lignes d'échantillon
[Information] Colonne 'ClientID': INT
[Information] Colonne 'Nom': NVARCHAR(100)
[Information] Colonne 'Prenom': NVARCHAR(100)
[Information] Colonne 'Email': NVARCHAR(255)
[Information] Colonne 'DateInscription': DATE
[Information] Colonne 'Actif': BIT
[Information] Colonne 'Solde': DECIMAL(18,2)
[Information] Table Clients créée automatiquement avec 7 colonnes
```

## 🎯 Cas d'usage

### Scénario 1 : Nouveau fichier CSV
```
1. Vous recevez un nouveau fichier "Produits.csv"
2. Vous l'uploadez via l'interface web
3. L'application détecte que la table n'existe pas
4. Elle analyse le CSV et crée la table automatiquement
5. Elle importe les données
```

### Scénario 2 : Développement/Tests
```json
{
  "AutoCreateTables": true  // Activer pour les environnements de dev/test
}
```

### Scénario 3 : Production
```json
{
  "AutoCreateTables": false  // Désactiver en production pour contrôler le schéma
}
```

## ⚠️ Limitations et considérations

### Limitations

1. **Pas de clé primaire**
   - Les tables créées automatiquement n'ont pas de clé primaire
   - Vous devrez l'ajouter manuellement si nécessaire

2. **Pas d'index**
   - Aucun index n'est créé automatiquement
   - Ajoutez-les manuellement pour optimiser les performances

3. **Pas de contraintes**
   - Pas de contraintes de clé étrangère
   - Pas de contraintes CHECK ou UNIQUE

4. **Détection de type approximative**
   - Basée sur un échantillon (pas toutes les lignes)
   - Peut se tromper si l'échantillon n'est pas représentatif

5. **Noms de colonnes nettoyés**
   - Les caractères spéciaux sont remplacés par des underscores
   - Les espaces deviennent des underscores
   - Exemple : `"Date d'achat"` → `Date_d_achat`

### Bonnes pratiques

✅ **Recommandations :**

1. **Utilisez AutoCreateTables en développement/test**
   ```json
   {
     "AutoCreateTables": true,  // Dev/Test
     "SampleRowsForTypeDetection": 200
   }
   ```

2. **Désactivez en production**
   ```json
   {
     "AutoCreateTables": false  // Production
   }
   ```
   Et créez les tables manuellement avec :
   - Clés primaires
   - Index appropriés
   - Contraintes métier
   - Types optimisés

3. **Augmentez l'échantillon pour plus de précision**
   ```json
   {
     "SampleRowsForTypeDetection": 500  // Plus précis mais plus lent
   }
   ```

4. **Vérifiez les tables créées**
   - Inspectez la structure générée
   - Ajoutez les clés primaires
   - Optimisez les types si nécessaire

5. **Format CSV cohérent**
   - Utilisez toujours le même format de date
   - Évitez les données incohérentes dans une colonne
   - La première ligne doit contenir les noms de colonnes

## 🔧 Ajustements manuels après création

Après la création automatique, vous pouvez améliorer la table :

```sql
-- Ajouter une clé primaire
ALTER TABLE [Clients]
ADD CONSTRAINT PK_Clients PRIMARY KEY (ClientID);

-- Ajouter un index
CREATE INDEX IX_Clients_Email ON [Clients](Email);

-- Ajouter une contrainte UNIQUE
ALTER TABLE [Clients]
ADD CONSTRAINT UQ_Clients_Email UNIQUE (Email);

-- Modifier un type si nécessaire
ALTER TABLE [Clients]
ALTER COLUMN Telephone NVARCHAR(20);

-- Ajouter une valeur par défaut
ALTER TABLE [Clients]
ADD CONSTRAINT DF_Clients_Actif DEFAULT (1) FOR Actif;
```

## 🐛 Dépannage

### "Colonne détectée incorrectement"

**Problème** : Une colonne numérique est détectée comme STRING

**Cause** : L'échantillon contient des valeurs non-numériques

**Solution** :
1. Nettoyez votre fichier CSV
2. Augmentez `SampleRowsForTypeDetection`
3. Ou créez la table manuellement

### "Type DECIMAL trop petit"

**Problème** : `DECIMAL(18,2)` ne suffit pas pour vos valeurs

**Solution** :
```sql
ALTER TABLE [MaTable]
ALTER COLUMN MaColonne DECIMAL(28,4);
```

### "Échec de création de table"

**Problème** : Nom de colonne invalide

**Solution** :
- Évitez les caractères spéciaux dans les en-têtes CSV
- Utilisez des noms simples sans espaces
- L'application nettoie automatiquement mais mieux vaut prévenir

### "Performance lente"

**Problème** : L'analyse prend trop de temps

**Solution** :
```json
{
  "SampleRowsForTypeDetection": 50  // Réduire l'échantillon
}
```

## 📊 Comparaison avec création manuelle

| Aspect | Création auto | Création manuelle |
|--------|--------------|-------------------|
| **Vitesse** | ⚡ Instantanée | 🐌 Nécessite scripting |
| **Clé primaire** | ❌ Non | ✅ Oui |
| **Index** | ❌ Non | ✅ Oui |
| **Contraintes** | ❌ Non | ✅ Oui |
| **Types optimisés** | ⚠️ Approximatifs | ✅ Précis |
| **Dev/Test** | ✅ Idéal | ⚠️ Fastidieux |
| **Production** | ⚠️ Déconseillé | ✅ Recommandé |

## 🎓 Workflow recommandé

### Phase 1 : Développement
```
1. Activez AutoCreateTables = true
2. Uploadez vos fichiers CSV de test
3. Les tables sont créées automatiquement
4. Testez l'import de données
```

### Phase 2 : Validation
```
1. Inspectez les tables créées
2. Notez les ajustements nécessaires
3. Créez les scripts SQL manuels optimisés
```

### Phase 3 : Production
```
1. Désactivez AutoCreateTables = false
2. Exécutez vos scripts SQL manuels
3. Les imports utilisent les tables optimisées
```

## 📚 Ressources

- [README.md](README.md) - Documentation générale
- [DEPLOYMENT.md](DEPLOYMENT.md) - Guide de déploiement
- [WEB_INTERFACE_GUIDE.md](WEB_INTERFACE_GUIDE.md) - Guide de l'interface
- [SQL/CreateUploadLogTable.sql](SQL/CreateUploadLogTable.sql) - Exemple de création de table
