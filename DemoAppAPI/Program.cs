using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
           // ValidateIssuer = true, // Validate the server that created the token
            //ValidateAudience = true, // Validate the recipient of the token
           // ValidateLifetime = true, // Validate the token's expiry
            ValidateIssuerSigningKey = true, // Validate the signing key

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddAuthorization();


//swagger

builder.Services.AddSwaggerGen(options =>
{
    // Define the security scheme (Bearer token)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter into field the word 'Bearer' followed by a space and then your JWT value (e.g. 'Bearer eyJ...').",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // The scheme for Bearer authentication
        BearerFormat = "JWT" // Optional: specify the format as JWT
    });

    // Add a security requirement to all endpoints (globally)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Refers to the name defined above
                }
            },
            Array.Empty<string>() // No specific scopes needed for simple JWT bearer
        }
    });

  
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
