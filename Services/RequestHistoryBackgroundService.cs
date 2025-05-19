using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Request.Services
{
    public class RequestHistoryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RequestHistoryBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var requestHistoryService = scope.ServiceProvider.GetRequiredService<RequestHistoryService>();
                    await requestHistoryService.CheckOverdueRequests();
                }

                // Kiểm tra mỗi giờ
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
} 