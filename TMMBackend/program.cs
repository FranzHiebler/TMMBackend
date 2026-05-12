using System.Text.Json.Serialization;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Settings
builder.Services.Configure<MongoDbSettings>(
	builder.Configuration.GetSection("MongoDb"));

// CORS für React-Frontend
builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:5173",
				"http://localhost:5174"
			)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

// Dependency Injection
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISystemRepository, SystemRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameAssignmentService, GameAssignmentService>();
builder.Services.AddScoped<IGameProposalService, GameProposalService>();
builder.Services.AddScoped<IGameSessionAuthorizationService, GameSessionAuthorizationService>();

builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ILocationLookupService>(sp => sp.GetRequiredService<ILocationService>());

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Controller + Swagger
builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();