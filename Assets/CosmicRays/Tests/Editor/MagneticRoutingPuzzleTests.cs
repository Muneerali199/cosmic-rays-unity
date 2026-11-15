#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class MagneticRoutingPuzzleTests
{
    [Test]
    public void InitialRouteIsDisconnected()
    {
        var puzzle = new MagneticRoutingPuzzle();

        MagneticRoutingPuzzle.RouteResult result = puzzle.Trace();

        Assert.That(result.Success, Is.False);
        StringAssert.Contains("disconnected", result.Message.ToLowerInvariant());
    }

    [Test]
    public void AuthoredSolutionReachesCollector()
    {
        var puzzle = new MagneticRoutingPuzzle();
        Rotate(puzzle, 0, 1);
        Rotate(puzzle, 1, 1);
        Rotate(puzzle, 2, 2);
        Rotate(puzzle, 3, 2);
        Rotate(puzzle, 5, 3);

        MagneticRoutingPuzzle.RouteResult result = puzzle.Trace();

        Assert.That(result.Success, Is.True);
        CollectionAssert.AreEqual(new[] { 3, 0, 1, 2, 5 }, result.Path);
    }

    [Test]
    public void ProtectedCoreDoesNotRotate()
    {
        var puzzle = new MagneticRoutingPuzzle();

        bool rotated = puzzle.Rotate(4);

        Assert.That(rotated, Is.False);
        Assert.That(puzzle.Tiles[4].Rotation, Is.EqualTo(0));
    }

    private static void Rotate(MagneticRoutingPuzzle puzzle, int tileIndex, int turns)
    {
        for (int turn = 0; turn < turns; turn++)
        {
            Assert.That(puzzle.Rotate(tileIndex), Is.True);
        }
    }
}
#endif
