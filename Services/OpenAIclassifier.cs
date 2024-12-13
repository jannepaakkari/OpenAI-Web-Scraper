using OpenAI.Chat;


namespace Webscaper.OpenAI
{
    public static class OpenAIClassifier
    {
        public static async Task<string> ClassifyHeaderUsingOpenAIAsync(ChatClient openAIClient, string headerText)
        {
            try
            {
                var message = new UserChatMessage(
                    "Classify the following header text into the categories by responding with only a single word." +
                    $" No explanation or reasoning is required. Header: {headerText}"
                );

                ChatCompletion chatCompletion = await openAIClient.CompleteChatAsync(message);

                // May take awhile, so we print on console so we know it's not dead
                Console.WriteLine(chatCompletion.Content[0].Text ?? "Unknown");
                return chatCompletion.Content[0].Text ?? "Unknown";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error classifying header: {ex.Message}");
                return "Unknown";
            }
        }
    }
}