using System.Xml;

namespace Gs1DigitalLink.Core.Services.Conversion.Utils.Validation;

internal static class CompanyPrefix
{
    public static bool Validate(string input)
    {
        var gcpLength = GetCompanyPrefixLength(input);

        return gcpLength >= 0 && input.Length >= gcpLength; 
    }

    public static int GetCompanyPrefixLength(string value)
    {
        var current = _root;

        for (var i = 0; i < 12 && i < value.Length && !current.IsLeaf; i++)
        {
            current = current[value[i]];
        }

        return current.Length;
    }

    private static readonly Node _root = Node.Default;

    private static void SetPrefix(string prefix, int length)
    {
        var current = _root;

        for (var i = 0; i < prefix.Length - 1; i++)
        {
            if (current[prefix[i]].IsLeaf)
            {
                current[prefix[i]] = Node.Default;
            }

            current = current[prefix[i]];
        }

        current[prefix[^1]] = length;
    }

    internal static void Initialize(Stream gcpList)
    {
        using var reader = XmlReader.Create(gcpList, settings);

        while (reader.ReadToFollowing("entry"))
        {
            var prefix = reader.GetAttribute("prefix") ?? string.Empty;
            var length = int.Parse(reader.GetAttribute("gcpLength") ?? "-1");

            SetPrefix(prefix, length);
        }
    }
     
    private static readonly XmlReaderSettings settings = new()
    {
        IgnoreComments = true,
        IgnoreWhitespace = true,
        CloseInput = true
    };

    private sealed record Node
    {
        private readonly Node[]? _children;
        public int Length { get; private set; } = -1;

        private Node(Node[] children) => _children = children;
        private Node(int length) => Length = length;

        public bool IsLeaf => _children is null;

        public Node this[char index]
        {
            get
            {
                EnsureInRange(index);

                return _children?[index - '0'] ?? _values[0];
            }
            set
            {
                EnsureInRange(index);

                _children![index - '0'] = value;
            }
        }

        public static Node Default => new(new Node[10]);

        public static implicit operator Node(int value) => _values[value + 1];

        private static readonly Node[] _values = [.. Enumerable.Range(-1, 14).Select(x => new Node(x))];

        private static void EnsureInRange(char value)
        {
            if (!Convert.ToByte(value).IsNumeric())
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}