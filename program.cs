using System.Text.Json.Serialization;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
	builder.Configuration.GetSection("MongoDb"));

builder.Services.Configure<AdminSettings>(
	builder.Configuration.GetSection("Admin"));

builder.Services.Configure<AuthSettings>(
	builder.Configuration.GetSection("Auth"));

var allowedOrigins = builder.Configuration
	.GetSection("Cors:AllowedOrigins")
	.Get<string[]>() ?? [
		"http://localhost:5173",
		"http://localhost:5174",
		"https://tmmfrontend-production.up.railway.app"
	];

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		policy
			.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISystemRepository, SystemRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IPlayRequestRepository, PlayRequestRepository>();
builder.Services.AddScoped<IEventSeriesRepository, EventSeriesRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IUserAuthProviderRepository, UserAuthProviderRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameAssignmentService, GameAssignmentService>();
builder.Services.AddScoped<IGameProposalService, GameProposalService>();
builder.Services.AddScoped<IGamePlanningService, GamePlanningService>();
builder.Services.AddScoped<IGameSessionAuthorizationService, GameSessionAuthorizationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMailNotificationService, NoopMailNotificationService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IDiscoveryService, DiscoveryService>();
builder.Services.AddScoped<IPlayRequestService, PlayRequestService>();
builder.Services.AddScoped<IEventSeriesService, EventSeriesService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddSingleton<MongoIndexInitializer>();

builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ILocationLookupService>(sp => sp.GetRequiredService<ILocationService>());

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAdminAuthorizationService, AdminAuthorizationService>();
builder.Services.AddScoped<IAuthSessionService, AuthSessionService>();
builder.Services.AddHttpClient();

builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("x-user-id", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "x-user-id",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Test UserId"
	});

	options.AddSecurityDefinition("x-display-name", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "x-display-name",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Test DisplayName"
	});

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "x-user-id"
				}
			},
			Array.Empty<string>()
		},
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "x-display-name"
				}
			},
			Array.Empty<string>()
		}
	});
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var indexInitializer = scope.ServiceProvider.GetRequiredService<MongoIndexInitializer>();
	await indexInitializer.EnsureIndexesAsync();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("Frontend");

app.Use(async (context, next) =>
{
	var path = context.Request.Path;
	if (path.StartsWithSegments("/api") && !path.StartsWithSegments("/api/Auth"))
	{
		var sessions = context.RequestServices.GetRequiredService<IAuthSessionService>();
		var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
		var hasDevelopmentHeaders =
			environment.IsDevelopment()
			&& context.Request.Headers.ContainsKey("x-user-id")
			&& context.Request.Headers.ContainsKey("x-display-name");

		if (sessions.GetCurrentSession() == null && !hasDevelopmentHeaders)
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new { error = "Anmeldung erforderlich." });
			return;
		}
	}

	await next();
});

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
	status = "ok",
	service = "Tabletop Matchmaker Backend",
	timeUtc = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
