using System.Net;

namespace Dvigunity.IsolatedFunction.Exceptions;

public class AuthenticationException : IsolatedFunctionException
{
    public AuthenticationException(string localizedMessage, string? localizedTitle)
        : base(localizedMessage, localizedTitle, HttpStatusCode.Unauthorized)
    {
    }
}