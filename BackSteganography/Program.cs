using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddPolicy("HiEveryone", builder => builder
                    .WithOrigins("http://127.0.0.1:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod())
);

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = 1073741824;
    x.MultipartBodyLengthLimit = 1073741824;
});

builder.WebHost.ConfigureKestrel(opts => {
    opts.Limits.MaxRequestBodySize = 1073741824;
});

var app = builder.Build();

app.UseCors("HiEveryone");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
