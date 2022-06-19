using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using VEBlazor.Demo.Publishing.Server.Services;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.NFT.Tags;

namespace VEBlazor.Demo.Publishing.Server.Controllers
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // We'd normally just use "*" for the allow-origin header, 
            // but Chrome (and perhaps others) won't allow you to use authentication if
            // the header is set to "*".
            // TODO: Check elsewhere to see if the origin is actually on the list of trusted domains.
            var ctx = filterContext.HttpContext;
            //var origin = ctx.Request.Headers["Origin"];
            //var allowOrigin = !string.IsNullOrWhiteSpace(origin) ? origin : "*";
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            base.OnActionExecuting(filterContext);
        }
    }
    public class NFTController : Controller
    {
        // GET: NFTController/GetNFT/{Utxo}
        [HttpGet("GetNFT/{utxo}")]        
        public async Task<INFT> GetNFT(string utxo)
        {
            return await Services.DataCoreService.GetNFT(utxo);
        }

        [HttpGet("GetNFTArticles")]
        public async Task<Dictionary<string, INFT>> GetNFTArticles()
        {
            return await GetNFTArticles(0);
        }        
        [HttpGet("GetNFTArticles/{settings}/{skip}/{take}")]
        public async Task<Dictionary<string, INFT>> GetNFTArticles(int settings = 0, int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, INFT>();
                           
            if (MainDataContext.ObservedAccountsTabs.TryGetValue(MainDataContext.MainCoruzantPublishingAddress, out var tab))
            {
                foreach (var nft in tab.NFTs.Where(n => n.Type == NFTTypes.CoruzantArticle).Skip(skip).Take(take))
                {
                    nft.TxDetails = null;
                    dict.Add($"{nft.Utxo}:{nft.UtxoIndex}", nft);
                }
            }
            return dict;
        }

        [HttpGet("GetNFTProfiles")]
        public async Task<Dictionary<string, INFT>> GetNFTProfiles()
        {
            return await GetNFTProfiles(0);
        }        
        [HttpGet("GetNFTProfiles/{settings}/{skip}/{take}")]        
        public async Task<Dictionary<string, INFT>> GetNFTProfiles(int settings = 0, int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, INFT>();

            if (MainDataContext.ObservedAccountsTabs.TryGetValue(MainDataContext.MainCoruzantPublishingAddress, out var tab))
            {
                foreach (var nft in tab.NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile).Skip(skip).Take(take))
                {
                    nft.TxDetails = null;
                    dict.Add($"{nft.Utxo}:{nft.UtxoIndex}", nft);
                }
            }
            return dict;
        }

        [HttpGet("GetNFTPodcasts")]
        public async Task<Dictionary<string, INFT>> GetNFTPodcasts()
        {
            return await GetNFTPodcasts(0);
        }
        [HttpGet("GetNFTPodcasts/{settings}/{skip}/{take}")]
        public async Task<Dictionary<string, INFT>> GetNFTPodcasts(int settings = 0, int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, INFT>();

            if (MainDataContext.ObservedAccountsTabs.TryGetValue(MainDataContext.MainCoruzantPublishingAddress, out var tab))
            {
                foreach (var nft in tab.NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile).Skip(skip).Take(take))
                {
                    if (!string.IsNullOrEmpty((nft as CoruzantProfileNFT)?.PodcastId))
                    {
                        nft.TxDetails = null;
                        dict.Add($"{nft.Utxo}:{nft.UtxoIndex}", nft);
                    }
                }
            }
            return dict;
        }        

        [HttpGet("GetNFTTags")]
        public async Task<Dictionary<string, Tag>> GetNFTTags()
        {
            var dict = new Dictionary<string, Tag>();
            var tags = NFTDataContext.Tags.Values.ToList()?.OrderByDescending(t => t.Count).ToList();
            if (tags == null) return dict;
            
            foreach (var tag in tags)
                dict.Add(tag.Name, tag);
            return dict;      
        }
        [HttpGet("GetNFTTags/{skip}/{take}")]
        public async Task<Dictionary<string, Tag>> GetNFTTags(int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, Tag>();
            var tags = NFTDataContext.Tags.Values.ToList()?.OrderByDescending(t => t.Count).ToList().Skip(skip).Take(take);
            if (tags == null) return dict;

            foreach (var tag in tags)
                dict.Add(tag.Name, tag);
            return dict;
        }

        [HttpGet("GetNFTArticlesByTags/{tagName}")]
        public async Task<Dictionary<string, INFT>> GetNFTArticlesByTags(string tagName)
        {
            return await GetNFTArticlesByTags(tagName, 0);
        }
        [HttpGet("GetNFTArticlesByTags/{tagName}/{settings}/{skip}/{take}")]
        public async Task<Dictionary<string, INFT>> GetNFTArticlesByTags(string tagName, int settings = 0, int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, INFT>();

            if (MainDataContext.ObservedAccountsTabs.TryGetValue(MainDataContext.MainCoruzantPublishingAddress, out var tab))
            {
                foreach (var nft in tab.NFTs.Where(n => n.Type == NFTTypes.CoruzantArticle).Where(n => n.TagsList.Contains(tagName)).Skip(skip).Take(take))
                {
                    nft.TxDetails = null;
                    dict.Add($"{nft.Utxo}:{nft.UtxoIndex}", nft);
                }                    
            }            

            return dict;
        }

        [HttpGet("GetNFTProfilessByTags/{tagName}")]
        public async Task<Dictionary<string, INFT>> GetNFTProfilesByTags(string tagName)
        {
            return await GetNFTProfilesByTags(tagName, 0);
        }
        [HttpGet("GetNFTProfilessByTags/{tagName}/{settings}/{skip}/{take}")]
        public async Task<Dictionary<string, INFT>> GetNFTProfilesByTags(string tagName, int settings = 0, int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, INFT>();

            if (MainDataContext.ObservedAccountsTabs.TryGetValue(MainDataContext.MainCoruzantPublishingAddress, out var tab))
            {
                foreach (var nft in tab.NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile).Where(n => n.TagsList.Contains(tagName)).Skip(skip).Take(take))
                {
                    nft.TxDetails = null;
                    dict.Add($"{nft.Utxo}:{nft.UtxoIndex}", nft);
                }                    
            }

            return dict;
        }

        [HttpGet("GetNFTPodcastsByTags/{tagName}")]
        public async Task<Dictionary<string, INFT>> GetNFTPodcastsByTags(string tagName)
        {
            return await GetNFTPodcastsByTags(tagName, 0);
        }
        [HttpGet("GetNFTPodcastsByTags/{tagName}/{settings}/{skip}/{take}")]
        public async Task<Dictionary<string, INFT>> GetNFTPodcastsByTags(string tagName, int settings = 0, int skip = 0, int take = 25)
        {
            var dict = new Dictionary<string, INFT>();

            if (MainDataContext.ObservedAccountsTabs.TryGetValue(MainDataContext.MainCoruzantPublishingAddress, out var tab))
            {
                foreach (var nft in tab.NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile).Where(n => n.TagsList.Contains(tagName)).Skip(skip).Take(take))
                {
                    if (!string.IsNullOrEmpty((nft as CoruzantProfileNFT).PodcastId))
                    {
                        nft.TxDetails = null;
                        dict.Add($"{nft.Utxo}:{nft.UtxoIndex}", nft);
                    }
                }
            }

            return dict;
        }


        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetBuzzsproutData/{profile}/{podcast}")]
        public async Task<List<BuzzsproutEpisodeDto>> GetBuzzsproutData(string profile, string podcast)
        {
            if (string.IsNullOrEmpty(profile))
                return null;

            try
            {
                var apiToken = "3ede6be154859d46dbbc11b9e3f3094f";

                using (var client = new HttpClient())
                {
                    var url = $"https://www.buzzsprout.com/api/{profile}/episodes.json";

                    using (var content = new MultipartFormDataContent())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Token token={apiToken}");
                        var response = await client.GetAsync(url);
                        var jsonresponse = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<List<BuzzsproutEpisodeDto>>(jsonresponse);
                        if (res != null)
                        {
                            var pod = res.FirstOrDefault(p => p.id == Convert.ToInt32(podcast));
                            if (pod != null)
                                return new List<BuzzsproutEpisodeDto>() { pod };
                            else
                                return res;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }
            return null;
        }

    }
}
