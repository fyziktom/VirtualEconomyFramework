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
    public class Node : Entity
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int Type { get; set; }
        public string Address { get; set; }
        public string Parameters { get; set; }
        public string AccountId { get; set; }
        public bool? IsActivated { get; set; }

        [ForeignKey(nameof(Key.Id))]
        virtual public HashSet<Key> Keys { get; set; } = new HashSet<Key>();


        public void Update(INode node)
        {
            if (string.IsNullOrEmpty(Id))
            {
                if (!string.IsNullOrEmpty(node.Id.ToString()))
                {
                    Id = node.Id.ToString();
                }
                else
                {
                    Id = Guid.NewGuid().ToString();
                }
            }

            IsActivated = node.IsActivated;

            Name = node.Name;
            Parameters = node.Parameters;

            if (node.AccountId != Guid.Empty)
                AccountId = node.AccountId.ToString();
            else
                AccountId = null;

            Type = (int)node.Type;
            if (!string.IsNullOrEmpty(node.Version))
                Version = node.Version;
            else
                Version = "0.1";
            ModifiedOn = DateTime.UtcNow;

            if (node.Deleted != null)
                Deleted = (bool)node.Deleted;

            if (string.IsNullOrEmpty(ModifiedBy) && !string.IsNullOrEmpty(node.CreatedBy))
                ModifiedBy = node.ModifiedBy;
            else if (string.IsNullOrEmpty(ModifiedBy) && string.IsNullOrEmpty(node.ModifiedBy))
                ModifiedBy = "admin";

            if (string.IsNullOrEmpty(CreatedBy) && !string.IsNullOrEmpty(node.CreatedBy))
                CreatedBy = node.CreatedBy;
            else if (string.IsNullOrEmpty(CreatedBy) && string.IsNullOrEmpty(node.CreatedBy))
                CreatedBy = "admin";

            CreatedOn = node.CreatedOn;

        }

        public INode Fill(INode node)
        {
            if (node == null)
                return null;

            try
            {
                node.Id = new Guid(Id);
            }
            catch (Exception ex)
            {
                log.Error("Node Guid stored in db is not valid, creating new one", ex);
                node.Id = new Guid();
            }

            try
            {
                node.AccountId = new Guid(AccountId);
            }
            catch (Exception ex)
            {
                log.Error("AccountId Guid stored in db is not valid, creating new one", ex);
                node.AccountId = new Guid();
            }

            node.IsActivated = IsActivated;
            node.Name = Name;
            node.Parameters = Parameters;
            node.Type = (NodeTypes)Type;
            node.Version = Version;
            node.ModifiedOn = ModifiedOn;
            node.ModifiedBy = ModifiedBy;
            node.Deleted = Deleted;
            node.CreatedBy = CreatedBy;
            node.CreatedOn = CreatedOn;

            return node;
        }

    }
}
