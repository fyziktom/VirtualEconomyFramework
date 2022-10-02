using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Tree
{
    public class TreeItem
    {
        /// <summary>
        /// Id of the Item
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Name of the Item
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Depth in the tree
        /// </summary>
        public int? Depth { get; set; }
        /// <summary>
        /// Parent TreeItem
        /// </summary>
        public TreeItem? Parent { get; set; }
        /// <summary>
        /// Collection of TreeItem childs
        /// </summary>
        public ICollection<TreeItem>? Children { get; set; }
        /// <summary>
        /// Check if this item is parent = contains some children
        /// </summary>
        public bool IsParent { get => Children?.Count > 0; }
        /// <summary>
        /// Type of the Entity
        /// </summary>
        public EntityType Type { get; set; }

        /// <summary>
        /// Get Enumerable of all childs
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeItem> GetChilds()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                    yield return child;
            }
        }

        /// <summary>
        /// Add one child to the TreeItem
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddChild(TreeItem item)
        {
            if (Children == null) Children = new List<TreeItem>();

            if (item != null && !Children.Contains(item))
            {
                Children.Add(item);
                return true;
            }

            return false;
        }

        public TreeItem ContainsChild(string id)
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    if (child.Id == id)
                        return child;
                }
            }
            return null;
        }
    }
}
