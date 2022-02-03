using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Bookmarks
{
    public class BookmarksHandler
    {
        public ConcurrentDictionary<string, Bookmark> Bookmarks { get; set; } = new ConcurrentDictionary<string, Bookmark>();

        /// <summary>
        /// Load bookmarks from previous serialized list of bookmarks. 
        /// </summary>
        /// <param name="bookmarks"></param>
        /// <returns></returns>
        public async Task LoadBookmarks(string bookmarks)
        {
            try
            {
                var bkm = JsonConvert.DeserializeObject<List<Bookmark>>(bookmarks);
                if (bkm != null)
                    foreach (var b in bkm)
                        if (!Bookmarks.TryGetValue(b.Address, out var bk))
                            Bookmarks.TryAdd(b.Address, b);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize the bookmarks. " + ex.Message);
            }
        }

        /// <summary>
        /// Add new bookmark to bookmark list and return serialized list for save
        /// </summary>
        /// <param name="name">Name of the bookmark. It is important for most functions which work with the bookmarks</param>
        /// <param name="address">Neblio Address</param>
        /// <param name="note">optional note</param>
        /// <returns>Serialized list in string for save</returns>
        public async Task<(bool, string)> AddBookmark(string name, string address, string note)
        {
            if (!Bookmarks.TryGetValue(address, out var bk))
            {
                var bkm = new Bookmark()
                {
                    Name = name,
                    Address = address,
                    Note = note
                };
                Bookmarks.TryAdd(address, bkm);
                return (true, JsonConvert.SerializeObject(Bookmarks));
            }
            else
            {
                Console.WriteLine("Bookmark Already Exists.");
                return (false, "Already Exists.");
            }
        }

        /// <summary>
        /// Remove bookmark by the neblio address. It must be found in the bookmark list
        /// </summary>
        /// <param name="address"></param>
        /// <returns>Serialized list in string for save</returns>
        public async Task<(bool, string)> RemoveBookmark(string address)
        {
            if (Bookmarks.TryGetValue(address, out var bk))
                Bookmarks.TryRemove(address, out bk);
            else
            {
                Console.WriteLine("Bookmark Not Found.");
                return (false, string.Empty);
            }
            return (true, JsonConvert.SerializeObject(Bookmarks.Values));
        }

        /// <summary>
        /// Get serialized bookmarks list as string
        /// </summary>
        /// <returns></returns>
        public async Task<string> SerializeBookmarks()
        {
            return JsonConvert.SerializeObject(Bookmarks);
        }

        /// <summary>
        /// Check if the address is already in the bookmarks and return this bookmark
        /// </summary>
        /// <param name="address">Address which should be in the bookmarks</param>
        /// <returns>true and bookmark class if exists</returns>
        public async Task<(bool, Bookmark)> IsInTheBookmarks(string address)
        {
            if (Bookmarks.TryGetValue(address, out var bk))
                return (true, bk);
            else
                return (false, new Bookmark());
        }
    }
}
