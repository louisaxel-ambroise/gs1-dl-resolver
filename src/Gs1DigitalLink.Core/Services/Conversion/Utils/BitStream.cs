using System.Text;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils;

internal sealed class BitStream(string compressedString)
{
    private int _position;
    private string _buffer = string.Empty;

    public int Remaining => compressedString.Length * 6 - _position;

    public ReadOnlySpan<char> Current => _current.AsSpan();
    private string _current = string.Empty;

    public bool Buffer(int length)
    {
        if(Remaining < length)
        {
            return false;
        }
    
        var output = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            output.Append(ReadChar());
        }

        _current = output.ToString();

        return true;
    }

    private char ReadChar()
    {
        if (_position % 6 == 0)
        {
            _buffer = Alphabets.GetBinary(compressedString.ElementAt(_position / 6));
        }

        return _buffer[_position++ % 6];
    }

    public override string ToString()
    {
        return string.Concat(compressedString.Select(Alphabets.GetBinary));
    }
}
