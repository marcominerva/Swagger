using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using AwesomeBackend.Authentication;
using AwesomeBackend.Authentication.Models;
using AwesomeBackend.BusinessLayer.Services;
using AwesomeBackend.DataAccessLayer;
using AwesomeBackend.Models;
using AwesomeBackend.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((context, builder) =>
{
    builder.AddJsonFile("appsettings.local.json", optional: true);
})
.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, providerOptions =>
    {
        providerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

builder.Services.AddDbContext<AuthenticationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(setup =>
{
    setup.Password.RequiredLength = 6;
    setup.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthenticationDbContext>();

// Get JWT token settings.
var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
var jwtSettings = jwtSection.Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(configureOptions =>
{
    configureOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = jwtSettings.Audience,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
        RequireExpirationTime = true,
        //ClockSkew = TimeSpan.Zero // Default is 5 minutes
    };
});

builder.Services.AddAuthorization(options =>
{
    //options.FallbackPolicy = options.DefaultPolicy;

    //var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
    //    .RequireClaim("Frullino")
    //    .Build();

    //options.DefaultPolicy = policy;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<DefaultResponseOperationFilter>();
    options.OperationFilter<AuthResponseOperationFilter>();

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AwesomeBackend", Version = "v1" });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insert JWT token with the \"Bearer \" prefix",
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });

    options.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
        Example = new OpenApiString(new DateTime(2022, 04, 08, 16, 22, 0).ToString("yyyy-MM-ddTHH:mm:ssZ"))
    });

    options.UseAllOfToExtendReferenceSchemas();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IRestaurantsService, RestaurantsService>();
builder.Services.AddScoped<IRatingsService, RatingsService>();

var app = builder.Build();
app.UseHttpsRedirection();

var isSwaggerEnabled = builder.Configuration.GetValue<bool>("AppSettings:EnableSwagger");
if (isSwaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My Awesome API");
    });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
