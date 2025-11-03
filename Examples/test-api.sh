#!/bin/bash

# Script Bash pour tester l'API FTP Bulk Insert

# Configuration
API_URL="https://votre-site.smarterasp.net"
API_KEY="VotreCleAPI"

echo "=== Test de l'API FTP Bulk Insert ==="

# Test 1 : Health Check
echo -e "\n1. Test du endpoint health..."
response=$(curl -s -w "\n%{http_code}" "$API_URL/api/bulkinsert/health")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" -eq 200 ]; then
    echo "   ✓ Service opérationnel (HTTP $http_code)"
    echo "   Réponse: $body"
else
    echo "   ✗ Erreur HTTP $http_code"
fi

# Test 2 : Process sans API Key (doit échouer)
echo -e "\n2. Test sans API Key (doit échouer)..."
response=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/bulkinsert/process")
http_code=$(echo "$response" | tail -n1)

if [ "$http_code" -eq 401 ]; then
    echo "   ✓ Authentification requise (comportement attendu)"
else
    echo "   ? Code HTTP inattendu: $http_code"
fi

# Test 3 : Process avec API Key
echo -e "\n3. Test du traitement avec API Key..."
response=$(curl -s -w "\n%{http_code}" \
    -X POST "$API_URL/api/bulkinsert/process?deleteAfterImport=false" \
    -H "X-API-Key: $API_KEY")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" -eq 200 ]; then
    echo "   ✓ Traitement effectué (HTTP $http_code)"
    echo "   Réponse: $body"
else
    echo "   ✗ Erreur HTTP $http_code"
    echo "   Réponse: $body"
fi

echo -e "\n=== Tests terminés ==="
