using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Tree
{
    public static class TreeViewHelpers
    {
        public static void PrintTree(TreeItem tree, String indent, bool last)
        {
            if (tree.Type == EntityType.Source)
                Console.WriteLine(indent + "+- " + " Source: " + tree.Name + "\n");
            else
                Console.WriteLine(indent + "+- " + " Consumer: " + tree.Name + "\n");
            indent += last ? "   " : "|  ";

            for (int i = 0; i < tree.Children?.Count; i++)
            {
                PrintTree(tree.Children.ToList()[i], indent, i == tree.Children?.Count - 1);
            }
        }
    }
}
