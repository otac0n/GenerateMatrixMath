namespace GenerateMatrixMath.Model
{
    public record class Matrix(Dimension Size, bool Integral, HashSet<Dimension> AllSizes, IEnumerable<(Dimension Left, Dimension Right)> MultiplyOperators)
    {
    }
}
