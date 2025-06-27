namespace GenerateMatrixMath
{
    using System.Numerics;
    using GenerateMatrixMath.Model;
    using static GenerateMatrixMath.Model.ArgumentMultiplicity;

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

            var extensions = new Extension[]
            {
                // INumber<>
                new(typeof(INumber<>), nameof(INumber<>.Sign), [Memberwise]),
                new(typeof(INumber<>), nameof(INumber<>.Max), [Memberwise, Memberwise]),
                new(typeof(INumber<>), nameof(INumber<>.Max), [Memberwise, One]),
                new(typeof(INumber<>), nameof(INumber<>.Min), [Memberwise, Memberwise]),
                new(typeof(INumber<>), nameof(INumber<>.Min), [Memberwise, One]),
                new(typeof(INumber<>), nameof(INumber<>.Clamp), [Memberwise, Memberwise, Memberwise]),
                new(typeof(INumber<>), nameof(INumber<>.Clamp), [Memberwise, One, One]),
                new(typeof(INumber<>), nameof(INumber<>.CopySign), [Memberwise, Memberwise]),
                new(typeof(INumber<>), nameof(INumber<>.CopySign), [Memberwise, One]),

                // INumberBase<>
                new(typeof(INumberBase<>), nameof(INumberBase<>.Abs), [Memberwise]),

                // IBinaryNumber<>
                new(typeof(IBinaryNumber<>), nameof(IBinaryNumber<>.Log2), [Memberwise]),

                // IFloatingPointIeee754<>
                new(typeof(IFloatingPointIeee754<>), nameof(IFloatingPointIeee754<>.Lerp), [Memberwise, Memberwise, One]),

                // IPowerFunctions<>
                new(typeof(IPowerFunctions<>), nameof(IPowerFunctions<>.Pow), [Memberwise, One]),
                new(typeof(IPowerFunctions<>), nameof(IPowerFunctions<>.Pow), [Memberwise, Memberwise]),

                // IRootFunctions<>
                new(typeof(IRootFunctions<>), nameof(IRootFunctions<>.Cbrt), [Memberwise]),
                new(typeof(IRootFunctions<>), nameof(IRootFunctions<>.Sqrt), [Memberwise]),
                new(typeof(IRootFunctions<>), nameof(IRootFunctions<>.RootN), [Memberwise, One]),
                new(typeof(IRootFunctions<>), nameof(IRootFunctions<>.RootN), [Memberwise, Memberwise]),
                new(typeof(IRootFunctions<>), nameof(IRootFunctions<>.Hypot), [Memberwise, Memberwise]),

                // ILogarithmicFunctions<>,
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.Log), [Memberwise]),
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.Log), [Memberwise, One]),
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.LogP1), [Memberwise]),
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.Log2), [Memberwise]),
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.Log2P1), [Memberwise]),
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.Log10), [Memberwise]),
                new(typeof(ILogarithmicFunctions<>), nameof(ILogarithmicFunctions<>.Log10P1), [Memberwise]),

                // IExponentialFunctions<>
                new(typeof(IExponentialFunctions<>), nameof(IExponentialFunctions<>.Exp), [Memberwise]),
                new(typeof(IExponentialFunctions<>), nameof(IExponentialFunctions<>.ExpM1), [Memberwise]),
                new(typeof(IExponentialFunctions<>), nameof(IExponentialFunctions<>.Exp2), [Memberwise]),
                new(typeof(IExponentialFunctions<>), nameof(IExponentialFunctions<>.Exp2M1), [Memberwise]),
                new(typeof(IExponentialFunctions<>), nameof(IExponentialFunctions<>.Exp10), [Memberwise]),
                new(typeof(IExponentialFunctions<>), nameof(IExponentialFunctions<>.Exp10M1), [Memberwise]),

                //ITrigonometricFunctions<>
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.Acos), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.AcosPi), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.Asin), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.AsinPi), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.Atan), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.AtanPi), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.Cos), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.CosPi), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.Sin), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.SinPi), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.Tan), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.TanPi), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.DegreesToRadians), [Memberwise]),
                new(typeof(ITrigonometricFunctions<>), nameof(ITrigonometricFunctions<>.RadiansToDegrees), [Memberwise]),

                //IHyperbolicFunctions<>
                new(typeof(IHyperbolicFunctions<>), nameof(IHyperbolicFunctions<>.Acosh), [Memberwise]),
                new(typeof(IHyperbolicFunctions<>), nameof(IHyperbolicFunctions<>.Asinh), [Memberwise]),
                new(typeof(IHyperbolicFunctions<>), nameof(IHyperbolicFunctions<>.Atanh), [Memberwise]),
                new(typeof(IHyperbolicFunctions<>), nameof(IHyperbolicFunctions<>.Cosh), [Memberwise]),
                new(typeof(IHyperbolicFunctions<>), nameof(IHyperbolicFunctions<>.Sinh), [Memberwise]),
                new(typeof(IHyperbolicFunctions<>), nameof(IHyperbolicFunctions<>.Tanh), [Memberwise]),
            };

            var sizes = Enumerable.Range(2, 3);
            var vectorModels =
                (from d in sizes
                 from integral in new[] { true, false }
                 select new Model.Vector(d, integral, sizes, extensions)).ToList();

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
