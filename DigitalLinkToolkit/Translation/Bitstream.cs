namespace DigitalLinkToolkit.Translation;

public sealed class Bitstream(string remaining)
{
    private int _position = 0;

    public int Remaining => remaining.Length - _position;
    public string RemainingStr => remaining[_position..];

    public string ReadUntil(int bitNumber)
    {
        var toRead = remaining.Length - _position > bitNumber ? bitNumber : remaining.Length - _position;

        return Read(toRead);
    }

    public string Read(int bitNumber)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_position + bitNumber, remaining.Length, nameof(bitNumber));
        _position += bitNumber;

        return remaining.Substring(_position-bitNumber, bitNumber);
    }
}
