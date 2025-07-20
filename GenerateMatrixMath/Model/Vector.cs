namespace GenerateMatrixMath.Model
{
    using System.Collections.Immutable;

    public record class Vector(int Size, IEnumerable<int> Sizes, Type[] Casts, Type[] NumericsTypes, IEnumerable<Extension> Extensions)
    {
        public static readonly ImmutableList<string> VectorFieldNames = ["X", "Y", "Z", "W"];
    }
}
