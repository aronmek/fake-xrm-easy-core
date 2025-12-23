using System.Text.Json;
using System.Text.Json.Serialization;

namespace FakeXrmEasy.Snapshots
{
    /// <summary>
    /// JSON implementation of snapshot serialization using System.Text.Json
    /// </summary>
    public class JsonSnapshotSerializer : ISnapshotSerializer
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Creates a new instance of JsonSnapshotSerializer with default settings
        /// </summary>
        public JsonSnapshotSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Creates a new instance of JsonSnapshotSerializer with custom settings
        /// </summary>
        /// <param name="options">Custom JSON serializer options</param>
        public JsonSnapshotSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Serializes snapshot data to JSON using flattened DTOs
        /// </summary>
        /// <param name="data">The snapshot data to serialize</param>
        /// <returns>The JSON string representation</returns>
        public string Serialize(SnapshotData data)
        {
            return JsonSerializer.Serialize(data, _options);
        }

        /// <summary>
        /// Deserializes snapshot data from JSON
        /// </summary>
        /// <param name="content">The JSON string content</param>
        /// <returns>The deserialized snapshot data</returns>
        public SnapshotData Deserialize(string content)
        {
            return JsonSerializer.Deserialize<SnapshotData>(content, _options);
        }
    }
}
