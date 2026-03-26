#define CLOCKID_REALTIME (0)

#define ERRNO_SUCCESS (0)
#define ERRNO_BADF (8)
#define ERRNO_INVAL (28)
#define ERRNO_PERM (63)

#define FD_STDIN (0)
#define FD_STDOUT (1)
#define FD_STDERR (2)

typedef unsigned char uint8_t;
typedef char int8_t;
typedef unsigned int uint16_t;
typedef int int16_t;
typedef unsigned int uint32_t;
typedef int int32_t;
typedef unsigned long long uint64_t;
typedef long long int64_t;

#define STDFD_BUFFER_SIZE (1024)
#define STDFD_CNT (3)

typedef struct {
    uint32_t ptr;
    uint32_t len;
} List_t;

typedef struct {
    uint8_t tag; /* 0 = none, 1 = some */
} OptionTag_t;
