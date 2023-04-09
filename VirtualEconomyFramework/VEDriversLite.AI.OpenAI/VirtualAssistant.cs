using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3;
using System.Text;
using VEDriversLite.AI.OpenAI.Dto;
using Newtonsoft.Json;
using VEDriversLite.Common.Enums;
using OpenAI.GPT3.Interfaces;
using System.Security.Cryptography;

namespace VEDriversLite.AI.OpenAI
{
    public class VirtualAssistant
    {
        public VirtualAssistant(string apikey, string id = "")
        {
            ApiKey = apikey;
            if (string.IsNullOrEmpty(id))
                Id = Guid.NewGuid().ToString();
            else
                Id = id;
        }
        /// <summary>
        /// ID of the Assistant
        /// Guid is created if it is not provided in constructor
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// API Key for OpenAI account. It must be filled in the constructor
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        /// <summary>
        /// History of all messages
        /// </summary>
        public List<ChatMessage> MessagesHistory { get; set; } = new List<ChatMessage>();
        /// <summary>
        /// OpenAI API wrapper service
        /// </summary>
        public OpenAIService? AIService { get; set; } = null;
        /// <summary>
        /// Main used model
        /// </summary>
        public string MainTextModel { get; set; } = Models.ChatGpt3_5Turbo;

        /// <summary>
        /// Init the assistants. It must be called before calls
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, string)> InitAssistant()
        {
            try
            {
                AIService = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = ApiKey
                });

                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private string FindEndOfSentense(string input)
        {
            var split = input.Split('.');

            var result = new StringBuilder();

            for (var i = 0; i < split.Length - 1; i++)
                result.Append(split[i] + ".");

            if (result.Length > 1)
                result.Remove(result.Length - 1, 1);

            if (split.Length == 1)
                result.Append(split[0]);

            return result.Append(".").ToString();
        }

        /// <summary>
        /// Create welcome phrase.
        /// </summary>
        /// <param name="language"> you can use "en" to get welcome phrase in english. Default is Czech "cs" </param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> GetWelcome(string language = "cs", int maxTokens = 70)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {
                var input = "Vytvoř prosím nějaké přivítání nového uživatele, který s tebou bude nyní chatovat. Nezapomeň, že v češtině je umělá inteligence ženského rodu.";
                if (language == "en")
                    input = "Create some greeting for new user who will chat with you now, please.";

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem(input)
                    },
                    Model = MainTextModel,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    //Console.WriteLine(msg.Content);
                    MessagesHistory.Add(msg);

                    return (true, FindEndOfSentense(msg.Content));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }

        /// <summary>
        /// Returns the question for user if they want to ask something else.
        /// </summary>
        /// <param name="language"> you can use "en" to get welcome phrase in english. Default is Czech "cs"</param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> GetNextQuestion(string language = "cz", int maxTokens = 70)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {
                var input = "Vytvoř prosím nějakou otázku pro uživatele aby věděl, že se může ještě na něco zeptat, pokud má nějaký další dotaz k tématu.";
                if (language == "en")
                    input = "Create some question for user to let them know if they need to ask for something else about this topic, please.";

                var messages = new List<ChatMessage>();
                if (MessagesHistory.Count > 2)
                {
                    messages.Add(MessagesHistory[MessagesHistory.Count - 1]);
                    messages.Add(MessagesHistory[MessagesHistory.Count - 2]);
                }
                else if (MessagesHistory.Count == 1)
                {
                    messages.Add(MessagesHistory[0]);
                }

                messages.Add(ChatMessage.FromSystem(input));

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Model = MainTextModel,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    //Console.WriteLine(msg.Content);
                    MessagesHistory.Add(msg);

                    return (true, FindEndOfSentense(msg.Content));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }


        /// <summary>
        /// Send question from the user and wait for the response from ChatGPT
        /// This will send also all previous messages to keep the conversation
        /// </summary>
        /// <param name="question"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendUserQuestion(string question, int maxTokens = 250)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {
                MessagesHistory.Add(ChatMessage.FromUser(question));

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = MessagesHistory,
                    Model = MainTextModel,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    //Console.WriteLine(msg.Content);
                    MessagesHistory.Add(msg);

                    return (true, FindEndOfSentense(msg.Content));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }

        /// <summary>
        /// Send one question from the user and wait for the response from ChatGPT
        /// </summary>
        /// <param name="question"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendSimpleQuestion(string question, int maxTokens = 250)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {
                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>() 
                    { 
                        ChatMessage.FromUser(question)
                    },
                    Model = MainTextModel,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    return (true, FindEndOfSentense(msg.Content));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }

        /// <summary>
        /// Send file content to ChatGPT for analysis.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendFileContent(string filename, int maxTokens = 1000)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {
                var filecontent = VEDriversLite.Common.FileHelpers.ReadTextFromFile(filename);
                if (string.IsNullOrEmpty(filecontent))
                    return (false, "No content in file.");

                MessagesHistory.Add(ChatMessage.FromUser($"Můžeš prosím zpracovat obsah souboru do formy vhodného mermaid diagramu prosím? Zde je obsah souboru: \"{filecontent}\""));

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = MessagesHistory,
                    Model = Models.ChatGpt3_5Turbo,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    //Console.WriteLine(msg.Content);
                    MessagesHistory.Add(msg);

                    return (true, FindEndOfSentense(msg.Content));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nft"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> GetNewTagsForNFT(string name, string description, string text, string tags, int maxTokens = 1000)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {
                var content = $"NFT name: {name}, NFT Description: {description}, NFT Text: {text}, NFT Tagy:  {tags}. ";

                MessagesHistory.Add(ChatMessage.FromUser($"Vymysli a vypiš novou sadu tagů pro NFT jehož data jsou níže? Tagy jsou bez mezer na jednom řádku a jednotlivé tagy jsou v řádku oddělené mezerou. Potřebuji alespoň 5 tagů. Zde je NFT: \"{content}\""));

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = MessagesHistory,
                    Model = MainTextModel,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    //Console.WriteLine(msg.Content);
                    MessagesHistory.Add(msg);

                    return (true, msg.Content.Replace("\n", string.Empty));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }


        /// <summary>
        /// Get new data for NFT minting. ChatGPT will recommend Name, Description and Tags based on the input text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, NewDataForNFTResult)> GetNewDataForNFT(string text, int maxTokens = 1000)
        {
            if (AIService == null)
                return (false, null);

            try
            {
                //MessagesHistory.Add(ChatMessage.FromUser("Vytvoř data pro NFT ze zdrojového textu. Potřebuji jako výstup JSON: {\"Name\": \"\", \"Description\": \"\", \"Tags\":\"\" } kde Tags (tagy) jsou bez mezer na jednom řádku a jednotlivé tagy jsou v řádku oddělené mezerou. Minimálně 5 tagů. Name (jméno) maximálně 30 znaků. Description (popis) maximálně 160 znaků. Zdrojový text: \"" + text.Substring(0, (int)(text.Length * 0.5)) + "\""));
                var question = "Vytvoř data pro NFT ze zdrojového textu. Potřebuji jako výstup JSON: {\"Name\": \"\", \"Description\": \"\", \"Tags\":\"\" } kde Tags (tagy) jsou bez mezer na jednom řádku a jednotlivé tagy jsou v řádku oddělené mezerou. Minimálně 5 tagů, maximálně 10. Name (jméno) maximálně 30 znaků. Description (popis) musí být výstižné a lákavé maximálně 160 znaků. Zdrojový text: \"" + text.Substring(0, (int)(text.Length * 0.5)) + "\"";

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>()
                    {
                        ChatMessage.FromUser(question)
                    },
                    Model = MainTextModel,
                    MaxTokens = maxTokens//optional
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    
                    var jsonOut = ParseOutOneJSONFromString(msg.Content);
                    if (jsonOut.Item1)
                    {
                        try
                        {
                            var resData = JsonConvert.DeserializeObject<NewDataForNFTResult>(jsonOut.Item2);
                            if (resData != null)
                            {
                                return (true, resData);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync("Cannot deserialize response with new NFT data. " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, null);
            }

            return (false, null);
        }

        /// <summary>
        /// Create image based on the text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>true and Base64 string if success</returns>
        public async Task<(bool, List<string>)> GetImageForText(string text)
        {
            if (AIService == null)
                return (false, new List<string>());

            try
            {
                var imageResult = await AIService.Image.CreateImage(new ImageCreateRequest
                {
                    Prompt = text,
                    N = 2,
                    Size = StaticValues.ImageStatics.Size.Size256,
                    ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Base64,
                    User = "TestUser"
                });

                if (imageResult.Successful)
                {
                    var res = imageResult.Results.Select(r => r.B64).FirstOrDefault();

                    var images = new List<string>();
                    if (imageResult.Results.Count > 1)
                    {
                        foreach(var img in imageResult.Results)
                            images.Add(img.B64);
                    }

                    if (images.Count > 0)
                        return (true, images);
                }
            }
            catch (Exception ex)
            {
                return (false, new List<string>());
            }

            return (false, new List<string>());
        }

        private (bool, string) ParseOutOneJSONFromString(string input)
        {
            if (input.Contains("{") && input.Contains("}"))
            {
                var res = new StringBuilder();
                var start = false;
                for (var i = 0; i < input.Length; i++)
                {
                    var ch = input[i];
                    if (!start && ch == '{')
                        start = true;

                    if (start)
                        res.Append(ch);

                    if (!start && ch == '}')
                        break;
                }
                if (res.Length > 0)
                    return (true, res.ToString());
            }
            return (false, string.Empty);
        }

        /// <summary>
        /// Let the ChatGPT create the Mermaid graph from the text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="graphType">Mermaid graph type</param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        public async Task<(bool, string)> GetMermaidFromText(string text, MermaidGraphTypes graphType = MermaidGraphTypes.Sequence, int maxTokens = 1500)
        {
            if (AIService == null)
                return (false, "Init AI service first.");

            try
            {

                var requestMessage = $"Vytvoř prosím z tohoto textu Mermaid \"{Enum.GetName(graphType)}\" diagram. Zde je text: \"{text}\".";

                var completionResult = await AIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>()
                    {
                        ChatMessage.FromUser(requestMessage)
                    },
                    Model = MainTextModel,
                    MaxTokens = maxTokens
                });

                if (completionResult.Successful)
                {
                    var msg = completionResult.Choices.First().Message;
                    if (msg != null)
                        return (true, ParseMermaidFromText(msg.Content));
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (false, "Unknown Error");
        }

        private string ParseMermaidFromText(string text)
        {
            var result = new StringBuilder();

            using (var reader = new StringReader(text))
            {
                var start = false;
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (line.StartsWith("```") && start)
                        break;

                    if (start)
                        result.AppendLine(line);

                    if (line.StartsWith("```mermaid"))
                        start = true;
                }
            }
            return result.ToString();
        }

        public async Task<(bool, string)> GetTranscriptionOfAudio(byte[] bytes, string fileName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    fileName = $"audio-recording-{DateTime.UtcNow.ToString("yyyy_MM_ddThh-mm-ss")}.mp3";

                var audioResult = await AIService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
                {
                    FileName = fileName,
                    File = bytes,
                    Model = Models.WhisperV1,
                    ResponseFormat = StaticValues.AudioStatics.ResponseFormat.VerboseJson
                });


                if (audioResult.Successful)
                {
                    Console.WriteLine(string.Join("\n", audioResult.Text));
                    return (true, audioResult.Text);
                }
                else
                {
                    if (audioResult.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }

                    Console.WriteLine($"{audioResult.Error.Code}: {audioResult.Error.Message}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return (false, string.Empty);
        }
    }
}