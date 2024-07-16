#nullable enable

namespace Implan.Api.Utilities.Text;

/// <summary>
/// Compares two strings by using logical numeric and character order
/// </summary>
/// <remarks>
/// This sorts text by comparing groups of digit or non-digit characters together and compares those groups<br/>
/// <c>"1" &lt; "10" &lt; "100"</c>
/// <c>"ABC8" &lt; "ABC13" &lt; "ABC147"</c>
/// </remarks>
public class AlphanumericTextComparer : IComparer<string>
{
    /// <summary>
    /// The default <see cref="AlphanumericTextComparer"/> instance
    /// </summary>
    public static AlphanumericTextComparer Default { get; } = new AlphanumericTextComparer();

    
    private readonly StringComparison _stringComparison;

    public AlphanumericTextComparer(StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        _stringComparison = stringComparison;
    }

    public int Compare(string? left, string? right)
    {
        return Compare(left.AsSpan(), right.AsSpan());
    }
    
    public int Compare(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        int l = 0;
        int leftLength = left.Length;

        int r = 0;
        int rightLength = right.Length;

        int result = 0;

        while (l < leftLength || r < rightLength)
        {
            if (l >= leftLength) return -1; // Left ran out, right is longer
            if (r >= rightLength) return 1; // Right ran out, left is longer

            // Get left chunk (grouped based on Digits / NonDigits
            bool leftChunkIsDigits = char.IsDigit(left[l]);
            int leftChunkStart = l;
            do
            {
                l++;
            } while (l < leftLength && char.IsDigit(left[l]) == leftChunkIsDigits);

            ReadOnlySpan<char> leftChunk = left.Slice(leftChunkStart, l - leftChunkStart);

            // Get right chunk (grouped based on Digits / NonDigits
            bool rightChunkIsDigits = char.IsDigit(right[r]);
            int rightChunkStart = r;
            do
            {
                r++;
            } while (r < rightLength && char.IsDigit(right[r]) == rightChunkIsDigits);

            ReadOnlySpan<char> rightChunk = right.Slice(rightChunkStart, r - rightChunkStart);

            // If they are both digit chunks, compare them
            if (leftChunkIsDigits && rightChunkIsDigits &&
                int.TryParse(leftChunk, out int leftChunkValue) &&
                int.TryParse(rightChunk, out int rightChunkValue))
            {
                if (leftChunkValue < rightChunkValue)
                    result = -1;
                else if (leftChunkValue > rightChunkValue)
                    result = 1;
            }
            // Otherwise, use our string comparer
            else
            {
                result = leftChunk.CompareTo(rightChunk, _stringComparison);
            }

            // Did we find a difference?
            if (result != 0)
                return result;
        }

        // They are the same
        return 0;
    }
}