using System.Collections.Generic;

namespace FakeXrmEasy.Snapshots
{
    /// <summary>
    /// Represents a snapshot of entity data using flattened DTOs for serialization
    /// </summary>
    public class SnapshotData
    {
        /// <summary>
        /// All entities stored in the snapshot as flattened DTOs
        /// </summary>
        public List<EntitySnapshot> Entities { get; set; }

        /// <summary>
        /// Creates a new instance of SnapshotData
        /// </summary>
        public SnapshotData()
        {
            Entities = new List<EntitySnapshot>();
        }
    }
}
