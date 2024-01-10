namespace Dvigunity.IsolatedFunction.Authentication;

/// <summary>
///     Specifies that the class or method that this attribute is applied to does not require authentication and authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AllowAnonymousAttribute : Attribute
// ReSharper disable once RedundantTypeDeclarationBody
{
}