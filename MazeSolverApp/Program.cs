using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MazeSolverApp;
using MazeSolverApp.Services;

static class Program
{
    static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConsole();
            }) 
            .AddSingleton<IDataService>(provider =>
            {
                Environment.SetEnvironmentVariable("BASE_URL", "https://mazegame.plingot.com/");
                // Optionally configure the HttpClient for future test 
                var httpClient = new HttpClient();
                var logger = provider.GetRequiredService<ILogger<DataService>>(); // Get ILogger<DataService> from the service provider

                return new DataService(httpClient, logger);
            })
            .AddSingleton<MazeSolver>()
            .BuildServiceProvider();

        var mazeSolver = serviceProvider.GetService<MazeSolver>();

        Console.WriteLine(await mazeSolver.FindExitAsync() ? "Exit found!" : "Exit not found.");
    }


}