<Query Kind="Program">
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
</Query>

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Dimension = (int rows, int columns);

static int CompareDimensions(Dimension a, Dimension b)
{
    int comp;
    var aMax = Math.Max(a.rows, a.columns);
    var bMax = Math.Max(b.rows, b.columns);

    comp = aMax.CompareTo(bMax);
    if (comp != 0) return comp;

    var aMin = Math.Min(a.rows, a.columns);
    var bMin = Math.Min(b.rows, b.columns);

    comp = aMin.CompareTo(bMin);
    return comp;
}

static Comparer<Dimension> DimensionComparer = Comparer<Dimension>.Create(CompareDimensions);

static NameSyntax SilkNetMaths = ParseName("Silk.NET.Maths");

void Main()
{
    var outputPath = @"C:\Users\otac0n\Projects\Silk.NET\sources\Maths\Maths\";
    void SaveType(TypeDeclarationSyntax type)
    {
        var filename = Path.Combine(outputPath, type.Identifier.ToFullString() + ".gen.cs");
        var code = NamespaceDeclaration(SilkNetMaths)
            .AddUsings(
                UsingDirective(ParseName("System.Diagnostics.CodeAnalysis")),
                UsingDirective(ParseName("System.Numerics")))
            .AddMembers(type)
            .NormalizeWhitespace()
            .ToFullString();
        File.WriteAllText(filename, code);
    }

    var dims = new HashSet<Dimension>();
    for (var r = 1; r < 4; r++)
        for (var c = 1; c < 4; c++)
            dims.Add((r + 1, c + 1));
    dims.Add((5, 4));

    var multiplicationOperators =
        (from left in dims
         from right in dims
         where left.columns == right.rows
         let o = (left.rows, right.columns)
         where dims.Contains(o)
         select (left, right))
         .ToLookup(m => new[] { m.left, m.right }.Max(DimensionComparer));

    foreach (var integral in new[] { true, false })
    {
        for (var i = 2; i <= 4; i++)
        {
            //SaveType(MakeVectorType(i, integral));
        }

        foreach (var dim in dims)
        {
            SaveType(MakeMatrixType(dim, integral, multiplicationOperators[dim]));
        }
    }
}

string[] VectorFieldNames = ["X", "Y", "Z", "W"];

SyntaxTriviaList InheritDoc = ParseLeadingTrivia("/// <inheridoc/>" + Environment.NewLine);

(string Name, SyntaxToken Identifier, TypeSyntax TypeName) GetTypeIdentifiers(string name, bool integral, TypeSyntax genericArgument)
{
    var extendedName = $"{name}{(integral ? 'I' : 'F')}";
    var identifier = Identifier(extendedName);
    var typeName = GenericName(identifier)
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    genericArgument)));
    return (extendedName, identifier, typeName);
}

GenericNameSyntax MakeNumberConstraint(bool integral, TypeSyntax typeParameter)
{
    return GenericName(Identifier(integral ? "IBinaryInteger" : "IFloatingPointIeee754"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SingletonSeparatedList<TypeSyntax>(
                        typeParameter)));
}

TypeDeclarationSyntax MakeVectorType(int elements, bool integral)
{
    var TParam = Identifier("T");
    var T = IdentifierName("T");

    var (name, identifier, typeName) = GetTypeIdentifiers($"Vector{elements}", integral, T);
    var constraintType = MakeNumberConstraint(integral, T);

    TypeDeclarationSyntax type = StructDeclaration(identifier)
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PartialKeyword)))
        .WithTypeParameterList(
            TypeParameterList(
                SingletonSeparatedList<TypeParameterSyntax>(
                    TypeParameter(
                        TParam))))
        .WithConstraintClauses(
            SingletonList<TypeParameterConstraintClauseSyntax>(
                TypeParameterConstraintClause(
                    T)
                .WithConstraints(
                    SingletonSeparatedList<TypeParameterConstraintSyntax>(
                        TypeConstraint(
                            constraintType)))));

    var storage = new List<SimpleNameSyntax>();
    for (var i = 0; i < elements; i++)
    {
        type = type.AddMembers(
            FieldDeclaration(
                VariableDeclaration(
                    T,
                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(
                            Identifier(VectorFieldNames[i])))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword))));
        storage.Add(IdentifierName(VectorFieldNames[i]));
    }

    type = AddEquals(type, typeName, storage);

    return type;
}

TypeDeclarationSyntax MakeMatrixType(Dimension dim, bool integral, IEnumerable<(Dimension, Dimension)> multiplyOperators)
{
    var TParam = Identifier("T");
    var T = IdentifierName("T");

    var (name, identifier, typeName) = GetTypeIdentifiers($"Matrix{dim.rows}x{dim.columns}", integral, T);
    var constraintType = MakeNumberConstraint(integral, T);

    var (_, _, rowTypeName) = GetTypeIdentifiers($"Vector{dim.columns}", integral, T);

    TypeDeclarationSyntax type = StructDeclaration(identifier)
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PartialKeyword)))
        .WithTypeParameterList(
            TypeParameterList(
                SingletonSeparatedList<TypeParameterSyntax>(
                    TypeParameter(
                        TParam))))
        .WithConstraintClauses(
            SingletonList<TypeParameterConstraintClauseSyntax>(
                TypeParameterConstraintClause(
                    T)
                .WithConstraints(
                    SingletonSeparatedList<TypeParameterConstraintSyntax>(
                        TypeConstraint(
                            constraintType)))));

    var storage = new List<SimpleNameSyntax>(dim.rows);
    for (var r = 1; r <= dim.rows; r++)
    {
        var rowName = $"Row{r}";
        var rowIdentifier = IdentifierName(rowName);
        type = type.AddMembers(
            FieldDeclaration(
                VariableDeclaration(
                    rowTypeName,
                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(
                            Identifier(rowName)))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithLeadingTrivia(
                    ParseLeadingTrivia($"/// <summary>The {AddOrdinal(r)} row of the matrix represented as a vector.</summary>" + Environment.NewLine)));
        storage.Add(rowIdentifier);
    }

    type = type.AddMembers(
        ConstructorDeclaration(identifier)
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(
                ParameterList(
                    SeparatedList(
                        Enumerable.Range(1, dim.rows)
                            .Select(r => Parameter(Identifier($"row{r}")).WithType(rowTypeName)),
                        Enumerable.Range(1, dim.rows - 1).Select(r => Token(SyntaxKind.CommaToken)))))
            .WithExpressionBody(
                ArrowExpressionClause(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        TupleExpression(
                            SeparatedList(
                                storage.Select(Argument),
                                storage.Skip(1).Select(_ => Token(SyntaxKind.CommaToken)))),
                        TupleExpression(
                            SeparatedList(
                                Enumerable.Range(1, dim.rows)
                                    .Select(r => Argument(IdentifierName($"row{r}"))),
                                Enumerable.Range(1, dim.rows - 1).Select(_ => Token(SyntaxKind.CommaToken)))))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken)),
        IndexerDeclaration(RefType(rowTypeName))
            .WithAttributeLists(
                SingletonList(
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName("UnscopedRef"))))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(
                BracketedParameterList(
                    SingletonSeparatedList<ParameterSyntax>(
                        Parameter(
                            Identifier("row"))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword))))))
            .WithAccessorList(AccessorList(SingletonList(
                AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration)
                    .WithBody(
                        Block(
                            SwitchStatement(
                                IdentifierName("row"))
                            .WithSections(
                                List<SwitchSectionSyntax>(
                                    storage.Select((s, i) => SwitchSection()
                                        .WithLabels(
                                            SingletonList<SwitchLabelSyntax>(
                                                CaseSwitchLabel(
                                                    LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        Literal(i)))))
                                        .WithStatements(
                                            SingletonList<StatementSyntax>(
                                                ReturnStatement(
                                                    RefExpression(s))))))),
                            ThrowStatement(
                                ObjectCreationExpression(
                                    IdentifierName("ArgumentOutOfRangeException"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                InvocationExpression(
                                                    IdentifierName(
                                                        Identifier(
                                                            TriviaList(),
                                                            SyntaxKind.NameOfKeyword,
                                                            "nameof",
                                                            "nameof",
                                                            TriviaList())))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                            Argument(
                                                                IdentifierName("row"))))))))))))))));

    for (var r = 1; r <= dim.rows; r++)
    {
        for (var c = 1; c <= dim.columns; c++)
        {
            type = type.AddMembers(
                PropertyDeclaration(
                    T,
                    Identifier($"M{r}{c}"))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                storage[r - 1],
                                IdentifierName(VectorFieldNames[c - 1]))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken))
                    .WithLeadingTrivia(
                        ParseLeadingTrivia($"/// <summary>Gets the element in the {AddOrdinal(r)} row and {AddOrdinal(c)} column of the matrix.</summary>" + Environment.NewLine)));
        }
    }

    type = AddEquals(type, typeName, storage);

    type = type.AddMembers(
        MakeMatrixAddition(dim, integral, T),
        MakeMatrixSubtraction(dim, integral, T));
    foreach (var (leftSize, rightSize) in multiplyOperators)
    {
        type = type.AddMembers(MakeMatrixMultiplication(leftSize, rightSize, integral, T));
    }

    return type;
}

OperatorDeclarationSyntax MakeMatrixAddition(Dimension size, bool integral, TypeSyntax genericArgument)
{
    var (_, _, typeName) = GetTypeIdentifiers($"Matrix{size.rows}x{size.columns}", integral, genericArgument);

    var left = IdentifierName("left");
    var right = IdentifierName("right");

    var implementation =
        ImplicitObjectCreationExpression()
            .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        Enumerable.Range(1, size.rows)
                            .Select(r => IdentifierName($"Row{r}"))
                            .Select(r =>
                                Argument(
                                    BinaryExpression(
                                        SyntaxKind.AddExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            left,
                                            r),
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            right,
                                            r)))),
                        Enumerable.Range(1, size.rows - 1).Select(r => Token(SyntaxKind.CommaToken)))));

    return OperatorDeclaration(
        typeName,
        Token(SyntaxKind.PlusToken))
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword)))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
                    Parameter(
                        Identifier("left"))
                        .WithType(typeName),
                    Token(SyntaxKind.CommaToken),
                    Parameter(
                        Identifier("right"))
                        .WithType(typeName)
                })))
        .WithExpressionBody(
            ArrowExpressionClause(implementation))
        .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
        .WithLeadingTrivia(ParseLeadingTrivia("""
            /// <summary>Adds two matrices together.</summary>
            /// <param name="left">The first source matrix.</param>
            /// <param name="right">The second source matrix.</param>
            /// <returns>The result of the addition.</returns>
        """ + Environment.NewLine));
}

OperatorDeclarationSyntax MakeMatrixSubtraction(Dimension size, bool integral, TypeSyntax genericArgument)
{
    var (_, _, typeName) = GetTypeIdentifiers($"Matrix{size.rows}x{size.columns}", integral, genericArgument);

    var left = IdentifierName("left");
    var right = IdentifierName("right");

    var implementation =
        ImplicitObjectCreationExpression()
            .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        Enumerable.Range(1, size.rows)
                            .Select(r => IdentifierName($"Row{r}"))
                            .Select(r =>
                                Argument(
                                    BinaryExpression(
                                        SyntaxKind.SubtractExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            left,
                                            r),
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            right,
                                            r)))),
                        Enumerable.Range(1, size.rows - 1).Select(r => Token(SyntaxKind.CommaToken)))));

    return OperatorDeclaration(
        typeName,
        Token(SyntaxKind.MinusToken))
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword)))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
                    Parameter(
                        Identifier("left"))
                        .WithType(typeName),
                    Token(SyntaxKind.CommaToken),
                    Parameter(
                        Identifier("right"))
                        .WithType(typeName)
                })))
        .WithExpressionBody(
            ArrowExpressionClause(implementation))
        .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
        .WithLeadingTrivia(ParseLeadingTrivia("""
            /// <summary>Subtracts the second matrix from the first.</summary>
            /// <param name="left">The first source matrix.</param>
            /// <param name="right">The second source matrix.</param>
            /// <returns>The result of the subtraction.</returns>
        """ + Environment.NewLine));
}

OperatorDeclarationSyntax MakeMatrixMultiplication(Dimension leftSize, Dimension rightSize, bool integral, TypeSyntax genericArgument)
{
    Debug.Assert(leftSize.columns == rightSize.rows);
    var middleSize = leftSize.columns;
    var outputSize = (leftSize.rows, rightSize.columns);
    var (_, _, outputTypeName) = GetTypeIdentifiers($"Matrix{outputSize.rows}x{outputSize.columns}", integral, genericArgument);
    var (_, _, outputRowType) = GetTypeIdentifiers($"Vector{outputSize.rows}", integral, genericArgument);
    var (_, _, leftTypeName) = GetTypeIdentifiers($"Matrix{leftSize.rows}x{leftSize.columns}", integral, genericArgument);
    var (_, _, rightTypeName) = GetTypeIdentifiers($"Matrix{rightSize.rows}x{rightSize.columns}", integral, genericArgument);

    var left = IdentifierName("left");
    var right = IdentifierName("right");

    var implementation =
        ImplicitObjectCreationExpression()
            .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        Enumerable.Range(1, outputSize.rows).Select(r =>
                            Argument(
                                Enumerable.Range(1, middleSize)
                                    .Select(m =>
                                        BinaryExpression(
                                            SyntaxKind.MultiplyExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                left,
                                                IdentifierName($"M{r}{m}")),
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                right,
                                                IdentifierName($"Row{m}"))))
                                    .Aggregate((a, b) =>
                                        BinaryExpression(
                                            SyntaxKind.AddExpression,
                                            a,
                                            b)))),
                        Enumerable.Range(1, outputSize.rows - 1).Select(r => Token(SyntaxKind.CommaToken)))));

    return OperatorDeclaration(
        outputTypeName,
        Token(SyntaxKind.AsteriskToken))
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword)))
        .WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
                    Parameter(
                        Identifier("left"))
                        .WithType(leftTypeName),
                    Token(SyntaxKind.CommaToken),
                    Parameter(
                        Identifier("right"))
                        .WithType(rightTypeName)
                })))
        .WithExpressionBody(
            ArrowExpressionClause(implementation))
        .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
        .WithLeadingTrivia(ParseLeadingTrivia("""
            /// <summary>Multiplies a matrix by another matrix.</summary>
            /// <param name="left">The first source matrix.</param>
            /// <param name="right">The second source matrix.</param>
            /// <returns>The result of the multiplication.</returns>
        """ + Environment.NewLine));
}

TypeDeclarationSyntax AddEquals(TypeDeclarationSyntax type, TypeSyntax typeName, IEnumerable<SimpleNameSyntax> storage)
{
    type = (TypeDeclarationSyntax)type
        .AddBaseListTypes(
                SimpleBaseType(
                    GenericName(
                        Identifier("IEquatable"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                typeName)))));
    return type
        .AddMembers(
            MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier("Equals"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                Identifier("obj"))
                                .WithType(
                                    NullableType(
                                        PredefinedType(
                                            Token(SyntaxKind.ObjectKeyword)))))))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        BinaryExpression(
                            SyntaxKind.LogicalAndExpression,
                            IsPatternExpression(
                                IdentifierName("obj"),
                                DeclarationPattern(
                                    typeName,
                                    SingleVariableDesignation(
                                        Identifier("other")))),
                            InvocationExpression(
                                IdentifierName("Equals"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                IdentifierName("other"))))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(InheritDoc),
            MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier("Equals"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                Identifier("other"))
                                .WithType(
                                    typeName))))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            ThisExpression(),
                            IdentifierName("other"))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(InheritDoc),
            MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.IntKeyword)),
                Identifier("GetHashCode"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("HashCode"),
                                IdentifierName("Combine")))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    storage.Select(n => Argument(n)),
                                    storage.Skip(1).Select(_ => Token(SyntaxKind.CommaToken)))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(InheritDoc),
            OperatorDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Token(SyntaxKind.EqualsEqualsToken))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
                            Parameter(
                                Identifier("left"))
                                .WithType(typeName),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("right"))
                                .WithType(typeName)
                        })))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        storage.Select(f =>
                            BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("left"),
                                    f),
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("right"),
                                    f)))
                            .Aggregate((a, b) =>
                                BinaryExpression(
                                    SyntaxKind.LogicalAndExpression,
                                    a,
                                    b))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(ParseLeadingTrivia("""
                    /// <summary>Returns a boolean indicating whether the given two matrices are equal.</summary>
                    /// <param name="left">The first matrix to compare.</param>
                    /// <param name="right">The second matrix to compare.</param>
                    /// <returns><c>true</c> if the given matrices are equal; <c>false</c> otherwise.</returns>
                """ + Environment.NewLine)),
            OperatorDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Token(SyntaxKind.ExclamationEqualsToken))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[] {
                            Parameter(
                                Identifier("left"))
                                .WithType(typeName),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("right"))
                                .WithType(typeName)
                        })))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        PrefixUnaryExpression(
                            SyntaxKind.LogicalNotExpression,
                            ParenthesizedExpression(
                                BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    IdentifierName("left"),
                                    IdentifierName("right"))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(ParseLeadingTrivia("""
                    /// <summary>Returns a boolean indicating whether the given two matrices are not equal.</summary>
                    /// <param name="left">The first matrix to compare.</param>
                    /// <param name="right">The second matrix to compare.</param>
                    /// <returns><c>true</c> if the given matrices are not equal; <c>false</c> otherwise.</returns>
                """ + Environment.NewLine)));
}

public static string AddOrdinal(int num)
{
    if (num <= 0) return num.ToString();

    switch (num % 100)
    {
        case 11:
        case 12:
        case 13:
            return num + "th";
    }

    switch (num % 10)
    {
        case 1:
            return num + "st";
        case 2:
            return num + "nd";
        case 3:
            return num + "rd";
        default:
            return num + "th";
    }
}
