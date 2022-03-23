// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.DotNet.Interactive.CSharpProject;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using Microsoft.TryDotNet;

IEnumerable<IKernelCommandEnvelope> ReadCommands(JsonElement bundle)
{
    foreach (var commandEnvelope in bundle.GetProperty("commands").EnumerateArray())
    {
        var rawText = commandEnvelope.GetRawText();
        var envelope = KernelCommandEnvelope.Deserialize(rawText);

        yield return envelope;
    }
}

var builder = WebApplication.CreateBuilder(args);

CSharpProjectKernel.RegisterEventsAndCommands();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();


app.MapPost("/commands", async (IConfiguration config, HttpRequest request) =>
    {
        var kernelEvents = new List<KernelEvent>();
        await using (var requestBody = request.Body)
        {
            using var kernel = new CSharpProjectKernel("project-kernel");
            var body = await new StreamReader(requestBody).ReadToEndAsync();

            var bundle = JsonDocument.Parse(body).RootElement;
          
            kernel.KernelEvents.Subscribe(e => kernelEvents.Add(e));

            foreach (var commandEnvelope in ReadCommands(bundle))
            {
                var results = await kernel.SendAsync(commandEnvelope.Command, CancellationToken.None);
            }
        }
        var eventBundle = new { events = kernelEvents.Select(e => KernelEventEnvelope.Create(e).ToJsonElement()) };
        return Results.Json(eventBundle, statusCode: 200);
    })
    .WithName("commands");

app.Run();
