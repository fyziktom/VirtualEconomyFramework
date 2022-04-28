using System;
using VEDriversLite;
using Xunit;
using Moq;
using VEDriversLite.NFT;
using System.Threading.Tasks;
using System.Collections.Generic;
using VEDriversLite.NFT.DevicesNFTs;
using VEDriversLite.NFT.Coruzant;
using Newtonsoft.Json;

namespace VEFrameworkUnitTest.NFT.NFTDtos
{
    public class CommonNFTTests
    {
        public CommonNFTTests()
        {
            LoadNFTTypes();
        }

        public Dictionary<NFTTypes,INFT> nfts = new Dictionary<NFTTypes, INFT>();

        public async void LoadNFTTypes()
        {
            nfts.Add(NFTTypes.Image, new ImageNFT(""));
            nfts.Add(NFTTypes.Music, new MusicNFT(""));
            nfts.Add(NFTTypes.Post, new PostNFT(""));
            nfts.Add(NFTTypes.Profile, new ProfileNFT(""));
            nfts.Add(NFTTypes.Message, new MessageNFT(""));
            nfts.Add(NFTTypes.Payment, new PaymentNFT(""));
            nfts.Add(NFTTypes.Receipt, new ReceiptNFT(""));
            nfts.Add(NFTTypes.Event, new EventNFT(""));
            nfts.Add(NFTTypes.Ticket, new TicketNFT(""));
            nfts.Add(NFTTypes.Product, new ProductNFT(""));
            nfts.Add(NFTTypes.Order, new OrderNFT(""));
            nfts.Add(NFTTypes.Invoice, new InvoiceNFT(""));
            nfts.Add(NFTTypes.HWSrc, new HWSrcNFT(""));
            nfts.Add(NFTTypes.FWSrc, new FWSrcNFT(""));
            nfts.Add(NFTTypes.SWSrc, new SWSrcNFT(""));
            nfts.Add(NFTTypes.MechSrc, new MechSrcNFT(""));
            nfts.Add(NFTTypes.Protocol, new ProtocolNFT(""));
            nfts.Add(NFTTypes.Device, new DeviceNFT(""));
            nfts.Add(NFTTypes.IoTDevice, new IoTDeviceNFT(""));
            nfts.Add(NFTTypes.IoTMessage, new IoTMessageNFT(""));
            nfts.Add(NFTTypes.CoruzantProfile, new CoruzantProfileNFT(""));
            nfts.Add(NFTTypes.CoruzantArticle, new CoruzantArticleNFT(""));
        }

        public async Task FillCommontPropertiesTestCall(INFT nft)
        {
            var newnft = await NFTFactory.CloneNFT(nft); ;

            newnft.ImageLink = "";
            newnft.Name = "";
            newnft.Link = "";
            newnft.Description = "";
            newnft.Author = "";

            nft.ImageLink = "myimagelink";
            nft.Name = "mynft";
            nft.Link = "mylink";
            nft.Description = "mydescription";
            nft.Author = "myauthor";

            await newnft.Fill(nft);

            Assert.Equal(nft.ImageLink, newnft.ImageLink);
            Assert.Equal(nft.Name, newnft.Name);
            Assert.Equal(nft.Link, newnft.Link);
            Assert.Equal(nft.Description, newnft.Description);
            Assert.Equal(nft.Author, newnft.Author);

        }

        [Fact]
        public async Task FillCommontPropertiesTest()
        {
            foreach(var nft in nfts)
                await FillCommontPropertiesTestCall(nft.Value);
        }

        [Fact]
        public async Task FillSpecificPropertiesTest()
        {
            var random = new Random();
            var strnft = string.Empty;

            foreach(var nft in nfts)
            {
                var typeprops = nft.Value.GetType().GetProperties();
                foreach(var prop in typeprops)
                {
                    if (prop.PropertyType == typeof(string) && prop.CanWrite && prop.Name != "TypeText")
                        prop.SetValue(nft.Value, Convert.ChangeType(Guid.NewGuid().ToString(), prop.PropertyType), null);
                    if (prop.PropertyType == typeof(int) && prop.CanWrite)
                        prop.SetValue(nft.Value, Convert.ChangeType(random.Next(0,1000), prop.PropertyType), null);
                    if (prop.PropertyType == typeof(double) && prop.CanWrite)
                        prop.SetValue(nft.Value, Convert.ChangeType(random.NextDouble(), prop.PropertyType), null);
                    if (prop.PropertyType == typeof(DateTime) && prop.CanWrite)
                        prop.SetValue(nft.Value, Convert.ChangeType(DateTime.UtcNow, prop.PropertyType), null);
                    if (prop.PropertyType == typeof(bool) && prop.CanWrite)
                        prop.SetValue(nft.Value, Convert.ChangeType(true, prop.PropertyType), null);

                }

                strnft = JsonConvert.SerializeObject(nft.Value);

                var newnft = await NFTFactory.CloneNFT(nft.Value);
                var strnewnft = JsonConvert.SerializeObject(newnft);
                try
                {
                    Assert.Equal(strnft, strnewnft);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Wrong Cloning in {nft.Value.TypeText}");
                    throw ex;
                }
            }
        }

        [Fact]
        public async Task ClearPriceTest()
        {
            foreach (var nft in nfts.Values)
            {
                nft.Price = 5.0;
                nft.PriceActive = true;
                await nft.ClearPrices();
                Assert.NotEqual(nft.Price, 5.0);
                Assert.Equal(nft.Price, 0.0);
                Assert.Equal(nft.PriceActive, false);
            }
        }

        [Fact]
        public async Task ParseTagsTests()
        {
            foreach (var nft in nfts.Values)
            {
                nft.Tags = "venft nfts neblio";
                nft.ParseTags();
                if (nft.Type == NFTTypes.Ticket)
                {
                    Assert.Equal(nft.TagsList.Count, 4);
                    Assert.Equal(nft.TagsList[1], "venft");
                    Assert.Equal(nft.TagsList[3], "neblio");
                }
                else
                {
                    Assert.Equal(nft.TagsList.Count, 3);
                    Assert.Equal(nft.TagsList[0], "venft");
                    Assert.Equal(nft.TagsList[2], "neblio");
                }
            }
        }

        [Fact]
        public async Task ParsePriceTests()
        {
            var meta = new Dictionary<string, string>();
            meta.Add("Price", "10.0");
            meta.Add("DogePrice", "30.0");

            foreach (var nft in nfts.Values)
            {
                meta["Price"] = "10.0";
                meta["DogePrice"] = "30.0";
                meta.Add("SellJustCopy", "true");
                nft.ParsePrice(meta);
                Assert.Equal(nft.Price, 10.0);
                Assert.Equal(nft.PriceActive, true);
                Assert.Equal(nft.DogePrice, 30);
                Assert.Equal(nft.DogePriceActive, true);
                Assert.Equal(nft.SellJustCopy, true);
                meta["Price"] = "0.0";
                meta["DogePrice"] = "0.0";
                meta.Remove("SellJustCopy");
                nft.ParsePrice(meta);
                Assert.Equal(nft.Price, 0.0);
                Assert.Equal(nft.PriceActive, false);
                Assert.Equal(nft.DogePrice, 0);
                Assert.Equal(nft.DogePriceActive, false);
                Assert.Equal(nft.SellJustCopy, false);
            }
        }

        [Fact]
        public async Task ParseCommonTests()
        {
            var meta = new Dictionary<string, string>();
            meta.Add("Name", "myname");
            meta.Add("Description", "mydescription");
            meta.Add("Text", "mytext");

            foreach (var nft in nfts.Values)
            {
                nft.ParseCommon(meta);
                Assert.Equal(nft.Name, "myname");
                Assert.Equal(nft.Description, "mydescription");
                Assert.Equal(nft.Text, "mytext");
            }
        }

        [Fact]
        public async Task GetCommonMetadataTests()
        {
            var name = "myname";
            var description = "myname";
            var text = "mytext";
            var imagelink = "myimagelink";
            var price = 10.0;

            foreach (var nft in nfts.Values)
            {
                nft.Name = name;
                nft.Description = description;
                nft.Text = text;
                nft.ImageLink = imagelink;
                nft.Price = price;

                var meta = await nft.GetCommonMetadata();
                Assert.Equal(name, meta["Name"]);
                Assert.Equal(description, meta["Description"]);
                Assert.Equal(text, meta["Text"]);
                Assert.Equal(imagelink, meta["Image"]);
                Assert.Equal(nft.TypeText, meta["Type"]);
                Assert.Equal("10.000000", meta["Price"]);
                Assert.NotEqual("10,000000", meta["Price"]);
                Assert.NotEqual("10", meta["Price"]);
                Assert.Equal(nft.Type, NFTFactory.ParseNFTType(meta));
            }
        }
    }
}
