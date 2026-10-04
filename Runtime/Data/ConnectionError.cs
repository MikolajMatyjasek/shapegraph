namespace Galaretka.ShapeGraph.Data
{
    public enum ConnectionError
    {
        None = 0,
        MissingNode,
        MissingPort,
        DirectionMismatch,
        TypeIncompatible,
        OutputToOutput,
        InputToInput
    }
}
