public interface IPathGrid
{
    List<Cell> GetGrid();
    List<Cell> GetObstacles();
    RoverCommandResponse SetRoverPosition(int x, int y, Orientation o);
    string GetRoverInformation();
    RoverCommandResponse MoveRover(RoverCommandRequest request);
}