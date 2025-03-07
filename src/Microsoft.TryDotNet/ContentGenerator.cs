﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Web;

namespace Microsoft.TryDotNet;

public class ContentGenerator
{
    public static Task<string> GenerateEditorPageAsync(HttpRequest request)
    {
        var referer = request.Headers.Referer.FirstOrDefault();
        var hostUri = new Uri(request.Scheme + "://" +  request.Host.Value, UriKind.Absolute);
        var wasmRunnerUri = new Uri(hostUri, "/wasmrunner");
        var commansdUri = new Uri(hostUri, "/commands");
        var enableLogging = false;
        if (request.Query.TryGetValue("enableLogging", out var enableLoggingString))
        {
            enableLogging = enableLoggingString.FirstOrDefault()?.ToLowerInvariant() == "true";
        }
        var configuration = new
        {
            wasmRunnerUrl = wasmRunnerUri.AbsoluteUri,
            commandsUrl = commansdUri.AbsoluteUri,
            refererUrl = !string.IsNullOrWhiteSpace(referer) ? new Uri(referer, UriKind.Absolute) : null,
            enableLogging
        };

        var configString = JsonSerializer.Serialize(configuration);

        var value =$@"<!doctype html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>TryDotNet Editor</title>
    <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
    <script defer=""defer"" src=""api/editor/app.bundle.js"" id=""trydotnet-editor-script"" data-trydotnet-configuration=""{HttpUtility.HtmlAttributeEncode(configString)}""></script>
    <script defer=""defer"" src=""api/editor/editor.worker.bundle.js""></script>
    <script defer=""defer"" src=""api/editor/json.worker.bundle.js""></script>
    <script defer=""defer"" src=""api/editor/css.worker.bundle.js""></script>
    <script defer=""defer"" src=""api/editor/html.worker.bundle.js""></script>
    <script defer=""defer"" src=""api/editor/ts.worker.bundle.js""></script>
</head>
<body>
</body>
</html>";

        return Task.FromResult(value);
    }
}