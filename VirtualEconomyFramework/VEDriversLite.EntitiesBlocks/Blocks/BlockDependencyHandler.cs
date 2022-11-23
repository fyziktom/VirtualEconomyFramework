using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public class BlockDependencyHandler
    {
        /// <summary>
        /// Connection to other blocks
        /// </summary>
        public ConcurrentDictionary<string, BlockDependency> BlocksDependencies { get; set; } = new ConcurrentDictionary<string, BlockDependency>();

        /// <summary>
        /// Add new dependency
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public bool AddBlockDependency(BlockDependency dependency)
        {
            if (!BlocksDependencies.ContainsKey(dependency.Id))
                return BlocksDependencies.TryAdd(dependency.Id, dependency);
            else
                return false;
        }
        /// <summary>
        /// remove dependency
        /// </summary>
        /// <param name="dependencyId"></param>
        /// <returns></returns>
        public bool RemoveDependency(string dependencyId)
        {
            return BlocksDependencies.TryRemove(dependencyId, out var dependency);
        }

        /// <summary>
        /// Add connection to dependency
        /// </summary>
        /// <param name="dependencyId"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool AddDependencyConnection(string dependencyId, Connection connection)
        {
            if (BlocksDependencies.TryGetValue(dependencyId, out var dependency))
                return dependency.AddConnection(connection);
            else
                return false;
        }
        /// <summary>
        /// Remove connection from dependency
        /// </summary>
        /// <param name="dependencyId"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public bool AddDependencyConnection(string dependencyId, string connectionId)
        {
            if (BlocksDependencies.TryGetValue(dependencyId, out var dependency))
                return dependency.RemoveConnection(connectionId);
            else
                return false;
        }

        /// <summary>
        /// Get connections of dependency
        /// </summary>
        /// <param name="dependencyId"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetDependencyConnections(string dependencyId)
        {
            if (BlocksDependencies.TryGetValue(dependencyId, out var dependency))
                return dependency.Connections.Values;
            else
                return new List<Connection>();
        }

        /// <summary>
        /// Get connections of dependency
        /// </summary>
        /// <param name="dependencyId"></param>
        /// <returns></returns>
        public Connection GetDependencyConnection(string dependencyId, string connectionId)
        {
            if (BlocksDependencies.TryGetValue(dependencyId, out var dependency))
            {
                if (dependency.Connections.TryGetValue(connectionId, out var connection))
                    return connection;
            }
            return new Connection();
        }

        /// <summary>
        /// Change parameters of connection in dependency
        /// </summary>
        /// <param name="dependencyId"></param>
        /// <param name="connectionId"></param>
        /// <param name="previousBlockId"></param>
        /// <param name="nextBlockId"></param>
        /// <param name="type"></param>
        /// <param name="rule"></param>
        /// <param name="beforeDelay"></param>
        /// <param name="afterDelay"></param>
        /// <returns></returns>
        public bool ChangeConnectionParameters(string dependencyId, 
                                               string connectionId,
                                               string? previousBlockId = null,
                                               string? nextBlockId = null,
                                               BlockConnectionType? type = null,
                                               ConnectionRule? rule = null,
                                               TimeSpan? beforeDelay = null,
                                               TimeSpan? afterDelay = null)
        {
            if (BlocksDependencies.TryGetValue(dependencyId, out var dependency))
            {
                if (previousBlockId != null)
                    dependency.AddPreviousBlockToConnection(connectionId, previousBlockId);
                if (nextBlockId != null)
                    dependency.AddNextBlockToConnection(connectionId, nextBlockId);
                if (type != null)
                    dependency.ChangeConnectionType(connectionId, (BlockConnectionType)type);
                if (rule != null)
                    dependency.ChangeConnectionRule(connectionId, (ConnectionRule)rule);
                if (beforeDelay != null)
                    dependency.ChangeConnectionDelayBeforeStart(connectionId, (TimeSpan)beforeDelay);
                if (afterDelay != null)
                    dependency.ChangeConnectionDelayBeforeStart(connectionId, (TimeSpan)afterDelay);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear list with all relations
        /// </summary>
        public void ClearAllDependencies()
        {
            BlocksDependencies?.Clear();
        }

        /// <summary>
        /// Clear all connections in dependency
        /// </summary>
        public void ClearAllConnectionsInDependency(string dependencyId)
        {
            if (BlocksDependencies.TryGetValue(dependencyId, out var dependency))
                dependency.ClearAllConnections();
        }

        /// <summary>
        /// Get all block connections. The connections are sorted by how the block is used in the connection
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns>Connection Dto with dictionaries of connections</returns>
        public ConnectionsDto GetBlockConnections(string blockId)
        {
            var result = new ConnectionsDto() { BlockId = blockId };

            foreach (var dependency in BlocksDependencies.Values)
            {
                var connections = dependency.GetConnectionByPreviousBlockId(blockId).ToList();
                if (connections != null && connections.Count > 0)
                    foreach (var connection in connections)
                        result.UsedAsPreviousBlockConnectionIds.TryAdd(connection.Id, connection);

                connections = dependency.GetConnectionByBlockId(blockId).ToList();
                if (connections != null && connections.Count > 0)
                    foreach(var connection in connections)
                        result.UsedAsCenterBlockConnectionIds.TryAdd(connection.Id, connection);

                connections = dependency.GetConnectionByNextBlockId(blockId).ToList();
                if (connections != null && connections.Count > 0)
                    foreach (var connection in connections)
                        result.UsedAsNextBlockConnectionIds.TryAdd(connection.Id, connection);
            }

            return result;
        }
    }
}
