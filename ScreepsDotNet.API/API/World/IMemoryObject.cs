namespace ScreepsDotNet.API.World
{
    public interface IMemoryObject
    {
        bool TryGetInt(string key, out int value);

        bool TryGetString(string key, out string value);

        bool TryGetDouble(string key, out double value);

        bool TryGetBool(string key, out bool value);

        void SetValue(string key, int value);

        void SetValue(string key, string value);

        void SetValue(string key, double value);

        void SetValue(string key, bool value);
    }
}
