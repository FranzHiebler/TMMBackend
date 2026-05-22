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

var allowedOrigins = builder.Configuration
	.GetSection("Cors:AllowedOrigins")
	.Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:5173",
				"http://localhost:5174",
				"https://tmmfrontend-production.up.railway.app"
			)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISystemRepository, SystemRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameAssignmentService, GameAssignmentService>();
builder.Services.AddScoped<IGameProposalService, GameProposalService>();
builder.Services.AddScoped<IGameSessionAuthorizationService, GameSessionAuthorizationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IDiscoveryService, DiscoveryService>();
builder.Services.AddSingleton<MongoIndexInitializer>();

builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ILocationLookupService>(sp => sp.GetRequiredService<ILocationService>());

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAdminAuthorizationService, AdminAuthorizationService>();

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

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
