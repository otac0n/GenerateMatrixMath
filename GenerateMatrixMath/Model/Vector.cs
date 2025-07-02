namespace GenerateMatrixMath.Model
{
    using System.Collections.Immutable;

    public record class Vector(int Size, IEnumerable<int> Sizes, Type[] Casts, IEnumerable<Extension> Extensions)
    {
        public static readonly ImmutableList<string> VectorFieldNames = ["X", "Y", "Z", "W"];
    }
}
