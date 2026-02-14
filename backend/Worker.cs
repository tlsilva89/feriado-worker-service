using feriado.Data;
using feriado.Models;
using feriado.Services;
using Microsoft.EntityFrameworkCore;

namespace feriado;

public class Worker(IServiceProvider serviceProvider, HolidayService holidayService, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Verificando feriados (Nacionais e SP)... {time}", DateTimeOffset.Now);
                }

                var holidays = await holidayService.GetHolidaysAsync();

                if (holidays.Count > 0)
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        int addedCount = 0;
                        foreach (var date in holidays)
                        {
                            var exists = await dbContext.Feriados.AnyAsync(f => f.Data == date, stoppingToken);
                            if (!exists)
                            {
                                dbContext.Feriados.Add(new Feriado { Data = date });
                                addedCount++;
                            }
                        }

                        if (addedCount > 0)
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                            logger.LogInformation("{Count} novos feriados adicionados.", addedCount);
                        }
                        else
                        {
                            logger.LogInformation("Nenhum feriado novo encontrado.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar feriados. Verifique se o banco e a tabela 'feriados' existem.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}