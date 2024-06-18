// Ported from https://github.com/WebAssembly/wasi-libc/blob/31845366d4a2212a9a6bfe4d2336f7869ef3f6d9/libc-top-half/musl/src/string/memcpy.c
@inline()
export function memcpy(dest: i32, src: i32, n: i32): i32 {
  let d = <u32>dest; // unsigned char*
  let s = <u32>src; // unsigned char*

  // If the source pointer is unaligned, copy byte-by-byte until it's word-aligned
  for (; s % 4 && n; n--) {
    store<u8>(d, load<u8>(s));
    d++; s++;
  }

  // If the destination pointer is word-aligned, do a normal word-by-word copy, and fall back to byte-by-byte for any remainder
  if (d % 4 == 0) {
    for (; n>=16; s+=16, d+=16, n-=16) {
      store<u32>(d, load<u32>(s, 0), 0);
      store<u32>(d, load<u32>(s, 4), 4);
      store<u32>(d, load<u32>(s, 8), 8);
      store<u32>(d, load<u32>(s, 12), 12);
    }
    if (n&8) {
      store<u32>(d, load<u32>(s, 0), 0);
      store<u32>(d, load<u32>(s, 4), 4);
      d += 8; s += 8;
    }
    if (n&4) {
      store<u32>(d, load<u32>(s, 0), 0);
      d += 4; s += 4;
    }
    if (n&2) {
      store<u16>(d, load<u16>(s, 0), 0);
      d += 2; s += 2;
    }
    if (n&1) {
      store<u8>(d, load<u8>(s, 0), 0);
      d++; s++;
    }
    return dest;
  }

  // The destination pointer is unaligned, fall back to an aliased word-by-word copy
  let w: u32, x: u32;
  if (n >= 32) {
    switch (d % 4) {
      case 1:
        w = load<u32>(s);
        store<u8>(d, load<u8>(s));
        d++; s++;
        store<u8>(d, load<u8>(s));
        d++; s++;
        store<u8>(d, load<u8>(s));
        d++; s++;
        n -= 3;
        for (; n>=17; s+=16, d+=16, n-=16) {
          x = load<u32>(s, 1);
          store<u32>(d, (w << 24) | (x >> 8), 0);
          w = load<u32>(s, 5);
          store<u32>(d, (x << 24) | (w >> 8), 4);
          x = load<u32>(s, 9);
          store<u32>(d, (w << 24) | (x >> 8), 8);
          w = load<u32>(s, 13);
          store<u32>(d, (x << 24) | (w >> 8), 12);
        }
        break;
      case 2:
        w = load<u32>(s);
        store<u8>(d, load<u8>(s));
        d++; s++;
        store<u8>(d, load<u8>(s));
        d++; s++;
        n -= 2;
        for (; n>=18; s+=16, d+=16, n-=16) {
          x = load<u32>(s, 2);
          store<u32>(d, (w << 16) | (x >> 16), 0);
          w = load<u32>(s, 6);
          store<u32>(d, (x << 16) | (w >> 16), 4);
          x = load<u32>(s, 10);
          store<u32>(d, (w << 16) | (x >> 16), 8);
          w = load<u32>(s, 14);
          store<u32>(d, (x << 16) | (w >> 16), 12);
        }
        break;
      case 3:
        w = load<u32>(s);
        store<u8>(d, load<u8>(s));
        d++; s++;
        n -= 1;
        for (; n>=19; s+=16, d+=16, n-=16) {
          x = load<u32>(s, 3);
          store<u32>(d, (w << 8) | (x >> 24), 0);
          w = load<u32>(s, 7);
          store<u32>(d, (x << 8) | (w >> 24), 4);
          x = load<u32>(s, 11);
          store<u32>(d, (w << 8) | (x >> 24), 8);
          w = load<u32>(s, 15);
          store<u32>(d, (x << 8) | (w >> 24), 12);
        }
        break;
    }
  	if (n&16) {
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
  	}
  	if (n&8) {
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
  	}
  	if (n&4) {
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
  	}
  	if (n&2) {
      store<u8>(d, load<u8>(s));
      d++; s++;
      store<u8>(d, load<u8>(s));
      d++; s++;
  	}
  	if (n&1) {
      store<u8>(d, load<u8>(s));
      d++; s++;
  	}
  	return dest;
  }

  return dest;
}

// Ported from https://github.com/WebAssembly/wasi-libc/blob/31845366d4a2212a9a6bfe4d2336f7869ef3f6d9/libc-top-half/musl/src/string/memmove.c
export function memmove(dest: i32, src: i32, n: i32): i32 {
  let d = <u32>dest; // unsigned char*
  let s = <u32>src; // unsigned char*

  // If dest and src are the same pointer, nothing to do
  if (d == s) {
    return dest;
  }

  // If the source and destination ranges do not overlap, a regular memcpy will suffice
  if (src-dest-n <= -2*n) {
    return memcpy(d, s, n);
  }

  // If dest comes before src in linear memory...
  if (d<s) {
    // Copy forward
    if (s % 4 == d % 4) {
      while (d % 4) {
        if (!n--) {
          return dest;
        }
        store<u8>(d, load<u8>(s));
        d++; s++;
      }
      for (; n>=4; n-=4, d+=4, s+=4) {
        store<u32>(d, load<u32>(s));
      }
    }
    for (; n; n--) {
      store<u8>(d, load<u8>(s));
      d++; s++;
    }
  } else {
    // Copy backward
    if (s % 4 == d % 4) {
      while ((d+n) % 4) {
        if (!n--) return dest;
        store<u8>(d+n, load<u8>(s+n));
      }
      for (; n>=4; n-=4) {
        store<u32>(d+n, load<u32>(s+n));
      }
    }
    while (n) {
      n--;
      store<u8>(d+n, load<u8>(s+n));
    }
  }

  return dest;
}

// Ported from https://github.com/WebAssembly/wasi-libc/blob/31845366d4a2212a9a6bfe4d2336f7869ef3f6d9/libc-top-half/musl/src/string/memset.c
export function memset(dest: i32, c: i32, n: i32): i32 {
  let s = <u32>dest; // unsigned char*

  let k: u32;

  // Fill head and tail with minimal branching
  // Each conditional ensures that all the subsequently used offsets are well-defined and in the dest region
  if (!n) { return dest; }
  store<i8>(s, c);
  store<i8>(s+n-1, c);
  if (n <= 2) { return dest; }
  store<i8>(s+1, c);
  store<i8>(s+2, c);
  store<i8>(s+n-2, c);
  store<i8>(s+n-3, c);
  if (n <= 6) { return dest; }
  store<i8>(s+3, c);
  store<i8>(s+n-4, c);
  if (n <= 8) { return dest; }

  // Advance pointer to align it at a 4-byte boundary, and truncate n to a multiple of 4
  // The previous code already took care of any head/tail that get cut off by the alignment

	k = -s & 3;
	s += k;
	n -= k;
	n &= -4;

  const c32: u32 = (<u32>-1)/255 * <u8>c;

	// In preparation to copy 32 bytes at a time, aligned on an 8-byte bounary, fill head/tail up to 28 bytes each
	// As in the initial byte-based head/tail fill, each conditional below ensures that the subsequent offsets are valid (e.g. !(n<=24) implies n>=28)

  store<u32>(s+0, c32);
  store<u32>(s+n-4, c32);
	if (n <= 8) { return dest; }
  store<u32>(s+4, c32);
  store<u32>(s+8, c32);
  store<u32>(s+n-12, c32);
  store<u32>(s+n-8, c32);
	if (n <= 24) { return dest; }
  store<u32>(s+12, c32);
  store<u32>(s+16, c32);
  store<u32>(s+20, c32);
  store<u32>(s+24, c32);
  store<u32>(s+n-28, c32);
  store<u32>(s+n-24, c32);
  store<u32>(s+n-20, c32);
  store<u32>(s+n-16, c32);

	// Align to a multiple of 8 so we can fill 64 bits at a time, and avoid writing the same bytes twice as much as is practical without introducing additional branching.

	k = 24 + (s & 4);
	s += k;
	n -= k;

	// If this loop is reached, 28 tail bytes have already been filled, so any remainder when n drops below 32 can be safely ignored

	const c64: u64 = c32 | (<u64>c32 << 32);

	for (; n >= 32; n-=32, s+=32) {
    store<u64>(s+0, c64);
    store<u64>(s+8, c64);
    store<u64>(s+16, c64);
    store<u64>(s+24, c64);
	}

  return dest;
}
