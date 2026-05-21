using System.Net;
using TabletopMatchMaker.Services;

namespace TabletopMatchMaker.Infrastructure;

public class GlobalExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionMiddleware> _logger;

	public GlobalExceptionMiddleware(
		RequestDelegate next,
		ILogger<GlobalExceptionMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception ex)
	{
		var statusCode = ex switch
		{
			DomainException => HttpStatusCode.BadRequest,
			KeyNotFoundException => HttpStatusCode.NotFound,
			UnauthorizedAccessException => HttpStatusCode.Forbidden,
			_ => HttpStatusCode.InternalServerError
		};

		if (statusCode == HttpStatusCode.InternalServerError)
			_logger.LogError(ex, "Unerwarteter Serverfehler");
		else
			_logger.LogWarning(ex, "Behandelter Fehler: {Message}", ex.Message);

		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)statusCode;

		var response = new
		{
			error = ex.Message
		};

		await context.Response.WriteAsJsonAsync(response);
	}
}