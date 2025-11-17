using MailDraftApi;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;


var builder = WebApplication.CreateBuilder(args);

// ------------------------
// 1) הגדרת תיקיית Uploads
// ------------------------
var uploadsFolder = Path.Combine(builder.Environment.ContentRootPath, "Uploads", "Cv");
Directory.CreateDirectory(uploadsFolder);

builder.Services.AddSingleton(new UploadSettings { CvFolder = uploadsFolder });

// ------------------------
// 2) הגדרת CORS
// ------------------------
var allowFrontend = "_allowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowFrontend, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")   // ה-React שלך
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ------------------------
// 3) שירותי MVC
// ------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // ? זה אומר ל-Swagger ש-IFormFile הוא קובץ (binary)
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});


// ------------------------
// 4) בניית ה-App
// ------------------------
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ------------------------
// 5) שימוש ב-CORS
// ------------------------
app.UseCors(allowFrontend);

// ------------------------
// 6) Static Files (uploads)
// ------------------------
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/uploads"
});

// ------------------------
// 7) Routing + Controllers
// ------------------------
app.UseRouting();
app.MapControllers();

// ------------------------
// 8) הפעלת השרת
// ------------------------
app.Run();
