namespace FakeXrmEasy.Snapshots
{
    /// <summary>
    /// Interface for serializing and deserializing snapshot data
    /// </summary>
    public interface ISnapshotSerializer
    {
        /// <summary>
        /// Serializes snapshot data to a string
        /// </summary>
        /// <param name="data">The snapshot data to serialize</param>
        /// <returns>The serialized string representation</returns>
        string Serialize(SnapshotData data);

        /// <summary>
        /// Deserializes snapshot data from a string
        /// </summary>
        /// <param name="content">The serialized string content</param>
        /// <returns>The deserialized snapshot data</returns>
        SnapshotData Deserialize(string content);
    }
}
