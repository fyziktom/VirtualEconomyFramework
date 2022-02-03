using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;

namespace VEDriversLite.Accounts.Handlers
{
    public class AccountHandler
    {
        public ConcurrentDictionary<string, IAccount> Accounts { get; set; } = new ConcurrentDictionary<string, IAccount>();
        public Bookmarks.BookmarksHandler Bookmarks { get; set; } = new Bookmarks.BookmarksHandler();
        public (bool, IAccount) IsAccountExists(string address)
        {
            if (Accounts.TryGetValue(address, out var acc))
                return (true, acc);
            else
                return (false, null);
        }


        public async Task<(bool,string)> CreateAccount(AccountLoadData data)
        {
            try
            {
                IAccount parent = null;
                if (data.LoadAsSubAccount && !string.IsNullOrEmpty(data.ParentAddress))
                {
                    var rp = IsAccountExists(data.ParentAddress);
                    if (!rp.Item1)
                        return (false, "Parent Address is not loaded yet. This address cannot be loaded as its SubAccount if parent not exists.");
                    else
                        parent = rp.Item2;
                }
                var account = await AccountFactory.GetAccount(data.Type);
                if (await account.CreateNewAccount(data))
                {
                    parent.SubAccounts.Add(account.Address);
                    Accounts.TryAdd(account.Address, account);
                    return (true, account.Address);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot create the account. " + ex.Message);
            }
            return (false, "Cannot create the account.");
        }
        public async Task<(bool,string)> LoadAccount(AccountLoadData data)
        {
            try
            {
                IAccount parent = null;
                if (data.LoadAsSubAccount && !string.IsNullOrEmpty(data.ParentAddress))
                {
                    var rp = IsAccountExists(data.ParentAddress);
                    if (!rp.Item1)
                        return (false, "Parent Address is not loaded yet. This address cannot be loaded as its SubAccount if parent not exists.");
                    else
                        parent = rp.Item2;
                }
                var account = await AccountFactory.GetAccount(data.Type);
                if (await account.LoadAccount(data))
                {
                    parent.SubAccounts.Add(account.Address);
                    Accounts.TryAdd(account.Address, account);
                    return (true, account.Address);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load the account. " + ex.Message);
            }
            return (false, "Cannot load the account.");
        }
        public async Task<(bool,string)> RemoveAccount(string address)
        {
            try
            {
                if (Accounts.TryRemove(address, out var addr))
                {
                    if (!string.IsNullOrEmpty(addr.ParentAddress))
                    {
                        if (Accounts.TryGetValue(addr.ParentAddress, out var parent))
                            if (parent.SubAccounts.Contains(address))
                                parent.SubAccounts.Remove(address);
                    }
                    return (true, "REMOVED");
                }
                else
                {
                    return (false, "Cannot find the account.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot create the account. " + ex.Message);
            }
            return (false, "Cannot create the account.");
        }
        /*
        public async Task<(bool,string)> Send(TxToSend data)
        {

        }

        public async Task<(bool,object)> ProcessAccountFunction(AccountFunctionData data)
        {

        }
        */
    }
}
