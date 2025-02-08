var builder = WebApplication.CreateBuilder(args);

// Dodavanje Swaggera
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dodavanje YARP konfiguracije za reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Uključivanje Swagger UI u razvojnom okruženju
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Mapiranje YARP reverse proxyja
app.MapReverseProxy();

// Pokretanje aplikacije
app.Run();
