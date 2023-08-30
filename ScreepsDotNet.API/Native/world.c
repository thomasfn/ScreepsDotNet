#include <emscripten.h>
#include <assert.h>

#include <mono/metadata/loader.h>

void* copyBufferPtr = 0;

void DecodeRoomPosition(void* encodedRoomPositionPtr, void* outPtr);

void DecodeRoomPositions(void* encodedRoomPositionPtr, int encodedRoomPositionStride, void* outPtr, int outStride, int count);

void DecodeObjectId(void* encodedIdPtr, void* outPtr);

void DecodeObjectIds(void* encodedIdPtr, int encodedIdStride, void* outPtr, int outStride, int count);

void DecodeRoomObjectListFromCopyBuffer(void* outPtr, int count);

EMSCRIPTEN_KEEPALIVE void* ScreepsDotNet_InitNative_World(int copyBufferSize)
{
    copyBufferPtr = malloc(copyBufferSize);
	mono_add_internal_call("ScreepsDotNet_Native::DecodeRoomPosition", DecodeRoomPosition);
    mono_add_internal_call("ScreepsDotNet_Native::DecodeRoomPositions", DecodeRoomPositions);
    mono_add_internal_call("ScreepsDotNet_Native::DecodeObjectId", DecodeObjectId);
    mono_add_internal_call("ScreepsDotNet_Native::DecodeObjectIds", DecodeObjectIds);
    mono_add_internal_call("ScreepsDotNet_Native::DecodeRoomObjectListFromCopyBuffer", DecodeRoomObjectListFromCopyBuffer);
    return copyBufferPtr;
}

void DecodeRoomPosition(void* encodedRoomPositionPtr, void* outPtr)
{
    int* out = (int*)outPtr;
    // X:i32(4), y:i32(4), roomName:6i8(6), total=14
    out[0] = ((int*)encodedRoomPositionPtr)[0];
    out[1] = ((int*)encodedRoomPositionPtr)[1];
    char* inRoomName = ((char*)encodedRoomPositionPtr) + 8;
    if (inRoomName[0] == 0)
    {
        // Unknown room, maybe sim or invalid room name
        out[2] = 0x7fffffff;
        out[3] = 0x7fffffff;
        return;
    }

    char _0 = inRoomName[0], _1 = inRoomName[1], _2 = inRoomName[2];
    if (_0 == 'W')
    {
        if (_2 >= '0' && _2 <= '9')
        {
            out[2] = -((_1 - '0') * 10 + (_2 - '0') + 1);
            inRoomName += 3;
        }
        else
        {
            out[2] = -(_1 - '0' + 1);
            inRoomName += 2;
        }
    }
    else if (_0 == 'E')
    {
        if (_2 >= '0' && _2 <= '9')
        {
            out[2] = (_1 - '0') * 10 + (_2 - '0');
            inRoomName += 3;
        }
        else
        {
            out[2] = _1 - '0';
            inRoomName += 2;
        }
    }

    _0 = inRoomName[0], _1 = inRoomName[1], _2 = inRoomName[2];
    if (_0 == 'S') {
        if (_2 >= '0' && _2 <= '9')
        {
            out[3] = -((_1 - '0') * 10 + (_2 - '0') + 1);
            inRoomName += 3;
        }
        else
        {
            out[3] = -(_1 - '0' + 1);
            inRoomName += 2;
        }
    }
    else if (_0 == 'N')
    {
        if (_2 >= '0' && _2 <= '9')
        {
            out[3] = (_1 - '0') * 10 + (_2 - '0');
            inRoomName += 3;
        }
        else
        {
            out[3] = _1 - '0';
            inRoomName += 2;
        }
    }
}

void DecodeRoomPositions(void* encodedRoomPositionPtr, int encodedRoomPositionStride, void* outPtr, int outStride, int count)
{
    for (int i = 0; i < count; ++i)
    {
        DecodeRoomPosition(encodedRoomPositionPtr, outPtr);
        encodedRoomPositionPtr = (char*)encodedRoomPositionPtr + encodedRoomPositionStride;
        outPtr = (char*)outPtr + outStride;
    }
}

__attribute__((always_inline)) int DecodeHexDigit(int digit)
{
    return digit >= 97 ? 10 + (digit - 97) : digit - 48;
}

__attribute__((always_inline)) int Decode4HexDigits(int digits)
{
    return (DecodeHexDigit(digits & 255) << 12) | (DecodeHexDigit((digits >> 8) & 255) << 8) | (DecodeHexDigit((digits >> 16) & 255) << 4) | DecodeHexDigit(digits >> 24);
}

void DecodeObjectId(void* encodedIdPtr, void* outPtr)
{
    int* encodedId = (int*)encodedIdPtr;
    int* out = (int*)outPtr;
    int len;
    for (len = 0; len < 24; ++len)
    {
        if (((char*)encodedIdPtr)[len] == 0) { break; }
    }
    for (int i = len; i < 24; ++i)
    {
        ((char*)encodedIdPtr)[len] = '0';
    }
    out[0] = (Decode4HexDigits(encodedId[0]) << 16) | Decode4HexDigits(encodedId[1]);
    out[1] = (Decode4HexDigits(encodedId[2]) << 16) | Decode4HexDigits(encodedId[3]);
    out[2] = (Decode4HexDigits(encodedId[4]) << 16) | Decode4HexDigits(encodedId[5]);
    int hash = ((17 * 31 + out[0]) * 31 + out[1]) * 31 + out[2];
    out[3] = (hash & ~31) | len;
}

void DecodeObjectIds(void* encodedIdPtr, int encodedIdStride, void* outPtr, int outStride, int count)
{
    for (int i = 0; i < count; ++i)
    {
        DecodeObjectId(encodedIdPtr, outPtr);
        encodedIdPtr = (char*)encodedIdPtr + encodedIdStride;
        outPtr = (char*)outPtr + outStride;
    }
}

#pragma pack(push)
#pragma pack(1)

struct InRoomObjectDataPacket
{
    unsigned char Id[24];
    int TypeId;
    int Flags;
    int Hits;
    int HitsMax;
    unsigned char RoomPos[14];
    unsigned char unused0[2];
};

struct OutRoomObjectDataPacket
{
    unsigned char Id[16];
    int TypeId;
    int Flags;
    int Hits;
    int HitsMax;
    unsigned char RoomPos[16];
};

#pragma pack(pop)


void DecodeRoomObjectListFromCopyBuffer(void* outPtr, int count)
{
    static_assert(sizeof(struct InRoomObjectDataPacket) == 56, "InRoomObjectDataPacket unexpected size");
    static_assert(sizeof(struct OutRoomObjectDataPacket) == 48, "OutRoomObjectDataPacket unexpected size");

    struct InRoomObjectDataPacket* inPackets = (struct InRoomObjectDataPacket*)copyBufferPtr;
    struct OutRoomObjectDataPacket* outPackets = (struct OutRoomObjectDataPacket*)outPtr;

    for (int i = 0; i < count; ++i)
    {
        struct InRoomObjectDataPacket* inPacket = inPackets + i;
        struct OutRoomObjectDataPacket* outPacket = outPackets + i;

        DecodeObjectId(inPacket->Id, outPacket->Id);
        outPacket->TypeId = inPacket->TypeId;
        outPacket->Flags = inPacket->Flags;
        outPacket->Hits = inPacket->Hits;
        outPacket->HitsMax = inPacket->HitsMax;
        DecodeRoomPosition(inPacket->RoomPos, outPacket->RoomPos);
    }
}
