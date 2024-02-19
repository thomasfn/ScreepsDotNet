#include <assert.h>

#include <mono/metadata/loader.h>

const int kMaxWorldSize = 256;
const int kMaxWorldSize2 = kMaxWorldSize >> 1;

#pragma pack(push)
#pragma pack(1)

// 16b
struct ObjectId
{
    int A;
    int B;
    int C;
    int H;
};

// 8b
struct Position
{
    int X;
    int Y;
};

// 8b
struct RoomCoord
{
    int X;
    int Y;
};

// 16b
struct RoomPosition
{
    struct Position Position;
    struct RoomCoord RoomCoord;
};

// 24b
struct RawObjectId
{
    unsigned char Id[24];
};

#pragma pack(pop)

__attribute__((__import_module__("bindings"), __import_name__("js_renew_object")))
extern int js_renew_object(void* jsHandle);

__attribute__((__import_module__("bindings"), __import_name__("js_batch_renew_objects")))
extern int js_batch_renew_objects(void** jsHandleList, int count);

__attribute__((__import_module__("bindings"), __import_name__("js_fetch_object_room_position")))
extern int js_fetch_object_room_position(void* jsHandle);

__attribute__((__import_module__("bindings"), __import_name__("js_batch_fetch_object_room_positions")))
extern void js_batch_fetch_object_room_positions(void** jsHandleList, int count, int* outPackedRoomPosList);

__attribute__((__import_module__("bindings"), __import_name__("js_get_object_by_id")))
extern void* js_get_object_by_id(struct RawObjectId* rawObjectId);

__attribute__((always_inline))
void DecodeRoomPosition(int packedRoomPos, struct RoomPosition* outPos)
{
    outPos->Position.X = (packedRoomPos >> 8) & 0xff;
    outPos->Position.Y = packedRoomPos & 0xff;
    outPos->RoomCoord.X = (int)((unsigned int)packedRoomPos >> 24) - kMaxWorldSize2;
    outPos->RoomCoord.Y = (int)(((unsigned int)packedRoomPos >> 16) & 0xff) - kMaxWorldSize2;
}

__attribute__((always_inline))
int DecodeHexDigit(int digit)
{
    return digit >= 97 ? 10 + (digit - 97) : digit - 48;
}

__attribute__((always_inline))
int Decode4HexDigits(int digits)
{
    return (DecodeHexDigit(digits & 255) << 12) | (DecodeHexDigit((digits >> 8) & 255) << 8) | (DecodeHexDigit((digits >> 16) & 255) << 4) | DecodeHexDigit(digits >> 24);
}

void DecodeObjectId(struct RawObjectId* rawObjectId, struct ObjectId* outObjectIdPtr)
{
    int* encodedId = (int*)rawObjectId->Id;
    int len;
    for (len = 23; len >= 0; --len)
    {
        if (rawObjectId->Id[len] != 0) { break; }
    }
    ++len;
    for (int i = len; i < 24; ++i)
    {
        rawObjectId->Id[len] = '0';
    }
    outObjectIdPtr->A = (Decode4HexDigits(encodedId[0]) << 16) | Decode4HexDigits(encodedId[1]);
    outObjectIdPtr->B = (Decode4HexDigits(encodedId[2]) << 16) | Decode4HexDigits(encodedId[3]);
    outObjectIdPtr->C = (Decode4HexDigits(encodedId[4]) << 16) | Decode4HexDigits(encodedId[5]);
    outObjectIdPtr->H = ((((17 * 31 + outObjectIdPtr->A) * 31 + outObjectIdPtr->B) * 31 + outObjectIdPtr->C) & ~31) | len;
}

void DecodeObjectIds(struct RawObjectId* rawObjectIdList, int count, struct ObjectId* outObjectIdList)
{
    for (int i = 0; i < count; ++i)
    {
        DecodeObjectId(rawObjectIdList + i, outObjectIdList + i);
    }
}

__attribute__((always_inline))
char EncodeHexDigit(int nibble)
{
    return nibble >= 10 ? 97 + (nibble - 10) : 48 + nibble;
}

__attribute__((always_inline))
int Encode4HexDigits(int value)
{
    return (EncodeHexDigit(value & 0xf) << 24) | (EncodeHexDigit((value >> 4) & 0xf) << 16) | (EncodeHexDigit((value >> 8) & 0xf) << 8) | EncodeHexDigit((value >> 12) & 0xf);
}

void EncodeObjectId(struct ObjectId* objectId, struct RawObjectId* outRawObjectId)
{
    int* encodedId = (int*)outRawObjectId->Id;
    encodedId[0] = Encode4HexDigits(objectId->A >> 16);
    encodedId[1] = Encode4HexDigits(objectId->A & 0xffff);
    encodedId[2] = Encode4HexDigits(objectId->B >> 16);
    encodedId[3] = Encode4HexDigits(objectId->B & 0xffff);
    encodedId[4] = Encode4HexDigits(objectId->C >> 16);
    encodedId[5] = Encode4HexDigits(objectId->C & 0xffff);
    int len = objectId->H & 31;
    if (len < 24) { outRawObjectId->Id[len] = '\0'; }
}

void FetchObjectRoomPosition(void* jsHandle, struct RoomPosition* outRoomPos)
{
    DecodeRoomPosition(js_fetch_object_room_position(jsHandle), outRoomPos);
}

void BatchFetchObjectRoomPositions(void** jsHandleList, int count, struct RoomPosition* outRoomPosList)
{
    int packedRoomPositions[count];
    js_batch_fetch_object_room_positions(jsHandleList, count, packedRoomPositions);
    for (int i = 0; i < count; ++i)
    {
        DecodeRoomPosition(packedRoomPositions[i], outRoomPosList + i);
    }
}

void* GetObjectById(struct ObjectId* objectId)
{
    struct RawObjectId rawObjectId;
    EncodeObjectId(objectId, &rawObjectId);
    return js_get_object_by_id(&rawObjectId);
}

__attribute__((export_name("screepsdotnet_init_world")))
void screepsdotnet_init_world()
{
    mono_add_internal_call("ScreepsDotNet_Native::RenewObject", js_renew_object);
    mono_add_internal_call("ScreepsDotNet_Native::BatchRenewObjects", js_batch_renew_objects);
    mono_add_internal_call("ScreepsDotNet_Native::DecodeObjectIds", DecodeObjectIds);
    mono_add_internal_call("ScreepsDotNet_Native::FetchObjectRoomPosition", FetchObjectRoomPosition);
    mono_add_internal_call("ScreepsDotNet_Native::BatchFetchObjectRoomPositions", BatchFetchObjectRoomPositions);
    mono_add_internal_call("ScreepsDotNet_Native::GetObjectById", GetObjectById);
}
