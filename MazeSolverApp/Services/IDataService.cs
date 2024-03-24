using System;
using System.Net.Http;
using System.Threading.Tasks;
using MazeSolverApp.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MazeSolverApp.Services
{
    public interface IDataService
    {
        Task<GameData> StartGameAsync();
        Task<RoomData> GetCurrentRoomAsync();
        Task<bool> MovePlayerAsync(string direction);
    }

    public class DataService : IDataService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<DataService> _logger;

        public DataService(HttpClient httpClient, ILogger<DataService> logger)
        {
            _httpClient = httpClient;
            _baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
            _logger = logger;
        }

        public async Task<GameData> StartGameAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}Game/start", null);
                response.EnsureSuccessStatusCode(); 
                var result = await response.Content.ReadAsStringAsync();
                var gameData = JsonConvert.DeserializeObject<GameData>(result);
                var _token = gameData.Token;

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                return gameData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the game.");
                throw; 
            }
        }

        public async Task<RoomData> GetCurrentRoomAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Room/current");
                response.EnsureSuccessStatusCode(); 
                var roomData = JsonConvert.DeserializeObject<RoomData>(await response.Content.ReadAsStringAsync());
                return roomData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the current room.");
                throw; 
            }
        }

        public async Task<bool> MovePlayerAsync(string direction)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{_baseUrl}Player/move?direction={direction}", null);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while moving the player in direction {direction}.");
                throw; 
            }
        }
    }
}