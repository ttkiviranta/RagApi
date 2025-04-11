using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RagApi.Interfaces;
using RagApi.Models;

namespace RagApi.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ISystemPromptService _systemPromptService;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _chatDeployment;
        private readonly int _maxHistoryMessages;

        public OpenAIService(
            HttpClient httpClient,
            ISystemPromptService systemPromptService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _systemPromptService = systemPromptService;
            _endpoint = configuration["Azure:OpenAI:Endpoint"];
            _apiKey = configuration["Azure:OpenAI:Key"];
            _chatDeployment = configuration["Azure:OpenAI:ChatDeployment"];
            _maxHistoryMessages = int.Parse(configuration["OpenAI:MaxHistoryMessages"] ?? "10");

            // Set API key for all requests
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        public async Task<string> GenerateAnswerAsync(string query, List<SearchResult> searchResults, string userId = null)
        {
            // Get appropriate system prompt based on user
            SystemPrompt systemPrompt;
            if (userId != null)
            {
                systemPrompt = await _systemPromptService.GetUserSystemPromptAsync(userId);
            }
            else
            {
                systemPrompt = await _systemPromptService.GetDefaultSystemPromptAsync();
            }

            // If no system prompt found, use a fallback
            string systemPromptText = systemPrompt?.PromptText ?? GetFallbackSystemPrompt();

            // Build context from search results
            var contextBuilder = new StringBuilder();

            contextBuilder.AppendLine("### Context:");
            foreach (var result in searchResults)
            {
                contextBuilder.AppendLine($"---");
                contextBuilder.AppendLine(result.Content);
                contextBuilder.AppendLine($"(Source: {result.Source})");
                contextBuilder.AppendLine();
            }

            // Create user query input
            string userPrompt = $"{contextBuilder}\n\n### Question: {query}";

            // Create chat completion request
            var requestData = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPromptText },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 1000,
                temperature = 0.3f
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            // Send request to Azure OpenAI API
            var response = await _httpClient.PostAsync(
                $"{_endpoint}openai/deployments/{_chatDeployment}/chat/completions?api-version=2023-05-15",
                requestContent);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);

            return responseObject.choices[0].message.content;
        }

        public async Task<string> GenerateAnswerWithHistoryAsync(string query, List<SearchResult> searchResults, List<Message> conversationHistory, string userId = null)
        {
            // Get appropriate system prompt based on user
            SystemPrompt systemPrompt;
            if (userId != null)
            {
                systemPrompt = await _systemPromptService.GetUserSystemPromptAsync(userId);
            }
            else
            {
                systemPrompt = await _systemPromptService.GetDefaultSystemPromptAsync();
            }

            // If no system prompt found, use a fallback
            string systemPromptText = systemPrompt?.PromptText ?? GetFallbackSystemPrompt();

            // Limit history to most recent messages if there are too many
            var limitedHistory = conversationHistory
                .OrderByDescending(m => m.CreatedAt)
                .Take(_maxHistoryMessages)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            // Create messages
            var messages = new List<object>
            {
                new { role = "system", content = systemPromptText }
            };

            // Add conversation history
            foreach (var message in limitedHistory)
            {
                string role = message.Type == MessageType.User ? "user" : "assistant";
                messages.Add(new { role, content = message.Content });
            }

            // Build context from search results
            var contextBuilder = new StringBuilder();

            contextBuilder.AppendLine("### Context:");
            foreach (var result in searchResults)
            {
                contextBuilder.AppendLine($"---");
                contextBuilder.AppendLine(result.Content);
                contextBuilder.AppendLine($"(Source: {result.Source})");
                contextBuilder.AppendLine();
            }

            // Add current query and context
            string userPrompt = $"{contextBuilder}\n\n### Question: {query}";
            messages.Add(new { role = "user", content = userPrompt });

            // Create completion request
            var requestData = new
            {
                messages,
                max_tokens = 1000,
                temperature = 0.3f
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            // Send request to Azure OpenAI API
            var response = await _httpClient.PostAsync(
                $"{_endpoint}openai/deployments/{_chatDeployment}/chat/completions?api-version=2023-05-15",
                requestContent);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);

            return responseObject.choices[0].message.content;
        }

        public async Task<string> GenerateDirectAnswerWithHistoryAsync(string query, List<Message> conversationHistory, string userId = null)
        {
            // Get appropriate system prompt based on user
            SystemPrompt systemPrompt;
            if (userId != null)
            {
                systemPrompt = await _systemPromptService.GetUserSystemPromptAsync(userId);
            }
            else
            {
                systemPrompt = await _systemPromptService.GetDefaultSystemPromptAsync();
            }

            // If no system prompt found, use a fallback
            string systemPromptText = systemPrompt?.PromptText ?? GetGeneralChatPrompt();

            // Limit history to most recent messages if there are too many
            var limitedHistory = conversationHistory
                .OrderByDescending(m => m.CreatedAt)
                .Take(_maxHistoryMessages)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            // Create messages list
            var messages = new List<object>
            {
                new { role = "system", content = systemPromptText }
            };

            // Add conversation history
            foreach (var message in limitedHistory)
            {
                string role = message.Type == MessageType.User ? "user" : "assistant";
                messages.Add(new { role, content = message.Content });
            }

            // Add current query
            messages.Add(new { role = "user", content = query });

            // Create completion request
            var requestData = new
            {
                messages,
                max_tokens = 1000,
                temperature = 0.7f
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            // Send request to Azure OpenAI API
            var response = await _httpClient.PostAsync(
                $"{_endpoint}openai/deployments/{_chatDeployment}/chat/completions?api-version=2023-05-15",
                requestContent);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent);

            return responseObject.choices[0].message.content;
        }

        // Default HR job matching system prompt
        private string GetFallbackSystemPrompt()
        {
            return @"
You are an HR assistant that analyzes job postings and candidate resumes/cover letters to find the best matches.

Analyze the job posting and candidate information provided in the context with these steps:
1. Extract key requirements, skills, and qualifications from the job description.
2. For each candidate, evaluate how well they match against each requirement.
3. Consider both technical skills and soft skills mentioned in the job description.
4. Pay attention to years of experience, relevant projects, and education requirements.
5. When evaluating, give higher weight to recent and directly relevant experience.

Provide a clear analysis that:
- Ranks candidates from most suitable to least suitable
- For each candidate, explain key strengths and weaknesses relative to the job
- Highlight specific qualifications that make them a good or poor fit
- Be objective and focus only on job-relevant qualifications

If the context doesn't contain enough information about the job or candidates, clearly state what additional information would help make a better assessment.
";
        }

        private string GetGeneralChatPrompt()
        {
            return @"
You are a helpful assistant that provides informative and educational responses.
Your job is to be helpful, harmless, and honest in your interactions.

When responding:
1. If you know the answer, explain it clearly and concisely
2. If you don't know, admit that you don't know rather than making up information
3. Provide examples when it helps understanding
4. Format information in an easy-to-read way
";
        }

        // Simple response classes for JSON deserialization
        private class ChatCompletionResponse
        {
            public Choice[] choices { get; set; }

            public class Choice
            {
                public Message message { get; set; }

                public class Message
                {
                    public string content { get; set; }
                }
            }
        }
    }
}