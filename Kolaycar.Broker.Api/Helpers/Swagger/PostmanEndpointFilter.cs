using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KolayCAR.Broker.API.Helpers.Swagger;

public sealed class PostmanEndpointFilter
{
    private readonly HashSet<string> _endpoints;

    private PostmanEndpointFilter(HashSet<string> endpoints)
    {
        _endpoints = endpoints;
    }

    public static PostmanEndpointFilter Load(string fileName)
    {
        var endpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var filePath = ResolveFilePath(fileName);

        if (filePath is null)
        {
            return new PostmanEndpointFilter(endpoints);
        }

        using var stream = File.OpenRead(filePath);
        using var document = JsonDocument.Parse(stream);

        if (document.RootElement.TryGetProperty("item", out var items) &&
            items.ValueKind == JsonValueKind.Array)
        {
            AddEndpoints(items, endpoints);
        }

        return new PostmanEndpointFilter(endpoints);
    }

    public bool ShouldInclude(ApiDescription apiDescription)
    {
        if (_endpoints.Count == 0)
        {
            return true;
        }

        var method = NormalizeMethod(apiDescription.HttpMethod);
        var path = NormalizePath(apiDescription.RelativePath);

        return _endpoints.Contains($"{method} {path}");
    }

    private static string? ResolveFilePath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(Directory.GetCurrentDirectory(), fileName)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static void AddEndpoints(JsonElement items, HashSet<string> endpoints)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (item.TryGetProperty("request", out var request) && request.ValueKind == JsonValueKind.Object)
            {
                var method = request.TryGetProperty("method", out var methodElement)
                    ? methodElement.GetString()
                    : null;
                var path = ExtractPath(request);

                if (!string.IsNullOrWhiteSpace(method) && !string.IsNullOrWhiteSpace(path))
                {
                    endpoints.Add($"{NormalizeMethod(method)} {NormalizePath(path)}");
                }
            }

            if (item.TryGetProperty("item", out var children) && children.ValueKind == JsonValueKind.Array)
            {
                AddEndpoints(children, endpoints);
            }
        }
    }

    private static string? ExtractPath(JsonElement request)
    {
        if (!request.TryGetProperty("url", out var url))
        {
            return null;
        }

        if (url.ValueKind == JsonValueKind.String)
        {
            return url.GetString();
        }

        if (url.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (url.TryGetProperty("path", out var pathElement) && pathElement.ValueKind == JsonValueKind.Array)
        {
            var segments = new List<string>();

            foreach (var segment in pathElement.EnumerateArray())
            {
                if (segment.ValueKind == JsonValueKind.String)
                {
                    var value = segment.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        segments.Add(value);
                    }
                }
            }

            if (segments.Count > 0)
            {
                return string.Join('/', segments);
            }
        }

        if (url.TryGetProperty("raw", out var rawElement) && rawElement.ValueKind == JsonValueKind.String)
        {
            return rawElement.GetString();
        }

        return null;
    }

    private static string NormalizeMethod(string? method)
    {
        return (method ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var normalizedPath = path.Trim();
        normalizedPath = normalizedPath.Replace("{{baseUrl}}", string.Empty, StringComparison.OrdinalIgnoreCase);

        var queryIndex = normalizedPath.IndexOf('?');
        if (queryIndex >= 0)
        {
            normalizedPath = normalizedPath[..queryIndex];
        }

        if (Uri.TryCreate(normalizedPath, UriKind.Absolute, out var uri))
        {
            normalizedPath = uri.AbsolutePath;
        }

        var segments = normalizedPath
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => segment.ToLowerInvariant());

        return string.Join('/', segments);
    }
}