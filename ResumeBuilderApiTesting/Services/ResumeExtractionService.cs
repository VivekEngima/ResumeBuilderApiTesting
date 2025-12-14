using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace ResumeBuilderApiTesting.Services
{
    public class ResumeExtractionService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public ResumeExtractionService(IConfiguration config)
        {
            _apiKey = config["Perplexity:ApiKey"]
                  ?? throw new Exception("Perplexity API key not configured.");

            _httpClient = new HttpClient();
        }

        public async Task<string> ParseResumeToJsonAsync(IFormFile file)
        {
            var text = ExtractTextFromPdf(file);
            var json = await CallPerplexityAsync(text);
            return json;
        }

        private static string ExtractTextFromPdf(IFormFile file)
        {
            using var reader = new PdfReader(file.OpenReadStream());
            var sb = new StringBuilder();

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private async Task<string> CallPerplexityAsync(string resumeText)
        {
            var schemaPrompt = @"
            You are a professional resume parser. Extract and structure the resume data into a valid JSON object.

            Return ONLY the JSON object (no markdown, no explanation) following this schema:

            {
              ""name"": ""string"",
              ""title"": ""string"",
              ""contact"": {
                ""phone"": ""string"",
                ""email"": ""string"",
                ""address"": ""string"",
                ""website"": ""string""
              },
              ""profile"": ""string"",
              ""skills"": [""string""],
              ""languages"": [
                { ""language"": ""string"", ""proficiency"": ""string"" }
              ],
              ""education"": [
                { ""institution"": ""string"", ""degree"": ""string"", ""gpa"": ""string"", ""years"": ""string"" }
              ],
              ""work_experience"": [
                { ""company"": ""string"", ""role"": ""string"", ""years"": ""string"", ""responsibilities"": [""string""] }
              ],
              ""references"": [
                { ""name"": ""string"", ""company_role"": ""string"", ""phone"": ""string"", ""email"": ""string"" }
              ]
            }

            If any field is missing, use empty string or empty array. Return ONLY valid JSON.";

            // Build request body for Perplexity
            var body = new
            {
                model = "sonar-pro",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"{schemaPrompt}\n\nRESUME:\n{resumeText}"
                    }
                },
                stream = false
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.perplexity.ai/chat/completions"
            );

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Perplexity API error ({response.StatusCode}): {content}");

            return ExtractJsonFromResponse(content);
        }

        public async Task<string> TestPerplexityAsync()
        {
            var body = new
            {
                model = "sonar-pro",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = "Say: Perplexity API integration is working."
                    }
                },
                stream = false
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.perplexity.ai/chat/completions"
            );

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"HTTP {(int)response.StatusCode}: {content}";

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                var text = root
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return text ?? "(no response)";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        private static string ExtractJsonFromResponse(string responseContent)
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var messageContent = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrEmpty(messageContent))
                return "{}";

            // Clean markdown formatting if present
            messageContent = messageContent.Replace("``````", "").Trim();

            return messageContent;
        }
    }
}
