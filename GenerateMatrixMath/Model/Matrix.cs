namespace GenerateMatrixMath.Model
{
    public record class Matrix(Dimension Size, HashSet<Dimension> AllSizes, Type[] Casts, IEnumerable<(Dimension Left, Dimension Right)> MultiplyOperators)
    {
    }
}
