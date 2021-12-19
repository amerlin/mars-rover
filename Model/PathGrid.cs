public class PathGrid : IPathGrid
{
    private List<Cell> _grid;
    private const int _defaultPlanetRows =10;
    private const int _defaultPlanetColums =10;
    private const int _defaultObstaclsNumber = 5;

    public PathGrid(){
        
        _grid = new List<Cell>();

        for (var i = 0; i < _defaultPlanetRows; i++)
        {
            for (var j = 0; j < _defaultPlanetColums; j++)
                _grid.Add(new Cell() { Row = i, Col = j, Value = string.Empty });
        }

        for (var k = 0; k < _defaultObstaclsNumber; k++)
        {
            Random r = new Random();
            var row = r.Next(0, _defaultPlanetRows);
            var col = r.Next(0, _defaultPlanetColums);
            var c = _grid.First(a => a.Row == row && a.Col == col);
            c.Value = "X";
        }
    }

    public List<Cell> GetGrid()
    {
        return _grid;
    }

    public List<Cell> GetObstacles(){
        var returnValue =new List<Cell>();
        foreach(var a in _grid){
            if(a.Value=="X")
                returnValue.Add(new Cell{Row=a.Row, Col=a.Col, Value=a.Value});
        }
        return returnValue;
    }

    public RoverCommandResponse SetRoverPosition(int row, int col, Orientation orientation)
    {

        if (row >= _defaultPlanetRows || col >= _defaultPlanetColums)
        {
            return new RoverCommandResponse()
            {
                Message = "Is not possibile use this location. You are out of planet",
                Grid = _grid,
            };
        }

        if (!Enum.IsDefined(typeof(Orientation), orientation))
        {
            return new RoverCommandResponse()
            {
                Message = "Is not possibile use this orientation. Possibile value are 0 (North), 1 (Sud), 2 (Ovest), 3 (Est)",
                Grid = _grid,
            };
        }

        if (IsObstaclePosition(row, col))
            return new RoverCommandResponse()
            {
                Message = "Is not possibile use this location. Obstacle is present.",
                Grid = _grid,
            };

        foreach (var element in _grid)
        {
            if (element.Value != "X")
                element.Value = "";
        }

        var cell = _grid.First(a=>a.Row==row && a.Col==col);
        cell.Value=$"ROVER";
        cell.Orientation=orientation;
        return new RoverCommandResponse()
        {
            Message = $"Initial setup is ok. Rover position Row: {row} Col: {col} Orientation: {orientation}",
            Grid = _grid,
            NewOrientation = orientation,
            Orientation = orientation.ToString()
        };
    }

    public string GetRoverInformation()
    {
        var position = _grid.FirstOrDefault(a => a.Value == "ROVER");
        return position == null ? "Sorry. I'm not landed" : $"I'm at position Row: {position.Row} Col: {position.Col} with orientation: {position.Orientation}";
    }

    public bool IsObstaclePosition(int x, int y)
    {

        var cell = _grid.First(a => a.Row == x && a.Col == y);
        return cell.Value=="X";
    }

    public string ObstacleIsPresentInPath()
    {
        return string.Empty;
    }

    public RoverCommandResponse MoveRover(RoverCommandRequest request)
    {
        var roverPosition = _grid.FirstOrDefault(a => a.Value == "ROVER");
        if (roverPosition == null)
        {
            return new RoverCommandResponse()
            {
                Message = "Rover is not present in the planet",
                Grid = _grid,
                NewOrientation  = request.Orientation,
                Orientation = request.Orientation.ToString()
            };
        }

        if (!Enum.IsDefined(typeof(Orientation), request.Orientation))
         {
            return new RoverCommandResponse()
            {
                Message = "This orientation is not possibile",
                Grid = _grid,
                NewOrientation = roverPosition?.Orientation,
                Orientation = roverPosition?.Orientation.ToString()
            };
         }

        if (request.Step == 0)
        {
           return new RoverCommandResponse()
            {
                Message = "0 step. I will not move.",
                Grid = _grid,
                NewOrientation = request.Orientation,
                Orientation = request.Orientation.ToString()
            };
        }

        //Rover step
        var newPosition =  TryToMove(roverPosition, request);
        roverPosition.Value="";
        roverPosition.Orientation=null;
        var r = _grid.FirstOrDefault(a=>a.Row==newPosition.Row && a.Col==newPosition.Col);
        r.Value=$"ROVER";
        r.Orientation=newPosition.Orientation;
        
        var message= string.IsNullOrEmpty(newPosition.Value)?
            $"Rover is at {r.Row}: {r.Col}: {r.Orientation}":
            newPosition.Value;

        return new RoverCommandResponse()
        {
                Message=message,
                Grid = _grid, 
                NewOrientation = request.Orientation,
                Orientation = request.Orientation.ToString()
        };

    }

    private Cell TryToMove(Cell roverPosition, RoverCommandRequest request)
    {
        var currentRow = roverPosition.Row;
        var currentCol = roverPosition.Col;

        switch (request.Orientation)
        {
            case Orientation.North:
                if ((currentRow - request.Step) > 0){
                    currentRow = currentRow - request.Step;
                }
                else
                {
                    var value = currentRow - request.Step;
                    while (value < 0)
                    {
                        value = value + _defaultPlanetRows;
                    }
                    currentRow = value;
                }
                break;
            case Orientation.South:
                if ((currentRow + request.Step) < _defaultPlanetRows)
                {
                    currentRow = currentRow + request.Step;
                }
                else
                {
                    var value = currentRow + request.Step;
                    while (value > _defaultPlanetRows)
                    {
                        value = value - _defaultPlanetRows;
                    }
                    currentRow = value;
                }
                break;
            case Orientation.Est:
                if ((currentCol + request.Step) < _defaultPlanetColums)
                {
                    currentCol = currentCol + request.Step;
                }
                else
                {
                    var value = currentCol + request.Step;
                    while(value > _defaultPlanetColums){
                        value = value - _defaultPlanetColums;
                    }
                    currentCol = value;
                }
                break;
            case Orientation.Ovest: 
                if((currentCol - request.Step >0 )){
                    currentCol = currentCol - request.Step;
                }
                else{
                    var value = currentCol - request.Step;
                    while (value < 0)
                    {
                        value = value + _defaultPlanetColums;
                    }
                    currentCol = value;
                }
                break;
        }

        var cpostition = new Cell() { Row = currentRow, Col = currentCol, Orientation = request.Orientation};
        var checkedPath = CheckIfObstacleIsPresent(roverPosition, cpostition, request.Step, request.Orientation);
        if (!checkedPath.IsPossiblePath && checkedPath.LastPossiblePosition != null){
            checkedPath.LastPossiblePosition.Value = $"Obstacle Found: Last possibile position ==> Row {checkedPath.LastPossiblePosition.Row}: Col{checkedPath.LastPossiblePosition.Col}";
            return checkedPath.LastPossiblePosition;
        }

        return new Cell()
        {
            Row = currentRow,
            Col = currentCol,
            Orientation = request.Orientation,
            Value = String.Empty
        };
    }

    private PossiblePath CheckIfObstacleIsPresent(Cell oldPosition, Cell newPosition, int step, Orientation orientation)
    {
        if (orientation == Orientation.North)
        {
            var rowToCheck = oldPosition.Row;
            var destinationRow = newPosition.Row;
            var currentCol = oldPosition.Col;

            for (var i = 1; i <= step; i++)
            {
                rowToCheck = rowToCheck - 1;

                if (rowToCheck < 0)
                    rowToCheck = _defaultPlanetRows - 1;

                Console.WriteLine($"I'm checking: {rowToCheck}:{currentCol}");

                var currentCell = _grid.First(a => a.Row == rowToCheck && a.Col == currentCol);

                if (currentCell.Value == "X")
                {
                    var cell = new Cell { Row = rowToCheck + 1, Col = currentCol, Orientation = orientation };
                    if (rowToCheck == (_defaultPlanetRows - 1))
                        cell.Row = 0;
                    return new PossiblePath() { IsPossiblePath = false, LastPossiblePosition = cell };
                }

                if (rowToCheck == destinationRow)
                    break;
            }
        }

        if (orientation == Orientation.South)
        {

            var rowToCheck = oldPosition.Row;
            var destinationRow = newPosition.Row;
            var currentCol = oldPosition.Col;

            for (var i = 1; i <= step; i++)
            {
                rowToCheck = rowToCheck + 1;

                if (rowToCheck > (_defaultPlanetRows - 1))
                    rowToCheck = 0;

                Console.WriteLine($"I'm checking: {rowToCheck}:{currentCol}");

                var currentCell = _grid.First(a => a.Row == rowToCheck && a.Col == currentCol);

                if (currentCell.Value == "X")
                {
                    var cell = new Cell { Row = rowToCheck - 1, Col = currentCol, Orientation = orientation };
                    if (rowToCheck == 0)
                        cell.Row = _defaultPlanetRows - 1;
                    return new PossiblePath() { IsPossiblePath = false, LastPossiblePosition = cell };
                }

                if (rowToCheck == destinationRow)
                    break;

            }
        }

        if (orientation == Orientation.Ovest)
        {

            var currentRow = oldPosition.Row;
            var destinationCol = newPosition.Col;
            var colToCheck = oldPosition.Col;

            for (var i = 1; i <= step; i++)
            {
                colToCheck = colToCheck - 1;

                if (colToCheck < 0)
                    colToCheck = _defaultPlanetColums - 1;

                Console.WriteLine($"I'm checking: {currentRow}:{colToCheck}");

                var currentCell = _grid.First(a => a.Row == currentRow && a.Col == colToCheck);

                if (currentCell.Value == "X")
                {
                    var cell = new Cell { Row = currentRow, Col = colToCheck + 1, Orientation = orientation };
                    if (colToCheck == (_defaultPlanetColums - 1))
                        cell.Col = 0;
                    return new PossiblePath() { IsPossiblePath = false, LastPossiblePosition = cell };
                }

                if (colToCheck == destinationCol)
                    break;
            }

        }

        if (orientation == Orientation.Est)
        {
            var currentRow = oldPosition.Row;
            var destinationCol = newPosition.Col;
            var colToCheck = oldPosition.Col;
            for (var i = 1; i <= step; i++)
            {
                colToCheck = colToCheck + 1;

                if (colToCheck > _defaultPlanetColums - 1)
                    colToCheck = 0;

                Console.WriteLine($"I'm checking: {currentRow}:{colToCheck}");

                var currentCell = _grid.First(a => a.Row == currentRow && a.Col == colToCheck);
                
                if (currentCell.Value == "X")
                {
                    var cell = new Cell { Row = currentRow, Col = colToCheck-1, Orientation = orientation };
                    if (colToCheck == 0)
                        cell.Col = _defaultPlanetColums - 1;
                    return new PossiblePath() { IsPossiblePath = false, LastPossiblePosition = cell };
                }
            }
        }
        var returnValue = new PossiblePath() { IsPossiblePath = true, LastPossiblePosition = null };
        return returnValue;
    }
}