using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KolayCAR.Broker.API.Helpers.Swagger;

public sealed class PostmanOperationFilter : IOperationFilter
{
    private static readonly Lazy<IReadOnlyDictionary<string, PostmanEndpointDocumentation>> Documentation = new(() =>
        PostmanEndpointDocumentation.Load("docRacapi.txt"));

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var key = PostmanEndpointDocumentation.CreateKey(context.ApiDescription.HttpMethod, context.ApiDescription.RelativePath);
        if (!Documentation.Value.TryGetValue(key, out var documentation))
        {
            return;
        }

        operation.Summary = documentation.Name;
        operation.Description = documentation.Description;

        ApplyParameterDescriptions(operation, documentation);
        ApplyRequestBodyDescription(operation, documentation);
        ApplyResponseDescriptions(operation, documentation);
    }

    private static void ApplyParameterDescriptions(OpenApiOperation operation, PostmanEndpointDocumentation documentation)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            if (documentation.Parameters.TryGetValue(parameter.Name, out var parameterDocumentation))
            {
                parameter.Description = parameterDocumentation.Description;
            }
        }
    }

    private static void ApplyRequestBodyDescription(OpenApiOperation operation, PostmanEndpointDocumentation documentation)
    {
        if (operation.RequestBody is null)
        {
            return;
        }

        var bodySection = documentation.Sections.TryGetValue("Body", out var body) ? body : null;
        operation.RequestBody.Description = !string.IsNullOrWhiteSpace(bodySection)
            ? bodySection
            : documentation.Description;
    }

    private static void ApplyResponseDescriptions(OpenApiOperation operation, PostmanEndpointDocumentation documentation)
    {
        if (operation.Responses is null || documentation.Responses.Count == 0)
        {
            return;
        }

        foreach (var responseDocumentation in documentation.Responses)
        {
            var statusCode = responseDocumentation.StatusCode.ToString();
            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                response = new OpenApiResponse();
                operation.Responses[statusCode] = response;
            }

            response.Description = string.IsNullOrWhiteSpace(responseDocumentation.Description)
                ? response.Description
                : responseDocumentation.Description;
        }
    }
}

internal sealed record PostmanEndpointDocumentation(
    string Name,
    string Method,
    string Path,
    string Description,
    IReadOnlyDictionary<string, PostmanParameterDocumentation> Parameters,
    IReadOnlyDictionary<string, string> Sections,
    IReadOnlyList<PostmanResponseDocumentation> Responses)
{
    public static IReadOnlyDictionary<string, PostmanEndpointDocumentation> Load(string fileName)
    {
        var filePath = ResolveFilePath(fileName);
        var result = new Dictionary<string, PostmanEndpointDocumentation>(StringComparer.OrdinalIgnoreCase);

        if (filePath is null)
        {
            return result;
        }

        using var stream = File.OpenRead(filePath);
        using var document = JsonDocument.Parse(stream);

        if (document.RootElement.TryGetProperty("item", out var items) && items.ValueKind == JsonValueKind.Array)
        {
            AddEndpoints(items, result);
        }

        return result;
    }

    public static string CreateKey(string? method, string? path)
    {
        return $"{NormalizeMethod(method)} {NormalizePath(path)}";
    }

    private static string? ResolveFilePath(string fileName)
    {
        var candidates = new[]
        {
            System.IO.Path.Combine(AppContext.BaseDirectory, fileName),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileName)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static void AddEndpoints(JsonElement items, Dictionary<string, PostmanEndpointDocumentation> result)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (item.TryGetProperty("request", out var request) && request.ValueKind == JsonValueKind.Object)
            {
                var method = GetStringProperty(request, "method");
                var path = ExtractPath(request);
                if (!string.IsNullOrWhiteSpace(method) && !string.IsNullOrWhiteSpace(path))
                {
                    var documentation = Create(item, request, method, path);
                    result[CreateKey(method, path)] = documentation;
                }
            }

            if (item.TryGetProperty("item", out var children) && children.ValueKind == JsonValueKind.Array)
            {
                AddEndpoints(children, result);
            }
        }
    }

    private static PostmanEndpointDocumentation Create(JsonElement item, JsonElement request, string method, string path)
    {
        var name = GetStringProperty(item, "name") ?? $"{method} {NormalizePath(path)}";
        var description = GetStringProperty(request, "description") ?? GetStringProperty(item, "description") ?? string.Empty;
        var requestBody = ExtractRequestBody(request);
        var responses = ExtractResponses(item);
        var fullDescription = BuildDescription(description, requestBody, responses);
        var sections = ExtractSections(description);
        var parameters = ExtractParameters(description);

        return new PostmanEndpointDocumentation(
            name,
            NormalizeMethod(method),
            NormalizePath(path),
            fullDescription,
            parameters,
            sections,
            responses);
    }

    private static string BuildDescription(string description, string? requestBody, IReadOnlyList<PostmanResponseDocumentation> responses)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(description))
        {
            builder.AppendLine(description.Trim());
        }

        if (!string.IsNullOrWhiteSpace(requestBody))
        {
            AppendSection(builder, "Example Request Body", MaskSensitiveValues(requestBody));
        }

        foreach (var response in responses)
        {
            if (!string.IsNullOrWhiteSpace(response.Body))
            {
                AppendSection(builder, $"Example Response ({response.StatusCode})", MaskSensitiveValues(response.Body));
            }
        }

        return builder.ToString().Trim();
    }

    private static void AppendSection(StringBuilder builder, string title, string content)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine().AppendLine();
        }

        builder.AppendLine($"## {title}");
        builder.AppendLine("```json");
        builder.AppendLine(content.Trim());
        builder.AppendLine("```");
    }

    private static string? ExtractRequestBody(JsonElement request)
    {
        if (!request.TryGetProperty("body", out var body) || body.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return GetStringProperty(body, "raw");
    }

    private static IReadOnlyList<PostmanResponseDocumentation> ExtractResponses(JsonElement item)
    {
        var responses = new List<PostmanResponseDocumentation>();

        if (!item.TryGetProperty("response", out var responseElement) || responseElement.ValueKind != JsonValueKind.Array)
        {
            return responses;
        }

        foreach (var response in responseElement.EnumerateArray())
        {
            if (response.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var statusCode = response.TryGetProperty("code", out var codeElement) && codeElement.TryGetInt32(out var parsedCode)
                ? parsedCode
                : 200;
            var status = GetStringProperty(response, "status");
            var body = GetStringProperty(response, "body");
            var description = !string.IsNullOrWhiteSpace(status)
                ? status
                : $"Postman example response ({statusCode})";

            responses.Add(new PostmanResponseDocumentation(statusCode, description, body));
        }

        return responses;
    }

    private static IReadOnlyDictionary<string, string> ExtractSections(string description)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var currentTitle = string.Empty;
        var currentContent = new StringBuilder();

        foreach (var line in SplitLines(description))
        {
            var titleMatch = Regex.Match(line, @"^##\s+(.+?)\s*$");
            if (titleMatch.Success)
            {
                AddCurrentSection();
                currentTitle = NormalizeSectionTitle(titleMatch.Groups[1].Value);
                currentContent.Clear();
                continue;
            }

            currentContent.AppendLine(line);
        }

        AddCurrentSection();
        return sections;

        void AddCurrentSection()
        {
            if (!string.IsNullOrWhiteSpace(currentTitle))
            {
                sections[currentTitle] = currentContent.ToString().Trim();
            }
        }
    }

    private static IReadOnlyDictionary<string, PostmanParameterDocumentation> ExtractParameters(string description)
    {
        var result = new Dictionary<string, PostmanParameterDocumentation>(StringComparer.OrdinalIgnoreCase);
        var lines = SplitLines(description).ToArray();

        for (var i = 0; i < lines.Length; i++)
        {
            if (!IsTableHeader(lines[i]))
            {
                continue;
            }

            var headers = SplitMarkdownTableRow(lines[i]);
            if (headers.Count == 0)
            {
                continue;
            }

            var nameIndex = headers.FindIndex(header => IsParameterNameHeader(header));
            var descriptionIndex = headers.FindIndex(header => string.Equals(header, "Description", StringComparison.OrdinalIgnoreCase));
            var sampleIndex = headers.FindIndex(header => string.Equals(header, "Sample", StringComparison.OrdinalIgnoreCase));
            var requiredIndex = headers.FindIndex(header => string.Equals(header, "Required", StringComparison.OrdinalIgnoreCase));

            if (nameIndex < 0 || descriptionIndex < 0)
            {
                continue;
            }

            i++;
            while (i < lines.Length && IsSeparatorRow(lines[i]))
            {
                i++;
            }

            for (; i < lines.Length && lines[i].TrimStart().StartsWith('|'); i++)
            {
                var columns = SplitMarkdownTableRow(lines[i]);
                if (columns.Count <= Math.Max(nameIndex, descriptionIndex))
                {
                    continue;
                }

                var name = CleanMarkdown(columns[nameIndex]);
                var parameterDescription = CleanMarkdown(columns[descriptionIndex]);
                var sample = sampleIndex >= 0 && columns.Count > sampleIndex ? CleanMarkdown(columns[sampleIndex]) : null;
                var required = requiredIndex >= 0 && columns.Count > requiredIndex && IsRequired(columns[requiredIndex]);

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(parameterDescription))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sample))
                {
                    parameterDescription = $"{parameterDescription} Example: `{sample}`";
                }

                result.TryAdd(name, new PostmanParameterDocumentation(parameterDescription, required));
            }
        }

        return result;
    }

    private static bool IsTableHeader(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith('|') &&
            trimmed.EndsWith('|') &&
            (trimmed.Contains("Param", StringComparison.OrdinalIgnoreCase) ||
             trimmed.Contains("Parameter", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsParameterNameHeader(string header)
    {
        return string.Equals(header, "Param", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(header, "Parameter", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSeparatorRow(string line)
    {
        var trimmed = line.Trim().Trim('|', ' ');
        return !string.IsNullOrWhiteSpace(trimmed) && trimmed.All(character => character is '-' or ':' or '|' or ' ');
    }

    private static bool IsRequired(string value)
    {
        var normalized = CleanMarkdown(value).Trim();
        return normalized.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("required", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> SplitMarkdownTableRow(string line)
    {
        return line.Trim().Trim('|')
            .Split('|')
            .Select(column => column.Trim())
            .ToList();
    }

    private static string CleanMarkdown(string value)
    {
        return value.Trim().Trim('`').Replace("\\_", "_").Replace("\\*", "*");
    }

    private static IEnumerable<string> SplitLines(string value)
    {
        return (value ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    private static string NormalizeSectionTitle(string value)
    {
        var title = value.Trim();
        title = title.Replace("**", string.Empty);
        title = Regex.Replace(title, @"^[^A-Za-z0-9]+", string.Empty).Trim();
        title = title.Trim(':');
        return title;
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

        return url.TryGetProperty("raw", out var rawElement) && rawElement.ValueKind == JsonValueKind.String
            ? rawElement.GetString()
            : null;
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Object when property.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String => content.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => property.ToString()
        };
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

    private static string MaskSensitiveValues(string value)
    {
        var masked = Regex.Replace(value, "(\\\"(?:token|accessToken|access_token|password)\\\"\\s*:\\s*\\\")([^\\\"]+)(\\\")", "$1***$3", RegexOptions.IgnoreCase);
        return Regex.Replace(masked, "(Bearer\\s+)[A-Za-z0-9._-]+", "$1***", RegexOptions.IgnoreCase);
    }
}

internal sealed record PostmanParameterDocumentation(string Description, bool Required);

internal sealed record PostmanResponseDocumentation(int StatusCode, string Description, string? Body);