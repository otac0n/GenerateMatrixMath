namespace GenerateMatrixMath
{
    partial class Templates
    {
        public static string Name(dynamic model)
        {
            using var writer = new StringWriter();
            Name(model, writer, null);
            return writer.ToString();
        }

        public static void Name(dynamic model, TextWriter writer, string? indentation) => RenderName(model, writer);

        public static void Ordinal(int num, TextWriter writer, string? indentation) => writer.Write(Ordinal(num));

        public static string Ordinal(int num)
        {
            if (num <= 0)
            {
                return num.ToString();
            }

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        public static IEnumerable<int[]> PermuteDigits(int digits, int maxValue)
        {
            var indices = new int[digits];
            bool Increment()
            {
                for (var ix = 0; ix < digits; ix++)
                {
                    indices[ix]++;
                    if (indices[ix] < maxValue)
                    {
                        return false;
                    }

                    indices[ix] = 0;
                }

                return true;
            }

            do
            {
                yield return indices.ToArray();
            } while (!Increment());
        }
    }
}
