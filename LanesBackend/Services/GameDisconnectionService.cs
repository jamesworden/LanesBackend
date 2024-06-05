namespace LanesBackend.Services;

public class GameDisconnectionService : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await Task.Delay(30 * 1000);
  }
}
