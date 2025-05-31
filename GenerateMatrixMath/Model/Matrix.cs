namespace GenerateMatrixMath.Model
{
    public record class Matrix(Dimension Size, bool Integral, IEnumerable<(Dimension Left, Dimension Right)> MultiplyOperators)
    {
    }
}
