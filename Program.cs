// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }


using Notafy.Services;

using OpenCvSharp;

//var imagemrect = InteractiveQuadEditor.Run("C:/Users/Danilo.DESKTOP-B4KOUSG/OneDrive/Desktop/upToOcr/notinha.jpg");
var processeadImage = ImagePreProcessor.Process("C:/Users/Danilo.DESKTOP-B4KOUSG/OneDrive/Desktop/upToOcr/notinhawarp.jpg");

var textExtractor = new TextExtractor();
var text = textExtractor.Extract(processeadImage);

System.Console.WriteLine(text);

// Cv2.ImWrite("saida.png", imagemrect);

// Cv2.ImShow("Processado", imagemrect);

// Cv2.WaitKey();