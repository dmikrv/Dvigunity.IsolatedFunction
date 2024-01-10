using System.Net;

namespace Dvigunity.IsolatedFunction.Exceptions;

/// <summary>
///     Base exception for all domain-based exceptions.
/// </summary>
public class IsolatedFunctionException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IsolatedFunctionException" /> class.
    /// </summary>
    /// <param name="localizedMessage">The error message.</param>
    /// <param name="localizedTitle">The error title.</param>
    /// <param name="httpStatusCode">The http status code.</param>
    public IsolatedFunctionException(
        string? localizedMessage,
        string? localizedTitle,
        HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        : base(localizedMessage)
    {
        Title = localizedTitle;
        StatusCode = httpStatusCode;
    }

    public string? Title { get; }

    public HttpStatusCode StatusCode { get; }
}