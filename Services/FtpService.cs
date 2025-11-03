using System.Net;
using System.Text;

namespace FtpBulkInsert.Services
{
    public class FtpService : IFtpService
    {
        private readonly string _ftpServer;
        private readonly string _ftpUsername;
        private readonly string _ftpPassword;
        private readonly string _ftpPath;
        private readonly string _tempFolder;
        private readonly ILogger<FtpService> _logger;

        public FtpService(IConfiguration configuration, ILogger<FtpService> logger)
        {
            _ftpServer = configuration["FtpSettings:Server"] ?? throw new ArgumentNullException("FtpSettings:Server");
            _ftpUsername = configuration["FtpSettings:Username"] ?? throw new ArgumentNullException("FtpSettings:Username");
            _ftpPassword = configuration["FtpSettings:Password"] ?? throw new ArgumentNullException("FtpSettings:Password");
            _ftpPath = configuration["FtpSettings:Path"] ?? "/";
            _tempFolder = configuration["TempFolder"] ?? Path.Combine(Path.GetTempPath(), "FtpBulkInsert");
            _logger = logger;

            // Créer le dossier temporaire s'il n'existe pas
            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
        }

        public async Task<List<string>> ListCsvFilesAsync()
        {
            var csvFiles = new List<string>();

            try
            {
                var ftpUrl = $"{_ftpServer}{_ftpPath}";
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                request.UseBinary = true;
                request.UsePassive = true;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        csvFiles.Add(line);
                        _logger.LogInformation($"Fichier CSV trouvé: {line}");
                    }
                }

                _logger.LogInformation($"Total de {csvFiles.Count} fichiers CSV trouvés sur le serveur FTP");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la liste des fichiers FTP");
                throw;
            }

            return csvFiles;
        }

        public async Task<string> DownloadFileAsync(string fileName)
        {
            try
            {
                var ftpUrl = $"{_ftpServer}{_ftpPath}/{fileName}";
                var localPath = Path.Combine(_tempFolder, fileName);

                _logger.LogInformation($"Téléchargement de {fileName} depuis {ftpUrl}");

                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
                request.UseBinary = true;
                request.UsePassive = true;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var responseStream = response.GetResponseStream();
                using var fileStream = new FileStream(localPath, FileMode.Create);

                await responseStream.CopyToAsync(fileStream);

                _logger.LogInformation($"Fichier téléchargé avec succès: {localPath}");
                return localPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du téléchargement du fichier {fileName}");
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var ftpUrl = $"{_ftpServer}{_ftpPath}/{fileName}";

                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

                using var response = (FtpWebResponse)await request.GetResponseAsync();

                _logger.LogInformation($"Fichier supprimé du FTP: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Impossible de supprimer le fichier {fileName} du FTP");
                // Ne pas lancer l'exception pour ne pas bloquer le processus
            }
        }
    }
}
