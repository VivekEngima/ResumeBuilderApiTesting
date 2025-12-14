using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using System.Text.Json;

namespace ResumeBuilderApiTesting.Services
{
    public class CoverLetterExtractionService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public CoverLetterExtractionService(IConfiguration config)
        {
            _apiKey = config["Perplexity:ApiKey"]
                  ?? throw new Exception("Perplexity API key not configured.");

            _httpClient = new HttpClient();
        }

        // Default dummy company data
        private static readonly dynamic DummyCompany = new
        {
            Name = "TechCorp Solutions",
            Address = "123 Silicon Valley Road, San Francisco, CA 94105",
            HiringManager = "John Smith"
        };

        public async Task<string> GenerateCoverLetterFromResumeAsync(IFormFile file)
        {
            var resumeText = ExtractTextFromPdf(file);
            var json = await CallPerplexityForCoverLetterAsync(resumeText);
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

        private async Task<string> CallPerplexityForCoverLetterAsync(string resumeText)
        {
            var schemaPrompt = $@"
                You are a professional cover letter writer. Based on the provided resume and company details, generate a professional cover letter in JSON format.

                Return ONLY the JSON object (no markdown, no explanation) following this exact schema:

                {{
                  ""applicant"": {{
                    ""name"": ""string"",
                    ""email"": ""string"",
                    ""phone"": ""string"",
                    ""address"": ""string""
                  }},
                  ""company"": {{
                    ""name"": ""string"",
                    ""address"": ""string"",
                    ""hiringManager"": ""string""
                  }},
                  ""letter"": {{
                    ""date"": ""string (YYYY-MM-DD format)"",
                    ""subject"": ""string (e.g., 'Application for Senior Software Engineer')"",
                    ""salutation"": ""string (e.g., 'Dear Mr. Smith')"",
                    ""opening"": ""string (1-2 sentences introducing yourself and position)"",
                    ""body"": [
                      ""string (1st paragraph - why interested)"",
                      ""string (2nd paragraph - key skills)"",
                      ""string (3rd paragraph - contribution)""
                    ],
                    ""closing"": ""string (1-2 sentences with call to action)"",
                    ""signatureName"": ""string (full name)""
                  }}
                }}

                COMPANY DETAILS (use these):
                - Company Name: {DummyCompany.Name}
                - Company Address: {DummyCompany.Address}
                - Hiring Manager: {DummyCompany.HiringManager}

                Extract applicant details from resume. Generate a compelling, professional cover letter.
                Use today's date in YYYY-MM-DD format. If any field missing, use reasonable defaults.
                Return ONLY valid JSON, no extra text.";

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

            messageContent = messageContent.Replace("``````", "").Trim();

            return messageContent;
        }
    }
}
