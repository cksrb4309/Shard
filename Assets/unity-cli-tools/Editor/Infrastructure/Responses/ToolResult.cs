using System;
using UnityCliConnector;

namespace UnityCliTools.Infrastructure.Responses
{

public static class ToolResult
{
    public static SuccessResponse Success(string message, object data = null)
    {
        return new SuccessResponse(message, data);
    }

    public static ErrorResponse Error(string message, object data = null)
    {
        return new ErrorResponse(message, data);
    }

    public static object FromException(Exception ex, string message)
    {
        return Error(
            message,
            new
            {
                error = ex.Message
            });
    }
}
}
