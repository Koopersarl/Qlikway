#!/bin/bash

# Script Bash pour compiler et préparer le déploiement
# Usage: ./Scripts/compile-and-prepare.sh

echo "========================================"
echo "  Compilation pour SmarterASP"
echo "========================================"
echo ""

# Vérifier que nous sommes dans le bon dossier
if [ ! -f "FtpBulkInsert.csproj" ]; then
    echo "❌ Erreur: FtpBulkInsert.csproj non trouvé"
    echo "   Exécutez ce script depuis le dossier racine du projet"
    exit 1
fi

# Nettoyer les anciennes compilations
echo "🧹 Nettoyage des anciennes compilations..."
rm -rf publish
rm -rf bin
rm -rf obj

# Compiler l'application
echo ""
echo "🔨 Compilation de l'application en mode Release..."
dotnet publish -c Release -o ./publish

if [ $? -ne 0 ]; then
    echo ""
    echo "❌ Erreur lors de la compilation"
    exit 1
fi

echo ""
echo "✅ Compilation réussie!"
echo ""

# Vérifier que appsettings.json est présent
if [ ! -f "publish/appsettings.json" ]; then
    echo "❌ Erreur: appsettings.json manquant dans le dossier publish"
    exit 1
fi

# Compter les fichiers
fileCount=$(find publish -type f | wc -l)
echo "📦 $fileCount fichiers générés dans le dossier 'publish'"
echo ""

# Afficher les fichiers importants
echo "📄 Fichiers principaux:"
ls -1 publish/*.dll 2>/dev/null | head -5 | while read file; do
    echo "   - $(basename $file)"
done
echo "   ..."
echo ""

# Vérifier appsettings.json
echo "⚠️  IMPORTANT: Vérification de appsettings.json"
echo ""

warnings=0

# Vérifier si les valeurs par défaut sont présentes
if grep -q "YOUR_SQL_SERVER\|YOUR_DATABASE\|YOUR_USERNAME" publish/appsettings.json; then
    echo "   ❌ Connexion SQL Server non configurée"
    warnings=$((warnings+1))
fi

if grep -q "YOUR_FTP_SERVER\|YOUR_FTP_USERNAME" publish/appsettings.json; then
    echo "   ❌ Serveur FTP non configuré"
    warnings=$((warnings+1))
fi

if grep -q "YOUR_SECRET_API_KEY" publish/appsettings.json; then
    echo "   ⚠️  Clé API non changée (utilisez une clé sécurisée)"
    warnings=$((warnings+1))
fi

if [ $warnings -gt 0 ]; then
    echo ""
    echo "📝 Éditez publish/appsettings.json avant de déployer!"
    echo ""
else
    echo "✅ Configuration semble correcte"
    echo ""
fi

# Instructions de déploiement
echo "========================================"
echo "  Prochaines étapes"
echo "========================================"
echo ""
echo "1. Vérifiez/Modifiez les paramètres dans:"
echo "   publish/appsettings.json"
echo ""
echo "2. Uploadez TOUT le contenu du dossier 'publish' vers:"
echo "   /wwwroot sur votre serveur SmarterASP via FTP"
echo ""
echo "3. Testez votre application:"
echo "   https://votre-site.smarterasp.net/api/bulkinsert/health"
echo ""
echo "4. Accédez à l'interface de gestion:"
echo "   https://votre-site.smarterasp.net/manage"
echo ""
echo "📚 Consultez PRODUCTION_DEPLOYMENT_GUIDE.md pour plus de détails"
echo ""
echo "🎉 Bon déploiement!"
