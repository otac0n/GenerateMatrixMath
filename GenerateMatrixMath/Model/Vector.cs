namespace GenerateMatrixMath.Model
{
    using System.Collections.Immutable;

    public record class Vector(int Size, bool Integral)
    {
        public static readonly ImmutableList<string> VectorFieldNames = ["X", "Y", "Z", "W"];
    }
}
