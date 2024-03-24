namespace MazeSolverApp.Models;

public class RoomData
{
    public string Id { get; set; }
    public List<Path> Paths { get; set; } 
    public string Contents { get; set; }
    public string Effect { get; set; }
}