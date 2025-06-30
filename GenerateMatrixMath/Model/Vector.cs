namespace GenerateMatrixMath.Model
{
    using System.Collections.Immutable;

    public record class Vector(Options Options, int Size, bool? Integral, IEnumerable<int> Sizes, IEnumerable<Extension> Extensions)
    {
        public static readonly ImmutableList<string> VectorFieldNames = ["X", "Y", "Z", "W"];
    }
}
