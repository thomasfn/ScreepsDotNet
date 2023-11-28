using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreepsDotNet.API.World
{
    public interface IInterShardMemory
    {
        /// <summary>
        /// Returns the string contents of the current shard's data.
        /// </summary>
        /// <returns></returns>
        string? GetLocal();

        /// <summary>
        /// Replace the current shard's data with the new value.
        /// </summary>
        /// <param name="value"></param>
        void SetLocal(string value);

        /// <summary>
        /// Returns the string contents of another shard's data.
        /// </summary>
        /// <param name="shard"></param>
        /// <returns></returns>
        string? GetRemote(string shard);
    }
}
