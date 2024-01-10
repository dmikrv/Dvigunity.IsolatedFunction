using Microsoft.Azure.Functions.Worker;

namespace Dvigunity.IsolatedFunction.FuncContextAccessor;

public interface IFunctionContextAccessor
{
    FunctionContext? FunctionContext { get; set; }
}