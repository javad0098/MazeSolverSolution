using Microsoft.Extensions.Logging;
using MazeSolverApp.Models;
using MazeSolverApp.Services;
using Path = MazeSolverApp.Models.Path;

namespace MazeSolverApp
{
    public class MazeSolver
    {
        private readonly IDataService _dataService;
        private readonly ILogger<MazeSolver> _logger;
        private readonly HashSet<string> _visitedRooms = new HashSet<string>();

        public MazeSolver(IDataService dataService, ILogger<MazeSolver> logger)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> StartGameAsync()
        {
            try
            {
                var gameData = await _dataService.StartGameAsync();
                _logger.LogInformation($"Game started with token: {gameData.Token}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the game.");
                return false;
            }
        }

        public async Task<bool> FindExitAsync()
        {
            if (!await StartGameAsync())
            {
                _logger.LogError("Failed to start the game.");
                return false;
            }

            var currentRoom = await _dataService.GetCurrentRoomAsync();
            return await DFSAsync(currentRoom);
        }

        private async Task<bool> DFSAsync(RoomData currentRoom)
        {
            if (currentRoom == null)
            {
                _logger.LogError("Current room cannot be null.");
                return false;
            }

            if (IsRoomVisited(currentRoom))
            {
                _logger.LogInformation($"Room {currentRoom.Id} has already been visited.");
                return false;
            }

            if (IsExitFound(currentRoom))
            {
                _logger.LogInformation($"Exit found in room {currentRoom.Id}!");
                return true;
            }

            return await ExplorePathsAsync(currentRoom);
        }

        private bool IsRoomVisited(RoomData room)
        {
            if (_visitedRooms.Contains(room.Id))
            {
                return true;
            }

            _visitedRooms.Add(room.Id);
            return false;
        }

        private bool IsExitFound(RoomData room)
        {
            return room.Effect == "Victory";
        }

        private async Task<bool> ExplorePathsAsync(RoomData room)
        {
            if (room.Paths == null)
            {
                _logger.LogError("Current room paths cannot be null.");
                return false;
            }

            foreach (var path in room.Paths)
            {
                if (!_visitedRooms.Contains(path.Destination))
                {
                    _logger.LogInformation($"Attempting to move to {path.Direction} from room {room.Id}");
                    if (await TryMoveAndExploreAsync(path))
                    {
                        return true;
                    }
                }
            }

            _logger.LogWarning($"All paths explored in room {room.Id}, but the exit has not been found.");
            return false;
        }

        private async Task<bool> TryMoveAndExploreAsync(Path path)
        {
            try
            {
                if (!await _dataService.MovePlayerAsync(path.Direction))
                {
                    _logger.LogWarning($"Failed to move to {path.Direction}.");
                    return false;
                }

                var nextRoom = await _dataService.GetCurrentRoomAsync();
                _logger.LogInformation($"Moved to room {nextRoom.Id}");

                if (await DFSAsync(nextRoom))
                {
                    return true;
                }

                _logger.LogInformation($"Backtracking from room {nextRoom.Id}");
                await _dataService.MovePlayerAsync(OppositeDirection(path.Direction));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while trying to move and explore from room {path.Destination}.");
                return false;
            }
        }

        private string OppositeDirection(string direction)
        {
            return direction switch
            {
                "North" => "South",
                "South" => "North",
                "East" => "West",
                "West" => "East",
                _ => throw new ArgumentException("Invalid direction")
            };
        }
    }
}