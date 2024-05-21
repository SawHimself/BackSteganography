using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//AllowAnyOrigin - is enabled for debugging. Please use WithOrigins to publish your project !
builder.Services.AddCors(options => options.AddPolicy("OnlyFrontendServer", builder => builder
                    //.WithOrigins("frontend server ip")
                    .AllowAnyOrigin()
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

app.UseCors("OnlyFrontendServer");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

