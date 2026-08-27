namespace FarmApp.MVTParser;

public static class Command
{
    /**
     * MoveTo: 1. (2 parameters follow)
     */
    public const int MoveTo = 1;

    /**
     * LineTo: 2. (2 parameters follow)
     */
    public const int LineTo = 2;

    /**
     * ClosePath: 7. (no parameters follow)
     */
    public const int ClosePath = 7;
}