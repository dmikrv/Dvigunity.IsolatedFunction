using System.Net;

namespace Dvigunity.IsolatedFunction.Exceptions;

public class AuthorizationException : IsolatedFunctionException
{
    public AuthorizationException(string localizedMessage, string? localizedTitle)
        : base(localizedMessage, localizedTitle, HttpStatusCode.Forbidden)
    {
    }
}