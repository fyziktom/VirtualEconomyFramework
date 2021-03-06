using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Wallets;

namespace VEDrivers.Database.Models
{
    public class Wallet : Entity
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int Type { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        [ForeignKey(nameof(Account.Id))]
        virtual public HashSet<Account> Settings { get; set; } = new HashSet<Account>();

        public void Update(IWallet wallet)
        {
            if (string.IsNullOrEmpty(Id))
            {
                if (!string.IsNullOrEmpty(wallet.Id.ToString()))
                {
                    Id = wallet.Id.ToString();
                }
                else
                {
                    Id = Guid.NewGuid().ToString();
                }
            }
                
            Name = wallet.Name;
            Host = wallet.ConnectionUrlBaseAddress;
            Port = wallet.ConnectionPort;

            Type = (int)wallet.Type;

            if (!string.IsNullOrEmpty(wallet.Version))
                Version = wallet.Version;
            else
                Version = "0.1";

            ModifiedOn = DateTime.UtcNow;

            if (wallet.Deleted != null)
                Deleted = (bool)wallet.Deleted;

            if (string.IsNullOrEmpty(ModifiedBy) && !string.IsNullOrEmpty(wallet.CreatedBy))
                ModifiedBy = wallet.ModifiedBy;
            else if (string.IsNullOrEmpty(wallet.ModifiedBy) && string.IsNullOrEmpty(wallet.ModifiedBy))
                ModifiedBy = "admin";

            if (string.IsNullOrEmpty(wallet.CreatedBy) && !string.IsNullOrEmpty(wallet.CreatedBy))
                CreatedBy = wallet.CreatedBy;
            else if (string.IsNullOrEmpty(CreatedBy) && string.IsNullOrEmpty(wallet.CreatedBy))
                CreatedBy = "admin";

            CreatedOn = wallet.CreatedOn;
        }

        public IWallet Fill(IWallet wallet)
        {
            if (wallet == null)
                return null;

            try
            {
                wallet.Id = new Guid(Id);
            }
            catch (Exception ex)
            {
                log.Error("Wallet Guid stored in db is not valid, creating new one", ex);
                wallet.Id = new Guid();
            }
            
            wallet.Name = Name;
            wallet.ConnectionUrlBaseAddress = Host;
            wallet.ConnectionPort = Port;
            wallet.Type = (WalletTypes)Type;
            wallet.Version = Version;
            wallet.ModifiedOn = ModifiedOn;
            wallet.ModifiedBy = ModifiedBy;
            wallet.Deleted = Deleted;
            wallet.CreatedBy = CreatedBy;
            wallet.CreatedOn = CreatedOn;

            return wallet;
        }

    }
}
