using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace LanesBackendLauncher.Util;

public class DatabaseBackupSettings
{
  public string BucketName { get; set; } = string.Empty;
  public string DatabaseFilePath { get; set; } = string.Empty;
  public string DatabaseBackupFilePath { get; set; } = string.Empty;
  public string KeyPrefix { get; set; } = string.Empty;
}

public class DatabaseBackupService(
  ILogger<DatabaseBackupSettings> _logger,
  IAmazonS3 _s3Client,
  IOptions<DatabaseBackupSettings> _settings
) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        _logger.LogInformation("Starting file upload to S3 at {Time}", DateTime.UtcNow);
        await UploadFileToS3Async();
        _logger.LogInformation("File uploaded successfully at {Time}", DateTime.UtcNow);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while uploading the file to S3");
      }

      await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
    }
  }

  private async Task UploadFileToS3Async()
  {
    await CreateDatabaseBackupAsync(
      _settings.Value.DatabaseFilePath,
      _settings.Value.DatabaseBackupFilePath
    );

    var request = new PutObjectRequest
    {
      BucketName = _settings.Value.BucketName,
      Key = _settings.Value.KeyPrefix + DateTime.UtcNow.ToString("o") + ".db",
      FilePath = _settings.Value.DatabaseBackupFilePath
    };

    request.Metadata.Add("x-amz-meta-expiration", DateTime.UtcNow.AddMonths(1).ToString("o"));

    try
    {
      await _s3Client.PutObjectAsync(request);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error uploading file to S3");
      throw;
    }
    finally
    {
      File.Delete(_settings.Value.DatabaseBackupFilePath);
    }
  }

  private async Task CreateDatabaseBackupAsync(string sourcePath, string backupPath)
  {
    string connectionString = $"Data Source={sourcePath};";
    using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $"VACUUM INTO '{backupPath}';";
    await command.ExecuteNonQueryAsync();
  }
}
