// Gestion de l'interface d'administration

// Charger les logs au démarrage
document.addEventListener('DOMContentLoaded', () => {
    loadLogs();
});

// Upload manuel de fichiers CSV
document.getElementById('uploadForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const fileInput = document.getElementById('csvFiles');
    const files = fileInput.files;

    if (files.length === 0) {
        showResult('uploadResult', 'Veuillez sélectionner au moins un fichier', 'error');
        return;
    }

    // Préparer les données
    const formData = new FormData();
    for (let i = 0; i < files.length; i++) {
        formData.append('files', files[i]);
    }

    // Afficher le spinner
    setButtonLoading('uploadBtnText', 'uploadSpinner', true);
    document.getElementById('uploadResult').classList.add('hidden');

    try {
        const response = await fetch('/api/upload/manual', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (response.ok && result.success) {
            showUploadResult(result);
        } else {
            showResult('uploadResult', `Erreur: ${result.errorMessage || 'Erreur inconnue'}`, 'error');
        }

        // Rafraîchir les logs
        loadLogs();

        // Réinitialiser le formulaire
        fileInput.value = '';

    } catch (error) {
        showResult('uploadResult', `Erreur de connexion: ${error.message}`, 'error');
    } finally {
        setButtonLoading('uploadBtnText', 'uploadSpinner', false);
    }
});

// Déclencher l'import FTP
document.getElementById('ftpTriggerBtn').addEventListener('click', async () => {
    const deleteAfterImport = document.getElementById('deleteAfterImport').checked;

    // Afficher le spinner
    setButtonLoading('ftpBtnText', 'ftpSpinner', true);
    document.getElementById('ftpResult').classList.add('hidden');

    try {
        const response = await fetch(`/api/bulkinsert/process?deleteAfterImport=${deleteAfterImport}`, {
            method: 'POST'
        });

        const result = await response.json();

        if (response.ok && result.success) {
            showFtpResult(result);
        } else {
            showResult('ftpResult', `Erreur: ${result.errorMessage || 'Erreur inconnue'}`, 'error');
        }

        // Rafraîchir les logs
        loadLogs();

    } catch (error) {
        showResult('ftpResult', `Erreur de connexion: ${error.message}`, 'error');
    } finally {
        setButtonLoading('ftpBtnText', 'ftpSpinner', false);
    }
});

// Rafraîchir les logs
document.getElementById('refreshLogsBtn').addEventListener('click', () => {
    loadLogs();
});

// Fonction pour afficher le résultat de l'upload
function showUploadResult(result) {
    let message = `✅ Upload terminé: ${result.totalFilesProcessed} fichier(s), ${result.totalRowsInserted} ligne(s) insérée(s)`;

    let detailsHtml = '';
    if (result.results && result.results.length > 0) {
        detailsHtml = '<div class="result-details">';
        result.results.forEach(r => {
            const status = r.success ? 'success' : 'error';
            const icon = r.success ? '✓' : '✗';
            const rows = r.success ? `${r.rowsInserted} lignes` : 'Échec';
            const error = r.errorMessage ? ` - ${r.errorMessage}` : '';

            detailsHtml += `
                <div class="result-item ${status}">
                    ${icon} <strong>${r.tableName}</strong>: ${rows}${error}
                </div>
            `;
        });
        detailsHtml += '</div>';
    }

    showResult('uploadResult', message + detailsHtml, 'success');
}

// Fonction pour afficher le résultat FTP
function showFtpResult(result) {
    if (result.totalFilesProcessed === 0) {
        showResult('ftpResult', '⚠️ Aucun fichier CSV trouvé sur le serveur FTP', 'warning');
        return;
    }

    let message = `✅ Import FTP terminé: ${result.totalFilesProcessed} fichier(s), ${result.totalRowsInserted} ligne(s) insérée(s)`;

    let detailsHtml = '';
    if (result.results && result.results.length > 0) {
        detailsHtml = '<div class="result-details">';
        result.results.forEach(r => {
            const status = r.success ? 'success' : 'error';
            const icon = r.success ? '✓' : '✗';
            const rows = r.success ? `${r.rowsInserted} lignes` : 'Échec';
            const error = r.errorMessage ? ` - ${r.errorMessage}` : '';

            detailsHtml += `
                <div class="result-item ${status}">
                    ${icon} <strong>${r.tableName}</strong>: ${rows}${error}
                </div>
            `;
        });
        detailsHtml += '</div>';
    }

    showResult('ftpResult', message + detailsHtml, 'success');
}

// Fonction pour afficher un résultat
function showResult(elementId, message, type) {
    const resultBox = document.getElementById(elementId);
    resultBox.innerHTML = message;
    resultBox.className = `result-box ${type}`;
    resultBox.classList.remove('hidden');
}

// Fonction pour gérer l'état de chargement des boutons
function setButtonLoading(textId, spinnerId, loading) {
    const textElement = document.getElementById(textId);
    const spinnerElement = document.getElementById(spinnerId);

    if (loading) {
        textElement.classList.add('hidden');
        spinnerElement.classList.remove('hidden');
        // Désactiver le bouton parent
        spinnerElement.closest('button').disabled = true;
    } else {
        textElement.classList.remove('hidden');
        spinnerElement.classList.add('hidden');
        // Réactiver le bouton parent
        spinnerElement.closest('button').disabled = false;
    }
}

// Fonction pour charger les logs
async function loadLogs() {
    const container = document.getElementById('logsContainer');
    container.innerHTML = '<div class="loading">Chargement des logs...</div>';

    try {
        const response = await fetch('/api/upload/logs?count=50');

        if (!response.ok) {
            throw new Error('Impossible de charger les logs');
        }

        const logs = await response.json();

        if (logs.length === 0) {
            container.innerHTML = '<div class="no-logs">Aucun log disponible</div>';
            return;
        }

        // Créer la table
        let tableHtml = `
            <table class="log-table">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Fichier</th>
                        <th>Table</th>
                        <th>Lignes</th>
                        <th>Durée</th>
                        <th>Statut</th>
                    </tr>
                </thead>
                <tbody>
        `;

        logs.forEach(log => {
            const date = new Date(log.uploadDate).toLocaleString('fr-FR');
            const duration = formatDuration(log.duration);
            const statusClass = log.success ? 'success' : 'error';
            const statusText = log.success ? 'Succès' : 'Échec';
            const errorInfo = log.errorMessage ? `<br><small style="color: #dc3545;">${log.errorMessage}</small>` : '';

            tableHtml += `
                <tr>
                    <td>${date}</td>
                    <td>${log.fileName}</td>
                    <td><strong>${log.tableName}</strong></td>
                    <td>${log.rowsInserted.toLocaleString()}</td>
                    <td>${duration}</td>
                    <td>
                        <span class="status-badge ${statusClass}">${statusText}</span>
                        ${errorInfo}
                    </td>
                </tr>
            `;
        });

        tableHtml += `
                </tbody>
            </table>
        `;

        container.innerHTML = tableHtml;

    } catch (error) {
        container.innerHTML = `<div class="no-logs">Erreur: ${error.message}</div>`;
    }
}

// Fonction pour formater la durée
function formatDuration(durationString) {
    // durationString est au format "HH:MM:SS.mmmmmmm"
    const parts = durationString.split(':');
    const hours = parseInt(parts[0]);
    const minutes = parseInt(parts[1]);
    const seconds = parseFloat(parts[2]);

    if (hours > 0) {
        return `${hours}h ${minutes}m ${seconds.toFixed(1)}s`;
    } else if (minutes > 0) {
        return `${minutes}m ${seconds.toFixed(1)}s`;
    } else {
        return `${seconds.toFixed(2)}s`;
    }
}
