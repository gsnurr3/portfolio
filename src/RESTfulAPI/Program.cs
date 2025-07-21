var builder = WebApplication.CreateBuilder(args);

// register MVC controllers
builder.Services.AddControllers();

// register the OpenAPI generator
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// map conventional controllers
app.MapControllers();

app.Run();