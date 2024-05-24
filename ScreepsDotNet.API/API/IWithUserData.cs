using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ScreepsDotNet.API
{
    /// <summary>
    /// Describes an object capable of storing user data.
    /// Storing and retrieving user data associated with an object is fast but data should be considered ephemeral and will be lost if the object loses visibility or is destroyed.
    /// User data does not cross the interop boundary.
    /// </summary>
    public interface IWithUserData
    {
        /// <summary>
        /// Associate some user data with this object.
        /// Only one user data per user data type may be associated with a single object.
        /// Null can be passed to clear user data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userData"></param>
        void SetUserData<T>(T? userData) where T : class;

        /// <summary>
        /// Try and retrieve previously stored user data with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userData"></param>
        /// <returns></returns>
        bool TryGetUserData<T>([MaybeNullWhen(false)] out T userData) where T : class;

        /// <summary>
        /// Retrieve previously stored user data with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T? GetUserData<T>() where T : class;

        /// <summary>
        /// Gets if user data has previously been stored with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasUserData<T>() where T : class;
    }

    internal static class UserDataType
    {
        private static int nextId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetId()
        {
            return nextId++;
        }
    }

    internal static class UserDataType<T> where T : class
    {
        public static readonly int Value;

        static UserDataType()
        {
            Value = UserDataType.GetId();
        }
    }

    internal struct UserDataStorage : IWithUserData
    {
        private object?[]? storage;

        public void SetUserData<T>(T? userData) where T : class
        {
            if (userData == null)
            {
                if (storage == null) { return; }
                int id = UserDataType<T>.Value;
                if (id >= storage.Length) { return; }
                storage[id] = null;
            }
            else
            {
                int id = UserDataType<T>.Value;
                if (storage == null)
                {
                    storage = new object[id + 1];
                }
                else if (id >= storage.Length)
                {
                    Array.Resize(ref storage, id + 1);
                }
                storage[id] = userData;
            }
        }

        public readonly bool TryGetUserData<T>([MaybeNullWhen(false)] out T userData) where T : class
        {
            if (storage == null)
            {
                userData = null;
                return false;
            }
            int id = UserDataType<T>.Value;
            if (id >= storage.Length)
            {
                userData = null;
                return false;
            }
            userData = storage[id] as T;
            return userData != null;
        }

        public readonly T? GetUserData<T>() where T : class
        {
            if (storage == null) { return null; }
            int id = UserDataType<T>.Value;
            if (id >= storage.Length) { return null;  }
            return storage[id] as T;
        }

        public readonly bool HasUserData<T>() where T : class
        {
            if (storage == null) { return false; }
            int id = UserDataType<T>.Value;
            if (id >= storage.Length) { return false; }
            return storage[id] is T;
        }
    }
}
