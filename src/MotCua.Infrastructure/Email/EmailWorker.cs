using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotCua.Domain.Interfaces;

namespace MotCua.Infrastructure.Email;

public class EmailWorker : BackgroundService
{
    private readonly IServiceScopeFactory  _scopeFactory;
    private readonly ILogger<EmailWorker>  _logger;
    private static readonly TimeSpan       _interval = TimeSpan.FromSeconds(30);

    public EmailWorker(IServiceScopeFactory scopeFactory, ILogger<EmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailWorker started — polling every {s}s", _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessAsync(stoppingToken); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { _logger.LogError(ex, "EmailWorker: unexpected error"); }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        await using var scope  = _scopeFactory.CreateAsyncScope();
        var repo   = scope.ServiceProvider.GetRequiredService<ITinNhanRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var pending = await repo.LayChuaGuiAsync(50, ct);
        if (pending.Count == 0) return;

        _logger.LogInformation("EmailWorker: {n} email(s) pending", pending.Count);

        foreach (var msg in pending)
        {
            msg.SoLanThu++;
            try
            {
                await sender.GuiAsync(msg.ToEmail, msg.Subject, msg.Body, ct);
                msg.DaGui  = true;
                msg.GuiLuc = DateTime.UtcNow;
                msg.LoiGui = null;
                _logger.LogInformation("EmailWorker: sent to {email}", msg.ToEmail);
            }
            catch (Exception ex)
            {
                msg.LoiGui = ex.Message.Length > 490 ? ex.Message[..490] : ex.Message;
                _logger.LogWarning("EmailWorker: failed id={id} attempt={n}: {err}", msg.Id, msg.SoLanThu, msg.LoiGui);
            }
            await repo.CapNhatAsync(msg, ct);
        }
    }
}
