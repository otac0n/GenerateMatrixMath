namespace GenerateMatrixMath.Model
{
    public record class Matrix(Dimension Size, HashSet<Dimension> AllSizes, IEnumerable<(Dimension Left, Dimension Right)> MultiplyOperators)
    {
    }
}
