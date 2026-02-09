namespace ExploringGame.Tests.TestHelpers;

/// <summary>
/// Result of a test assertion executed during game simulation
/// </summary>
public enum TestResult
{
    /// <summary>
    /// Test has failed - stop the game and fail the test
    /// </summary>
    FAIL,
    
    /// <summary>
    /// Test has passed - stop the game and pass the test
    /// </summary>
    PASS,
    
    /// <summary>
    /// Test is still running - continue the game
    /// </summary>
    CONTINUE
}
