using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Wraps an object that exists somewhere in the global screeps Memory object.
    /// </summary>
    public interface IMemoryObject
    {
        /// <summary>
        /// Gets the keys of all properties contained within the object.
        /// </summary>
        IEnumerable<string> Keys { get; }

        /// <summary>
        /// Attempts to retrieve an int property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetInt(string key, out int value);

        /// <summary>
        /// Attempts to retrieve an int property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetInt(Name key, out int value);

        /// <summary>
        /// Attempts to retrieve a string property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetString(string key, [MaybeNullWhen(false)] out string value);

        /// <summary>
        /// Attempts to retrieve a string property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetString(Name key, [MaybeNullWhen(false)] out string value);

        /// <summary>
        /// Attempts to retrieve a string property as a Name from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetName(string key, out Name value);

        /// <summary>
        /// Attempts to retrieve a string property as a Name from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetName(Name key, out Name value);

        /// <summary>
        /// Attempts to retrieve a double property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetDouble(string key, out double value);

        /// <summary>
        /// Attempts to retrieve a double property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetDouble(Name key, out double value);

        /// <summary>
        /// Attempts to retrieve a bool property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetBool(string key, out bool value);

        /// <summary>
        /// Attempts to retrieve a bool property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetBool(Name key, out bool value);

        /// <summary>
        /// Attempts to retrieve an object property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetObject(string key, [MaybeNullWhen(false)] out IMemoryObject value);

        /// <summary>
        /// Attempts to retrieve an object property from the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetObject(Name key, [MaybeNullWhen(false)] out IMemoryObject value);

        /// <summary>
        /// Sets an int property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, int value);

        /// <summary>
        /// Sets an int property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(Name key, int value);

        /// <summary>
        /// Sets a string property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, string value);

        /// <summary>
        /// Sets a string property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(Name key, string value);

        /// <summary>
        /// Sets a string property as a Name on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, Name value);

        /// <summary>
        /// Sets a string property as a Name on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(Name key, Name value);

        /// <summary>
        /// Sets a double property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, double value);

        /// <summary>
        /// Sets a double property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(Name key, double value);

        /// <summary>
        /// Sets a bool property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, bool value);

        /// <summary>
        /// Sets a bool property on the object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(Name key, bool value);

        /// <summary>
        /// Retrieves an object property from the object, creating it if it does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IMemoryObject GetOrCreateObject(string key);

        /// <summary>
        /// Retrieves an object property from the object, creating it if it does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IMemoryObject GetOrCreateObject(Name key);

        /// <summary>
        /// Removes a property from the object.
        /// </summary>
        /// <param name="key"></param>
        void ClearValue(string key);

        /// <summary>
        /// Removes a property from the object.
        /// </summary>
        /// <param name="key"></param>
        void ClearValue(Name key);
    }
}
