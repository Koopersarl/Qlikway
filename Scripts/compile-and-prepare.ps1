# Script PowerShell pour compiler et préparer le déploiement
# Usage: .\Scripts\compile-and-prepare.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Compilation pour SmarterASP" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier que nous sommes dans le bon dossier
if (-not (Test-Path "FtpBulkInsert.csproj")) {
    Write-Host "❌ Erreur: FtpBulkInsert.csproj non trouvé" -ForegroundColor Red
    Write-Host "   Exécutez ce script depuis le dossier racine du projet" -ForegroundColor Yellow
    exit 1
}

# Nettoyer les anciennes compilations
Write-Host "🧹 Nettoyage des anciennes compilations..." -ForegroundColor Yellow
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}

# Compiler l'application
Write-Host ""
Write-Host "🔨 Compilation de l'application en mode Release..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Erreur lors de la compilation" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Compilation réussie!" -ForegroundColor Green
Write-Host ""

# Vérifier que appsettings.json est présent
if (-not (Test-Path "publish/appsettings.json")) {
    Write-Host "❌ Erreur: appsettings.json manquant dans le dossier publish" -ForegroundColor Red
    exit 1
}

# Compter les fichiers
$fileCount = (Get-ChildItem -Recurse "publish").Count
Write-Host "📦 $fileCount fichiers générés dans le dossier 'publish'" -ForegroundColor Cyan
Write-Host ""

# Afficher les fichiers importants
Write-Host "📄 Fichiers principaux:" -ForegroundColor Cyan
Get-ChildItem "publish" -Filter "*.dll" | Select-Object -First 5 | ForEach-Object {
    Write-Host "   - $($_.Name)" -ForegroundColor Gray
}
Write-Host "   ..." -ForegroundColor Gray
Write-Host ""

# Vérifier appsettings.json
Write-Host "⚠️  IMPORTANT: Vérification de appsettings.json" -ForegroundColor Yellow
Write-Host ""

$appsettings = Get-Content "publish/appsettings.json" -Raw | ConvertFrom-Json

$warnings = @()

# Vérifier la connexion SQL
if ($appsettings.ConnectionStrings.DefaultConnection -like "*YOUR_SQL_SERVER*" -or
    $appsettings.ConnectionStrings.DefaultConnection -like "*YOUR_DATABASE*") {
    $warnings += "   ❌ Connexion SQL Server non configurée"
}

# Vérifier FTP
if ($appsettings.FtpSettings.Server -like "*YOUR_FTP_SERVER*") {
    $warnings += "   ❌ Serveur FTP non configuré"
}

# Vérifier ApiKey
if ($appsettings.ApiKey -like "*YOUR_SECRET_API_KEY*") {
    $warnings += "   ⚠️  Clé API non changée (utilisez une clé sécurisée)"
}

if ($warnings.Count -gt 0) {
    Write-Host "⚠️  Avertissements de configuration:" -ForegroundColor Yellow
    $warnings | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    Write-Host ""
    Write-Host "📝 Éditez publish/appsettings.json avant de déployer!" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "✅ Configuration semble correcte" -ForegroundColor Green
    Write-Host ""
}

# Instructions de déploiement
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Prochaines étapes" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Vérifiez/Modifiez les paramètres dans:" -ForegroundColor White
Write-Host "   publish/appsettings.json" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Uploadez TOUT le contenu du dossier 'publish' vers:" -ForegroundColor White
Write-Host "   /wwwroot sur votre serveur SmarterASP via FTP" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Testez votre application:" -ForegroundColor White
Write-Host "   https://votre-site.smarterasp.net/api/bulkinsert/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Accédez à l'interface de gestion:" -ForegroundColor White
Write-Host "   https://votre-site.smarterasp.net/manage" -ForegroundColor Cyan
Write-Host ""
Write-Host "📚 Consultez PRODUCTION_DEPLOYMENT_GUIDE.md pour plus de détails" -ForegroundColor Yellow
Write-Host ""
Write-Host "🎉 Bon déploiement!" -ForegroundColor Green
