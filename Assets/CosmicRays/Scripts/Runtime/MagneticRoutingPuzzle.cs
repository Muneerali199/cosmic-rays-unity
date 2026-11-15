using System;
using System.Collections.Generic;

/// <summary>Pure gameplay model for the SPA-077 magnetic-containment routing puzzle.</summary>
public sealed class MagneticRoutingPuzzle
{
    public const int Rows = 3;
    public const int Columns = 3;

    /// <summary>Cardinal field-connection directions, ordered clockwise.</summary>
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    /// <summary>Available magnetic-coil shapes.</summary>
    public enum TileType
    {
        Elbow,
        Straight,
        Core
    }

    /// <summary>One rotatable coil or protected core in the routing grid.</summary>
    public sealed class Tile
    {
        public string Id { get; private set; }
        public TileType Type { get; private set; }
        public int Rotation { get; private set; }
        public bool Rotatable { get; private set; }

        public Tile(string id, TileType type, int rotation, bool rotatable)
        {
            Id = id;
            Type = type;
            Rotation = NormalizeRotation(rotation);
            Rotatable = rotatable;
        }

        public void RotateClockwise()
        {
            if (Rotatable)
            {
                Rotation = (Rotation + 1) % 4;
            }
        }

        public List<Direction> GetConnections()
        {
            var connections = new List<Direction>();
            if (Type == TileType.Core)
            {
                return connections;
            }

            if (Type == TileType.Straight)
            {
                connections.Add(RotateDirection(Direction.North, Rotation));
                connections.Add(RotateDirection(Direction.South, Rotation));
                return connections;
            }

            connections.Add(RotateDirection(Direction.North, Rotation));
            connections.Add(RotateDirection(Direction.East, Rotation));
            return connections;
        }

        private static int NormalizeRotation(int rotation)
        {
            int normalized = rotation % 4;
            return normalized < 0 ? normalized + 4 : normalized;
        }
    }

    /// <summary>Result of tracing one particle through the current field.</summary>
    public sealed class RouteResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public IReadOnlyList<int> Path { get; private set; }

        public RouteResult(bool success, string message, List<int> path)
        {
            Success = success;
            Message = message;
            Path = path.AsReadOnly();
        }
    }

    private readonly List<Tile> _tiles = new List<Tile>();

    /// <summary>Current puzzle tiles in row-major order.</summary>
    public IReadOnlyList<Tile> Tiles { get { return _tiles; } }

    public MagneticRoutingPuzzle()
    {
        Reset();
    }

    /// <summary>Restores the authored starting coil rotations.</summary>
    public void Reset()
    {
        _tiles.Clear();
        _tiles.Add(new Tile("A1", TileType.Elbow, 0, true));
        _tiles.Add(new Tile("A2", TileType.Straight, 0, true));
        _tiles.Add(new Tile("A3", TileType.Elbow, 0, true));
        _tiles.Add(new Tile("B1", TileType.Elbow, 1, true));
        _tiles.Add(new Tile("B2", TileType.Core, 0, false));
        _tiles.Add(new Tile("B3", TileType.Elbow, 1, true));
        _tiles.Add(new Tile("C1", TileType.Elbow, 0, true));
        _tiles.Add(new Tile("C2", TileType.Straight, 0, true));
        _tiles.Add(new Tile("C3", TileType.Elbow, 0, true));
    }

    /// <summary>Rotates one coil clockwise. Returns false for invalid or protected cells.</summary>
    public bool Rotate(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= _tiles.Count || !_tiles[tileIndex].Rotatable)
        {
            return false;
        }

        _tiles[tileIndex].RotateClockwise();
        return true;
    }

    /// <summary>Traces a particle from the middle-left source to the middle-right collector.</summary>
    public RouteResult Trace()
    {
        int row = 1;
        int column = -1;
        Direction travelDirection = Direction.East;
        var path = new List<int>();
        var visited = new HashSet<string>(StringComparer.Ordinal);

        for (int step = 0; step < Rows * Columns * 4; step++)
        {
            Move(ref row, ref column, travelDirection);
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
            {
                bool reachedCollector = row == 1 && column == Columns;
                return new RouteResult(
                    reachedCollector,
                    reachedCollector
                        ? "Stable route confirmed. Particle reached the collector."
                        : "The particle escaped the containment field.",
                    path);
            }

            int tileIndex = row * Columns + column;
            Tile tile = _tiles[tileIndex];
            Direction entryDirection = Opposite(travelDirection);
            List<Direction> connections = tile.GetConnections();
            string visitKey = tileIndex + ":" + entryDirection;

            if (!visited.Add(visitKey))
            {
                return new RouteResult(false, "The field loops back at coil " + tile.Id + ".", path);
            }

            if (!connections.Contains(entryDirection))
            {
                string message = tile.Type == TileType.Core
                    ? "The particle struck the protected research core."
                    : "The field is disconnected at coil " + tile.Id + ".";
                return new RouteResult(false, message, path);
            }

            Direction? exit = null;
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i] != entryDirection)
                {
                    if (exit.HasValue)
                    {
                        return new RouteResult(
                            false,
                            "Coil " + tile.Id + " does not provide a stable exit path.",
                            path);
                    }
                    exit = connections[i];
                }
            }

            if (!exit.HasValue)
            {
                return new RouteResult(
                    false,
                    "Coil " + tile.Id + " does not provide a stable exit path.",
                    path);
            }

            path.Add(tileIndex);
            travelDirection = exit.Value;
        }

        return new RouteResult(false, "The field could not stabilize the particle route.", path);
    }

    /// <summary>Returns a short compass label for one coil's active connections.</summary>
    public static string DescribeConnections(Tile tile)
    {
        List<Direction> connections = tile.GetConnections();
        if (connections.Count == 0)
        {
            return "CORE";
        }

        var labels = new List<string>();
        for (int i = 0; i < connections.Count; i++)
        {
            labels.Add(connections[i].ToString().Substring(0, 1));
        }
        return string.Join("-", labels.ToArray());
    }

    private static Direction RotateDirection(Direction direction, int turns)
    {
        return (Direction)(((int)direction + turns) % 4);
    }

    private static Direction Opposite(Direction direction)
    {
        return (Direction)(((int)direction + 2) % 4);
    }

    private static void Move(ref int row, ref int column, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                row--;
                break;
            case Direction.East:
                column++;
                break;
            case Direction.South:
                row++;
                break;
            case Direction.West:
                column--;
                break;
        }
    }
}
