using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class GalleryMiddleware
{
    private readonly RequestDelegate _next;

    public GalleryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        Console.WriteLine("Middleware invoked.");

        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NarijeDBContext>();

            var originalBodyStream = context.Response.Body;
            using (var newBodyStream = new MemoryStream())
            {
                context.Response.Body = newBodyStream;


                await _next(context);
                if (context.Response.ContentType == null || !context.Response.ContentType.Contains("application/json"))
                {
                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    await newBodyStream.CopyToAsync(originalBodyStream);
                    return;
                }


              else  if (context.Response.ContentType?.Contains("application/json") == true)
                {

                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    var responseBody = await new StreamReader(newBodyStream).ReadToEndAsync();
                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    try
                    {


                        var jsonDocument = JsonDocument.Parse(responseBody);
                        var root = jsonDocument.RootElement;

                        if (root.TryGetProperty("data", out var dataElement))
                        {
                            var dataElements = dataElement;
                            var updatedDataElement = await TraverseAndReplaceGalleryIds(dataElement, dbContext);


                            var updatedResponse = new Dictionary<string, object>();
                            foreach (var property in root.EnumerateObject())
                            {
                                if (property.Name == "data")
                                {
                                    updatedResponse["data"] = JsonDocument.Parse(JsonSerializer.Serialize(updatedDataElement)).RootElement;
                                }
                                else
                                {
                                    updatedResponse[property.Name] = property.Value;
                                }
                            }

                            context.Response.Body = originalBodyStream;
                            context.Response.ContentType = "application/json";
                            if (context.Response.Headers.ContainsKey("Content-Encoding"))
                            {
                                var contentEncoding = context.Response.Headers["Content-Encoding"];
                                context.Response.Headers["Content-Encoding"] = contentEncoding;
                            }

                            context.Response.Headers.Remove("Content-Length");

                            var options = new JsonSerializerOptions
                            {
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                WriteIndented = true
                            };

                            await context.Response.WriteAsync(JsonSerializer.Serialize(updatedResponse, options));
                        }
                        else
                        {

                            context.Response.Body = originalBodyStream;
                            await context.Response.WriteAsync(responseBody);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("JSON Parsing Error:");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Response Body:");
                        Console.WriteLine(responseBody);
                        context.Response.Body = originalBodyStream;
                        await context.Response.WriteAsync(responseBody);
                    }


                }
              
            }
        }
    }
    private async Task<JsonElement> TraverseAndReplaceGalleryIds(JsonElement element, NarijeDBContext dbContext)
    {
     
        if (element.ValueKind == JsonValueKind.Object)
        {
            var updatedObject = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
             
                if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                {
                    updatedObject[property.Name] = await TraverseAndReplaceGalleryIds(property.Value, dbContext);
                }
              
                else if (property.Name == "galleryId" || property.Name == "companyGalleryId")
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var galleryIdString = property.Value.GetString();
                        if (int.TryParse(galleryIdString, out int galleryId))
                        {
                            var gallery = await dbContext.Galleries.FindAsync(galleryId);
                            if (gallery != null)
                            {
                                updatedObject[property.Name] = $"{galleryId}{gallery.SystemFileName}";
                                updatedObject[$"{property.Name}Alt"] = gallery.Alt;
                            }
                            else
                            {
                                updatedObject[property.Name] = null;
                            }
                        }
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out int galleryId))
                    {
                        var gallery = await dbContext.Galleries.FindAsync(galleryId);
                        if (gallery != null)
                        {
                            updatedObject[property.Name] = $"{galleryId}{gallery.SystemFileName}";
                            updatedObject[$"{property.Name}Alt"] = gallery.Alt;
                        }
                        else
                        {
                            updatedObject[property.Name] = null;
                        }
                    }
                }
                else if (property.Name == "imageUrl")
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var imageUrl = property.Value.GetString();
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var parts = imageUrl.Split('/');
                            if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int imageId))
                            {
                                var gallery = await dbContext.Galleries.FindAsync(imageId);
                                if (gallery != null)
                                {
                                    updatedObject[property.Name] = $"{imageUrl}{gallery.SystemFileName}";
                                    updatedObject[$"{property.Name}Alt"] = gallery.Alt;
                                }
                                else
                                {
                                    updatedObject[property.Name] = null;
                                }
                            }
                            else
                            {
                                updatedObject[property.Name] = imageUrl;
                            }
                        }
                        else
                        {
                            updatedObject[property.Name] = null;
                        }
                    }
                }


                else
                {
                    updatedObject[property.Name] = property.Value;
                }
            }

           
            return JsonDocument.Parse(JsonSerializer.Serialize(updatedObject)).RootElement;
        }

        
        if (element.ValueKind == JsonValueKind.Array)
        {
            var updatedArray = new List<JsonElement>();

            foreach (var item in element.EnumerateArray())
            {
                
                updatedArray.Add(await TraverseAndReplaceGalleryIds(item, dbContext));
            }

  
            return JsonDocument.Parse(JsonSerializer.Serialize(updatedArray)).RootElement;
        }


        return element;
    }

    private bool IsJsonString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        value = value.Trim();
        return (value.StartsWith("{") && value.EndsWith("}")) || (value.StartsWith("[") && value.EndsWith("]"));
    }

    private string FixInvalidJsonString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        value = value.Trim();
        return value;
    }
}