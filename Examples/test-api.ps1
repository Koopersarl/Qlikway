# Script PowerShell pour tester l'API FTP Bulk Insert

# Configuration
$apiUrl = "https://votre-site.smarterasp.net"
$apiKey = "VotreCleAPI"

Write-Host "=== Test de l'API FTP Bulk Insert ===" -ForegroundColor Cyan

# Test 1 : Health Check
Write-Host "`n1. Test du endpoint health..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$apiUrl/api/bulkinsert/health" -Method Get
    Write-Host "   ✓ Service opérationnel" -ForegroundColor Green
    Write-Host "   Status: $($healthResponse.status)" -ForegroundColor Gray
    Write-Host "   Version: $($healthResponse.version)" -ForegroundColor Gray
    Write-Host "   Timestamp: $($healthResponse.timestamp)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ Erreur: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2 : Process sans API Key (doit échouer)
Write-Host "`n2. Test sans API Key (doit échouer)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/api/bulkinsert/process" -Method Post
    Write-Host "   ✗ Erreur: L'API aurait dû rejeter la requête" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "   ✓ Authentification requise (comportement attendu)" -ForegroundColor Green
    } else {
        Write-Host "   ? Erreur inattendue: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Test 3 : Process avec API Key
Write-Host "`n3. Test du traitement avec API Key..." -ForegroundColor Yellow
try {
    $headers = @{
        "X-API-Key" = $apiKey
    }

    $response = Invoke-RestMethod -Uri "$apiUrl/api/bulkinsert/process?deleteAfterImport=false" `
                                   -Method Post `
                                   -Headers $headers

    Write-Host "   ✓ Traitement effectué" -ForegroundColor Green
    Write-Host "   Succès: $($response.success)" -ForegroundColor Gray
    Write-Host "   Fichiers traités: $($response.totalFilesProcessed)" -ForegroundColor Gray
    Write-Host "   Lignes insérées: $($response.totalRowsInserted)" -ForegroundColor Gray

    if ($response.results.Count -gt 0) {
        Write-Host "`n   Détails:" -ForegroundColor Cyan
        foreach ($result in $response.results) {
            Write-Host "   - $($result.tableName): $($result.rowsInserted) lignes en $($result.duration)" -ForegroundColor Gray
            if (-not $result.success) {
                Write-Host "     Erreur: $($result.errorMessage)" -ForegroundColor Red
            }
        }
    }
} catch {
    Write-Host "   ✗ Erreur: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Tests terminés ===" -ForegroundColor Cyan
