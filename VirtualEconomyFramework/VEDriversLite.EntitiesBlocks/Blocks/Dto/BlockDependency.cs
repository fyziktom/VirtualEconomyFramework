using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks.Dto
{
    public class ConnectionsDto
    {
        public string BlockId { get; set; } = string.Empty;
        public Dictionary<string, Connection> UsedAsPreviousBlockConnectionIds { get; set; } = new Dictionary<string, Connection>();
        public Dictionary<string, Connection> UsedAsCenterBlockConnectionIds { get; set; } = new Dictionary<string, Connection>();
        public Dictionary<string, Connection> UsedAsNextBlockConnectionIds { get; set; } = new Dictionary<string, Connection>();
    }
    /// <summary>
    /// Connection rule from some block
    /// NOT means that previous block was not filled
    /// AND means that previous block was filled
    /// ANY means that previous block end no matters how
    /// </summary>
    public enum ConnectionRule
    {
        None,
        NOT,
        AND,
        ANY
    }
    /// <summary>
    /// Connection type in way of start time
    /// The block can be sync with start of previous block, end or custom value.
    /// Custom value means for example 50% for start at half of previous block
    /// </summary>
    public enum BlockConnectionType
    {
        None,
        Start,
        End,
        Percentage
    }
    public class Connection
    {
        /// <summary>
        /// Id of connection record
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Previous block Id
        /// </summary>
        public string PreviousBlockId { get; set; } = string.Empty;
        /// <summary>
        /// Block Id
        /// </summary>
        public string BlockId { get; set; } = string.Empty;
        /// <summary>
        /// Next Block Id
        /// </summary>
        public string NextBlockId { get; set; } = string.Empty;
        /// <summary>
        /// Rule of connection to previous block
        /// </summary>
        public ConnectionRule Rule { get; set; } = ConnectionRule.AND;
        /// <summary>
        /// Type of connection
        /// </summary>
        public BlockConnectionType Type { get; set; } = BlockConnectionType.End;
        /// <summary>
        /// Start in X percentage of previous block
        /// </summary>
        public double Percentage { get; set; } = 0;
        /// <summary>
        /// Delay before block start after the connection condition is filled
        /// </summary>
        public TimeSpan DelayBeforeStart { get; set; } = new TimeSpan(0);
        /// <summary>
        /// Delay after block end
        /// </summary>
        public TimeSpan DelayAfterEnd { get; set; } = new TimeSpan(0);
    }
    /// <summary>
    /// Relation to other blocks
    /// </summary>
    public class BlockDependency
    {
        /// <summary>
        /// Id of rule
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString(); 
        /// <summary>
        /// Name of rule
        /// </summary>
        public string Name { get; set; } = string.Empty; 
        /// <summary>
        /// Description of rule
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Priority of the relation
        /// 0 means maximum priority
        /// </summary>
        public int Priority { get; set; } = 0;

        private ConcurrentDictionary<string, Connection> connections = new ConcurrentDictionary<string, Connection>();
        /// <summary>
        /// List of connections to other blocks
        /// </summary>
        public ConcurrentDictionary<string, Connection> Connections { get => connections; }

        /// <summary>
        /// Add new connection between blocks
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool AddConnection(Connection connection)
        {
            if (string.IsNullOrEmpty(connection.BlockId))
                return false;

            if (string.IsNullOrEmpty(connection.PreviousBlockId) &&
                string.IsNullOrEmpty(connection.NextBlockId))
                return false;

            if (!connections.ContainsKey(connection.Id))
                return connections.TryAdd(connection.Id, connection);
            else
                return false;
        }
        /// <summary>
        /// Remove connection between blocks
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public bool RemoveConnection(string connectionId)
        {
            return connections.TryRemove(connectionId, out var connection);
        }

        /// <summary>
        /// Get connections list filtered by Previous block Id
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetConnectionByPreviousBlockId(string blockId)
        {
            return connections.Values.Where(c => c.PreviousBlockId == blockId).Select(c => c);
        }
        /// <summary>
        /// Get connections list filtered by Block Id
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetConnectionByBlockId(string blockId)
        {
            return connections.Values.Where(c => c.BlockId == blockId).Select(c => c);
        }
        /// <summary>
        /// Get connections list filtered by Next block Id
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetConnectionByNextBlockId(string blockId)
        {
            return connections.Values.Where(c => c.NextBlockId == blockId).Select(c => c);
        }

        /// <summary>
        /// Get connections list filtered by Connection rule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetConnectionByRule(ConnectionRule rule)
        {
            return connections.Values.Where(c => c.Rule == rule).Select(c => c);
        }
        /// <summary>
        /// Get connections list filtered by Connection type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Connection> GetConnectionByConnectionType(BlockConnectionType type)
        {
            return connections.Values.Where(c => c.Type == type).Select(c => c);
        }

        /// <summary>
        /// Clear list with all connections
        /// </summary>
        public void ClearAllConnections()
        {
            connections?.Clear();
        }
        /// <summary>
        /// Add or change previous block Id
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="previousBlockId"></param>
        /// <returns></returns>
        public bool AddPreviousBlockToConnection(string connectionId, string previousBlockId)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.PreviousBlockId = previousBlockId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Clear previous block Id
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public bool ClearPreviousBlockFromConnection(string connectionId)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.PreviousBlockId = string.Empty;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Add or change next block in connection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="nextBlockId"></param>
        /// <returns></returns>
        public bool AddNextBlockToConnection(string connectionId, string nextBlockId)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.NextBlockId = nextBlockId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Clear next block Id
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public bool ClearNextBlockFromConnection(string connectionId)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.NextBlockId = string.Empty;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Change connection type
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool ChangeConnectionType(string connectionId, BlockConnectionType type)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.Type = type;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Change connection rule
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool ChangeConnectionRule(string connectionId, ConnectionRule rule)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.Rule = rule;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Change connection percentage
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public bool ConnectionPercentage(string connectionId, double percentage)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.Percentage = percentage;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Change connection delay before start
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="delayBeforeStart"></param>
        /// <returns></returns>
        public bool ChangeConnectionDelayBeforeStart(string connectionId, TimeSpan delayBeforeStart)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.DelayBeforeStart = delayBeforeStart;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Change connection delay after end
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="delayAfterEnd"></param>
        /// <returns></returns>
        public bool ChangeConnectionDelayAfterEnd(string connectionId, TimeSpan delayAfterEnd)
        {
            if (connections.TryGetValue(connectionId, out var connection))
            {
                connection.DelayAfterEnd = delayAfterEnd;
                return true;
            }
            return false;
        }
    }
}
