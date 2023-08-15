using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public readonly struct ForeignSegment
    {
        /// <summary>
        /// Another player's name.
        /// </summary>
        public readonly string Username;
        /// <summary>
        /// The ID of the requested memory segment.
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// The segment contents.
        /// </summary>
        public readonly string Data;

        public ForeignSegment(string username, int id, string data)
        {
            Username = username;
            Id = id;
            Data = data;
        }
    }

    /// <summary>
    /// RawMemory object allows to implement your own memory stringifier instead of built-in serializer based on JSON.stringify.
    /// It also allows to request up to 10 MB of additional memory using asynchronous memory segments feature.
    /// You can also access memory segments of other players using methods below.
    /// </summary>
    public interface IRawMemory
    {
        /// <summary>
        /// An object with asynchronous memory segments available on this tick.
        /// Each object key is the segment ID with data in string values.
        /// Use setActiveSegments to fetch segments on the next tick.
        /// Segments data is saved automatically in the end of the tick.
        /// The maximum size per segment is 100 KB.
        /// </summary>
        IDictionary<int, string> Segments { get; }

        /// <summary>
        /// An object with a memory segment of another player available on this tick.
        /// Use setActiveForeignSegment to fetch segments on the next tick.
        /// </summary>
        ForeignSegment? ForeignSegment { get; }

        /// <summary>
        /// Get a raw string representation of the Memory object.
        /// </summary>
        /// <returns></returns>
        string Get();

        /// <summary>
        /// Set new Memory value.
        /// </summary>
        /// <param name="value"></param>
        void Set(string value);

        /// <summary>
        /// Request memory segments using the list of their IDs. Memory segments will become available on the next tick in segments object.
        /// </summary>
        /// <param name="ids">An array of segment IDs. Each ID should be a number from 0 to 99. Maximum 10 segments can be active at the same time. Subsequent calls of setActiveSegments override previous ones.</param>
        void SetActiveSegments(IEnumerable<int> ids);

        /// <summary>
        /// Request a memory segment of another user.
        /// The segment should be marked by its owner as public using setPublicSegments.
        /// The segment data will become available on the next tick in foreignSegment object.
        /// You can only have access to one foreign segment at the same time.
        /// </summary>
        /// <param name="username">The name of another user. Pass null to clear the foreign segment.</param>
        /// <param name="id">The ID of the requested segment from 0 to 99. If undefined, the user's default public segment is requested as set by setDefaultPublicSegment.</param>
        void SetActiveForeignSegment(string? username, int? id = null);

        /// <summary>
        /// Set the specified segment as your default public segment. It will be returned if no id parameter is passed to setActiveForeignSegment by another user.
        /// </summary>
        /// <param name="id">The ID of the memory segment from 0 to 99. Pass null to remove your default public segment.</param>
        void SetDefaultPublicSegment(int? id);

        /// <summary>
        /// Set specified segments as public. Other users will be able to request access to them using setActiveForeignSegment.
        /// </summary>
        /// <param name="ids">An array of segment IDs. Each ID should be a number from 0 to 99. Subsequent calls of setPublicSegments override previous ones.</param>
        void SetPublicSegments(IEnumerable<int> ids);
    }
}
