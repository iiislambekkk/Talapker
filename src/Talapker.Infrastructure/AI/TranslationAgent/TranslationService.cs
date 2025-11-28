using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

namespace Talapker.Infrastructure.AI.TranslationAgent;

public interface ITranslationService
{
    Task<TranslationResult> TranslateToAllLanguagesAsync(string text, bool shouldImprove);
}

public class TranslationService(IConfiguration configuration) : ITranslationService
{
    public async Task<TranslationResult> TranslateToAllLanguagesAsync(string text, bool shouldImprove)
    {
        string shouldImproveInstructions = shouldImprove ? 
             """
                 8. Enhance clarity, coherence, and overall quality of the text in each language
                 9. Use more sophisticated vocabulary and varied sentence structures
                 10. Ensure the improved text flows naturally and is engaging to read
             """
             :
             """
                8. Do not alter the meaning or intent of the original text.
             """;
        

        var instructions = $"""
            You are a professional translator. Translate the user's text to Kazakh (kk), Russian (ru), and English (en).

            CRITICAL RULES:
            1. Preserve all markdown formatting, links, and special characters exactly
            2. Maintain the original structure, lists, headers, and code blocks
            3. Only translate the textual content, not the markdown syntax
            4. Return valid JSON with "kk", "ru", "en" keys
            5. Keep placeholders, variables, and technical terms unchanged
            6. Maintain consistent terminology across all translations
            7. Ensure proper grammar, punctuation, and spelling in each language
            {shouldImproveInstructions}

            Respond with proper JSON only, no other text.
            JSON must be correct, without any syntax errors that can cause parsing problems.
            """;
        
        
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TranslationResult));
        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "TranslationResult",
                schemaDescription: "Translation result with keys for Kazakh (kk), Russian (ru), and English (en)"),
            Instructions = instructions
        };
        
        AIAgent agent = new OpenAIClient(configuration["OpenAIKey"]!).GetChatClient("gpt-5-mini").AsIChatClient().CreateAIAgent(
            new ChatClientAgentOptions()
            {
                ChatOptions = chatOptions, 
                Instructions = instructions
            }
        );
        
        var response = await agent.RunAsync("Translate the following text:\n" + text);
        
        var translationResult = response.Deserialize<TranslationResult>(JsonSerializerOptions.Web);
        
        return translationResult;
    }
}