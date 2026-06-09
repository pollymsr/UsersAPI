using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "Erro interno do servidor";
        var detail = _environment.IsDevelopment() ? exception.Message : "Ocorreu um erro interno no servidor.";

        if (exception is UnauthorizedAccessException)
        {
            statusCode = StatusCodes.Status403Forbidden;
            title = "Acesso Negado";
            detail = "Você não tem permissão para acessar este recurso.";
        }
        else if (exception is InvalidOperationException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Operação Inválida";
            detail = exception.Message;
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            title = "Não Encontrado";
            detail = exception.Message;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(problem);
    }
}
