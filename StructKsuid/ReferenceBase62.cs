namespace StructKsuid;

public static class ReferenceBase62
{
    public static int[] Reworked(ReadOnlySpan<int> source)
    {
        const int sourceBase = 62;
        const int targetBase = 256;
        
        var intList1 = new int[8];
        var intList1Index = 7;
        Span<int> intList2 = new int[27];
        int intList2Index;
        int length;
        for (; (length = source.Length) > 0; source = intList2[..intList2Index])
        {
            intList2Index = 0;
            int remainder = 0;
            for (int index = 0; index != length; ++index)
            {
                int value = source[index] + remainder * sourceBase;
                int digit = value / targetBase;
                remainder = value % targetBase;
                if (intList2Index > 0 || digit > 0)
                {
                    intList2[intList2Index] = digit;
                    intList2Index++;
                }
            }
            intList1[intList1Index] = remainder;
            intList1Index--;
        }
        return intList1;
    }
    
    public static int[] BaseConvert(Span<int> source)
    {
        const int sourceBase = 62;
        const int targetBase = 256;
        
        List<int> intList1 = new List<int>();
        List<int> intList2;
        int length;
        for (; (length = source.Length) > 0; source = intList2.ToArray())
        {
            intList2 = new List<int>();
            int remainder = 0;
            for (int index = 0; index != length; ++index)
            {
                int value = source[index] + remainder * sourceBase;
                int digit = value / targetBase;
                remainder = value % targetBase;
                if (intList2.Count > 0 || digit > 0)
                    intList2.Add(digit);
            }
            intList1.Insert(0, remainder);
        }
        return intList1.ToArray();
    }
}