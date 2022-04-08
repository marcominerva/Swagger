using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AwesomeBackend.Workaround;

// todo: remove when the issue is resolved ... https://github.com/dotnet/aspnetcore/issues/39802
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ProducesAttribute : Microsoft.AspNetCore.Mvc.ProducesAttribute
{
    public ProducesAttribute(string contentType, params string[] additionalContentTypes)
        : base(contentType, additionalContentTypes) { }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: ProblemDetails })
        {
            return;
        }

        base.OnResultExecuting(context);
    }
}
