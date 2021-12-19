using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace codice_plastico.Controllers;

[ApiController]
[Route("[controller]")]
public class MarsRoverController : ControllerBase
{
    private readonly ILogger<MarsRoverController> _logger;
    private readonly IPathGrid _grid;

    public MarsRoverController(ILogger<MarsRoverController> logger, IPathGrid grid)
    {
        _logger = logger;
        _grid = grid;
    }

    [HttpPost("/SetInitialRoverPosition")]
    public RoverCommandResponse PostRoverInizialPosition(RoverInitialPositionRequest request)
    {
        return _grid.SetRoverPosition(request.Row, request.Col, request.O);
    }

    [HttpGet("/GetRoverPosition")]
    public string GetRovePosition()
    {
        return _grid.GetRoverInformation();
    }

    [HttpGet("/GetObstacles")]
    public IEnumerable<Cell> GetObstacles()
    {
        return _grid.GetObstacles();
    }

    [HttpGet("/GetPlanet")]
    public IEnumerable<Cell> GetGrid()
    {
        return _grid.GetGrid();
    }

    [HttpPost("/MoveRover")]
    public RoverCommandResponse PostRoverMovePosition(RoverCommandRequest request)
    {
        return _grid.MoveRover(request);
    }
}
