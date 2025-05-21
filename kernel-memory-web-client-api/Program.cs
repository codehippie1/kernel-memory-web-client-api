using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DataFormats;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IKernelMemory>(_ =>
{
    // Change the endpoint URL to match your running Kernel Memory service
    return new MemoryWebClient("http://127.0.0.1:9001");
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/ask", async (
    [FromServices] IKernelMemory memory,
    [FromBody] AskRequest request) =>
{
    try 
    {
        Console.WriteLine("=== Ask Request Details ===");
        Console.WriteLine($"Request Input: {request.Input}");
        Console.WriteLine($"Request Filters: {JsonSerializer.Serialize(request.Filters)}");
        Console.WriteLine("=========================");

        if (string.IsNullOrEmpty(request.Input))
        {
            return Results.BadRequest("Input cannot be empty");
        }

        var filters = new List<MemoryFilter>();
        if (request.Filters != null && request.Filters.Count > 0)
        {
            // Create a separate filter for each key-value pair (OR logic)
            foreach (var f in request.Filters)
            {
                if (!string.IsNullOrEmpty(f.Key) && !string.IsNullOrEmpty(f.Value))
                {
                    var filter = MemoryFilters.ByTag(f.Key, f.Value);
                    filters.Add(filter);
                }
            }
        }

        // Log the raw filter objects for debugging
        Console.WriteLine("Raw filters:");
        foreach (var f in filters)
        {
            var filterDict = new Dictionary<string, string>();
            foreach (var tag in f)
            {
                // Take the first value from the list if it exists
                if (tag.Value != null && tag.Value.Count > 0)
                {
                    filterDict[tag.Key] = tag.Value[0];
                }
            }
            Console.WriteLine($"Filter: {JsonSerializer.Serialize(filterDict)}");
        }
        
        var result = await memory.AskAsync(
            question: request.Input,
            //filter: MemoryFilters.ByTag("secrecy", "private")
            filters: filters
        );
        
        Console.WriteLine($"KernelMemoryWebClientApi ask result -------------------------------------------------> {result.Result}");
        return Results.Ok(result.Result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in /ask endpoint: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error processing request: {ex.Message}");
    }
});

app.MapPost("/uploadText", async (
    [FromServices] IKernelMemory memory,
    [FromBody] UploadTextRequest request) =>
{
    Console.WriteLine("=== UploadText Request ===");
    Console.WriteLine($"Text: {request.Text}");
    Console.WriteLine($"DocumentId: {request.DocumentId}");
    Console.WriteLine($"Index: {request.Index}");
    //Console.WriteLine($"Steps: {string.Join(", ", request.Steps ?? new List<string>())}");
    Console.WriteLine("Tags:");
    string tagsString = "Tags:\n";
    if (request.Tags != null)
    {
        foreach (var tag in request.Tags)
        {
            tagsString += $"  {tag.Key}: {tag.Value}\n";
        }
    }
    Console.WriteLine(tagsString);
    Console.WriteLine("========================");

    if (string.IsNullOrEmpty(request.Text))
    {
        Console.WriteLine("Error: Text content is null or empty");
        return Results.BadRequest("Text content cannot be null or empty.");
    }

    var tags = new TagCollection();
    if (request.Tags != null)
    {
        foreach (var tag in request.Tags)
        {
            if (!string.IsNullOrEmpty(tag.Key) && !string.IsNullOrEmpty(tag.Value))
            {
                tags.Add(tag.Key, tag.Value);
            }
        }
    }

    Console.WriteLine($"Tags: {JsonSerializer.Serialize(tags)}");

    try
    {
        await memory.ImportTextAsync(
            text: $"Text: {request.Text}\nTags: {tagsString}",
            documentId: request.DocumentId,
            //index: request.Index ?? "default",
            tags: tags,
            //steps: request.Steps,
            cancellationToken: default
        );

        var status = await memory.GetDocumentStatusAsync(request.DocumentId);

        while (status is { Completed: false })
        {
            Console.WriteLine("* Work in progress...");
            Console.WriteLine("Steps:     " + string.Join(", ", status.Steps));
            Console.WriteLine("Completed: " + string.Join(", ", status.CompletedSteps));
            Console.WriteLine("Remaining: " + string.Join(", ", status.RemainingSteps));
            //Console.WriteLine($"Tags: {JsonSerializer.Serialize(status.Tags)}");
            Console.WriteLine($"Tag 1: {status.Tags.FirstOrDefault().Value}");
            Console.WriteLine();
            await Task.Delay(TimeSpan.FromSeconds(3));
            status = await memory.GetDocumentStatusAsync(request.DocumentId);
        }

        Console.WriteLine("Text uploaded successfully");
        return Results.Ok("Text uploaded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during import: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error during import: {ex.Message}");
    }
});

app.MapPost("/uploadFile", async (
    [FromServices] IKernelMemory memory,
    HttpRequest httpRequest) =>
{
    if (!httpRequest.HasFormContentType || httpRequest.Form.Files.Count == 0)
        return Results.BadRequest("File upload required.");

    var file = httpRequest.Form.Files[0];
    var documentId = httpRequest.Form["documentId"].FirstOrDefault();
    var index = httpRequest.Form["index"].FirstOrDefault();
    //var steps = httpRequest.Form["steps"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    var tagsJson = httpRequest.Form["tags"].FirstOrDefault();
    
    var tags = new TagCollection();
    if (!string.IsNullOrEmpty(tagsJson))
    {
        var tagDict = JsonSerializer.Deserialize<Dictionary<string, string>>(tagsJson);
        if (tagDict != null)
        {
            foreach (var tag in tagDict)
            {
                tags.Add(tag.Key, tag.Value);
            }
        }
    }

    using var stream = file.OpenReadStream();
    await memory.ImportDocumentAsync(
        content: stream,
        fileName: file.FileName,
        documentId: documentId,
        index: index ?? "default",
        tags: tags,
        //steps: steps,
        cancellationToken: default
    );

    return Results.Ok("File uploaded successfully.");
});

app.MapDelete("/delete", async (
    [FromServices] IKernelMemory memory,
    [FromQuery] string documentId,
    [FromQuery] string index) =>
{
    await memory.DeleteDocumentAsync(documentId, index);
    return Results.Ok("Document deleted.");
});

app.Run();

public record AskRequest(
    string Input,
    string Index = "default",
    Dictionary<string, string>? Filters = null
);

public record UploadTextRequest(
    string Text,
    string? DocumentId = null,
    //List<string>? Steps = null,
    Dictionary<string, string>? Tags = null,
    string Index = "default"
);