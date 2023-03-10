using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.AI.OpenAI;
using VEDriversLite.NFT;
using VEDriversLite.StorageDriver.Helpers;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;
using VEDriversLite.StorageDriver.StorageDrivers;
using VEDriversLite.NFT.Dto;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;

namespace TestVEDriversLite
{
    public static class AITests
    {
        private static NeblioAccount account = new NeblioAccount();

        private static VirtualAssistant assistant = null;

        private static VEDriversLite.StorageDriver.StorageHandler Storage = new VEDriversLite.StorageDriver.StorageHandler();

        [TestEntry]
        public static void AI_InitAssistant(string param)
        {
            AI_InitAssistantAsync(param);
        }
        public static async Task AI_InitAssistantAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input OpenAIApiKey, VENFTbackupname, password");
            var apiKey = split[0];
            var backupFile = split[1];
            var password = split[2];

            assistant = new VirtualAssistant(apiKey);
            var init = await assistant.InitAssistant();
            if (init.Item1)
                await Console.Out.WriteLineAsync("AI Assistant Initialized.");

            //init the storage driver
            IPFSHelpers.GatewayURL = "https://ve-framework.com/";

            var res = await Storage.AddDriver(new StorageDriverConfigDto()
            {
                Type = "IPFS",
                Name = "ChatGPTDriver",
                Location = "Cloud",
                ID = "ChatGPT",
                IsPublicGateway = true,
                IsLocal = false,
                ConnectionParams = new StorageDriverConnectionParams()
                {
                    APIUrl = "https://ve-framework.com/",
                    APIPort = 443,
                    Secured = false,
                    GatewayURL = "https://ve-framework.com/ipfs/",
                    GatewayPort = 443,
                }
            });


            if (account.IsLocked())
            {
                // load the Neblio Address for minting of NFT
                Console.WriteLine("Loading VENFT Backup file.");
                account.FirsLoadingStatus += Account_FirsLoadingStatus;
                if (await account.LoadAccountFromVENFTBackup(password, "", backupFile))
                    Console.WriteLine($"Adresa {account.Address} loaded.");
            }

        }

        [TestEntry]
        public static void AI_DALLE_RequestCreation(string param)
        {
            AI_DALLE_RequestCreationAsync(param);
        }
        public static async Task AI_DALLE_RequestCreationAsync(string param)
        {
            var text = FileHelpers.ReadTextFromFile("text.txt");
            //var baseForImage = await assistant.SendSimpleQuestion($"How do you visual imagine this text? Describe it in one english sentense. Source text: \"{text}\".", 100);
            var baseForImage = await assistant.SendSimpleQuestion($"{param} Zdrojový text: \"{text}\".", 100);
            await Console.Out.WriteLineAsync($"Result: " + baseForImage.Item2);
        }
        [TestEntry]
        public static void AI_CreateNFTBasedOnText(string param)
        {
            AI_CreateNFTBasedOnTextAsync(param);
        }
        public static async Task AI_CreateNFTBasedOnTextAsync(string param)
        {
            if (assistant == null || Storage.StorageDrivers.Count == 0)
            {
                await Console.Out.WriteLineAsync("Please init assistant first with test AI_InitAssistant.");
                return;
            }

            var split = param.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input textfilename, articleLength, storybase");
            var fileName = split[0];
            var articleLength = Convert.ToInt32(split[1]);
            var storybase = split[2];

            if (articleLength < 250)
                articleLength = 250;

            var withGreetings = false;

            var text = string.Empty;

            if (fileName != "random")
            {
                // load text content from the file
                text = FileHelpers.ReadTextFromFile(fileName);
            }
            else
            {
                Console.WriteLine("Creating base text...");
                // create random text by ChatGPT
                
                var baseEn = "Create some short article about how people could treat each other better please. Output will be in Markdown.";
                var baseCz = "Vytvoř prosím krátký článek o tom, jak by se k sobě lidé mohli chovat lépe. Výstup bude Markdown.";
                if (!string.IsNullOrEmpty(storybase))
                {
                    baseCz = "Vytvoř prosím krátký článek o " + storybase + ". Výstup bude Markdown.";
                }

                var story = await assistant.SendSimpleQuestion(baseCz, articleLength);
                if (story.Item1)
                {
                    Console.WriteLine("Text created: ");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(story.Item2);
                    Console.WriteLine("-------------------------");
                    text = story.Item2;
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Text cannot be empty.");
                return;
            }

            // say welcome to the user
            if (withGreetings)
            {
                var welcome = await assistant.GetWelcome();
                if (welcome.Item1)
                {
                    Console.WriteLine("--------------Assistant---------------");
                    Console.WriteLine(welcome.Item2);
                }
            }

            Console.WriteLine("Creating NFT content...");
            // Create set of the data for the new NFT based on the input text
            var responseForNFT = await assistant.GetNewDataForNFT(text);
            if (responseForNFT.Item1)
            {
                var nft = new PostNFT("");
                var nftInput = responseForNFT.Item2;
                nft.Name = nftInput.Name;
                nft.Description = nftInput.Description;
                nft.Tags = nftInput.Tags;
                nft.Text = text;
                Console.WriteLine("");
                Console.WriteLine("-------------------------");
                Console.WriteLine("Design of NFT:");
                Console.WriteLine($"Name: {nft.Name}");
                Console.WriteLine($"Description: {nft.Description}");
                Console.WriteLine($"Tags: {nft.Tags}");
                Console.WriteLine($"Text: {nft.Text}");
                Console.WriteLine("-------------------------");
                Console.WriteLine("");
                Console.WriteLine("Creating image for NFT...");
                // Create image for the NFT. It returns Base64 which will be uploaded to the IPFS later.
                var responseForImage = await assistant.GetImageForText($"Název: {nft.Name}, Popis: {nft.Description}, Tagy: {nft.Tags}");
                if (responseForImage.Item1 && responseForImage.Item2.Count > 0)
                {
                    var item = new NFTDataItem()
                    {
                        Storage = DataItemStorageType.IPFS,
                        IsMain = true,
                        Type = DataItemType.Image
                    };
                    var link = string.Empty;

                    try
                    {
                        await Console.Out.WriteLineAsync("Uploading image to IPFS...");
                        var bytes = Convert.FromBase64String(responseForImage.Item2.FirstOrDefault());
                        using (Stream stream = new MemoryStream(bytes))
                        {
                            //Request IPFS upload with StorageDriver
                            var result = await Storage.SaveFileToIPFS(new WriteStreamRequestDto()
                            {
                                Data = stream,
                                Filename = $"{DateTime.UtcNow.ToString("yyyy_MM_ddThh_mm_ss")}-{nft.Name.Replace(' ', '_')}.png",
                                DriverType = StorageDriverType.IPFS,
                                StorageId = "ChatGPT",
                                BackupInLocal = false,
                            });

                            if (result.Item1)
                            {
                                await Console.Out.WriteLineAsync("Upload finished.");
                                await Console.Out.WriteLineAsync("Image Link: " + result.Item2);

                                // store link to image for later use
                                link = result.Item2;

                                // store IPFS hash of image in DataItem
                                var hash = IPFSHelpers.GetHashFromIPFSLink(result.Item2);
                                if (!string.IsNullOrEmpty(hash))
                                    item.Hash = hash;
                            }
                            else
                                Console.WriteLine("Cannot upload. " + result.Item2);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync("Cannot upload Image to IPFS. " + ex.Message);
                    }

                    // Mint the NFT
                    if (!string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(item.Hash))
                    {
                        Console.WriteLine($"Link to NFT Image: {link}");
                        nft.ImageLink = link;
                        nft.DataItems.Add(item);
                        nft.Author = $"{account.Profile.Name} {account.Profile.Surname}";

                        Console.WriteLine("Minting NFT...");
                        var mintres = await account.MintNFT(nft);
                        if (mintres.Item1)
                        {
                            Console.WriteLine("-------------------------");
                            Console.WriteLine("NFT minted.");
                            Console.WriteLine($"NFT minted. Transaction ID is: {mintres.Item2}");
                            Console.WriteLine("-------------------------");
                        }
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("Cannot mint without the image.");
                    }
                }
                else
                {
                    await Console.Out.WriteLineAsync("Cannot create image.");
                }
            }
            else
            {
                await Console.Out.WriteLineAsync("Cannot create NFT description.");
            }

        }
        private static void Account_FirsLoadingStatus(object? sender, string e)
        {
            Console.WriteLine("Loading of account: " + e);
        }

    }
}