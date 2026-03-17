using MailDraftApi;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;


var builder = WebApplication.CreateBuilder(args);


var uploadsFolder = Path.Combine(builder.Environment.ContentRootPath, "Uploads", "Cv");
Directory.CreateDirectory(uploadsFolder);

builder.Services.AddSingleton(new UploadSettings { CvFolder = uploadsFolder });

var allowFrontend = "_allowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowFrontend, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")  
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});



var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseCors(allowFrontend);


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/uploads"
});


app.UseRouting();
app.MapControllers();

app.Run();
