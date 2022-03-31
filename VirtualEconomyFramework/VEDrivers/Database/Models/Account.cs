using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;

namespace VEDrivers.Database.Models
{
    public class Account : Entity
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int Type { get; set; }
        public string Address { get; set; }
        public string WalletId { get; set; }
        public string AccountKeyId { get; set; }


        [ForeignKey(nameof(Node.Id))]
        virtual public HashSet<Node> Nodes { get; set; } = new HashSet<Node>();

        [ForeignKey(nameof(Key.Id))]
        virtual public HashSet<Key> Keys { get; set; } = new HashSet<Key>();

        public void Update(IAccount acc)
        {
            if (string.IsNullOrEmpty(Id))
            {
                if (!string.IsNullOrEmpty(acc.Id.ToString()))
                {
                    Id = acc.Id.ToString();
                }
                else
                {
                    Id = Guid.NewGuid().ToString();
                }
            }

            if (!string.IsNullOrEmpty(acc.AccountKeyId.ToString()))
                AccountKeyId = acc.AccountKeyId.ToString();

            Name = acc.Name;
            Address = acc.Address;
            Type = (int)acc.Type;
            if (!string.IsNullOrEmpty(acc.Version))
                Version = acc.Version;
            else
                Version = "0.1";
            ModifiedOn = DateTime.UtcNow;

            if (acc.Deleted != null)
                Deleted = (bool)acc.Deleted;

            if (string.IsNullOrEmpty(ModifiedBy) && !string.IsNullOrEmpty(acc.CreatedBy))
                ModifiedBy = acc.ModifiedBy;
            else if (string.IsNullOrEmpty(ModifiedBy) && string.IsNullOrEmpty(acc.ModifiedBy))
                ModifiedBy = "admin";

            if (string.IsNullOrEmpty(CreatedBy) && !string.IsNullOrEmpty(acc.CreatedBy))
                CreatedBy = acc.CreatedBy;
            else if (string.IsNullOrEmpty(CreatedBy) && string.IsNullOrEmpty(acc.CreatedBy))
                CreatedBy = "admin";

            CreatedOn = acc.CreatedOn;

            WalletId = acc.WalletId.ToString();
        }

        public IAccount Fill(IAccount acc)
        {
            if (acc == null)
                return null;

            try
            {
                acc.Id = new Guid(Id);
            }
            catch (Exception ex)
            {
                log.Error("Account Guid stored in db is not valid, creating new one", ex);
                acc.Id = new Guid();
            }

            try
            {
                acc.AccountKeyId = new Guid(AccountKeyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Account Guid Key stored in db is not valid or empty, create empty!" + ex.Message);
                acc.AccountKeyId = Guid.Empty;
            }

            acc.Name = Name;
            acc.Address = Address;
            acc.Type = (AccountTypes)Type;
            acc.Version = Version;
            acc.ModifiedOn = ModifiedOn;
            acc.ModifiedBy = ModifiedBy;
            acc.Deleted = Deleted;
            acc.CreatedBy = CreatedBy;
            acc.CreatedOn = CreatedOn;

            return acc;
        }

    }
}
