using FtpBulkInsert.Models;
using FtpBulkInsert.Services;
using Microsoft.AspNetCore.Mvc;

namespace FtpBulkInsert.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BulkInsertController : ControllerBase
    {
        private readonly IFtpService _ftpService;
        private readonly IBulkInsertService _bulkInsertService;
        private readonly ILogService _logService;
        private readonly ILogger<BulkInsertController> _logger;
        private readonly string? _apiKey;

        public BulkInsertController(
            IFtpService ftpService,
            IBulkInsertService bulkInsertService,
            ILogService logService,
            IConfiguration configuration,
            ILogger<BulkInsertController> logger)
        {
            _ftpService = ftpService;
            _bulkInsertService = bulkInsertService;
            _logService = logService;
            _logger = logger;
            _apiKey = configuration["ApiKey"];
        }

        /// <summary>
        /// Endpoint principal pour déclencher le processus de BULK INSERT depuis FTP
        /// </summary>
        /// <param name="apiKey">Clé API pour l'authentification</param>
        /// <param name="deleteAfterImport">Supprimer les fichiers du FTP après import (défaut: false)</param>
        /// <returns>Résultat du traitement</returns>
        [HttpPost("process")]
        public async Task<ActionResult<ProcessResult>> ProcessFtpFiles(
            [FromHeader(Name = "X-API-Key")] string? apiKey,
            [FromQuery] bool deleteAfterImport = false)
        {
            var processResult = new ProcessResult();

            try
            {
                // Vérification de la clé API si configurée
                if (!string.IsNullOrEmpty(_apiKey) && apiKey != _apiKey)
                {
                    _logger.LogWarning("Tentative d'accès avec une clé API invalide");
                    return Unauthorized(new { message = "Clé API invalide" });
                }

                _logger.LogInformation("Début du traitement des fichiers FTP");

                // Lister les fichiers CSV sur le serveur FTP
                var csvFiles = await _ftpService.ListCsvFilesAsync();

                if (csvFiles.Count == 0)
                {
                    _logger.LogInformation("Aucun fichier CSV trouvé sur le serveur FTP");
                    processResult.Success = true;
                    processResult.ErrorMessage = "Aucun fichier CSV trouvé";
                    return Ok(processResult);
                }

                _logger.LogInformation($"Traitement de {csvFiles.Count} fichiers CSV");

                // Traiter chaque fichier
                foreach (var fileName in csvFiles)
                {
                    try
                    {
                        _logger.LogInformation($"Traitement du fichier: {fileName}");

                        // Télécharger le fichier
                        var localPath = await _ftpService.DownloadFileAsync(fileName);

                        // Effectuer le BULK INSERT
                        var result = await _bulkInsertService.ProcessFileAsync(fileName, localPath);

                        // Enregistrer le log
                        await _logService.LogUploadAsync(result);

                        // Ajouter aux résultats
                        processResult.Results.Add(result);

                        if (result.Success)
                        {
                            processResult.TotalRowsInserted += result.RowsInserted;

                            // Supprimer le fichier du FTP si demandé
                            if (deleteAfterImport)
                            {
                                await _ftpService.DeleteFileAsync(fileName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erreur lors du traitement du fichier {fileName}");

                        var errorResult = new BulkInsertResult
                        {
                            FileName = fileName,
                            TableName = Path.GetFileNameWithoutExtension(fileName),
                            Success = false,
                            ErrorMessage = ex.Message
                        };

                        processResult.Results.Add(errorResult);
                        await _logService.LogUploadAsync(errorResult);
                    }
                }

                processResult.TotalFilesProcessed = csvFiles.Count;
                processResult.Success = processResult.Results.Any(r => r.Success);

                _logger.LogInformation($"Traitement terminé: {processResult.TotalFilesProcessed} fichiers, {processResult.TotalRowsInserted} lignes insérées");

                return Ok(processResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur globale lors du traitement");
                processResult.Success = false;
                processResult.ErrorMessage = ex.Message;
                return StatusCode(500, processResult);
            }
        }

        /// <summary>
        /// Endpoint de vérification de l'état du service
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.Now,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Endpoint pour obtenir les derniers logs
        /// </summary>
        [HttpGet("logs")]
        public async Task<ActionResult> GetRecentLogs(
            [FromHeader(Name = "X-API-Key")] string? apiKey,
            [FromQuery] int count = 50)
        {
            try
            {
                // Vérification de la clé API si configurée
                if (!string.IsNullOrEmpty(_apiKey) && apiKey != _apiKey)
                {
                    return Unauthorized(new { message = "Clé API invalide" });
                }

                var logs = await _logService.GetRecentLogsAsync(count);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des logs");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
