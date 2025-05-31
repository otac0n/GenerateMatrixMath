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
                 select new Matrix(d, integral, dims, multiplicationOperators[d])).ToList();

            foreach (var model in matrixModels)
            {
                var name = Templates.Name(new { Name = "Matrix", model.Size, model.Integral });
                var path = Path.Combine(outputPath, $"{name}.gen.cs");
                using var writer = new StreamWriter(path);
                Templates.RenderMatrix(model, writer);
                Console.WriteLine($"Wrote {name}");
            }

            var interfaces = new[]
            {
                //typeof(System.Numerics.IRootFunctions<>),
                //typeof(System.Numerics.IPowerFunctions<>),
                typeof(System.Numerics.ILogarithmicFunctions<>),
                typeof(System.Numerics.IExponentialFunctions<>),
                //typeof(System.Numerics.ITrigonometricFunctions<>),
                //typeof(System.Numerics.IHyperbolicFunctions<>),
            };

            var sizes = Enumerable.Range(2, 3);
            var vectorModels =
                (from d in sizes
                 from integral in new[] { true, false }
                 select new Vector(d, integral, sizes, interfaces)).ToList();

            foreach (var model in vectorModels)
            {
                var name = Templates.Name(new { Name = "Vector", model.Size, model.Integral });
                var path = Path.Combine(outputPath, $"{name}.gen.cs");
                using var writer = new StreamWriter(path);
                Templates.RenderVector(model, writer);
                Console.WriteLine($"Wrote {name}");
            }
        }
    }
}
