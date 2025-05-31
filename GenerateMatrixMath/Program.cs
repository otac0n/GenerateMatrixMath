namespace GenerateMatrixMath
{
    using GenerateMatrixMath.Model;

    internal class Program
    {
        static void Main()
        {
            var outputPath = @"C:\Users\otac0n\Projects\Silk.NET\sources\Maths\Maths\";

            var dims = new HashSet<Dimension>();
            for (var r = 1; r < 4; r++)
            {
                for (var c = 1; c < 4; c++)
                {
                    dims.Add((r + 1, c + 1));
                }
            }

            dims.Add((5, 4));

            var multiplicationOperators =
                (from left in dims
                 from right in dims
                 where left.Columns == right.Rows
                 let o = (left.Rows, right.Columns)
                 where dims.Contains(o)
                 select (left, right))
                 .ToLookup(m => new[] { m.left, m.right }.Max());

            var matrixModels =
                (from d in dims
                 from integral in new[] { true, false }
                 select new Matrix(d, integral, multiplicationOperators[d])).ToList();

            foreach (var model in matrixModels)
            {
                var name = Templates.Name(new { Name = "Matrix", model.Size, model.Integral });
                using var writer = new StreamWriter(Path.Combine(outputPath, $"{name}.gen.cs"));
                Templates.RenderMatrix(model, writer);
                Console.WriteLine($"Wrote {name}");
            }
        }
    }
}
