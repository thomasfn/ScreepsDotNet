var bootloader = (function (exports) {
    'use strict';

    // DEFLATE is a complex format; to read this code, you should probably check the RFC first:
    // https://tools.ietf.org/html/rfc1951
    // You may also wish to take a look at the guide I made about this program:
    // https://gist.github.com/101arrowz/253f31eb5abc3d9275ab943003ffecad
    // Some of the following code is similar to that of UZIP.js:
    // https://github.com/photopea/UZIP.js
    // However, the vast majority of the codebase has diverged from UZIP.js to increase performance and reduce bundle size.
    // Sometimes 0 will appear where -1 would be more appropriate. This is because using a uint
    // is better for memory in most engines (I *think*).

    // aliases for shorter compressed code (most minifers don't do this)
    var u8 = Uint8Array,
      u16 = Uint16Array,
      i32 = Int32Array;
    // fixed length extra bits
    var fleb = new u8([0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, /* unused */0, 0, /* impossible */0]);
    // fixed distance extra bits
    var fdeb = new u8([0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, /* unused */0, 0]);
    // code length index map
    var clim = new u8([16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15]);
    // get base, reverse index map from extra bits
    var freb = function (eb, start) {
      var b = new u16(31);
      for (var i = 0; i < 31; ++i) {
        b[i] = start += 1 << eb[i - 1];
      }
      // numbers here are at max 18 bits
      var r = new i32(b[30]);
      for (var i = 1; i < 30; ++i) {
        for (var j = b[i]; j < b[i + 1]; ++j) {
          r[j] = j - b[i] << 5 | i;
        }
      }
      return {
        b: b,
        r: r
      };
    };
    var _a = freb(fleb, 2),
      fl = _a.b,
      revfl = _a.r;
    // we can ignore the fact that the other numbers are wrong; they never happen anyway
    fl[28] = 258, revfl[258] = 28;
    var _b = freb(fdeb, 0),
      fd = _b.b;
    // map of value to reverse (assuming 16 bits)
    var rev = new u16(32768);
    for (var i = 0; i < 32768; ++i) {
      // reverse table algorithm from SO
      var x$1 = (i & 0xAAAA) >> 1 | (i & 0x5555) << 1;
      x$1 = (x$1 & 0xCCCC) >> 2 | (x$1 & 0x3333) << 2;
      x$1 = (x$1 & 0xF0F0) >> 4 | (x$1 & 0x0F0F) << 4;
      rev[i] = ((x$1 & 0xFF00) >> 8 | (x$1 & 0x00FF) << 8) >> 1;
    }
    // create huffman tree from u8 "map": index -> code length for code index
    // mb (max bits) must be at most 15
    // TODO: optimize/split up?
    var hMap = function (cd, mb, r) {
      var s = cd.length;
      // index
      var i = 0;
      // u16 "map": index -> # of codes with bit length = index
      var l = new u16(mb);
      // length of cd must be 288 (total # of codes)
      for (; i < s; ++i) {
        if (cd[i]) ++l[cd[i] - 1];
      }
      // u16 "map": index -> minimum code for bit length = index
      var le = new u16(mb);
      for (i = 1; i < mb; ++i) {
        le[i] = le[i - 1] + l[i - 1] << 1;
      }
      var co;
      if (r) {
        // u16 "map": index -> number of actual bits, symbol for code
        co = new u16(1 << mb);
        // bits to remove for reverser
        var rvb = 15 - mb;
        for (i = 0; i < s; ++i) {
          // ignore 0 lengths
          if (cd[i]) {
            // num encoding both symbol and bits read
            var sv = i << 4 | cd[i];
            // free bits
            var r_1 = mb - cd[i];
            // start value
            var v = le[cd[i] - 1]++ << r_1;
            // m is end value
            for (var m = v | (1 << r_1) - 1; v <= m; ++v) {
              // every 16 bit value starting with the code yields the same result
              co[rev[v] >> rvb] = sv;
            }
          }
        }
      } else {
        co = new u16(s);
        for (i = 0; i < s; ++i) {
          if (cd[i]) {
            co[i] = rev[le[cd[i] - 1]++] >> 15 - cd[i];
          }
        }
      }
      return co;
    };
    // fixed length tree
    var flt = new u8(288);
    for (var i = 0; i < 144; ++i) flt[i] = 8;
    for (var i = 144; i < 256; ++i) flt[i] = 9;
    for (var i = 256; i < 280; ++i) flt[i] = 7;
    for (var i = 280; i < 288; ++i) flt[i] = 8;
    // fixed distance tree
    var fdt = new u8(32);
    for (var i = 0; i < 32; ++i) fdt[i] = 5;
    // fixed length map
    var flrm = /*#__PURE__*/hMap(flt, 9, 1);
    // fixed distance map
    var fdrm = /*#__PURE__*/hMap(fdt, 5, 1);
    // find max of array
    var max = function (a) {
      var m = a[0];
      for (var i = 1; i < a.length; ++i) {
        if (a[i] > m) m = a[i];
      }
      return m;
    };
    // read d, starting at bit p and mask with m
    var bits = function (d, p, m) {
      var o = p / 8 | 0;
      return (d[o] | d[o + 1] << 8) >> (p & 7) & m;
    };
    // read d, starting at bit p continuing for at least 16 bits
    var bits16 = function (d, p) {
      var o = p / 8 | 0;
      return (d[o] | d[o + 1] << 8 | d[o + 2] << 16) >> (p & 7);
    };
    // get end of byte
    var shft = function (p) {
      return (p + 7) / 8 | 0;
    };
    // typed array slice - allows garbage collector to free original reference,
    // while being more compatible than .slice
    var slc = function (v, s, e) {
      if (s == null || s < 0) s = 0;
      if (e == null || e > v.length) e = v.length;
      // can't use .constructor in case user-supplied
      var n = new u8(e - s);
      n.set(v.subarray(s, e));
      return n;
    };
    // error codes
    var ec = ['unexpected EOF', 'invalid block type', 'invalid length/literal', 'invalid distance', 'stream finished', 'no stream handler',, 'no callback', 'invalid UTF-8 data', 'extra field too long', 'date not in range 1980-2099', 'filename too long', 'stream finishing', 'invalid zip data'
    // determined by unknown compression method
    ];
    var err = function (ind, msg, nt) {
      var e = new Error(msg || ec[ind]);
      e.code = ind;
      if (Error.captureStackTrace) Error.captureStackTrace(e, err);
      if (!nt) throw e;
      return e;
    };
    // expands raw DEFLATE data
    var inflt = function (dat, st, buf, dict) {
      // source length       dict length
      var sl = dat.length,
        dl = dict ? dict.length : 0;
      if (!sl || st.f && !st.l) return buf || new u8(0);
      // have to estimate size
      var noBuf = !buf || st.i != 2;
      // no state
      var noSt = st.i;
      // Assumes roughly 33% compression ratio average
      if (!buf) buf = new u8(sl * 3);
      // ensure buffer can fit at least l elements
      var cbuf = function (l) {
        var bl = buf.length;
        // need to increase size to fit
        if (l > bl) {
          // Double or set to necessary, whichever is greater
          var nbuf = new u8(Math.max(bl * 2, l));
          nbuf.set(buf);
          buf = nbuf;
        }
      };
      //  last chunk         bitpos           bytes
      var final = st.f || 0,
        pos = st.p || 0,
        bt = st.b || 0,
        lm = st.l,
        dm = st.d,
        lbt = st.m,
        dbt = st.n;
      // total bits
      var tbts = sl * 8;
      do {
        if (!lm) {
          // BFINAL - this is only 1 when last chunk is next
          final = bits(dat, pos, 1);
          // type: 0 = no compression, 1 = fixed huffman, 2 = dynamic huffman
          var type = bits(dat, pos + 1, 3);
          pos += 3;
          if (!type) {
            // go to end of byte boundary
            var s = shft(pos) + 4,
              l = dat[s - 4] | dat[s - 3] << 8,
              t = s + l;
            if (t > sl) {
              if (noSt) err(0);
              break;
            }
            // ensure size
            if (noBuf) cbuf(bt + l);
            // Copy over uncompressed data
            buf.set(dat.subarray(s, t), bt);
            // Get new bitpos, update byte count
            st.b = bt += l, st.p = pos = t * 8, st.f = final;
            continue;
          } else if (type == 1) lm = flrm, dm = fdrm, lbt = 9, dbt = 5;else if (type == 2) {
            //  literal                            lengths
            var hLit = bits(dat, pos, 31) + 257,
              hcLen = bits(dat, pos + 10, 15) + 4;
            var tl = hLit + bits(dat, pos + 5, 31) + 1;
            pos += 14;
            // length+distance tree
            var ldt = new u8(tl);
            // code length tree
            var clt = new u8(19);
            for (var i = 0; i < hcLen; ++i) {
              // use index map to get real code
              clt[clim[i]] = bits(dat, pos + i * 3, 7);
            }
            pos += hcLen * 3;
            // code lengths bits
            var clb = max(clt),
              clbmsk = (1 << clb) - 1;
            // code lengths map
            var clm = hMap(clt, clb, 1);
            for (var i = 0; i < tl;) {
              var r = clm[bits(dat, pos, clbmsk)];
              // bits read
              pos += r & 15;
              // symbol
              var s = r >> 4;
              // code length to copy
              if (s < 16) {
                ldt[i++] = s;
              } else {
                //  copy   count
                var c = 0,
                  n = 0;
                if (s == 16) n = 3 + bits(dat, pos, 3), pos += 2, c = ldt[i - 1];else if (s == 17) n = 3 + bits(dat, pos, 7), pos += 3;else if (s == 18) n = 11 + bits(dat, pos, 127), pos += 7;
                while (n--) ldt[i++] = c;
              }
            }
            //    length tree                 distance tree
            var lt = ldt.subarray(0, hLit),
              dt = ldt.subarray(hLit);
            // max length bits
            lbt = max(lt);
            // max dist bits
            dbt = max(dt);
            lm = hMap(lt, lbt, 1);
            dm = hMap(dt, dbt, 1);
          } else err(1);
          if (pos > tbts) {
            if (noSt) err(0);
            break;
          }
        }
        // Make sure the buffer can hold this + the largest possible addition
        // Maximum chunk size (practically, theoretically infinite) is 2^17
        if (noBuf) cbuf(bt + 131072);
        var lms = (1 << lbt) - 1,
          dms = (1 << dbt) - 1;
        var lpos = pos;
        for (;; lpos = pos) {
          // bits read, code
          var c = lm[bits16(dat, pos) & lms],
            sym = c >> 4;
          pos += c & 15;
          if (pos > tbts) {
            if (noSt) err(0);
            break;
          }
          if (!c) err(2);
          if (sym < 256) buf[bt++] = sym;else if (sym == 256) {
            lpos = pos, lm = null;
            break;
          } else {
            var add = sym - 254;
            // no extra bits needed if less
            if (sym > 264) {
              // index
              var i = sym - 257,
                b = fleb[i];
              add = bits(dat, pos, (1 << b) - 1) + fl[i];
              pos += b;
            }
            // dist
            var d = dm[bits16(dat, pos) & dms],
              dsym = d >> 4;
            if (!d) err(3);
            pos += d & 15;
            var dt = fd[dsym];
            if (dsym > 3) {
              var b = fdeb[dsym];
              dt += bits16(dat, pos) & (1 << b) - 1, pos += b;
            }
            if (pos > tbts) {
              if (noSt) err(0);
              break;
            }
            if (noBuf) cbuf(bt + 131072);
            var end = bt + add;
            if (bt < dt) {
              var shift = dl - dt,
                dend = Math.min(dt, end);
              if (shift + bt < 0) err(3);
              for (; bt < dend; ++bt) buf[bt] = dict[shift + bt];
            }
            for (; bt < end; bt += 4) {
              buf[bt] = buf[bt - dt];
              buf[bt + 1] = buf[bt + 1 - dt];
              buf[bt + 2] = buf[bt + 2 - dt];
              buf[bt + 3] = buf[bt + 3 - dt];
            }
            bt = end;
          }
        }
        st.l = lm, st.p = lpos, st.b = bt, st.f = final;
        if (lm) final = 1, st.m = lbt, st.d = dm, st.n = dbt;
      } while (!final);
      return bt == buf.length ? buf : slc(buf, 0, bt);
    };
    // empty
    var et = /*#__PURE__*/new u8(0);
    /**
     * Expands DEFLATE data with no wrapper
     * @param data The data to decompress
     * @param opts The decompression options
     * @returns The decompressed version of the data
     */
    function inflateSync(data, opts) {
      return inflt(data, {
        i: 2
      }, opts && opts.out, opts && opts.dictionary);
    }
    // text decoder
    var td = typeof TextDecoder != 'undefined' && /*#__PURE__*/new TextDecoder();
    // text decoder stream
    var tds = 0;
    try {
      td.decode(et, {
        stream: true
      });
      tds = 1;
    } catch (e) {}

    /*
        Copyright 2018  Alfredo Mungo <alfredo.mungo@protonmail.ch>

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to
        deal in the Software without restriction, including without limitation the
        rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
        sell copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
        FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
        IN THE SOFTWARE.
    */
    if (!Object.fromEntries) {
      Object.defineProperty(Object, 'fromEntries', {
        value(entries) {
          if (!entries || !entries[Symbol.iterator]) {
            throw new Error('Object.fromEntries() requires a single iterable argument');
          }
          const o = {};
          Object.keys(entries).forEach(key => {
            const [k, v] = entries[key];
            o[k] = v;
          });
          return o;
        }
      });
    }

    var global$1 = (typeof global !== "undefined" ? global :
      typeof self !== "undefined" ? self :
      typeof window !== "undefined" ? window : {});

    var e = "undefined" == typeof global$1 ? "undefined" === typeof self ? {} : self : global$1,
      q = String.fromCharCode,
      r = {}.toString,
      t = e.SharedArrayBuffer,
      v = t ? r.call(t) : "",
      w = e.Uint8Array,
      x = w ? r.call(ArrayBuffer.prototype) : "",
      y = e.Buffer;
    try {
      !y && e.require && (y = e.require("Buffer"));
      var z = y.prototype,
        A = r.call(z);
    } catch (c) {}
    var B = y && y && y.allocUnsafe,
      C = !!w && !y,
      D = /[\x80-\uD7ff\uDC00-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF]?/g,
      E = new Uint16Array(32),
      F = !y || !!w && w.prototype.isPrototypeOf(z),
      H = G.prototype;
    function I() {}
    function J(c) {
      var a = c && c.buffer || c,
        f = r.call(a);
      if (f !== x && f !== A && f !== v && "[object ArrayBuffer]" !== f && void 0 !== c) throw TypeError("Failed to execute 'decode' on 'TextDecoder': The provided value is not of type '(ArrayBuffer or ArrayBufferView)'");
      c = F ? new w(a) : a || [];
      f = a = "";
      for (var d = 0, b = c.length | 0, k = b - 32 | 0, g, h, l = 0, u = 0, n, m = 0, p = -1; d < b;) {
        for (g = d <= k ? 32 : b - d | 0; m < g; d = d + 1 | 0, m = m + 1 | 0) {
          h = c[d] & 255;
          switch (h >> 4) {
            case 15:
              n = c[d = d + 1 | 0] & 255;
              if (2 !== n >> 6 || 247 < h) {
                d = d - 1 | 0;
                break;
              }
              l = (h & 7) << 6 | n & 63;
              u = 5;
              h = 256;
            case 14:
              n = c[d = d + 1 | 0] & 255, l <<= 6, l |= (h & 15) << 6 | n & 63, u = 2 === n >> 6 ? u + 4 | 0 : 24, h = h + 256 & 768;
            case 13:
            case 12:
              n = c[d = d + 1 | 0] & 255, l <<= 6, l |= (h & 31) << 6 | n & 63, u = u + 7 | 0, d < b && 2 === n >> 6 && l >> u && 1114112 > l ? (h = l, l = l - 65536 | 0, 0 <= l && (p = (l >> 10) + 55296 | 0, h = (l & 1023) + 56320 | 0, 31 > m ? (E[m] = p, m = m + 1 | 0, p = -1) : (n = p, p = h, h = n))) : (h >>= 8, d = d - h - 1 | 0, h = 65533), l = u = 0, g = d <= k ? 32 : b - d | 0;
            default:
              E[m] = h;
              continue;
            case 11:
            case 10:
            case 9:
            case 8:
          }
          E[m] = 65533;
        }
        f += q(E[0], E[1], E[2], E[3], E[4], E[5], E[6], E[7], E[8], E[9], E[10], E[11], E[12], E[13], E[14], E[15], E[16], E[17], E[18], E[19], E[20], E[21], E[22], E[23], E[24], E[25], E[26], E[27], E[28], E[29], E[30], E[31]);
        32 > m && (f = f.slice(0, m - 32 | 0));
        if (d < b) {
          if (E[0] = p, m = ~p >>> 31, p = -1, f.length < a.length) continue;
        } else -1 !== p && (f += q(p));
        a += f;
        f = "";
      }
      return a;
    }
    I.prototype.decode = J;
    function K(c) {
      var a = c.charCodeAt(0) | 0;
      if (55296 <= a) if (56320 > a) {
        if (c = c.charCodeAt(1) | 0, 56320 <= c && 57343 >= c) {
          if (a = (a << 10) + c - 56613888 | 0, 65535 < a) return q(240 | a >> 18, 128 | a >> 12 & 63, 128 | a >> 6 & 63, 128 | a & 63);
        } else a = 65533;
      } else 57343 >= a && (a = 65533);
      return 2047 >= a ? q(192 | a >> 6, 128 | a & 63) : q(224 | a >> 12, 128 | a >> 6 & 63, 128 | a & 63);
    }
    function G() {}
    function L(c) {
      c = void 0 === c ? "" : "" + c;
      var a = c.length | 0,
        f = C ? new w((a << 1) + 8 | 0) : B ? B((a << 1) + 8 | 0) : new y((a << 1) + 8 | 0),
        d,
        b = 0,
        k = !1;
      for (d = 0; d < a; d = d + 1 | 0, b = b + 1 | 0) {
        var g = c.charCodeAt(d) | 0;
        if (127 >= g) f[b] = g;else {
          if (2047 >= g) f[b] = 192 | g >> 6;else {
            a: {
              if (55296 <= g) if (56320 > g) {
                var h = c.charCodeAt(d = d + 1 | 0) | 0;
                if (56320 <= h && 57343 >= h) {
                  g = (g << 10) + h - 56613888 | 0;
                  if (65535 < g) {
                    f[b] = 240 | g >> 18;
                    f[b = b + 1 | 0] = 128 | g >> 12 & 63;
                    f[b = b + 1 | 0] = 128 | g >> 6 & 63;
                    f[b = b + 1 | 0] = 128 | g & 63;
                    continue;
                  }
                  break a;
                }
                g = 65533;
              } else 57343 >= g && (g = 65533);
              !k && d << 1 < b && d << 1 < (b - 7 | 0) && (k = !0, h = C ? new w(3 * a) : B ? B(3 * a) : new y(3 * a), h.set(f), f = h);
            }
            f[b] = 224 | g >> 12;
            f[b = b + 1 | 0] = 128 | g >> 6 & 63;
          }
          f[b = b + 1 | 0] = 128 | g & 63;
        }
      }
      return f.subarray(0, b);
    }
    function M(c, a) {
      var f = void 0 === c ? "" : ("" + c).replace(D, K),
        d = f.length | 0,
        b = 0,
        k = 0,
        g = a.length | 0,
        h = c.length | 0;
      g < d && (d = g);
      a: for (; b < d; b = b + 1 | 0) {
        c = f.charCodeAt(b) | 0;
        switch (c >> 4) {
          case 0:
          case 1:
          case 2:
          case 3:
          case 4:
          case 5:
          case 6:
          case 7:
            k = k + 1 | 0;
          case 8:
          case 9:
          case 10:
          case 11:
            break;
          case 12:
          case 13:
            if ((b + 1 | 0) < g) {
              k = k + 1 | 0;
              break;
            }
          case 14:
            if ((b + 2 | 0) < g) {
              k = k + 1 | 0;
              break;
            }
          case 15:
            if ((b + 3 | 0) < g) {
              k = k + 1 | 0;
              break;
            }
          default:
            break a;
        }
        a[b] = c;
      }
      return {
        written: b,
        read: h < k ? h : k
      };
    }
    H.encode = L;
    H.encodeInto = M;
    function N(c, a) {
      var f = c[a];
      return function () {
        return f.apply(c, arguments);
      };
    }
    var O = e.TextDecoder,
      P = e.TextEncoder,
      Q;
    P && (Q = new P(), Q.encodeInto ? N(Q, "encodeInto") : P.prototype.encodeInto = function (c, a) {
      var f = c.length | 0,
        d = a.length | 0;
      if (f < d >> 1) {
        var b = Q.encode(c);
        if ((b.length | 0) < d) return a.set(b), {
          read: f,
          written: b.length | 0
        };
      }
      return M(c, a);
    });
    var TextDecoder$1 = O || I,
      TextEncoder$1 = P || G;
      O ? N(new O(), "decode") : J;
      P ? N(Q, "encode") : L;
     //AnonyCo

    global$1.TextDecoder = global$1.TextEncoder || TextEncoder$1;
    global$1.TextEncoder = global$1.TextDecoder || TextDecoder$1;

    const encoder = new TextEncoder();
    new TextDecoder();
    function toBytes(base64) {
      base64 = base64.replace(/=/g, '');
      let n = base64.length;
      let rem = n % 4;
      let k = rem && rem - 1; // how many bytes the last base64 chunk encodes
      let m = (n >> 2) * 3 + k; // total encoded bytes

      let encoded = new Uint8Array(n + 3);
      encoder.encodeInto(base64 + '===', encoded);
      for (let i = 0, j = 0; i < n; i += 4, j += 3) {
        let x = (lookup$1[encoded[i]] << 18) + (lookup$1[encoded[i + 1]] << 12) + (lookup$1[encoded[i + 2]] << 6) + lookup$1[encoded[i + 3]];
        encoded[j] = x >> 16;
        encoded[j + 1] = x >> 8 & 0xff;
        encoded[j + 2] = x & 0xff;
      }
      return new Uint8Array(encoded.buffer, 0, m);
    }
    const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
    const lookup$1 = Object.fromEntries(Array.from(alphabet).map((a, i) => [a.charCodeAt(0), i]));
    lookup$1['='.charCodeAt(0)] = 0;
    lookup$1['-'.charCodeAt(0)] = 62;
    lookup$1['_'.charCodeAt(0)] = 63;
    Object.fromEntries(Array.from(alphabet).map((a, i) => [i, a.charCodeAt(0)]));

    /**
      Base32768 is a binary-to-text encoding optimised for UTF-16-encoded text.
      (e.g. Windows, Java, JavaScript)
    */

    // Z is a number, usually a uint15 but sometimes a uint7

    const BITS_PER_CHAR = 15; // Base32768 is a 15-bit encoding
    const BITS_PER_BYTE = 8;
    const pairStrings = ['ҠҿԀԟڀڿݠޟ߀ߟကဟႠႿᄀᅟᆀᆟᇠሿበቿዠዿጠጿᎠᏟᐠᙟᚠᛟកសᠠᡟᣀᣟᦀᦟ᧠᧿ᨠᨿᯀᯟᰀᰟᴀᴟ⇠⇿⋀⋟⍀⏟␀␟─❟➀➿⠀⥿⦠⦿⨠⩟⪀⪿⫠⭟ⰀⰟⲀⳟⴀⴟⵀⵟ⺠⻟㇀㇟㐀䶟䷀龿ꀀꑿ꒠꒿ꔀꗿꙀꙟꚠꛟ꜀ꝟꞀꞟꡀꡟ', 'ƀƟɀʟ'];
    const lookupD = {};
    pairStrings.forEach((pairString, r) => {
      // Decompression
      const encodeRepertoire = [];
      pairString.match(/../gu).forEach(pair => {
        const first = pair.codePointAt(0);
        const last = pair.codePointAt(1);
        for (let codePoint = first; codePoint <= last; codePoint++) {
          encodeRepertoire.push(String.fromCodePoint(codePoint));
        }
      });
      const numZBits = BITS_PER_CHAR - BITS_PER_BYTE * r; // 0 -> 15, 1 -> 7
      encodeRepertoire.forEach((chr, z) => {
        lookupD[chr] = [numZBits, z];
      });
    });
    const decode = str => {
      const length = str.length;

      // This length is a guess. There's a chance we allocate one more byte here
      // than we actually need. But we can count and slice it off later
      const uint8Array = new Uint8Array(Math.floor(length * BITS_PER_CHAR / BITS_PER_BYTE));
      let numUint8s = 0;
      let uint8 = 0;
      let numUint8Bits = 0;
      for (let i = 0; i < length; i++) {
        const chr = str.charAt(i);
        if (!(chr in lookupD)) {
          throw new Error(`Unrecognised Base32768 character: ${chr}`);
        }
        const [numZBits, z] = lookupD[chr];
        if (numZBits !== BITS_PER_CHAR && i !== length - 1) {
          throw new Error('Secondary character found before end of input at position ' + String(i));
        }

        // Take most significant bit first
        for (let j = numZBits - 1; j >= 0; j--) {
          const bit = z >> j & 1;
          uint8 = (uint8 << 1) + bit;
          numUint8Bits++;
          if (numUint8Bits === BITS_PER_BYTE) {
            uint8Array[numUint8s] = uint8;
            numUint8s++;
            uint8 = 0;
            numUint8Bits = 0;
          }
        }
      }

      // Final padding bits! Requires special consideration!
      // Remember how we always pad with 1s?
      // Note: there could be 0 such bits, check still works though
      if (uint8 !== (1 << numUint8Bits) - 1) {
        throw new Error('Padding mismatch');
      }
      return new Uint8Array(uint8Array.buffer, 0, numUint8s);
    };

    const pendingLogMessages = [];
    let suppressedLogMode = false;
    function dispatchLog(...data) {
      console['log']('DOTNET ', ...data);
    }
    function log(...data) {
      if (suppressedLogMode) {
        pendingLogMessages.push(data);
        return;
      }
      dispatchLog(...data);
    }
    function warn(...data) {
      log('WARN: ', ...data);
    }
    function debug(...data) {
      log('DEBUG: ', ...data);
    }
    function error(...data) {
      log('ERROR: ', ...data);
    }
    function trace(...data) {
      log('TRACE: ', ...data);
    }
    function assert(condition, ...data) {
      if (condition) {
        return;
      }
      log('ASSERTION FAIL: ', ...data);
    }
    function setSuppressedLogMode(mode) {
      suppressedLogMode = mode;
      if (!suppressedLogMode) {
        for (const data of pendingLogMessages) {
          dispatchLog(...data);
        }
        pendingLogMessages.length = 0;
      }
    }

    var importedLogging = /*#__PURE__*/Object.freeze({
        __proto__: null,
        assert: assert,
        debug: debug,
        error: error,
        log: log,
        setSuppressedLogMode: setSuppressedLogMode,
        trace: trace,
        warn: warn
    });

    function _wrapRegExp() {
      _wrapRegExp = function (re, groups) {
        return new BabelRegExp(re, void 0, groups);
      };
      var _super = RegExp.prototype,
        _groups = new WeakMap();
      function BabelRegExp(re, flags, groups) {
        var _this = new RegExp(re, flags);
        return _groups.set(_this, groups || _groups.get(re)), _setPrototypeOf(_this, BabelRegExp.prototype);
      }
      function buildGroups(result, re) {
        var g = _groups.get(re);
        return Object.keys(g).reduce(function (groups, name) {
          var i = g[name];
          if ("number" == typeof i) groups[name] = result[i];else {
            for (var k = 0; void 0 === result[i[k]] && k + 1 < i.length;) k++;
            groups[name] = result[i[k]];
          }
          return groups;
        }, Object.create(null));
      }
      return _inherits(BabelRegExp, RegExp), BabelRegExp.prototype.exec = function (str) {
        var result = _super.exec.call(this, str);
        if (result) {
          result.groups = buildGroups(result, this);
          var indices = result.indices;
          indices && (indices.groups = buildGroups(indices, this));
        }
        return result;
      }, BabelRegExp.prototype[Symbol.replace] = function (str, substitution) {
        if ("string" == typeof substitution) {
          var groups = _groups.get(this);
          return _super[Symbol.replace].call(this, str, substitution.replace(/\$<([^>]+)>/g, function (_, name) {
            var group = groups[name];
            return "$" + (Array.isArray(group) ? group.join("$") : group);
          }));
        }
        if ("function" == typeof substitution) {
          var _this = this;
          return _super[Symbol.replace].call(this, str, function () {
            var args = arguments;
            return "object" != typeof args[args.length - 1] && (args = [].slice.call(args)).push(buildGroups(args, _this)), substitution.apply(this, args);
          });
        }
        return _super[Symbol.replace].call(this, str, substitution);
      }, _wrapRegExp.apply(this, arguments);
    }
    function _inherits(subClass, superClass) {
      if (typeof superClass !== "function" && superClass !== null) {
        throw new TypeError("Super expression must either be null or a function");
      }
      subClass.prototype = Object.create(superClass && superClass.prototype, {
        constructor: {
          value: subClass,
          writable: true,
          configurable: true
        }
      });
      Object.defineProperty(subClass, "prototype", {
        writable: false
      });
      if (superClass) _setPrototypeOf(subClass, superClass);
    }
    function _setPrototypeOf(o, p) {
      _setPrototypeOf = Object.setPrototypeOf ? Object.setPrototypeOf.bind() : function _setPrototypeOf(o, p) {
        o.__proto__ = p;
        return o;
      };
      return _setPrototypeOf(o, p);
    }

    // shim for using process in browser
    // based off https://github.com/defunctzombie/node-process/blob/master/browser.js

    function defaultSetTimout() {
        throw new Error('setTimeout has not been defined');
    }
    function defaultClearTimeout () {
        throw new Error('clearTimeout has not been defined');
    }
    var cachedSetTimeout = defaultSetTimout;
    var cachedClearTimeout = defaultClearTimeout;
    if (typeof global$1.setTimeout === 'function') {
        cachedSetTimeout = setTimeout;
    }
    if (typeof global$1.clearTimeout === 'function') {
        cachedClearTimeout = clearTimeout;
    }

    function runTimeout(fun) {
        if (cachedSetTimeout === setTimeout) {
            //normal enviroments in sane situations
            return setTimeout(fun, 0);
        }
        // if setTimeout wasn't available but was latter defined
        if ((cachedSetTimeout === defaultSetTimout || !cachedSetTimeout) && setTimeout) {
            cachedSetTimeout = setTimeout;
            return setTimeout(fun, 0);
        }
        try {
            // when when somebody has screwed with setTimeout but no I.E. maddness
            return cachedSetTimeout(fun, 0);
        } catch(e){
            try {
                // When we are in I.E. but the script has been evaled so I.E. doesn't trust the global object when called normally
                return cachedSetTimeout.call(null, fun, 0);
            } catch(e){
                // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error
                return cachedSetTimeout.call(this, fun, 0);
            }
        }


    }
    function runClearTimeout(marker) {
        if (cachedClearTimeout === clearTimeout) {
            //normal enviroments in sane situations
            return clearTimeout(marker);
        }
        // if clearTimeout wasn't available but was latter defined
        if ((cachedClearTimeout === defaultClearTimeout || !cachedClearTimeout) && clearTimeout) {
            cachedClearTimeout = clearTimeout;
            return clearTimeout(marker);
        }
        try {
            // when when somebody has screwed with setTimeout but no I.E. maddness
            return cachedClearTimeout(marker);
        } catch (e){
            try {
                // When we are in I.E. but the script has been evaled so I.E. doesn't  trust the global object when called normally
                return cachedClearTimeout.call(null, marker);
            } catch (e){
                // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error.
                // Some versions of I.E. have different rules for clearTimeout vs setTimeout
                return cachedClearTimeout.call(this, marker);
            }
        }



    }
    var queue = [];
    var draining = false;
    var currentQueue;
    var queueIndex = -1;

    function cleanUpNextTick() {
        if (!draining || !currentQueue) {
            return;
        }
        draining = false;
        if (currentQueue.length) {
            queue = currentQueue.concat(queue);
        } else {
            queueIndex = -1;
        }
        if (queue.length) {
            drainQueue();
        }
    }

    function drainQueue() {
        if (draining) {
            return;
        }
        var timeout = runTimeout(cleanUpNextTick);
        draining = true;

        var len = queue.length;
        while(len) {
            currentQueue = queue;
            queue = [];
            while (++queueIndex < len) {
                if (currentQueue) {
                    currentQueue[queueIndex].run();
                }
            }
            queueIndex = -1;
            len = queue.length;
        }
        currentQueue = null;
        draining = false;
        runClearTimeout(timeout);
    }
    function nextTick(fun) {
        var args = new Array(arguments.length - 1);
        if (arguments.length > 1) {
            for (var i = 1; i < arguments.length; i++) {
                args[i - 1] = arguments[i];
            }
        }
        queue.push(new Item(fun, args));
        if (queue.length === 1 && !draining) {
            runTimeout(drainQueue);
        }
    }
    // v8 likes predictible objects
    function Item(fun, array) {
        this.fun = fun;
        this.array = array;
    }
    Item.prototype.run = function () {
        this.fun.apply(null, this.array);
    };
    var title = 'browser';
    var platform = 'browser';
    var browser = true;
    var env = {};
    var argv = [];
    var version = ''; // empty string to avoid regexp issues
    var versions = {};
    var release = {};
    var config = {};

    function noop$1() {}

    var on = noop$1;
    var addListener = noop$1;
    var once = noop$1;
    var off = noop$1;
    var removeListener = noop$1;
    var removeAllListeners = noop$1;
    var emit = noop$1;

    function binding(name) {
        throw new Error('process.binding is not supported');
    }

    function cwd () { return '/' }
    function chdir (dir) {
        throw new Error('process.chdir is not supported');
    }function umask() { return 0; }

    // from https://github.com/kumavis/browser-process-hrtime/blob/master/index.js
    var performance$1 = global$1.performance || {};
    var performanceNow =
      performance$1.now        ||
      performance$1.mozNow     ||
      performance$1.msNow      ||
      performance$1.oNow       ||
      performance$1.webkitNow  ||
      function(){ return (new Date()).getTime() };

    // generate timestamp or delta
    // see http://nodejs.org/api/process.html#process_process_hrtime
    function hrtime(previousTimestamp){
      var clocktime = performanceNow.call(performance$1)*1e-3;
      var seconds = Math.floor(clocktime);
      var nanoseconds = Math.floor((clocktime%1)*1e9);
      if (previousTimestamp) {
        seconds = seconds - previousTimestamp[0];
        nanoseconds = nanoseconds - previousTimestamp[1];
        if (nanoseconds<0) {
          seconds--;
          nanoseconds += 1e9;
        }
      }
      return [seconds,nanoseconds]
    }

    var startTime = new Date();
    function uptime() {
      var currentTime = new Date();
      var dif = currentTime - startTime;
      return dif / 1000;
    }

    var browser$1 = {
      nextTick: nextTick,
      title: title,
      browser: browser,
      env: env,
      argv: argv,
      version: version,
      versions: versions,
      on: on,
      addListener: addListener,
      once: once,
      off: off,
      removeListener: removeListener,
      removeAllListeners: removeAllListeners,
      emit: emit,
      binding: binding,
      cwd: cwd,
      chdir: chdir,
      umask: umask,
      hrtime: hrtime,
      platform: platform,
      release: release,
      config: config,
      uptime: uptime
    };

    var _polyfillNode_process = /*#__PURE__*/Object.freeze({
        __proto__: null,
        addListener: addListener,
        argv: argv,
        binding: binding,
        browser: browser,
        chdir: chdir,
        config: config,
        cwd: cwd,
        default: browser$1,
        emit: emit,
        env: env,
        hrtime: hrtime,
        nextTick: nextTick,
        off: off,
        on: on,
        once: once,
        platform: platform,
        release: release,
        removeAllListeners: removeAllListeners,
        removeListener: removeListener,
        title: title,
        umask: umask,
        uptime: uptime,
        version: version,
        versions: versions
    });

    var lookup = [];
    var revLookup = [];
    var Arr = typeof Uint8Array !== 'undefined' ? Uint8Array : Array;
    var inited = false;
    function init () {
      inited = true;
      var code = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
      for (var i = 0, len = code.length; i < len; ++i) {
        lookup[i] = code[i];
        revLookup[code.charCodeAt(i)] = i;
      }

      revLookup['-'.charCodeAt(0)] = 62;
      revLookup['_'.charCodeAt(0)] = 63;
    }

    function toByteArray (b64) {
      if (!inited) {
        init();
      }
      var i, j, l, tmp, placeHolders, arr;
      var len = b64.length;

      if (len % 4 > 0) {
        throw new Error('Invalid string. Length must be a multiple of 4')
      }

      // the number of equal signs (place holders)
      // if there are two placeholders, than the two characters before it
      // represent one byte
      // if there is only one, then the three characters before it represent 2 bytes
      // this is just a cheap hack to not do indexOf twice
      placeHolders = b64[len - 2] === '=' ? 2 : b64[len - 1] === '=' ? 1 : 0;

      // base64 is 4/3 + up to two characters of the original data
      arr = new Arr(len * 3 / 4 - placeHolders);

      // if there are placeholders, only get up to the last complete 4 chars
      l = placeHolders > 0 ? len - 4 : len;

      var L = 0;

      for (i = 0, j = 0; i < l; i += 4, j += 3) {
        tmp = (revLookup[b64.charCodeAt(i)] << 18) | (revLookup[b64.charCodeAt(i + 1)] << 12) | (revLookup[b64.charCodeAt(i + 2)] << 6) | revLookup[b64.charCodeAt(i + 3)];
        arr[L++] = (tmp >> 16) & 0xFF;
        arr[L++] = (tmp >> 8) & 0xFF;
        arr[L++] = tmp & 0xFF;
      }

      if (placeHolders === 2) {
        tmp = (revLookup[b64.charCodeAt(i)] << 2) | (revLookup[b64.charCodeAt(i + 1)] >> 4);
        arr[L++] = tmp & 0xFF;
      } else if (placeHolders === 1) {
        tmp = (revLookup[b64.charCodeAt(i)] << 10) | (revLookup[b64.charCodeAt(i + 1)] << 4) | (revLookup[b64.charCodeAt(i + 2)] >> 2);
        arr[L++] = (tmp >> 8) & 0xFF;
        arr[L++] = tmp & 0xFF;
      }

      return arr
    }

    function tripletToBase64 (num) {
      return lookup[num >> 18 & 0x3F] + lookup[num >> 12 & 0x3F] + lookup[num >> 6 & 0x3F] + lookup[num & 0x3F]
    }

    function encodeChunk (uint8, start, end) {
      var tmp;
      var output = [];
      for (var i = start; i < end; i += 3) {
        tmp = (uint8[i] << 16) + (uint8[i + 1] << 8) + (uint8[i + 2]);
        output.push(tripletToBase64(tmp));
      }
      return output.join('')
    }

    function fromByteArray (uint8) {
      if (!inited) {
        init();
      }
      var tmp;
      var len = uint8.length;
      var extraBytes = len % 3; // if we have 1 byte left, pad 2 bytes
      var output = '';
      var parts = [];
      var maxChunkLength = 16383; // must be multiple of 3

      // go through the array every three bytes, we'll deal with trailing stuff later
      for (var i = 0, len2 = len - extraBytes; i < len2; i += maxChunkLength) {
        parts.push(encodeChunk(uint8, i, (i + maxChunkLength) > len2 ? len2 : (i + maxChunkLength)));
      }

      // pad the end with zeros, but make sure to not forget the extra bytes
      if (extraBytes === 1) {
        tmp = uint8[len - 1];
        output += lookup[tmp >> 2];
        output += lookup[(tmp << 4) & 0x3F];
        output += '==';
      } else if (extraBytes === 2) {
        tmp = (uint8[len - 2] << 8) + (uint8[len - 1]);
        output += lookup[tmp >> 10];
        output += lookup[(tmp >> 4) & 0x3F];
        output += lookup[(tmp << 2) & 0x3F];
        output += '=';
      }

      parts.push(output);

      return parts.join('')
    }

    function read$1 (buffer, offset, isLE, mLen, nBytes) {
      var e, m;
      var eLen = nBytes * 8 - mLen - 1;
      var eMax = (1 << eLen) - 1;
      var eBias = eMax >> 1;
      var nBits = -7;
      var i = isLE ? (nBytes - 1) : 0;
      var d = isLE ? -1 : 1;
      var s = buffer[offset + i];

      i += d;

      e = s & ((1 << (-nBits)) - 1);
      s >>= (-nBits);
      nBits += eLen;
      for (; nBits > 0; e = e * 256 + buffer[offset + i], i += d, nBits -= 8) {}

      m = e & ((1 << (-nBits)) - 1);
      e >>= (-nBits);
      nBits += mLen;
      for (; nBits > 0; m = m * 256 + buffer[offset + i], i += d, nBits -= 8) {}

      if (e === 0) {
        e = 1 - eBias;
      } else if (e === eMax) {
        return m ? NaN : ((s ? -1 : 1) * Infinity)
      } else {
        m = m + Math.pow(2, mLen);
        e = e - eBias;
      }
      return (s ? -1 : 1) * m * Math.pow(2, e - mLen)
    }

    function write (buffer, value, offset, isLE, mLen, nBytes) {
      var e, m, c;
      var eLen = nBytes * 8 - mLen - 1;
      var eMax = (1 << eLen) - 1;
      var eBias = eMax >> 1;
      var rt = (mLen === 23 ? Math.pow(2, -24) - Math.pow(2, -77) : 0);
      var i = isLE ? 0 : (nBytes - 1);
      var d = isLE ? 1 : -1;
      var s = value < 0 || (value === 0 && 1 / value < 0) ? 1 : 0;

      value = Math.abs(value);

      if (isNaN(value) || value === Infinity) {
        m = isNaN(value) ? 1 : 0;
        e = eMax;
      } else {
        e = Math.floor(Math.log(value) / Math.LN2);
        if (value * (c = Math.pow(2, -e)) < 1) {
          e--;
          c *= 2;
        }
        if (e + eBias >= 1) {
          value += rt / c;
        } else {
          value += rt * Math.pow(2, 1 - eBias);
        }
        if (value * c >= 2) {
          e++;
          c /= 2;
        }

        if (e + eBias >= eMax) {
          m = 0;
          e = eMax;
        } else if (e + eBias >= 1) {
          m = (value * c - 1) * Math.pow(2, mLen);
          e = e + eBias;
        } else {
          m = value * Math.pow(2, eBias - 1) * Math.pow(2, mLen);
          e = 0;
        }
      }

      for (; mLen >= 8; buffer[offset + i] = m & 0xff, i += d, m /= 256, mLen -= 8) {}

      e = (e << mLen) | m;
      eLen += mLen;
      for (; eLen > 0; buffer[offset + i] = e & 0xff, i += d, e /= 256, eLen -= 8) {}

      buffer[offset + i - d] |= s * 128;
    }

    var toString = {}.toString;

    var isArray$1 = Array.isArray || function (arr) {
      return toString.call(arr) == '[object Array]';
    };

    /*!
     * The buffer module from node.js, for the browser.
     *
     * @author   Feross Aboukhadijeh <feross@feross.org> <http://feross.org>
     * @license  MIT
     */

    var INSPECT_MAX_BYTES = 50;

    /**
     * If `Buffer.TYPED_ARRAY_SUPPORT`:
     *   === true    Use Uint8Array implementation (fastest)
     *   === false   Use Object implementation (most compatible, even IE6)
     *
     * Browsers that support typed arrays are IE 10+, Firefox 4+, Chrome 7+, Safari 5.1+,
     * Opera 11.6+, iOS 4.2+.
     *
     * Due to various browser bugs, sometimes the Object implementation will be used even
     * when the browser supports typed arrays.
     *
     * Note:
     *
     *   - Firefox 4-29 lacks support for adding new properties to `Uint8Array` instances,
     *     See: https://bugzilla.mozilla.org/show_bug.cgi?id=695438.
     *
     *   - Chrome 9-10 is missing the `TypedArray.prototype.subarray` function.
     *
     *   - IE10 has a broken `TypedArray.prototype.subarray` function which returns arrays of
     *     incorrect length in some situations.

     * We detect these buggy browsers and set `Buffer.TYPED_ARRAY_SUPPORT` to `false` so they
     * get the Object implementation, which is slower but behaves correctly.
     */
    Buffer.TYPED_ARRAY_SUPPORT = global$1.TYPED_ARRAY_SUPPORT !== undefined
      ? global$1.TYPED_ARRAY_SUPPORT
      : true;

    /*
     * Export kMaxLength after typed array support is determined.
     */
    kMaxLength();

    function kMaxLength () {
      return Buffer.TYPED_ARRAY_SUPPORT
        ? 0x7fffffff
        : 0x3fffffff
    }

    function createBuffer (that, length) {
      if (kMaxLength() < length) {
        throw new RangeError('Invalid typed array length')
      }
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        // Return an augmented `Uint8Array` instance, for best performance
        that = new Uint8Array(length);
        that.__proto__ = Buffer.prototype;
      } else {
        // Fallback: Return an object instance of the Buffer class
        if (that === null) {
          that = new Buffer(length);
        }
        that.length = length;
      }

      return that
    }

    /**
     * The Buffer constructor returns instances of `Uint8Array` that have their
     * prototype changed to `Buffer.prototype`. Furthermore, `Buffer` is a subclass of
     * `Uint8Array`, so the returned instances will have all the node `Buffer` methods
     * and the `Uint8Array` methods. Square bracket notation works as expected -- it
     * returns a single octet.
     *
     * The `Uint8Array` prototype remains unmodified.
     */

    function Buffer (arg, encodingOrOffset, length) {
      if (!Buffer.TYPED_ARRAY_SUPPORT && !(this instanceof Buffer)) {
        return new Buffer(arg, encodingOrOffset, length)
      }

      // Common case.
      if (typeof arg === 'number') {
        if (typeof encodingOrOffset === 'string') {
          throw new Error(
            'If encoding is specified then the first argument must be a string'
          )
        }
        return allocUnsafe(this, arg)
      }
      return from(this, arg, encodingOrOffset, length)
    }

    Buffer.poolSize = 8192; // not used by this implementation

    // TODO: Legacy, not needed anymore. Remove in next major version.
    Buffer._augment = function (arr) {
      arr.__proto__ = Buffer.prototype;
      return arr
    };

    function from (that, value, encodingOrOffset, length) {
      if (typeof value === 'number') {
        throw new TypeError('"value" argument must not be a number')
      }

      if (typeof ArrayBuffer !== 'undefined' && value instanceof ArrayBuffer) {
        return fromArrayBuffer(that, value, encodingOrOffset, length)
      }

      if (typeof value === 'string') {
        return fromString(that, value, encodingOrOffset)
      }

      return fromObject(that, value)
    }

    /**
     * Functionally equivalent to Buffer(arg, encoding) but throws a TypeError
     * if value is a number.
     * Buffer.from(str[, encoding])
     * Buffer.from(array)
     * Buffer.from(buffer)
     * Buffer.from(arrayBuffer[, byteOffset[, length]])
     **/
    Buffer.from = function (value, encodingOrOffset, length) {
      return from(null, value, encodingOrOffset, length)
    };

    if (Buffer.TYPED_ARRAY_SUPPORT) {
      Buffer.prototype.__proto__ = Uint8Array.prototype;
      Buffer.__proto__ = Uint8Array;
      if (typeof Symbol !== 'undefined' && Symbol.species &&
          Buffer[Symbol.species] === Buffer) ;
    }

    function assertSize (size) {
      if (typeof size !== 'number') {
        throw new TypeError('"size" argument must be a number')
      } else if (size < 0) {
        throw new RangeError('"size" argument must not be negative')
      }
    }

    function alloc (that, size, fill, encoding) {
      assertSize(size);
      if (size <= 0) {
        return createBuffer(that, size)
      }
      if (fill !== undefined) {
        // Only pay attention to encoding if it's a string. This
        // prevents accidentally sending in a number that would
        // be interpretted as a start offset.
        return typeof encoding === 'string'
          ? createBuffer(that, size).fill(fill, encoding)
          : createBuffer(that, size).fill(fill)
      }
      return createBuffer(that, size)
    }

    /**
     * Creates a new filled Buffer instance.
     * alloc(size[, fill[, encoding]])
     **/
    Buffer.alloc = function (size, fill, encoding) {
      return alloc(null, size, fill, encoding)
    };

    function allocUnsafe (that, size) {
      assertSize(size);
      that = createBuffer(that, size < 0 ? 0 : checked(size) | 0);
      if (!Buffer.TYPED_ARRAY_SUPPORT) {
        for (var i = 0; i < size; ++i) {
          that[i] = 0;
        }
      }
      return that
    }

    /**
     * Equivalent to Buffer(num), by default creates a non-zero-filled Buffer instance.
     * */
    Buffer.allocUnsafe = function (size) {
      return allocUnsafe(null, size)
    };
    /**
     * Equivalent to SlowBuffer(num), by default creates a non-zero-filled Buffer instance.
     */
    Buffer.allocUnsafeSlow = function (size) {
      return allocUnsafe(null, size)
    };

    function fromString (that, string, encoding) {
      if (typeof encoding !== 'string' || encoding === '') {
        encoding = 'utf8';
      }

      if (!Buffer.isEncoding(encoding)) {
        throw new TypeError('"encoding" must be a valid string encoding')
      }

      var length = byteLength(string, encoding) | 0;
      that = createBuffer(that, length);

      var actual = that.write(string, encoding);

      if (actual !== length) {
        // Writing a hex string, for example, that contains invalid characters will
        // cause everything after the first invalid character to be ignored. (e.g.
        // 'abxxcd' will be treated as 'ab')
        that = that.slice(0, actual);
      }

      return that
    }

    function fromArrayLike (that, array) {
      var length = array.length < 0 ? 0 : checked(array.length) | 0;
      that = createBuffer(that, length);
      for (var i = 0; i < length; i += 1) {
        that[i] = array[i] & 255;
      }
      return that
    }

    function fromArrayBuffer (that, array, byteOffset, length) {
      array.byteLength; // this throws if `array` is not a valid ArrayBuffer

      if (byteOffset < 0 || array.byteLength < byteOffset) {
        throw new RangeError('\'offset\' is out of bounds')
      }

      if (array.byteLength < byteOffset + (length || 0)) {
        throw new RangeError('\'length\' is out of bounds')
      }

      if (byteOffset === undefined && length === undefined) {
        array = new Uint8Array(array);
      } else if (length === undefined) {
        array = new Uint8Array(array, byteOffset);
      } else {
        array = new Uint8Array(array, byteOffset, length);
      }

      if (Buffer.TYPED_ARRAY_SUPPORT) {
        // Return an augmented `Uint8Array` instance, for best performance
        that = array;
        that.__proto__ = Buffer.prototype;
      } else {
        // Fallback: Return an object instance of the Buffer class
        that = fromArrayLike(that, array);
      }
      return that
    }

    function fromObject (that, obj) {
      if (internalIsBuffer(obj)) {
        var len = checked(obj.length) | 0;
        that = createBuffer(that, len);

        if (that.length === 0) {
          return that
        }

        obj.copy(that, 0, 0, len);
        return that
      }

      if (obj) {
        if ((typeof ArrayBuffer !== 'undefined' &&
            obj.buffer instanceof ArrayBuffer) || 'length' in obj) {
          if (typeof obj.length !== 'number' || isnan(obj.length)) {
            return createBuffer(that, 0)
          }
          return fromArrayLike(that, obj)
        }

        if (obj.type === 'Buffer' && isArray$1(obj.data)) {
          return fromArrayLike(that, obj.data)
        }
      }

      throw new TypeError('First argument must be a string, Buffer, ArrayBuffer, Array, or array-like object.')
    }

    function checked (length) {
      // Note: cannot use `length < kMaxLength()` here because that fails when
      // length is NaN (which is otherwise coerced to zero.)
      if (length >= kMaxLength()) {
        throw new RangeError('Attempt to allocate Buffer larger than maximum ' +
                             'size: 0x' + kMaxLength().toString(16) + ' bytes')
      }
      return length | 0
    }
    Buffer.isBuffer = isBuffer;
    function internalIsBuffer (b) {
      return !!(b != null && b._isBuffer)
    }

    Buffer.compare = function compare (a, b) {
      if (!internalIsBuffer(a) || !internalIsBuffer(b)) {
        throw new TypeError('Arguments must be Buffers')
      }

      if (a === b) return 0

      var x = a.length;
      var y = b.length;

      for (var i = 0, len = Math.min(x, y); i < len; ++i) {
        if (a[i] !== b[i]) {
          x = a[i];
          y = b[i];
          break
        }
      }

      if (x < y) return -1
      if (y < x) return 1
      return 0
    };

    Buffer.isEncoding = function isEncoding (encoding) {
      switch (String(encoding).toLowerCase()) {
        case 'hex':
        case 'utf8':
        case 'utf-8':
        case 'ascii':
        case 'latin1':
        case 'binary':
        case 'base64':
        case 'ucs2':
        case 'ucs-2':
        case 'utf16le':
        case 'utf-16le':
          return true
        default:
          return false
      }
    };

    Buffer.concat = function concat (list, length) {
      if (!isArray$1(list)) {
        throw new TypeError('"list" argument must be an Array of Buffers')
      }

      if (list.length === 0) {
        return Buffer.alloc(0)
      }

      var i;
      if (length === undefined) {
        length = 0;
        for (i = 0; i < list.length; ++i) {
          length += list[i].length;
        }
      }

      var buffer = Buffer.allocUnsafe(length);
      var pos = 0;
      for (i = 0; i < list.length; ++i) {
        var buf = list[i];
        if (!internalIsBuffer(buf)) {
          throw new TypeError('"list" argument must be an Array of Buffers')
        }
        buf.copy(buffer, pos);
        pos += buf.length;
      }
      return buffer
    };

    function byteLength (string, encoding) {
      if (internalIsBuffer(string)) {
        return string.length
      }
      if (typeof ArrayBuffer !== 'undefined' && typeof ArrayBuffer.isView === 'function' &&
          (ArrayBuffer.isView(string) || string instanceof ArrayBuffer)) {
        return string.byteLength
      }
      if (typeof string !== 'string') {
        string = '' + string;
      }

      var len = string.length;
      if (len === 0) return 0

      // Use a for loop to avoid recursion
      var loweredCase = false;
      for (;;) {
        switch (encoding) {
          case 'ascii':
          case 'latin1':
          case 'binary':
            return len
          case 'utf8':
          case 'utf-8':
          case undefined:
            return utf8ToBytes(string).length
          case 'ucs2':
          case 'ucs-2':
          case 'utf16le':
          case 'utf-16le':
            return len * 2
          case 'hex':
            return len >>> 1
          case 'base64':
            return base64ToBytes(string).length
          default:
            if (loweredCase) return utf8ToBytes(string).length // assume utf8
            encoding = ('' + encoding).toLowerCase();
            loweredCase = true;
        }
      }
    }
    Buffer.byteLength = byteLength;

    function slowToString (encoding, start, end) {
      var loweredCase = false;

      // No need to verify that "this.length <= MAX_UINT32" since it's a read-only
      // property of a typed array.

      // This behaves neither like String nor Uint8Array in that we set start/end
      // to their upper/lower bounds if the value passed is out of range.
      // undefined is handled specially as per ECMA-262 6th Edition,
      // Section 13.3.3.7 Runtime Semantics: KeyedBindingInitialization.
      if (start === undefined || start < 0) {
        start = 0;
      }
      // Return early if start > this.length. Done here to prevent potential uint32
      // coercion fail below.
      if (start > this.length) {
        return ''
      }

      if (end === undefined || end > this.length) {
        end = this.length;
      }

      if (end <= 0) {
        return ''
      }

      // Force coersion to uint32. This will also coerce falsey/NaN values to 0.
      end >>>= 0;
      start >>>= 0;

      if (end <= start) {
        return ''
      }

      if (!encoding) encoding = 'utf8';

      while (true) {
        switch (encoding) {
          case 'hex':
            return hexSlice(this, start, end)

          case 'utf8':
          case 'utf-8':
            return utf8Slice(this, start, end)

          case 'ascii':
            return asciiSlice(this, start, end)

          case 'latin1':
          case 'binary':
            return latin1Slice(this, start, end)

          case 'base64':
            return base64Slice(this, start, end)

          case 'ucs2':
          case 'ucs-2':
          case 'utf16le':
          case 'utf-16le':
            return utf16leSlice(this, start, end)

          default:
            if (loweredCase) throw new TypeError('Unknown encoding: ' + encoding)
            encoding = (encoding + '').toLowerCase();
            loweredCase = true;
        }
      }
    }

    // The property is used by `Buffer.isBuffer` and `is-buffer` (in Safari 5-7) to detect
    // Buffer instances.
    Buffer.prototype._isBuffer = true;

    function swap (b, n, m) {
      var i = b[n];
      b[n] = b[m];
      b[m] = i;
    }

    Buffer.prototype.swap16 = function swap16 () {
      var len = this.length;
      if (len % 2 !== 0) {
        throw new RangeError('Buffer size must be a multiple of 16-bits')
      }
      for (var i = 0; i < len; i += 2) {
        swap(this, i, i + 1);
      }
      return this
    };

    Buffer.prototype.swap32 = function swap32 () {
      var len = this.length;
      if (len % 4 !== 0) {
        throw new RangeError('Buffer size must be a multiple of 32-bits')
      }
      for (var i = 0; i < len; i += 4) {
        swap(this, i, i + 3);
        swap(this, i + 1, i + 2);
      }
      return this
    };

    Buffer.prototype.swap64 = function swap64 () {
      var len = this.length;
      if (len % 8 !== 0) {
        throw new RangeError('Buffer size must be a multiple of 64-bits')
      }
      for (var i = 0; i < len; i += 8) {
        swap(this, i, i + 7);
        swap(this, i + 1, i + 6);
        swap(this, i + 2, i + 5);
        swap(this, i + 3, i + 4);
      }
      return this
    };

    Buffer.prototype.toString = function toString () {
      var length = this.length | 0;
      if (length === 0) return ''
      if (arguments.length === 0) return utf8Slice(this, 0, length)
      return slowToString.apply(this, arguments)
    };

    Buffer.prototype.equals = function equals (b) {
      if (!internalIsBuffer(b)) throw new TypeError('Argument must be a Buffer')
      if (this === b) return true
      return Buffer.compare(this, b) === 0
    };

    Buffer.prototype.inspect = function inspect () {
      var str = '';
      var max = INSPECT_MAX_BYTES;
      if (this.length > 0) {
        str = this.toString('hex', 0, max).match(/.{2}/g).join(' ');
        if (this.length > max) str += ' ... ';
      }
      return '<Buffer ' + str + '>'
    };

    Buffer.prototype.compare = function compare (target, start, end, thisStart, thisEnd) {
      if (!internalIsBuffer(target)) {
        throw new TypeError('Argument must be a Buffer')
      }

      if (start === undefined) {
        start = 0;
      }
      if (end === undefined) {
        end = target ? target.length : 0;
      }
      if (thisStart === undefined) {
        thisStart = 0;
      }
      if (thisEnd === undefined) {
        thisEnd = this.length;
      }

      if (start < 0 || end > target.length || thisStart < 0 || thisEnd > this.length) {
        throw new RangeError('out of range index')
      }

      if (thisStart >= thisEnd && start >= end) {
        return 0
      }
      if (thisStart >= thisEnd) {
        return -1
      }
      if (start >= end) {
        return 1
      }

      start >>>= 0;
      end >>>= 0;
      thisStart >>>= 0;
      thisEnd >>>= 0;

      if (this === target) return 0

      var x = thisEnd - thisStart;
      var y = end - start;
      var len = Math.min(x, y);

      var thisCopy = this.slice(thisStart, thisEnd);
      var targetCopy = target.slice(start, end);

      for (var i = 0; i < len; ++i) {
        if (thisCopy[i] !== targetCopy[i]) {
          x = thisCopy[i];
          y = targetCopy[i];
          break
        }
      }

      if (x < y) return -1
      if (y < x) return 1
      return 0
    };

    // Finds either the first index of `val` in `buffer` at offset >= `byteOffset`,
    // OR the last index of `val` in `buffer` at offset <= `byteOffset`.
    //
    // Arguments:
    // - buffer - a Buffer to search
    // - val - a string, Buffer, or number
    // - byteOffset - an index into `buffer`; will be clamped to an int32
    // - encoding - an optional encoding, relevant is val is a string
    // - dir - true for indexOf, false for lastIndexOf
    function bidirectionalIndexOf (buffer, val, byteOffset, encoding, dir) {
      // Empty buffer means no match
      if (buffer.length === 0) return -1

      // Normalize byteOffset
      if (typeof byteOffset === 'string') {
        encoding = byteOffset;
        byteOffset = 0;
      } else if (byteOffset > 0x7fffffff) {
        byteOffset = 0x7fffffff;
      } else if (byteOffset < -0x80000000) {
        byteOffset = -0x80000000;
      }
      byteOffset = +byteOffset;  // Coerce to Number.
      if (isNaN(byteOffset)) {
        // byteOffset: it it's undefined, null, NaN, "foo", etc, search whole buffer
        byteOffset = dir ? 0 : (buffer.length - 1);
      }

      // Normalize byteOffset: negative offsets start from the end of the buffer
      if (byteOffset < 0) byteOffset = buffer.length + byteOffset;
      if (byteOffset >= buffer.length) {
        if (dir) return -1
        else byteOffset = buffer.length - 1;
      } else if (byteOffset < 0) {
        if (dir) byteOffset = 0;
        else return -1
      }

      // Normalize val
      if (typeof val === 'string') {
        val = Buffer.from(val, encoding);
      }

      // Finally, search either indexOf (if dir is true) or lastIndexOf
      if (internalIsBuffer(val)) {
        // Special case: looking for empty string/buffer always fails
        if (val.length === 0) {
          return -1
        }
        return arrayIndexOf(buffer, val, byteOffset, encoding, dir)
      } else if (typeof val === 'number') {
        val = val & 0xFF; // Search for a byte value [0-255]
        if (Buffer.TYPED_ARRAY_SUPPORT &&
            typeof Uint8Array.prototype.indexOf === 'function') {
          if (dir) {
            return Uint8Array.prototype.indexOf.call(buffer, val, byteOffset)
          } else {
            return Uint8Array.prototype.lastIndexOf.call(buffer, val, byteOffset)
          }
        }
        return arrayIndexOf(buffer, [ val ], byteOffset, encoding, dir)
      }

      throw new TypeError('val must be string, number or Buffer')
    }

    function arrayIndexOf (arr, val, byteOffset, encoding, dir) {
      var indexSize = 1;
      var arrLength = arr.length;
      var valLength = val.length;

      if (encoding !== undefined) {
        encoding = String(encoding).toLowerCase();
        if (encoding === 'ucs2' || encoding === 'ucs-2' ||
            encoding === 'utf16le' || encoding === 'utf-16le') {
          if (arr.length < 2 || val.length < 2) {
            return -1
          }
          indexSize = 2;
          arrLength /= 2;
          valLength /= 2;
          byteOffset /= 2;
        }
      }

      function read (buf, i) {
        if (indexSize === 1) {
          return buf[i]
        } else {
          return buf.readUInt16BE(i * indexSize)
        }
      }

      var i;
      if (dir) {
        var foundIndex = -1;
        for (i = byteOffset; i < arrLength; i++) {
          if (read(arr, i) === read(val, foundIndex === -1 ? 0 : i - foundIndex)) {
            if (foundIndex === -1) foundIndex = i;
            if (i - foundIndex + 1 === valLength) return foundIndex * indexSize
          } else {
            if (foundIndex !== -1) i -= i - foundIndex;
            foundIndex = -1;
          }
        }
      } else {
        if (byteOffset + valLength > arrLength) byteOffset = arrLength - valLength;
        for (i = byteOffset; i >= 0; i--) {
          var found = true;
          for (var j = 0; j < valLength; j++) {
            if (read(arr, i + j) !== read(val, j)) {
              found = false;
              break
            }
          }
          if (found) return i
        }
      }

      return -1
    }

    Buffer.prototype.includes = function includes (val, byteOffset, encoding) {
      return this.indexOf(val, byteOffset, encoding) !== -1
    };

    Buffer.prototype.indexOf = function indexOf (val, byteOffset, encoding) {
      return bidirectionalIndexOf(this, val, byteOffset, encoding, true)
    };

    Buffer.prototype.lastIndexOf = function lastIndexOf (val, byteOffset, encoding) {
      return bidirectionalIndexOf(this, val, byteOffset, encoding, false)
    };

    function hexWrite (buf, string, offset, length) {
      offset = Number(offset) || 0;
      var remaining = buf.length - offset;
      if (!length) {
        length = remaining;
      } else {
        length = Number(length);
        if (length > remaining) {
          length = remaining;
        }
      }

      // must be an even number of digits
      var strLen = string.length;
      if (strLen % 2 !== 0) throw new TypeError('Invalid hex string')

      if (length > strLen / 2) {
        length = strLen / 2;
      }
      for (var i = 0; i < length; ++i) {
        var parsed = parseInt(string.substr(i * 2, 2), 16);
        if (isNaN(parsed)) return i
        buf[offset + i] = parsed;
      }
      return i
    }

    function utf8Write (buf, string, offset, length) {
      return blitBuffer(utf8ToBytes(string, buf.length - offset), buf, offset, length)
    }

    function asciiWrite (buf, string, offset, length) {
      return blitBuffer(asciiToBytes(string), buf, offset, length)
    }

    function latin1Write (buf, string, offset, length) {
      return asciiWrite(buf, string, offset, length)
    }

    function base64Write (buf, string, offset, length) {
      return blitBuffer(base64ToBytes(string), buf, offset, length)
    }

    function ucs2Write (buf, string, offset, length) {
      return blitBuffer(utf16leToBytes(string, buf.length - offset), buf, offset, length)
    }

    Buffer.prototype.write = function write (string, offset, length, encoding) {
      // Buffer#write(string)
      if (offset === undefined) {
        encoding = 'utf8';
        length = this.length;
        offset = 0;
      // Buffer#write(string, encoding)
      } else if (length === undefined && typeof offset === 'string') {
        encoding = offset;
        length = this.length;
        offset = 0;
      // Buffer#write(string, offset[, length][, encoding])
      } else if (isFinite(offset)) {
        offset = offset | 0;
        if (isFinite(length)) {
          length = length | 0;
          if (encoding === undefined) encoding = 'utf8';
        } else {
          encoding = length;
          length = undefined;
        }
      // legacy write(string, encoding, offset, length) - remove in v0.13
      } else {
        throw new Error(
          'Buffer.write(string, encoding, offset[, length]) is no longer supported'
        )
      }

      var remaining = this.length - offset;
      if (length === undefined || length > remaining) length = remaining;

      if ((string.length > 0 && (length < 0 || offset < 0)) || offset > this.length) {
        throw new RangeError('Attempt to write outside buffer bounds')
      }

      if (!encoding) encoding = 'utf8';

      var loweredCase = false;
      for (;;) {
        switch (encoding) {
          case 'hex':
            return hexWrite(this, string, offset, length)

          case 'utf8':
          case 'utf-8':
            return utf8Write(this, string, offset, length)

          case 'ascii':
            return asciiWrite(this, string, offset, length)

          case 'latin1':
          case 'binary':
            return latin1Write(this, string, offset, length)

          case 'base64':
            // Warning: maxLength not taken into account in base64Write
            return base64Write(this, string, offset, length)

          case 'ucs2':
          case 'ucs-2':
          case 'utf16le':
          case 'utf-16le':
            return ucs2Write(this, string, offset, length)

          default:
            if (loweredCase) throw new TypeError('Unknown encoding: ' + encoding)
            encoding = ('' + encoding).toLowerCase();
            loweredCase = true;
        }
      }
    };

    Buffer.prototype.toJSON = function toJSON () {
      return {
        type: 'Buffer',
        data: Array.prototype.slice.call(this._arr || this, 0)
      }
    };

    function base64Slice (buf, start, end) {
      if (start === 0 && end === buf.length) {
        return fromByteArray(buf)
      } else {
        return fromByteArray(buf.slice(start, end))
      }
    }

    function utf8Slice (buf, start, end) {
      end = Math.min(buf.length, end);
      var res = [];

      var i = start;
      while (i < end) {
        var firstByte = buf[i];
        var codePoint = null;
        var bytesPerSequence = (firstByte > 0xEF) ? 4
          : (firstByte > 0xDF) ? 3
          : (firstByte > 0xBF) ? 2
          : 1;

        if (i + bytesPerSequence <= end) {
          var secondByte, thirdByte, fourthByte, tempCodePoint;

          switch (bytesPerSequence) {
            case 1:
              if (firstByte < 0x80) {
                codePoint = firstByte;
              }
              break
            case 2:
              secondByte = buf[i + 1];
              if ((secondByte & 0xC0) === 0x80) {
                tempCodePoint = (firstByte & 0x1F) << 0x6 | (secondByte & 0x3F);
                if (tempCodePoint > 0x7F) {
                  codePoint = tempCodePoint;
                }
              }
              break
            case 3:
              secondByte = buf[i + 1];
              thirdByte = buf[i + 2];
              if ((secondByte & 0xC0) === 0x80 && (thirdByte & 0xC0) === 0x80) {
                tempCodePoint = (firstByte & 0xF) << 0xC | (secondByte & 0x3F) << 0x6 | (thirdByte & 0x3F);
                if (tempCodePoint > 0x7FF && (tempCodePoint < 0xD800 || tempCodePoint > 0xDFFF)) {
                  codePoint = tempCodePoint;
                }
              }
              break
            case 4:
              secondByte = buf[i + 1];
              thirdByte = buf[i + 2];
              fourthByte = buf[i + 3];
              if ((secondByte & 0xC0) === 0x80 && (thirdByte & 0xC0) === 0x80 && (fourthByte & 0xC0) === 0x80) {
                tempCodePoint = (firstByte & 0xF) << 0x12 | (secondByte & 0x3F) << 0xC | (thirdByte & 0x3F) << 0x6 | (fourthByte & 0x3F);
                if (tempCodePoint > 0xFFFF && tempCodePoint < 0x110000) {
                  codePoint = tempCodePoint;
                }
              }
          }
        }

        if (codePoint === null) {
          // we did not generate a valid codePoint so insert a
          // replacement char (U+FFFD) and advance only 1 byte
          codePoint = 0xFFFD;
          bytesPerSequence = 1;
        } else if (codePoint > 0xFFFF) {
          // encode to utf16 (surrogate pair dance)
          codePoint -= 0x10000;
          res.push(codePoint >>> 10 & 0x3FF | 0xD800);
          codePoint = 0xDC00 | codePoint & 0x3FF;
        }

        res.push(codePoint);
        i += bytesPerSequence;
      }

      return decodeCodePointsArray(res)
    }

    // Based on http://stackoverflow.com/a/22747272/680742, the browser with
    // the lowest limit is Chrome, with 0x10000 args.
    // We go 1 magnitude less, for safety
    var MAX_ARGUMENTS_LENGTH = 0x1000;

    function decodeCodePointsArray (codePoints) {
      var len = codePoints.length;
      if (len <= MAX_ARGUMENTS_LENGTH) {
        return String.fromCharCode.apply(String, codePoints) // avoid extra slice()
      }

      // Decode in chunks to avoid "call stack size exceeded".
      var res = '';
      var i = 0;
      while (i < len) {
        res += String.fromCharCode.apply(
          String,
          codePoints.slice(i, i += MAX_ARGUMENTS_LENGTH)
        );
      }
      return res
    }

    function asciiSlice (buf, start, end) {
      var ret = '';
      end = Math.min(buf.length, end);

      for (var i = start; i < end; ++i) {
        ret += String.fromCharCode(buf[i] & 0x7F);
      }
      return ret
    }

    function latin1Slice (buf, start, end) {
      var ret = '';
      end = Math.min(buf.length, end);

      for (var i = start; i < end; ++i) {
        ret += String.fromCharCode(buf[i]);
      }
      return ret
    }

    function hexSlice (buf, start, end) {
      var len = buf.length;

      if (!start || start < 0) start = 0;
      if (!end || end < 0 || end > len) end = len;

      var out = '';
      for (var i = start; i < end; ++i) {
        out += toHex(buf[i]);
      }
      return out
    }

    function utf16leSlice (buf, start, end) {
      var bytes = buf.slice(start, end);
      var res = '';
      for (var i = 0; i < bytes.length; i += 2) {
        res += String.fromCharCode(bytes[i] + bytes[i + 1] * 256);
      }
      return res
    }

    Buffer.prototype.slice = function slice (start, end) {
      var len = this.length;
      start = ~~start;
      end = end === undefined ? len : ~~end;

      if (start < 0) {
        start += len;
        if (start < 0) start = 0;
      } else if (start > len) {
        start = len;
      }

      if (end < 0) {
        end += len;
        if (end < 0) end = 0;
      } else if (end > len) {
        end = len;
      }

      if (end < start) end = start;

      var newBuf;
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        newBuf = this.subarray(start, end);
        newBuf.__proto__ = Buffer.prototype;
      } else {
        var sliceLen = end - start;
        newBuf = new Buffer(sliceLen, undefined);
        for (var i = 0; i < sliceLen; ++i) {
          newBuf[i] = this[i + start];
        }
      }

      return newBuf
    };

    /*
     * Need to make sure that buffer isn't trying to write out of bounds.
     */
    function checkOffset (offset, ext, length) {
      if ((offset % 1) !== 0 || offset < 0) throw new RangeError('offset is not uint')
      if (offset + ext > length) throw new RangeError('Trying to access beyond buffer length')
    }

    Buffer.prototype.readUIntLE = function readUIntLE (offset, byteLength, noAssert) {
      offset = offset | 0;
      byteLength = byteLength | 0;
      if (!noAssert) checkOffset(offset, byteLength, this.length);

      var val = this[offset];
      var mul = 1;
      var i = 0;
      while (++i < byteLength && (mul *= 0x100)) {
        val += this[offset + i] * mul;
      }

      return val
    };

    Buffer.prototype.readUIntBE = function readUIntBE (offset, byteLength, noAssert) {
      offset = offset | 0;
      byteLength = byteLength | 0;
      if (!noAssert) {
        checkOffset(offset, byteLength, this.length);
      }

      var val = this[offset + --byteLength];
      var mul = 1;
      while (byteLength > 0 && (mul *= 0x100)) {
        val += this[offset + --byteLength] * mul;
      }

      return val
    };

    Buffer.prototype.readUInt8 = function readUInt8 (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 1, this.length);
      return this[offset]
    };

    Buffer.prototype.readUInt16LE = function readUInt16LE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 2, this.length);
      return this[offset] | (this[offset + 1] << 8)
    };

    Buffer.prototype.readUInt16BE = function readUInt16BE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 2, this.length);
      return (this[offset] << 8) | this[offset + 1]
    };

    Buffer.prototype.readUInt32LE = function readUInt32LE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 4, this.length);

      return ((this[offset]) |
          (this[offset + 1] << 8) |
          (this[offset + 2] << 16)) +
          (this[offset + 3] * 0x1000000)
    };

    Buffer.prototype.readUInt32BE = function readUInt32BE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 4, this.length);

      return (this[offset] * 0x1000000) +
        ((this[offset + 1] << 16) |
        (this[offset + 2] << 8) |
        this[offset + 3])
    };

    Buffer.prototype.readIntLE = function readIntLE (offset, byteLength, noAssert) {
      offset = offset | 0;
      byteLength = byteLength | 0;
      if (!noAssert) checkOffset(offset, byteLength, this.length);

      var val = this[offset];
      var mul = 1;
      var i = 0;
      while (++i < byteLength && (mul *= 0x100)) {
        val += this[offset + i] * mul;
      }
      mul *= 0x80;

      if (val >= mul) val -= Math.pow(2, 8 * byteLength);

      return val
    };

    Buffer.prototype.readIntBE = function readIntBE (offset, byteLength, noAssert) {
      offset = offset | 0;
      byteLength = byteLength | 0;
      if (!noAssert) checkOffset(offset, byteLength, this.length);

      var i = byteLength;
      var mul = 1;
      var val = this[offset + --i];
      while (i > 0 && (mul *= 0x100)) {
        val += this[offset + --i] * mul;
      }
      mul *= 0x80;

      if (val >= mul) val -= Math.pow(2, 8 * byteLength);

      return val
    };

    Buffer.prototype.readInt8 = function readInt8 (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 1, this.length);
      if (!(this[offset] & 0x80)) return (this[offset])
      return ((0xff - this[offset] + 1) * -1)
    };

    Buffer.prototype.readInt16LE = function readInt16LE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 2, this.length);
      var val = this[offset] | (this[offset + 1] << 8);
      return (val & 0x8000) ? val | 0xFFFF0000 : val
    };

    Buffer.prototype.readInt16BE = function readInt16BE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 2, this.length);
      var val = this[offset + 1] | (this[offset] << 8);
      return (val & 0x8000) ? val | 0xFFFF0000 : val
    };

    Buffer.prototype.readInt32LE = function readInt32LE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 4, this.length);

      return (this[offset]) |
        (this[offset + 1] << 8) |
        (this[offset + 2] << 16) |
        (this[offset + 3] << 24)
    };

    Buffer.prototype.readInt32BE = function readInt32BE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 4, this.length);

      return (this[offset] << 24) |
        (this[offset + 1] << 16) |
        (this[offset + 2] << 8) |
        (this[offset + 3])
    };

    Buffer.prototype.readFloatLE = function readFloatLE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 4, this.length);
      return read$1(this, offset, true, 23, 4)
    };

    Buffer.prototype.readFloatBE = function readFloatBE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 4, this.length);
      return read$1(this, offset, false, 23, 4)
    };

    Buffer.prototype.readDoubleLE = function readDoubleLE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 8, this.length);
      return read$1(this, offset, true, 52, 8)
    };

    Buffer.prototype.readDoubleBE = function readDoubleBE (offset, noAssert) {
      if (!noAssert) checkOffset(offset, 8, this.length);
      return read$1(this, offset, false, 52, 8)
    };

    function checkInt (buf, value, offset, ext, max, min) {
      if (!internalIsBuffer(buf)) throw new TypeError('"buffer" argument must be a Buffer instance')
      if (value > max || value < min) throw new RangeError('"value" argument is out of bounds')
      if (offset + ext > buf.length) throw new RangeError('Index out of range')
    }

    Buffer.prototype.writeUIntLE = function writeUIntLE (value, offset, byteLength, noAssert) {
      value = +value;
      offset = offset | 0;
      byteLength = byteLength | 0;
      if (!noAssert) {
        var maxBytes = Math.pow(2, 8 * byteLength) - 1;
        checkInt(this, value, offset, byteLength, maxBytes, 0);
      }

      var mul = 1;
      var i = 0;
      this[offset] = value & 0xFF;
      while (++i < byteLength && (mul *= 0x100)) {
        this[offset + i] = (value / mul) & 0xFF;
      }

      return offset + byteLength
    };

    Buffer.prototype.writeUIntBE = function writeUIntBE (value, offset, byteLength, noAssert) {
      value = +value;
      offset = offset | 0;
      byteLength = byteLength | 0;
      if (!noAssert) {
        var maxBytes = Math.pow(2, 8 * byteLength) - 1;
        checkInt(this, value, offset, byteLength, maxBytes, 0);
      }

      var i = byteLength - 1;
      var mul = 1;
      this[offset + i] = value & 0xFF;
      while (--i >= 0 && (mul *= 0x100)) {
        this[offset + i] = (value / mul) & 0xFF;
      }

      return offset + byteLength
    };

    Buffer.prototype.writeUInt8 = function writeUInt8 (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 1, 0xff, 0);
      if (!Buffer.TYPED_ARRAY_SUPPORT) value = Math.floor(value);
      this[offset] = (value & 0xff);
      return offset + 1
    };

    function objectWriteUInt16 (buf, value, offset, littleEndian) {
      if (value < 0) value = 0xffff + value + 1;
      for (var i = 0, j = Math.min(buf.length - offset, 2); i < j; ++i) {
        buf[offset + i] = (value & (0xff << (8 * (littleEndian ? i : 1 - i)))) >>>
          (littleEndian ? i : 1 - i) * 8;
      }
    }

    Buffer.prototype.writeUInt16LE = function writeUInt16LE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 2, 0xffff, 0);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value & 0xff);
        this[offset + 1] = (value >>> 8);
      } else {
        objectWriteUInt16(this, value, offset, true);
      }
      return offset + 2
    };

    Buffer.prototype.writeUInt16BE = function writeUInt16BE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 2, 0xffff, 0);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value >>> 8);
        this[offset + 1] = (value & 0xff);
      } else {
        objectWriteUInt16(this, value, offset, false);
      }
      return offset + 2
    };

    function objectWriteUInt32 (buf, value, offset, littleEndian) {
      if (value < 0) value = 0xffffffff + value + 1;
      for (var i = 0, j = Math.min(buf.length - offset, 4); i < j; ++i) {
        buf[offset + i] = (value >>> (littleEndian ? i : 3 - i) * 8) & 0xff;
      }
    }

    Buffer.prototype.writeUInt32LE = function writeUInt32LE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 4, 0xffffffff, 0);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset + 3] = (value >>> 24);
        this[offset + 2] = (value >>> 16);
        this[offset + 1] = (value >>> 8);
        this[offset] = (value & 0xff);
      } else {
        objectWriteUInt32(this, value, offset, true);
      }
      return offset + 4
    };

    Buffer.prototype.writeUInt32BE = function writeUInt32BE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 4, 0xffffffff, 0);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value >>> 24);
        this[offset + 1] = (value >>> 16);
        this[offset + 2] = (value >>> 8);
        this[offset + 3] = (value & 0xff);
      } else {
        objectWriteUInt32(this, value, offset, false);
      }
      return offset + 4
    };

    Buffer.prototype.writeIntLE = function writeIntLE (value, offset, byteLength, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) {
        var limit = Math.pow(2, 8 * byteLength - 1);

        checkInt(this, value, offset, byteLength, limit - 1, -limit);
      }

      var i = 0;
      var mul = 1;
      var sub = 0;
      this[offset] = value & 0xFF;
      while (++i < byteLength && (mul *= 0x100)) {
        if (value < 0 && sub === 0 && this[offset + i - 1] !== 0) {
          sub = 1;
        }
        this[offset + i] = ((value / mul) >> 0) - sub & 0xFF;
      }

      return offset + byteLength
    };

    Buffer.prototype.writeIntBE = function writeIntBE (value, offset, byteLength, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) {
        var limit = Math.pow(2, 8 * byteLength - 1);

        checkInt(this, value, offset, byteLength, limit - 1, -limit);
      }

      var i = byteLength - 1;
      var mul = 1;
      var sub = 0;
      this[offset + i] = value & 0xFF;
      while (--i >= 0 && (mul *= 0x100)) {
        if (value < 0 && sub === 0 && this[offset + i + 1] !== 0) {
          sub = 1;
        }
        this[offset + i] = ((value / mul) >> 0) - sub & 0xFF;
      }

      return offset + byteLength
    };

    Buffer.prototype.writeInt8 = function writeInt8 (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 1, 0x7f, -0x80);
      if (!Buffer.TYPED_ARRAY_SUPPORT) value = Math.floor(value);
      if (value < 0) value = 0xff + value + 1;
      this[offset] = (value & 0xff);
      return offset + 1
    };

    Buffer.prototype.writeInt16LE = function writeInt16LE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 2, 0x7fff, -0x8000);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value & 0xff);
        this[offset + 1] = (value >>> 8);
      } else {
        objectWriteUInt16(this, value, offset, true);
      }
      return offset + 2
    };

    Buffer.prototype.writeInt16BE = function writeInt16BE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 2, 0x7fff, -0x8000);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value >>> 8);
        this[offset + 1] = (value & 0xff);
      } else {
        objectWriteUInt16(this, value, offset, false);
      }
      return offset + 2
    };

    Buffer.prototype.writeInt32LE = function writeInt32LE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 4, 0x7fffffff, -0x80000000);
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value & 0xff);
        this[offset + 1] = (value >>> 8);
        this[offset + 2] = (value >>> 16);
        this[offset + 3] = (value >>> 24);
      } else {
        objectWriteUInt32(this, value, offset, true);
      }
      return offset + 4
    };

    Buffer.prototype.writeInt32BE = function writeInt32BE (value, offset, noAssert) {
      value = +value;
      offset = offset | 0;
      if (!noAssert) checkInt(this, value, offset, 4, 0x7fffffff, -0x80000000);
      if (value < 0) value = 0xffffffff + value + 1;
      if (Buffer.TYPED_ARRAY_SUPPORT) {
        this[offset] = (value >>> 24);
        this[offset + 1] = (value >>> 16);
        this[offset + 2] = (value >>> 8);
        this[offset + 3] = (value & 0xff);
      } else {
        objectWriteUInt32(this, value, offset, false);
      }
      return offset + 4
    };

    function checkIEEE754 (buf, value, offset, ext, max, min) {
      if (offset + ext > buf.length) throw new RangeError('Index out of range')
      if (offset < 0) throw new RangeError('Index out of range')
    }

    function writeFloat (buf, value, offset, littleEndian, noAssert) {
      if (!noAssert) {
        checkIEEE754(buf, value, offset, 4);
      }
      write(buf, value, offset, littleEndian, 23, 4);
      return offset + 4
    }

    Buffer.prototype.writeFloatLE = function writeFloatLE (value, offset, noAssert) {
      return writeFloat(this, value, offset, true, noAssert)
    };

    Buffer.prototype.writeFloatBE = function writeFloatBE (value, offset, noAssert) {
      return writeFloat(this, value, offset, false, noAssert)
    };

    function writeDouble (buf, value, offset, littleEndian, noAssert) {
      if (!noAssert) {
        checkIEEE754(buf, value, offset, 8);
      }
      write(buf, value, offset, littleEndian, 52, 8);
      return offset + 8
    }

    Buffer.prototype.writeDoubleLE = function writeDoubleLE (value, offset, noAssert) {
      return writeDouble(this, value, offset, true, noAssert)
    };

    Buffer.prototype.writeDoubleBE = function writeDoubleBE (value, offset, noAssert) {
      return writeDouble(this, value, offset, false, noAssert)
    };

    // copy(targetBuffer, targetStart=0, sourceStart=0, sourceEnd=buffer.length)
    Buffer.prototype.copy = function copy (target, targetStart, start, end) {
      if (!start) start = 0;
      if (!end && end !== 0) end = this.length;
      if (targetStart >= target.length) targetStart = target.length;
      if (!targetStart) targetStart = 0;
      if (end > 0 && end < start) end = start;

      // Copy 0 bytes; we're done
      if (end === start) return 0
      if (target.length === 0 || this.length === 0) return 0

      // Fatal error conditions
      if (targetStart < 0) {
        throw new RangeError('targetStart out of bounds')
      }
      if (start < 0 || start >= this.length) throw new RangeError('sourceStart out of bounds')
      if (end < 0) throw new RangeError('sourceEnd out of bounds')

      // Are we oob?
      if (end > this.length) end = this.length;
      if (target.length - targetStart < end - start) {
        end = target.length - targetStart + start;
      }

      var len = end - start;
      var i;

      if (this === target && start < targetStart && targetStart < end) {
        // descending copy from end
        for (i = len - 1; i >= 0; --i) {
          target[i + targetStart] = this[i + start];
        }
      } else if (len < 1000 || !Buffer.TYPED_ARRAY_SUPPORT) {
        // ascending copy from start
        for (i = 0; i < len; ++i) {
          target[i + targetStart] = this[i + start];
        }
      } else {
        Uint8Array.prototype.set.call(
          target,
          this.subarray(start, start + len),
          targetStart
        );
      }

      return len
    };

    // Usage:
    //    buffer.fill(number[, offset[, end]])
    //    buffer.fill(buffer[, offset[, end]])
    //    buffer.fill(string[, offset[, end]][, encoding])
    Buffer.prototype.fill = function fill (val, start, end, encoding) {
      // Handle string cases:
      if (typeof val === 'string') {
        if (typeof start === 'string') {
          encoding = start;
          start = 0;
          end = this.length;
        } else if (typeof end === 'string') {
          encoding = end;
          end = this.length;
        }
        if (val.length === 1) {
          var code = val.charCodeAt(0);
          if (code < 256) {
            val = code;
          }
        }
        if (encoding !== undefined && typeof encoding !== 'string') {
          throw new TypeError('encoding must be a string')
        }
        if (typeof encoding === 'string' && !Buffer.isEncoding(encoding)) {
          throw new TypeError('Unknown encoding: ' + encoding)
        }
      } else if (typeof val === 'number') {
        val = val & 255;
      }

      // Invalid ranges are not set to a default, so can range check early.
      if (start < 0 || this.length < start || this.length < end) {
        throw new RangeError('Out of range index')
      }

      if (end <= start) {
        return this
      }

      start = start >>> 0;
      end = end === undefined ? this.length : end >>> 0;

      if (!val) val = 0;

      var i;
      if (typeof val === 'number') {
        for (i = start; i < end; ++i) {
          this[i] = val;
        }
      } else {
        var bytes = internalIsBuffer(val)
          ? val
          : utf8ToBytes(new Buffer(val, encoding).toString());
        var len = bytes.length;
        for (i = 0; i < end - start; ++i) {
          this[i + start] = bytes[i % len];
        }
      }

      return this
    };

    // HELPER FUNCTIONS
    // ================

    var INVALID_BASE64_RE = /[^+\/0-9A-Za-z-_]/g;

    function base64clean (str) {
      // Node strips out invalid characters like \n and \t from the string, base64-js does not
      str = stringtrim(str).replace(INVALID_BASE64_RE, '');
      // Node converts strings with length < 2 to ''
      if (str.length < 2) return ''
      // Node allows for non-padded base64 strings (missing trailing ===), base64-js does not
      while (str.length % 4 !== 0) {
        str = str + '=';
      }
      return str
    }

    function stringtrim (str) {
      if (str.trim) return str.trim()
      return str.replace(/^\s+|\s+$/g, '')
    }

    function toHex (n) {
      if (n < 16) return '0' + n.toString(16)
      return n.toString(16)
    }

    function utf8ToBytes (string, units) {
      units = units || Infinity;
      var codePoint;
      var length = string.length;
      var leadSurrogate = null;
      var bytes = [];

      for (var i = 0; i < length; ++i) {
        codePoint = string.charCodeAt(i);

        // is surrogate component
        if (codePoint > 0xD7FF && codePoint < 0xE000) {
          // last char was a lead
          if (!leadSurrogate) {
            // no lead yet
            if (codePoint > 0xDBFF) {
              // unexpected trail
              if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
              continue
            } else if (i + 1 === length) {
              // unpaired lead
              if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
              continue
            }

            // valid lead
            leadSurrogate = codePoint;

            continue
          }

          // 2 leads in a row
          if (codePoint < 0xDC00) {
            if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
            leadSurrogate = codePoint;
            continue
          }

          // valid surrogate pair
          codePoint = (leadSurrogate - 0xD800 << 10 | codePoint - 0xDC00) + 0x10000;
        } else if (leadSurrogate) {
          // valid bmp char, but last char was a lead
          if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
        }

        leadSurrogate = null;

        // encode utf8
        if (codePoint < 0x80) {
          if ((units -= 1) < 0) break
          bytes.push(codePoint);
        } else if (codePoint < 0x800) {
          if ((units -= 2) < 0) break
          bytes.push(
            codePoint >> 0x6 | 0xC0,
            codePoint & 0x3F | 0x80
          );
        } else if (codePoint < 0x10000) {
          if ((units -= 3) < 0) break
          bytes.push(
            codePoint >> 0xC | 0xE0,
            codePoint >> 0x6 & 0x3F | 0x80,
            codePoint & 0x3F | 0x80
          );
        } else if (codePoint < 0x110000) {
          if ((units -= 4) < 0) break
          bytes.push(
            codePoint >> 0x12 | 0xF0,
            codePoint >> 0xC & 0x3F | 0x80,
            codePoint >> 0x6 & 0x3F | 0x80,
            codePoint & 0x3F | 0x80
          );
        } else {
          throw new Error('Invalid code point')
        }
      }

      return bytes
    }

    function asciiToBytes (str) {
      var byteArray = [];
      for (var i = 0; i < str.length; ++i) {
        // Node's code seems to be doing this and not & 0x7F..
        byteArray.push(str.charCodeAt(i) & 0xFF);
      }
      return byteArray
    }

    function utf16leToBytes (str, units) {
      var c, hi, lo;
      var byteArray = [];
      for (var i = 0; i < str.length; ++i) {
        if ((units -= 2) < 0) break

        c = str.charCodeAt(i);
        hi = c >> 8;
        lo = c % 256;
        byteArray.push(lo);
        byteArray.push(hi);
      }

      return byteArray
    }


    function base64ToBytes (str) {
      return toByteArray(base64clean(str))
    }

    function blitBuffer (src, dst, offset, length) {
      for (var i = 0; i < length; ++i) {
        if ((i + offset >= dst.length) || (i >= src.length)) break
        dst[i + offset] = src[i];
      }
      return i
    }

    function isnan (val) {
      return val !== val // eslint-disable-line no-self-compare
    }


    // the following is from is-buffer, also by Feross Aboukhadijeh and with same lisence
    // The _isBuffer check is for Safari 5-7 support, because it's missing
    // Object.prototype.constructor. Remove this eventually
    function isBuffer(obj) {
      return obj != null && (!!obj._isBuffer || isFastBuffer(obj) || isSlowBuffer(obj))
    }

    function isFastBuffer (obj) {
      return !!obj.constructor && typeof obj.constructor.isBuffer === 'function' && obj.constructor.isBuffer(obj)
    }

    // For Node v0.10 support. Remove this eventually.
    function isSlowBuffer (obj) {
      return typeof obj.readFloatLE === 'function' && typeof obj.slice === 'function' && isFastBuffer(obj.slice(0, 0))
    }

    const virtualTimerQueue = [];
    let virtualTime = 0;
    let nextHandleId = 1;
    let isTicking = false;
    let shouldCancelAdvanceFrame = false;
    const pendingClearTimeouts = [];
    const pendingClearIntervals = [];
    const pendingInserts = [];
    function doInsert(entry) {
      let i;
      // TODO: Binary search for more efficient insertion, given that virtualTimerQueue is sorted by nextTime
      for (i = 0; i < virtualTimerQueue.length; ++i) {
        if (entry.nextTime < virtualTimerQueue[i].nextTime) {
          break;
        }
      }
      virtualTimerQueue.splice(i, 0, entry);
    }
    function insertNewTimeout(callback, args, nextTime, interval) {
      const handle = nextHandleId++;
      const entry = {
        handle,
        callback,
        args,
        nextTime,
        interval
      };
      if (isTicking) {
        pendingInserts.push(entry);
      } else {
        doInsert(entry);
      }
      return handle;
    }
    function setTimeout$1(callback, delay, ...args) {
      if (delay == null) {
        delay = 0;
      }
      const handle = insertNewTimeout(callback, args, virtualTime + delay, undefined);
      return handle;
    }
    function clearTimeout$1(handle) {
      if (isTicking) {
        pendingClearTimeouts.push(handle);
        return;
      }
      for (let i = 0; i < virtualTimerQueue.length; ++i) {
        if (virtualTimerQueue[i].interval != null) {
          return;
        }
        if (virtualTimerQueue[i].handle !== handle) {
          return;
        }
        virtualTimerQueue.splice(i, 1);
        break;
      }
    }
    function setInterval(callback, interval, ...args) {
      insertNewTimeout(callback, args, virtualTime + interval, interval);
      return -1;
    }
    function clearInterval(handle) {
      if (isTicking) {
        pendingClearIntervals.push(handle);
        return;
      }
      for (let i = 0; i < virtualTimerQueue.length; ++i) {
        if (virtualTimerQueue[i].interval == null) {
          return;
        }
        if (virtualTimerQueue[i].handle !== handle) {
          return;
        }
        virtualTimerQueue.splice(i, 1);
        break;
      }
    }
    function setImmediate$1(callback, ...args) {
      const handle = setTimeout$1(callback, 0, ...args);
      return handle;
    }
    function advanceFrame() {
      //log(`advanceFrame`);
      shouldCancelAdvanceFrame = false;
      if (isTicking) {
        // This can happen if script execution is terminated half way through processing timers last frame
        // In this case, let's process any pending ops now and clean up
        log(`detected incomplete async frame, possibly from script execution termination`);
        isTicking = false;
        processPendingOps();
      }
      if (virtualTimerQueue.length === 0) {
        return 0;
      }
      virtualTime = Math.max(virtualTime, virtualTimerQueue[0].nextTime);
      isTicking = true;
      let numProcessed = 0;
      while (!shouldCancelAdvanceFrame && virtualTimerQueue.length > 0 && virtualTimerQueue[0].nextTime <= virtualTime) {
        const entry = virtualTimerQueue[0];
        if (entry.interval == null && pendingClearTimeouts.includes(entry.handle)) {
          continue;
        }
        if (entry.interval != null && pendingClearIntervals.includes(entry.handle)) {
          continue;
        }
        processTimer(entry);
        virtualTimerQueue.shift();
        ++numProcessed;
      }
      isTicking = false;
      processPendingOps();
      return numProcessed;
    }
    function cancelAdvanceFrame() {
      if (!isTicking) {
        return;
      }
      shouldCancelAdvanceFrame = true;
    }
    function processPendingOps() {
      for (const handle of pendingClearTimeouts) {
        clearTimeout$1(handle);
      }
      pendingClearTimeouts.length = 0;
      for (const handle of pendingClearIntervals) {
        clearInterval(handle);
      }
      pendingClearIntervals.length = 0;
      for (const entry of pendingInserts) {
        doInsert(entry);
      }
      pendingInserts.length = 0;
    }
    function processTimer(entry) {
      try {
        entry.callback(...entry.args);
      } catch (err) {
        error(err);
      }
      if (entry.interval != null) {
        insertNewTimeout(entry.callback, entry.args, entry.nextTime + entry.interval, entry.interval);
      }
    }
    global$1.setTimeout = setTimeout$1;
    global$1.clearTimeout = clearTimeout$1;
    global$1.setInterval = setInterval;
    global$1.clearInterval = clearInterval;
    global$1.setImmediate = setImmediate$1;

    /**
     * @this {Promise}
     */
    function finallyConstructor(callback) {
      var constructor = this.constructor;
      return this.then(function (value) {
        // @ts-ignore
        return constructor.resolve(callback()).then(function () {
          return value;
        });
      }, function (reason) {
        // @ts-ignore
        return constructor.resolve(callback()).then(function () {
          // @ts-ignore
          return constructor.reject(reason);
        });
      });
    }

    function allSettled(arr) {
      var P = this;
      return new P(function (resolve, reject) {
        if (!(arr && typeof arr.length !== 'undefined')) {
          return reject(new TypeError(typeof arr + ' ' + arr + ' is not iterable(cannot read property Symbol(Symbol.iterator))'));
        }
        var args = Array.prototype.slice.call(arr);
        if (args.length === 0) return resolve([]);
        var remaining = args.length;
        function res(i, val) {
          if (val && (typeof val === 'object' || typeof val === 'function')) {
            var then = val.then;
            if (typeof then === 'function') {
              then.call(val, function (val) {
                res(i, val);
              }, function (e) {
                args[i] = {
                  status: 'rejected',
                  reason: e
                };
                if (--remaining === 0) {
                  resolve(args);
                }
              });
              return;
            }
          }
          args[i] = {
            status: 'fulfilled',
            value: val
          };
          if (--remaining === 0) {
            resolve(args);
          }
        }
        for (var i = 0; i < args.length; i++) {
          res(i, args[i]);
        }
      });
    }

    /**
     * @constructor
     */
    function AggregateError(errors, message) {
      this.name = 'AggregateError', this.errors = errors;
      this.message = message || '';
    }
    AggregateError.prototype = Error.prototype;
    function any(arr) {
      var P = this;
      return new P(function (resolve, reject) {
        if (!(arr && typeof arr.length !== 'undefined')) {
          return reject(new TypeError('Promise.any accepts an array'));
        }
        var args = Array.prototype.slice.call(arr);
        if (args.length === 0) return reject();
        var rejectionReasons = [];
        for (var i = 0; i < args.length; i++) {
          try {
            P.resolve(args[i]).then(resolve).catch(function (error) {
              rejectionReasons.push(error);
              if (rejectionReasons.length === args.length) {
                reject(new AggregateError(rejectionReasons, 'All promises were rejected'));
              }
            });
          } catch (ex) {
            reject(ex);
          }
        }
      });
    }

    // Store setTimeout reference so promise-polyfill will be unaffected by
    // other code modifying setTimeout (like sinon.useFakeTimers())
    var setTimeoutFunc = global.setTimeout;
    function isArray(x) {
      return Boolean(x && typeof x.length !== 'undefined');
    }
    function noop() {}

    // Polyfill for Function.prototype.bind
    function bind(fn, thisArg) {
      return function () {
        fn.apply(thisArg, arguments);
      };
    }

    /**
     * @constructor
     * @param {Function} fn
     */
    function Promise$1(fn) {
      if (!(this instanceof Promise$1)) throw new TypeError('Promises must be constructed via new');
      if (typeof fn !== 'function') throw new TypeError('not a function');
      /** @type {!number} */
      this._state = 0;
      /** @type {!boolean} */
      this._handled = false;
      /** @type {Promise|undefined} */
      this._value = undefined;
      /** @type {!Array<!Function>} */
      this._deferreds = [];
      doResolve(fn, this);
    }
    function handle(self, deferred) {
      while (self._state === 3) {
        self = self._value;
      }
      if (self._state === 0) {
        self._deferreds.push(deferred);
        return;
      }
      self._handled = true;
      Promise$1._immediateFn(function () {
        var cb = self._state === 1 ? deferred.onFulfilled : deferred.onRejected;
        if (cb === null) {
          (self._state === 1 ? resolve : reject)(deferred.promise, self._value);
          return;
        }
        var ret;
        try {
          ret = cb(self._value);
        } catch (e) {
          reject(deferred.promise, e);
          return;
        }
        resolve(deferred.promise, ret);
      });
    }
    function resolve(self, newValue) {
      try {
        // Promise Resolution Procedure: https://github.com/promises-aplus/promises-spec#the-promise-resolution-procedure
        if (newValue === self) throw new TypeError('A promise cannot be resolved with itself.');
        if (newValue && (typeof newValue === 'object' || typeof newValue === 'function')) {
          var then = newValue.then;
          if (newValue instanceof Promise$1) {
            self._state = 3;
            self._value = newValue;
            finale(self);
            return;
          } else if (typeof then === 'function') {
            doResolve(bind(then, newValue), self);
            return;
          }
        }
        self._state = 1;
        self._value = newValue;
        finale(self);
      } catch (e) {
        reject(self, e);
      }
    }
    function reject(self, newValue) {
      self._state = 2;
      self._value = newValue;
      finale(self);
    }
    function finale(self) {
      if (self._state === 2 && self._deferreds.length === 0) {
        Promise$1._immediateFn(function () {
          if (!self._handled) {
            Promise$1._unhandledRejectionFn(self._value);
          }
        });
      }
      for (var i = 0, len = self._deferreds.length; i < len; i++) {
        handle(self, self._deferreds[i]);
      }
      self._deferreds = null;
    }

    /**
     * @constructor
     */
    function Handler(onFulfilled, onRejected, promise) {
      this.onFulfilled = typeof onFulfilled === 'function' ? onFulfilled : null;
      this.onRejected = typeof onRejected === 'function' ? onRejected : null;
      this.promise = promise;
    }

    /**
     * Take a potentially misbehaving resolver function and make sure
     * onFulfilled and onRejected are only called once.
     *
     * Makes no guarantees about asynchrony.
     */
    function doResolve(fn, self) {
      var done = false;
      try {
        fn(function (value) {
          if (done) return;
          done = true;
          resolve(self, value);
        }, function (reason) {
          if (done) return;
          done = true;
          reject(self, reason);
        });
      } catch (ex) {
        if (done) return;
        done = true;
        reject(self, ex);
      }
    }
    Promise$1.prototype['catch'] = function (onRejected) {
      return this.then(null, onRejected);
    };
    Promise$1.prototype.then = function (onFulfilled, onRejected) {
      // @ts-ignore
      var prom = new this.constructor(noop);
      handle(this, new Handler(onFulfilled, onRejected, prom));
      return prom;
    };
    Promise$1.prototype['finally'] = finallyConstructor;
    Promise$1.all = function (arr) {
      return new Promise$1(function (resolve, reject) {
        if (!isArray(arr)) {
          return reject(new TypeError('Promise.all accepts an array'));
        }
        var args = Array.prototype.slice.call(arr);
        if (args.length === 0) return resolve([]);
        var remaining = args.length;
        function res(i, val) {
          try {
            if (val && (typeof val === 'object' || typeof val === 'function')) {
              var then = val.then;
              if (typeof then === 'function') {
                then.call(val, function (val) {
                  res(i, val);
                }, reject);
                return;
              }
            }
            args[i] = val;
            if (--remaining === 0) {
              resolve(args);
            }
          } catch (ex) {
            reject(ex);
          }
        }
        for (var i = 0; i < args.length; i++) {
          res(i, args[i]);
        }
      });
    };
    Promise$1.any = any;
    Promise$1.allSettled = allSettled;
    Promise$1.resolve = function (value) {
      if (value && typeof value === 'object' && value.constructor === Promise$1) {
        return value;
      }
      return new Promise$1(function (resolve) {
        resolve(value);
      });
    };
    Promise$1.reject = function (value) {
      return new Promise$1(function (resolve, reject) {
        reject(value);
      });
    };
    Promise$1.race = function (arr) {
      return new Promise$1(function (resolve, reject) {
        if (!isArray(arr)) {
          return reject(new TypeError('Promise.race accepts an array'));
        }
        for (var i = 0, len = arr.length; i < len; i++) {
          Promise$1.resolve(arr[i]).then(resolve, reject);
        }
      });
    };

    // Use polyfill for setImmediate for performance gains
    Promise$1._immediateFn =
    // @ts-ignore
    typeof setImmediate === 'function' && function (fn) {
      // @ts-ignore
      setImmediate(fn);
    } || function (fn) {
      setTimeoutFunc(fn, 0);
    };
    Promise$1._unhandledRejectionFn = function _unhandledRejectionFn(err) {
      if (typeof console !== 'undefined' && console) {
        console.log('Possible Unhandled Promise Rejection:', err); // eslint-disable-line no-console
      }
    };

    function _empty$1() {}
    const console$1 = {
      ...importedLogging
    };
    function _call$1(body, then, direct) {
      if (direct) {
        return then ? then(body()) : body();
      }
      try {
        var result = Promise$1.resolve(body());
        return then ? result.then(then) : result;
      } catch (e) {
        return Promise$1.reject(e);
      }
    }
    const globalThis = global;
    function _callIgnored(body, direct) {
      return _call$1(body, _empty$1, direct);
    }
    function _rethrow(thrown, value) {
      if (thrown) throw value;
      return value;
    }
    function _finallyRethrows(body, finalizer) {
      try {
        var result = body();
      } catch (e) {
        return finalizer(true, e);
      }
      if (result && result.then) {
        return result.then(finalizer.bind(null, false), finalizer.bind(null, true));
      }
      return finalizer(false, result);
    }
    function _continueIgnored(value) {
      if (value && value.then) {
        return value.then(_empty$1);
      }
    }
    function _async(f) {
      return function () {
        for (var args = [], i = 0; i < arguments.length; i++) {
          args[i] = arguments[i];
        }
        try {
          return Promise$1.resolve(f.apply(this, args));
        } catch (e) {
          return Promise$1.reject(e);
        }
      };
    }
    function _await$1(value, then, direct) {
      if (direct) {
        return then ? then(value) : value;
      }
      if (!value || !value.then) {
        value = Promise$1.resolve(value);
      }
      return then ? value.then(then) : value;
    }
    function _catch$1(body, recover) {
      try {
        var result = body();
      } catch (e) {
        return recover(e);
      }
      if (result && result.then) {
        return result.then(void 0, recover);
      }
      return result;
    }
    function _continue$1(value, then) {
      return value && value.then ? value.then(then) : then(value);
    }
    function _invoke(body, then) {
      var result = body();
      if (result && result.then) {
        return result.then(then);
      }
      return then(result);
    }
    function _invokeIgnored(body) {
      var result = body();
      if (result && result.then) {
        return result.then(_empty$1);
      }
    }
    function _awaitIgnored$1(value, direct) {
      if (!direct) {
        return value && value.then ? value.then(_empty$1) : Promise$1.resolve();
      }
    }
    function _settle(pact, state, value) {
      if (!pact.s) {
        if (value instanceof _Pact) {
          if (value.s) {
            if (state & 1) {
              state = value.s;
            }
            value = value.v;
          } else {
            value.o = _settle.bind(null, pact, state);
            return;
          }
        }
        if (value && value.then) {
          value.then(_settle.bind(null, pact, state), _settle.bind(null, pact, 2));
          return;
        }
        pact.s = state;
        pact.v = value;
        const observer = pact.o;
        if (observer) {
          observer(pact);
        }
      }
    }
    const _Pact = /*#__PURE__*/function () {
      function _Pact() {}
      _Pact.prototype.then = function (onFulfilled, onRejected) {
        const result = new _Pact();
        const state = this.s;
        if (state) {
          const callback = state & 1 ? onFulfilled : onRejected;
          if (callback) {
            try {
              _settle(result, 1, callback(this.v));
            } catch (e) {
              _settle(result, 2, e);
            }
            return result;
          } else {
            return this;
          }
        }
        this.o = function (_this) {
          try {
            const value = _this.v;
            if (_this.s & 1) {
              _settle(result, 1, onFulfilled ? onFulfilled(value) : value);
            } else if (onRejected) {
              _settle(result, 1, onRejected(value));
            } else {
              _settle(result, 2, value);
            }
          } catch (e) {
            _settle(result, 2, e);
          }
        };
        return result;
      };
      return _Pact;
    }();
    function _isSettledPact(thenable) {
      return thenable instanceof _Pact && thenable.s & 1;
    }
    function _for(test, update, body) {
      var stage;
      for (;;) {
        var shouldContinue = test();
        if (_isSettledPact(shouldContinue)) {
          shouldContinue = shouldContinue.v;
        }
        if (!shouldContinue) {
          return result;
        }
        if (shouldContinue.then) {
          stage = 0;
          break;
        }
        var result = body();
        if (result && result.then) {
          if (_isSettledPact(result)) {
            result = result.s;
          } else {
            stage = 1;
            break;
          }
        }
        if (update) {
          var updateValue = update();
          if (updateValue && updateValue.then && !_isSettledPact(updateValue)) {
            stage = 2;
            break;
          }
        }
      }
      var pact = new _Pact();
      var reject = _settle.bind(null, pact, 2);
      (stage === 0 ? shouldContinue.then(_resumeAfterTest) : stage === 1 ? result.then(_resumeAfterBody) : updateValue.then(_resumeAfterUpdate)).then(void 0, reject);
      return pact;
      function _resumeAfterBody(value) {
        result = value;
        do {
          if (update) {
            updateValue = update();
            if (updateValue && updateValue.then && !_isSettledPact(updateValue)) {
              updateValue.then(_resumeAfterUpdate).then(void 0, reject);
              return;
            }
          }
          shouldContinue = test();
          if (!shouldContinue || _isSettledPact(shouldContinue) && !shouldContinue.v) {
            _settle(pact, 1, result);
            return;
          }
          if (shouldContinue.then) {
            shouldContinue.then(_resumeAfterTest).then(void 0, reject);
            return;
          }
          result = body();
          if (_isSettledPact(result)) {
            result = result.v;
          }
        } while (!result || !result.then);
        result.then(_resumeAfterBody).then(void 0, reject);
      }
      function _resumeAfterTest(shouldContinue) {
        if (shouldContinue) {
          result = body();
          if (result && result.then) {
            result.then(_resumeAfterBody).then(void 0, reject);
          } else {
            _resumeAfterBody(result);
          }
        } else {
          _settle(pact, 1, result);
        }
      }
      function _resumeAfterUpdate() {
        if (shouldContinue = test()) {
          if (shouldContinue.then) {
            shouldContinue.then(_resumeAfterTest).then(void 0, reject);
          } else {
            _resumeAfterTest(shouldContinue);
          }
        } else {
          _settle(pact, 1, result);
        }
      }
    }
    const _iteratorSymbol = typeof Symbol !== "undefined" ? Symbol.iterator || (Symbol.iterator = Symbol("Symbol.iterator")) : "@@iterator";
    function _forTo(array, body, check) {
      var i = -1,
        pact,
        reject;
      function _cycle(result) {
        try {
          while (++i < array.length && (!check || !check())) {
            result = body(i);
            if (result && result.then) {
              if (_isSettledPact(result)) {
                result = result.v;
              } else {
                result.then(_cycle, reject || (reject = _settle.bind(null, pact = new _Pact(), 2)));
                return;
              }
            }
          }
          if (pact) {
            _settle(pact, 1, result);
          } else {
            pact = result;
          }
        } catch (e) {
          _settle(pact || (pact = new _Pact()), 2, e);
        }
      }
      _cycle();
      return pact;
    }
    function _forOf(target, body, check) {
      if (typeof target[_iteratorSymbol] === "function") {
        var iterator = target[_iteratorSymbol](),
          step,
          pact,
          reject;
        function _cycle(result) {
          try {
            while (!(step = iterator.next()).done && (!check || !check())) {
              result = body(step.value);
              if (result && result.then) {
                if (_isSettledPact(result)) {
                  result = result.v;
                } else {
                  result.then(_cycle, reject || (reject = _settle.bind(null, pact = new _Pact(), 2)));
                  return;
                }
              }
            }
            if (pact) {
              _settle(pact, 1, result);
            } else {
              pact = result;
            }
          } catch (e) {
            _settle(pact || (pact = new _Pact()), 2, e);
          }
        }
        _cycle();
        if (iterator.return) {
          var _fixup = function (value) {
            try {
              if (!step.done) {
                iterator.return();
              }
            } catch (e) {}
            return value;
          };
          if (pact && pact.then) {
            return pact.then(_fixup, function (e) {
              throw _fixup(e);
            });
          }
          _fixup();
        }
        return pact;
      }
      // No support for Symbol.iterator
      if (!("length" in target)) {
        throw new TypeError("Object is not iterable");
      }
      // Handle live collections properly
      var values = [];
      for (var i = 0; i < target.length; i++) {
        values.push(target[i]);
      }
      return _forTo(values, function (i) {
        return body(values[i]);
      }, check);
    }
    globalThis.setTimeout = setTimeout$1;
    globalThis.clearTimeout = clearTimeout$1;
    globalThis.setInterval = setInterval;
    globalThis.clearInterval = clearInterval;
    globalThis.setImmediate = setImmediate$1;
    function _import(moduleName) {
      debug(`intercepted import with module name of '${moduleName}'`);
      return new Promise$1(resolve => {});
    }
    const _document = {
      currentScript: {
        src: './'
      }
    };
    function _WebAssemblyInstantiate(bytes, imports) {
      try {
        const compiledModule = new WebAssembly.Module(bytes);
        const compiledInstance = new WebAssembly.Instance(compiledModule, imports);
        return Promise$1.resolve({
          instance: compiledInstance,
          module: compiledModule
        });
      } catch (err) {
        console$1.log(`Failed to create wasm instance: ${err}`);
        return Promise$1.reject(err);
      }
    }
    //! Licensed to the .NET Foundation under one or more agreements.
    //! The .NET Foundation licenses this file to you under the MIT license.
    var __dotnet_runtime = function (e) {

      const Ic = _async(function (e) {
        tc = o.config = b.config = Object.assign(b.config || {}, e || {});
        return _call$1(es, function () {
          const _rc3 = rc;
          return _await$1(_rc3 || us(), function (_us) {
          }, _rc3);
        });
      });
      const Rc = _async(function (e) {
        let _exit12 = false;
        return _invoke(function () {
          if (nc) {
            return _await$1(oc.promise, function () {
              const _temp6 = (void 0);
              _exit12 = true;
              return _temp6;
            });
          }
        }, function (_result19) {
          if (_exit12) return _result19;
          if (nc = true, !e) return t(), oc.promise_control.resolve(), void 0;
          b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_load_config");
          function t() {
            tc.environmentVariables = tc.environmentVariables || {}, tc.assets = tc.assets || [], tc.runtimeOptions = tc.runtimeOptions || [], tc.globalizationMode = tc.globalizationMode || "auto", tc.debugLevel, tc.diagnosticTracing, b.diagnosticTracing = !!b.config.diagnosticTracing;
          }
          return _catch$1(function () {
            const n = b.locateFile(e);
            return _await$1(b.fetch_like(n), function (r) {
              return _await$1(r.json(), function (s) {
                let _exit13 = false;
                if (s.environmentVariables && "object" !== typeof s.environmentVariables) throw new Error("Expected config.environmentVariables to be unset or a dictionary-style object");
                return _invoke(function () {
                  if (s.assets = [...(s.assets || []), ...(tc.assets || [])], s.environmentVariables = {
                    ...(s.environmentVariables || {}),
                    ...(tc.environmentVariables || {})
                  }, tc = b.config = o.config = Object.assign(o.config, s), t(), o.onConfigLoaded) return _catch$1(function () {
                    return _await$1(o.onConfigLoaded(b.config), function () {
                      0, t();
                    });
                  }, function (e) {
                    throw Sc("MONO_WASM: onConfigLoaded() failed", e), e;
                  });
                }, function (_result22) {
                  if (_exit13) ;
                  oc.promise_control.resolve();
                });
              });
            });
          }, function (t) {
            const n = `Failed to load config file ${e} ${t}`;
            throw pc(n, true), tc = b.config = o.config = {
              message: n,
              error: t,
              isError: true
            }, t;
          });
        });
      });
      const $c = _async(function () {
        try {
          const e = undefined;
          Oc("TZ", Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC");
        } catch (e) {
          Oc("TZ", "UTC");
        }
        for (const e in tc.environmentVariables) {
          const t = tc.environmentVariables[e];
          if ("string" !== typeof t) throw new Error(`Expected environment variable '${e}' to be a string but it was ${typeof t}: '${t}'`);
          Oc(e, t);
        }
        tc.runtimeOptions && xc(tc.runtimeOptions), tc.aotProfilerOptions && me(tc.aotProfilerOptions), tc.coverageProfilerOptions && ge(tc.coverageProfilerOptions);
        return _await$1();
      });
      const jc = _async(function (e, t) {
        return _continue$1(_catch$1(function () {
          return _await$1(Rc(o.configSrc), function () {
            0, b.diagnosticTracing && console$1.debug("MONO_WASM: instantiate_wasm_module");
            const n = Ko("dotnetwasm");
            return _await$1(ts(n, false), function () {
              return _await$1(ic.promise, function () {
                0, o.addRunDependency("instantiate_wasm_module"), as(n, e, t), b.diagnosticTracing && console$1.debug("MONO_WASM: instantiate_wasm_module done"), sc.promise_control.resolve();
              });
            });
          });
        }, function (e) {
          throw Sc("MONO_WASM: instantiate_wasm_module() failed", e), pc(e, true), e;
        }), function (_result18) {
          o.removeRunDependency("instantiate_wasm_module");
        });
      });
      const Ac = _async(function () {
        b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_after_user_runtime_initialized");
        return _catch$1(function () {
          if (!o.disableDotnet6Compatibility && o.exports) {
            const e = globalThis;
            for (let t = 0; t < o.exports.length; ++t) {
              const n = o.exports[t],
                r = o[n];
              void 0 != r ? e[n] = r : console$1.log(`MONO_WASM: The exported symbol ${n} could not be found in the emscripten module`);
            }
          }
          return function () {
            if (n, b.diagnosticTracing && console$1.debug("MONO_WASM: Initializing mono runtime"), o.onDotnetReady) return _catch$1(function () {
              return _awaitIgnored$1(o.onDotnetReady());
            }, function (e) {
              throw Sc("MONO_WASM: onDotnetReady () failed", e), e;
            });
          }();
        }, function (e) {
          throw Sc("MONO_WASM: Error in mono_wasm_after_user_runtime_initialized", e), e;
        });
      });
      const Ec = _async(function () {
        b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_before_user_runtime_initialized");
        return _catch$1(function () {
          return _call$1($c, function () {
            0, de(), b.mono_wasm_load_runtime_done || Nc("unused", tc.debugLevel), b.mono_wasm_runtime_is_ready || mono_wasm_runtime_ready(), b.mono_wasm_symbols_are_ready || ke("dotnet.js.symbols"), setTimeout$1(() => {
              Ar.init_fields();
            });
          });
        }, function (e) {
          throw Sc("MONO_WASM: Error in mono_wasm_before_user_runtime_initialized", e), e;
        });
      });
      const vc = _async(function () {
        o.addRunDependency("mono_wasm_pre_init_full");
          b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_pre_init_full");
        return _call$1(es, function () {
          o.removeRunDependency("mono_wasm_pre_init_full");
        });
      });
      const yc = _async(function () {
        o.addRunDependency("mono_wasm_pre_init_essential_async");
          b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_pre_init_essential_async");
        return _call$1(ms, function () {
          return _await$1(Rc(o.configSrc), function () {
            o.removeRunDependency("mono_wasm_pre_init_essential_async");
          });
        });
      });
      const hc = _async(function (e) {
        return _await$1(lc.promise, function () {
          b.diagnosticTracing && console$1.debug("MONO_WASM: postRunAsync");
          try {
            e.map(e => e());
          } catch (e) {
            throw Sc("MONO_WASM: user callback posRun() failed", e), pc(e, true), e;
          }
          fc.promise_control.resolve();
        });
      });
      const wc = _async(function (e) {
        return _await$1(cc.promise, function () {
          b.diagnosticTracing && console$1.debug("MONO_WASM: onRuntimeInitialized"), uc.promise_control.resolve();
          return _continue$1(_catch$1(function () {
            const _rc2 = rc;
            return _await$1(_rc2 || us(), function () {
              return _await$1(_rc2 || Ec(), function (_Ec) {
                _rc2 || _Ec, tc.runtimeOptions && xc(tc.runtimeOptions);
                try {
                  e();
                } catch (e) {
                  throw Sc("MONO_WASM: user callback onRuntimeInitialized() failed", e), e;
                }
                return _callIgnored(Ac);
              }, _rc2);
            }, _rc2);
          }, function (e) {
            throw Sc("MONO_WASM: onRuntimeInitializedAsync() failed", e), pc(e, true), e;
          }), function (_result13) {
            lc.promise_control.resolve();
          });
        });
      });
      const gc = _async(function (e) {
        o.addRunDependency("mono_pre_run_async");
        return _await$1(sc.promise, function () {
          return _await$1(ac.promise, function () {
            b.diagnosticTracing && console$1.debug("MONO_WASM: preRunAsync");
            try {
              e.map(e => e());
            } catch (e) {
              throw Sc("MONO_WASM: user callback preRun() failed", e), pc(e, true), e;
            }
            cc.promise_control.resolve(), o.removeRunDependency("mono_pre_run_async");
          });
        });
      });
      const _a = _async(function (e, t, n) {
        const r = new Span(t, n, 0);
        return _t(_async(function () {
          const _temp3 = !e.__chunk && e.body;
            _temp3 && (e.__reader = e.body.getReader());
          return _await$1(_temp3 && e.__reader.read(), function (_e$__reader$read) {
            let _exit9 = false;
            _temp3 && (e.__chunk = _e$__reader$read, e.__source_offset = 0);
            let t = 0,
              n = 0;
            return _continue$1(_for(function () {
              return !_exit9 && !!e.__reader && !!e.__chunk && !e.__chunk.done;
            }, void 0, function () {
              const o = e.__chunk.value.byteLength - e.__source_offset;
              return _invoke(function () {
                if (0 === o) {
                  return _await$1(e.__reader.read(), function (_e$__reader$read2) {
                    e.__chunk = _e$__reader$read2, e.__source_offset = 0;
                  });
                }
              }, function () {
                const s = r.byteLength - t,
                  i = Math.min(o, s),
                  a = e.__chunk.value.subarray(e.__source_offset, e.__source_offset + i);
                if (r.set(a, t), t += i, n += i, e.__source_offset += i, t == r.byteLength) {
                  _exit9 = true;
                  return n;
                }
              });
            }), function (_result12) {
              return _exit9 ? _result12 : n;
            });
          }, !_temp3);
        }));
      });
      const Hs = _async(function (e) {
        if (!b.mono_wasm_bindings_is_ready) throw new Error("Assert failed: The runtime must be initialized.");
        if (!Fs.get(e)) {
          const t = be(e);
          if (!t) throw new Error("Could not find assembly: " + e);
          M.mono_wasm_runtime_run_module_cctor(t);
        }
        return Fs.get(e) || {};
      });
      const Is = _async(function (e, t) {
        if (!e) throw new Error("Assert failed: Invalid module_name");
        if (!t) throw new Error("Assert failed: Invalid module_name");
        let n = Ts.get(e);
        const r = !n;
        r && (b.diagnosticTracing && console$1.debug(`MONO_WASM: importing ES6 module '${e}' from '${t}'`), n = _import(t), Ts.set(e, n));
        return _await$1(n, function (o) {
          return r && (Ms.set(e, o), b.diagnosticTracing && console$1.debug(`MONO_WASM: imported ES6 module '${e}' from '${t}'`)), o;
        });
      });
      const ws = _async(function (e, t) {
        let _exit8 = false;
        return _continue$1(_catch$1(function () {
          return _invoke(function () {
            if (a) {
              return _invoke(function () {
                if (!fs) {
                  return _await$1(b.requirePromise, function (e) {
                    _s = e("url"), fs = e("fs");
                  });
                }
              }, function () {
                e.startsWith("file://") && (e = _s.fileURLToPath(e));
                return _await$1(fs.promises.readFile(e), function (t) {
                  const _ok$url$arrayBuffer$j = {
                    ok: true,
                    url: e,
                    arrayBuffer: () => t,
                    json: () => JSON.parse(t)
                  };
                  _exit8 = true;
                  return _ok$url$arrayBuffer$j;
                });
              });
            }
          }, function (_result10) {
            if (_exit8) return _result10;
            if ("function" === typeof globalThis.fetch) {
              const _globalThis$fetch = globalThis.fetch(e, t || {
                credentials: "same-origin"
              });
              _exit8 = true;
              return _globalThis$fetch;
            }
            if ("function" === typeof read) {
              const t = new Uint8Array(read(e, "binary"));
              const _ok$url$arrayBuffer$j2 = {
                ok: true,
                url: e,
                arrayBuffer: () => t,
                json: () => JSON.parse(o.UTF8ArrayToString(t, 0, t.length))
              };
              _exit8 = true;
              return _ok$url$arrayBuffer$j2;
            }
          });
        }, function (t) {
          const _ok$url$status$status = {
            ok: false,
            url: e,
            status: 500,
            statusText: "ERR28: " + t,
            arrayBuffer: () => {
              throw t;
            },
            json: () => {
              throw t;
            }
          };
          _exit8 = true;
          return _ok$url$status$status;
        }), function (_result11) {
          if (_exit8) return _result11;
          throw new Error("No fetch implementation available");
        });
      });
      const ms = _async(function () {
        const _a2 = a;
        return _await$1(_a2 && b.requirePromise, function (_b$requirePromise) {
          if (_a2 && (s.require = _b$requirePromise, globalThis.performance === gs)) {
            const {
              performance: e
            } = s.require("perf_hooks");
            globalThis.performance = e;
          }
        }, !_a2);
      });
      const us = _async(function () {
        return _await$1(Fo.promise, function () {
          if (b.config.assets) {
            if (!(Ho == zo)) throw new Error(`Assert failed: Expected ${zo} assets to be downloaded, but only finished ${Ho}`);
            if (!(Vo == Lo)) throw new Error(`Assert failed: Expected ${Lo} assets to be in memory, but only instantiated ${Vo}`);
            Jo.forEach(e => Io.loaded_files.push(e.url)), b.diagnosticTracing && console$1.debug("MONO_WASM: all assets are loaded in wasm memory");
          }
        });
      });
      const as = _async(function (e, t, n) {
        if (!(e && e.pendingDownloadInternal)) throw new Error("Assert failed: Can't load dotnet.wasm");
        return _await$1(e.pendingDownloadInternal.response, function (r) {
          const o = r.headers ? r.headers.get("Content-Type") : void 0;
          let s, i;
          return _invoke(function () {
            if ("function" === typeof WebAssembly.instantiateStreaming && "application/wasm" === o) {
              b.diagnosticTracing && console$1.debug("MONO_WASM: instantiate_wasm_module streaming");
              return _await$1(WebAssembly.instantiateStreaming(r, t), function (e) {
                s = e.instance, i = e.module;
              });
            } else {
              u && "application/wasm" !== o && console$1.log('MONO_WASM: WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');
              return _await$1(r.arrayBuffer(), function (e) {
                b.diagnosticTracing && console$1.debug("MONO_WASM: instantiate_wasm_module buffered");
                return _await$1(_WebAssemblyInstantiate(e, t), function (n) {
                  s = n.instance, i = n.module;
                });
              });
            }
          }, function () {
            n(s, i);
          });
        });
      });
      const rs = _async(function (e) {
        let _exit6 = false;
        if (e.buffer) {
          const t = e.buffer;
          return e.buffer = null, e.pendingDownloadInternal = {
            url: "undefined://" + e.name,
            name: e.name,
            response: Promise$1.resolve({
              arrayBuffer: () => t,
              headers: {
                get: () => {}
              }
            })
          }, ++Ho, e.pendingDownloadInternal.response;
        }
        return _invoke(function () {
          if (e.pendingDownloadInternal && e.pendingDownloadInternal.response) {
            return _await$1(e.pendingDownloadInternal.response, function (_await$e$pendingDownl) {
              _exit6 = true;
              return _await$e$pendingDownl;
            });
          }
        }, function (_result7) {
          let _exit7 = false;
          if (_exit6) return _result7;
          const t = e.loadRemote && b.config.remoteSources ? b.config.remoteSources : [""];
          let n;
          return _continue$1(_forOf(t, function (r) {
            r = r.trim(), "./" === r && (r = "");
            const t = os(e, r);
            e.name === t ? b.diagnosticTracing && console$1.debug(`MONO_WASM: Attempting to download '${t}'`) : b.diagnosticTracing && console$1.debug(`MONO_WASM: Attempting to download '${t}' for ${e.name}`);
            return _catch$1(function () {
              const r = ss({
                name: e.name,
                resolvedUrl: t,
                hash: e.hash,
                behavior: e.behavior
              });
              const _e$pendingDownloadInt3 = e.pendingDownloadInternal = r;
              return _await$1(r.response, function (_r$response) {
                if (n = _r$response, !n.ok) return;
                const _temp2 = (++Ho, n);
                _exit7 = true;
                return _temp2;
              });
            }, function () {});
          }, function () {
            return _exit7;
          }), function (_result9) {
            if (_exit7) return _result9;
            const r = e.isOptional || e.name.match(/\.pdb$/) && b.config.ignorePdbLoadErrors;
            if (!n) throw new Error(`Assert failed: Response undefined ${e.name}`);
            if (r) return o.print(`MONO_WASM: optional download '${n.url}' for ${e.name} failed ${n.status} ${n.statusText}`), void 0;
            {
              const t = new Error(`MONO_WASM: download '${n.url}' for ${e.name} failed ${n.status} ${n.statusText}`);
              throw t.status = n.status, t;
            }
          });
        });
      });
      const ns = _async(function (e, t) {
        return _continue$1(_for(function () {
          return !!Yo;
        }, void 0, function () {
          return _awaitIgnored$1(Yo.promise);
        }), function () {
          return _finallyRethrows(function () {
            ++Go, Go == b.maxParallelDownloads && (b.diagnosticTracing && console$1.debug("MONO_WASM: Throttling further parallel downloads"), Yo = it());
            return _await$1(rs(e), function (n) {
              if (!t || !n) return;
              return _await$1(n.arrayBuffer());
            });
          }, function (_wasThrown2, _result6) {
            if (--Go, Yo && Go == b.maxParallelDownloads - 1) {
              b.diagnosticTracing && console$1.debug("MONO_WASM: Resuming more parallel downloads");
              const e = Yo;
              Yo = void 0, e.promise_control.resolve();
            }
            return _rethrow(_wasThrown2, _result6);
          });
        });
      });
      const ts = _async(function (e, t) {
        return _catch$1(function () {
          return _await$1(ns(e, t));
        }, function (n) {
          if (c || a) throw n;
          if (e.pendingDownload && e.pendingDownloadInternal == e.pendingDownload) throw n;
          if (e.resolvedUrl && -1 != e.resolvedUrl.indexOf("file://")) throw n;
          if (n && 404 == n.status) throw n;
          e.pendingDownloadInternal = void 0;
          return _await$1(Bo.promise, function (_Bo$promise) {
            return _catch$1(function () {
              return _await$1(ns(e, t));
            }, function () {
              e.pendingDownloadInternal = void 0;
              return _await$1(Wo(100), function () {
                return _await$1(ns(e, t));
              });
            });
          });
        });
      });
      const es = _async(function () {
        b.diagnosticTracing && console$1.debug("MONO_WASM: mono_download_assets"), b.maxParallelDownloads = b.config.maxParallelDownloads || b.maxParallelDownloads;
        try {
          const e = [];
          for (const t of b.config.assets) {
            const n = t;
            if (Qo[n.behavior] || Lo++, !Zo[n.behavior]) {
              const t = Xo[n.behavior];
              if (zo++, n.pendingDownload) {
                n.pendingDownloadInternal = n.pendingDownload;
                const r = _async(function () {
                  return _await$1(n.pendingDownloadInternal.response, function (e) {
                    const _Ho = ++Ho;
                    return _await$1(t || e.arrayBuffer(), function (_e$arrayBuffer) {
                      return t || (n.buffer = _e$arrayBuffer), {
                        asset: n,
                        buffer: n.buffer
                      };
                    }, t);
                  });
                });
                e.push(r());
              } else {
                const r = function () {
                  return _await$1(ts(n, !t), function (_ts) {
                    return n.buffer = _ts, {
                      asset: n,
                      buffer: n.buffer
                    };
                  });
                };
                e.push(r());
              }
            }
          }
          Bo.promise_control.resolve();
          const t = [];
          for (const n of e) t.push(function () {
            return _await$1(n, function (e) {
              const t = e.asset;
              return function () {
                if (e.buffer) {
                  return _invokeIgnored(function () {
                    if (!Qo[t.behavior]) {
                      const n = t.pendingDownloadInternal.url,
                        r = new Uint8Array(t.buffer);
                      const _e$buffer = e.buffer = null,
                        _t$buffer = t.buffer = null,
                        _t$pendingDownload = t.pendingDownload = null,
                        _t$pendingDownloadInt = t.pendingDownloadInternal = null;
                      return _await$1(uc.promise, function () {
                        0, is(t, n, r);
                      });
                    }
                  });
                } else {
                  const e = undefined;
                  if (!Xo[t.behavior]) {
                    if (!t.isOptional) throw new Error("Assert failed: Expected asset to have the downloaded buffer");
                    Zo[t.behavior] || zo--, Qo[t.behavior] || Lo--;
                  }
                }
              }();
            });
          }());
          Promise$1.all(t).then(() => {
            Fo.promise_control.resolve();
          }).catch(e => {
            o.printErr("MONO_WASM: Error in mono_download_assets: " + e), pc(e, true);
          });
        } catch (e) {
          throw o.printErr("MONO_WASM: Error in mono_download_assets: " + e), e;
        }
        return _await$1();
      });
      const Ue = _async(function () {
        return _continueIgnored(_catch$1(function () {
          return _await$1(Promise.resolve().then(function () { return _polyfillNode_process; }), function (e) {
            const t = e => new Promise$1((t, n) => {
                e.on("error", e => n(e)), e.write("", function () {
                  t();
                });
              }),
              n = t(e.stderr),
              r = t(e.stdout);
            return _awaitIgnored$1(Promise$1.all([r, n]));
          });
        }, function (e) {
          console$1.error(`flushing std* streams failed: ${e}`);
        }));
      });
      const Te = _async(function (e, t) {
        Mc(e, t);
          const _temp = -1 == b.waitForDebugger;
          _temp && console$1.log("MONO_WASM: waiting for debugger...");
        return _await$1(_temp && ee(), function (_ee) {
          const n = Me(e);
          return b.javaScriptExports.call_entry_point(n, t);
        }, !_temp);
      });
      const Re = _async(function (e, t) {
        return _catch$1(function () {
          return _await$1(Te(e, t), function (n) {
            return De(n), n;
          });
        }, function (e) {
          return e instanceof b.ExitStatus ? e.status : (De(1, e), 1);
        });
      });
      var t = "7.0.1",
        n = false,
        r = "Release";
      let o, s, i, a, c, u, l, f;
      const _ = {},
        d = {};
      let m;
      function g(e, t) {
        s = t.internal, i = t.marshaled_imports, o = t.module, w(e), a = e.isNode, c = e.isShell, u = e.isWeb, l = e.isWorker, f = e.isPThread, b.quit = e.quit_, b.ExitStatus = e.ExitStatus, b.requirePromise = e.requirePromise;
      }
      function w(e) {
        a = e.isNode, c = e.isShell, u = e.isWeb, l = e.isWorker, f = e.isPThread;
      }
      function h(e) {
        m = e;
      }
      const b = {
          javaScriptExports: {},
          mono_wasm_load_runtime_done: false,
          mono_wasm_bindings_is_ready: false,
          maxParallelDownloads: 16,
          config: {
            environmentVariables: {}
          },
          diagnosticTracing: false
        },
        x = -1;
      function R(e) {
        return void 0 === e || null === e;
      }
      const T = [[true, "mono_wasm_register_root", "number", ["number", "number", "string"]], [true, "mono_wasm_deregister_root", null, ["number"]], [true, "mono_wasm_string_get_data", null, ["number", "number", "number", "number"]], [true, "mono_wasm_string_get_data_ref", null, ["number", "number", "number", "number"]], [true, "mono_wasm_set_is_debugger_attached", "void", ["bool"]], [true, "mono_wasm_send_dbg_command", "bool", ["number", "number", "number", "number", "number"]], [true, "mono_wasm_send_dbg_command_with_parms", "bool", ["number", "number", "number", "number", "number", "number", "string"]], [true, "mono_wasm_setenv", null, ["string", "string"]], [true, "mono_wasm_parse_runtime_options", null, ["number", "number"]], [true, "mono_wasm_strdup", "number", ["string"]], [true, "mono_background_exec", null, []], [true, "mono_set_timeout_exec", null, []], [true, "mono_wasm_load_icu_data", "number", ["number"]], [true, "mono_wasm_get_icudt_name", "string", ["string"]], [false, "mono_wasm_add_assembly", "number", ["string", "number", "number"]], [true, "mono_wasm_add_satellite_assembly", "void", ["string", "string", "number", "number"]], [false, "mono_wasm_load_runtime", null, ["string", "number"]], [true, "mono_wasm_change_debugger_log_level", "void", ["number"]], [true, "mono_wasm_get_corlib", "number", []], [true, "mono_wasm_assembly_load", "number", ["string"]], [true, "mono_wasm_find_corlib_class", "number", ["string", "string"]], [true, "mono_wasm_assembly_find_class", "number", ["number", "string", "string"]], [true, "mono_wasm_runtime_run_module_cctor", "void", ["number"]], [true, "mono_wasm_find_corlib_type", "number", ["string", "string"]], [true, "mono_wasm_assembly_find_type", "number", ["number", "string", "string"]], [true, "mono_wasm_assembly_find_method", "number", ["number", "string", "number"]], [true, "mono_wasm_invoke_method", "number", ["number", "number", "number", "number"]], [false, "mono_wasm_invoke_method_ref", "void", ["number", "number", "number", "number", "number"]], [true, "mono_wasm_string_get_utf8", "number", ["number"]], [true, "mono_wasm_string_from_utf16_ref", "void", ["number", "number", "number"]], [true, "mono_wasm_get_obj_type", "number", ["number"]], [true, "mono_wasm_array_length", "number", ["number"]], [true, "mono_wasm_array_get", "number", ["number", "number"]], [true, "mono_wasm_array_get_ref", "void", ["number", "number", "number"]], [false, "mono_wasm_obj_array_new", "number", ["number"]], [false, "mono_wasm_obj_array_new_ref", "void", ["number", "number"]], [false, "mono_wasm_obj_array_set", "void", ["number", "number", "number"]], [false, "mono_wasm_obj_array_set_ref", "void", ["number", "number", "number"]], [true, "mono_wasm_register_bundled_satellite_assemblies", "void", []], [false, "mono_wasm_try_unbox_primitive_and_get_type_ref", "number", ["number", "number", "number"]], [true, "mono_wasm_box_primitive_ref", "void", ["number", "number", "number", "number"]], [true, "mono_wasm_intern_string_ref", "void", ["number"]], [true, "mono_wasm_assembly_get_entry_point", "number", ["number"]], [true, "mono_wasm_get_delegate_invoke_ref", "number", ["number"]], [true, "mono_wasm_string_array_new_ref", "void", ["number", "number"]], [true, "mono_wasm_typed_array_new_ref", "void", ["number", "number", "number", "number", "number"]], [true, "mono_wasm_class_get_type", "number", ["number"]], [true, "mono_wasm_type_get_class", "number", ["number"]], [true, "mono_wasm_get_type_name", "string", ["number"]], [true, "mono_wasm_get_type_aqn", "string", ["number"]], [true, "mono_wasm_event_pipe_enable", "bool", ["string", "number", "number", "string", "bool", "number"]], [true, "mono_wasm_event_pipe_session_start_streaming", "bool", ["number"]], [true, "mono_wasm_event_pipe_session_disable", "bool", ["number"]], [true, "mono_wasm_diagnostic_server_create_thread", "bool", ["string", "number"]], [true, "mono_wasm_diagnostic_server_thread_attach_to_runtime", "void", []], [true, "mono_wasm_diagnostic_server_post_resume_runtime", "void", []], [true, "mono_wasm_diagnostic_server_create_stream", "number", []], [true, "mono_wasm_string_from_js", "number", ["string"]], [false, "mono_wasm_exit", "void", ["number"]], [true, "mono_wasm_getenv", "number", ["string"]], [true, "mono_wasm_set_main_args", "void", ["number", "number"]], [false, "mono_wasm_enable_on_demand_gc", "void", ["number"]], [false, "mono_profiler_init_aot", "void", ["number"]], [false, "mono_wasm_exec_regression", "number", ["number", "string"]], [false, "mono_wasm_invoke_method_bound", "number", ["number", "number"]], [true, "mono_wasm_write_managed_pointer_unsafe", "void", ["number", "number"]], [true, "mono_wasm_copy_managed_pointer", "void", ["number", "number"]], [true, "mono_wasm_i52_to_f64", "number", ["number", "number"]], [true, "mono_wasm_u52_to_f64", "number", ["number", "number"]], [true, "mono_wasm_f64_to_i52", "number", ["number", "number"]], [true, "mono_wasm_f64_to_u52", "number", ["number", "number"]]],
        M = {};
      function I() {
        const e = !!f;
        for (const t of T) {
          const n = M,
            [r, s, i, a, c] = t;
          if (r || e) n[s] = function (...e) {
            const t = o.cwrap(s, i, a, c);
            return n[s] = t, t(...e);
          };else {
            const e = o.cwrap(s, i, a, c);
            n[s] = e;
          }
        }
      }
      function D(e, t, n) {
        const r = C(e, t, n);
        let o = "",
          s = 0,
          i = 0,
          a = 0,
          c = 0,
          u = 0,
          l = 0;
        const f = 16777215,
          _ = 262143,
          d = 4095,
          m = 63,
          g = 18,
          w = 12;
        for (; s = r.read(), i = r.read(), a = r.read(), null !== s;) null === i && (i = 0, u += 1), null === a && (a = 0, u += 1), l = s << 16 | i << 8 | a << 0, c = (l & f) >> g, o += U[c], c = (l & _) >> w, o += U[c], u < 2 && (c = (l & d) >> 6, o += U[c]), 2 === u ? o += "==" : 1 === u ? o += "=" : (c = (l & m) >> 0, o += U[c]);
        return o;
      }
      const U = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "+", "/"];
      function C(e, t, n) {
        let r = "number" === typeof t ? t : 0,
          o;
        o = "number" === typeof n ? r + n : e.length - r;
        const s = {
          read: function () {
            if (r >= o) return null;
            const t = e[r];
            return r += 1, t;
          }
        };
        return Object.defineProperty(s, "eof", {
          get: function () {
            return r >= o;
          },
          configurable: true,
          enumerable: true
        }), s;
      }
      const P = new Map();
      P.remove = function (e) {
        const t = this.get(e);
        return this.delete(e), t;
      };
      let W = {},
        F = 0,
        B = -1,
        H,
        V,
        z;
      function mono_wasm_runtime_ready() {
        if (s.mono_wasm_runtime_is_ready = b.mono_wasm_runtime_is_ready = true, F = 0, W = {}, B = -1, globalThis.dotnetDebugger) debugger;else console$1.debug("mono_wasm_runtime_ready", "fe00e07a-5519-4dfe-b35a-f867dbaf2e28");
      }
      function mono_wasm_fire_debugger_agent_message() {
        debugger;
      }
      function L(e, t, n, r) {
        const a = {
            res_ok: e,
            res: {
              id: t,
              value: D(new Uint8Array(o.HEAPU8.buffer, n, r))
            }
          };
        P.has(t) && console$1.log(`MONO_WASM: Adding an id (${t}) that already exists in commands_received`), P.set(t, a);
      }
      function J(e) {
        e.length > B && (H && o._free(H), B = Math.max(e.length, B, 256), H = o._malloc(B));
        const t = atob(e);
        for (let e = 0; e < t.length; e++) o.HEAPU8[H + e] = t.charCodeAt(e);
      }
      function q(e, t, n, r, o, s, i) {
        J(r), M.mono_wasm_send_dbg_command_with_parms(e, t, n, H, o, s, i.toString());
        const {
          res_ok: a,
          res: c
        } = P.remove(e);
        if (!a) throw new Error("Failed on mono_wasm_invoke_method_debugger_agent_with_parms");
        return c;
      }
      function G(e, t, n, r) {
        J(r), M.mono_wasm_send_dbg_command(e, t, n, H, r.length);
        const {
          res_ok: o,
          res: s
        } = P.remove(e);
        if (!o) throw new Error("Failed on mono_wasm_send_dbg_command");
        return s;
      }
      function Y() {
        const {
          res_ok: e,
          res: t
        } = P.remove(0);
        if (!e) throw new Error("Failed on mono_wasm_get_dbg_command_info");
        return t;
      }
      function Z() {}
      function X() {
        M.mono_wasm_set_is_debugger_attached(false);
      }
      function Q(e) {
        M.mono_wasm_change_debugger_log_level(e);
      }
      function K(e, t = {}) {
        if ("object" !== typeof e) throw new Error(`event must be an object, but got ${JSON.stringify(e)}`);
        if (void 0 === e.eventName) throw new Error(`event.eventName is a required parameter, in event: ${JSON.stringify(e)}`);
        if ("object" !== typeof t) throw new Error(`args must be an object, but got ${JSON.stringify(t)}`);
        console$1.debug("mono_wasm_debug_event_raised:aef14bca-5519-4dfe-b35a-f867abc123ae", JSON.stringify(e), JSON.stringify(t));
      }
      function ee() {
        return new Promise$1(e => {
          const t = setInterval(() => {
            1 == b.waitForDebugger && (clearInterval(t), e());
          }, 100);
        });
      }
      function te() {
        -1 == b.waitForDebugger && (b.waitForDebugger = 1), M.mono_wasm_set_is_debugger_attached(true);
      }
      function ne(e, t) {
        V = o.UTF8ToString(e).concat(".dll"), z = t, console$1.assert(true, `Adding an entrypoint breakpoint ${V} at method token  ${z}`);
        debugger;
      }
      function re(e, t) {
        if (e.startsWith("dotnet:array:")) {
          let e;
          if (void 0 === t.items) return e = t.map(e => e.value), e;
          if (void 0 === t.dimensionsDetails || 1 === t.dimensionsDetails.length) return e = t.items.map(e => e.value), e;
        }
        const n = {};
        return Object.keys(t).forEach(e => {
          const r = t[e];
          void 0 !== r.get ? Object.defineProperty(n, r.name, {
            get() {
              return G(r.get.id, r.get.commandSet, r.get.command, r.get.buffer);
            },
            set: function (e) {
              return q(r.set.id, r.set.commandSet, r.set.command, r.set.buffer, r.set.length, r.set.valtype, e), true;
            }
          }) : void 0 !== r.set ? Object.defineProperty(n, r.name, {
            get() {
              return r.value;
            },
            set: function (e) {
              return q(r.set.id, r.set.commandSet, r.set.command, r.set.buffer, r.set.length, r.set.valtype, e), true;
            }
          }) : n[r.name] = r.value;
        }), n;
      }
      function oe(e) {
        if (void 0 != e.arguments && !Array.isArray(e.arguments)) throw new Error(`"arguments" should be an array, but was ${e.arguments}`);
        const t = e.objectId,
          n = e.details;
        let r = {};
        if (t.startsWith("dotnet:cfo_res:")) {
          if (!(t in W)) throw new Error(`Unknown object id ${t}`);
          r = W[t];
        } else r = re(t, n);
        const o = void 0 != e.arguments ? e.arguments.map(e => JSON.stringify(e.value)) : [],
          s = `const fn = ${e.functionDeclaration}; return fn.apply(proxy, [${o}]);`,
          a = new Function("proxy", s)(r);
        if (void 0 === a) return {
          type: "undefined"
        };
        if (Object(a) !== a) return "object" == typeof a && null == a ? {
          type: typeof a,
          subtype: `${a}`,
          value: null
        } : {
          type: typeof a,
          description: `${a}`,
          value: `${a}`
        };
        if (e.returnByValue && void 0 == a.subtype) return {
          type: "object",
          value: a
        };
        if (Object.getPrototypeOf(a) == Array.prototype) {
          const e = ae(a);
          return {
            type: "object",
            subtype: "array",
            className: "Array",
            description: `Array(${a.length})`,
            objectId: e
          };
        }
        if (void 0 !== a.value || void 0 !== a.subtype) return a;
        if (a == r) return {
          type: "object",
          className: "Object",
          description: "Object",
          objectId: t
        };
        return {
          type: "object",
          className: "Object",
          description: "Object",
          objectId: ae(a)
        };
      }
      function se(e, t) {
        if (!(e in W)) throw new Error(`Could not find any object with id ${e}`);
        const n = W[e],
          r = Object.getOwnPropertyDescriptors(n);
        t.accessorPropertiesOnly && Object.keys(r).forEach(e => {
          void 0 === r[e].get && Reflect.deleteProperty(r, e);
        });
        const o = [];
        return Object.keys(r).forEach(e => {
          let t;
          const n = r[e];
          t = "object" == typeof n.value ? Object.assign({
            name: e
          }, n) : void 0 !== n.value ? {
            name: e,
            value: Object.assign({
              type: typeof n.value,
              description: "" + n.value
            }, n)
          } : void 0 !== n.get ? {
            name: e,
            get: {
              className: "Function",
              description: `get ${e} () {}`,
              type: "function"
            }
          } : {
            name: e,
            value: {
              type: "symbol",
              value: "<Unknown>",
              description: "<Unknown>"
            }
          }, o.push(t);
        }), {
          __value_as_json_string__: JSON.stringify(o)
        };
      }
      function ie(e, t = {}) {
        return se(`dotnet:cfo_res:${e}`, t);
      }
      function ae(e) {
        const t = "dotnet:cfo_res:" + F++;
        return W[t] = e, t;
      }
      function ce(e) {
        e in W && delete W[e];
      }
      function ue(e, t) {
        const n = o.UTF8ToString(t);
        if (s.logging && "function" === typeof s.logging.debugger) return s.logging.debugger(e, n), void 0;
      }
      let le = 0;
      function fe(e) {
        const t = 1 === M.mono_wasm_load_icu_data(e);
        return t && le++, t;
      }
      function _e(e) {
        return M.mono_wasm_get_icudt_name(e);
      }
      function de() {
        const e = b.config;
        let t = false;
        if (e.globalizationMode || (e.globalizationMode = "auto"), "invariant" === e.globalizationMode && (t = true), !t) if (le > 0) b.diagnosticTracing && console$1.debug("MONO_WASM: ICU data archive(s) loaded, disabling invariant mode");else {
          if ("icu" === e.globalizationMode) {
            const e = "invariant globalization mode is inactive and no ICU data archives were loaded";
            throw o.printErr(`MONO_WASM: ERROR: ${e}`), new Error(e);
          }
          b.diagnosticTracing && console$1.debug("MONO_WASM: ICU data archive(s) not loaded, using invariant globalization mode"), t = true;
        }
        t && M.mono_wasm_setenv("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1"), M.mono_wasm_setenv("DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY", "1");
      }
      function me(e) {
        null == e && (e = {}), "writeAt" in e || (e.writeAt = "System.Runtime.InteropServices.JavaScript.JavaScriptExports::StopProfile"), "sendTo" in e || (e.sendTo = "Interop/Runtime::DumpAotProfileData");
        const t = "aot:write-at-method=" + e.writeAt + ",send-to-method=" + e.sendTo;
        o.ccall("mono_wasm_load_profiler_aot", null, ["string"], [t]);
      }
      function ge(e) {
        null == e && (e = {}), "writeAt" in e || (e.writeAt = "WebAssembly.Runtime::StopProfile"), "sendTo" in e || (e.sendTo = "WebAssembly.Runtime::DumpCoverageProfileData");
        const t = "coverage:write-at-method=" + e.writeAt + ",send-to-method=" + e.sendTo;
        o.ccall("mono_wasm_load_profiler_coverage", null, ["string"], [t]);
      }
      const we = new Map(),
        he = new Map();
      let pe = 0;
      function be(e) {
        if (we.has(e)) return we.get(e);
        const t = M.mono_wasm_assembly_load(e);
        return we.set(e, t), t;
      }
      function ye(e, t, n) {
        let r = he.get(e);
        r || he.set(e, r = new Map());
        let o = r.get(t);
        return o || (o = new Map(), r.set(t, o)), o.get(n);
      }
      function ve(e, t, n, r) {
        const o = he.get(e);
        if (!o) throw new Error("internal error");
        const s = o.get(t);
        if (!s) throw new Error("internal error");
        s.set(n, r);
      }
      function Ee(e, t, n) {
        pe || (pe = M.mono_wasm_get_corlib());
        let r = ye(pe, e, t);
        if (void 0 !== r) return r;
        if (r = M.mono_wasm_assembly_find_class(pe, e, t), n && !r) throw new Error(`Failed to find corlib class ${e}.${t}`);
        return ve(pe, e, t, r), r;
      }
      //! Licensed to the .NET Foundation under one or more agreements.
      const Ae = new Map(),
        Se = [];
      function Oe(e) {
        try {
          if (0 == Ae.size) return e;
          const t = e;
          for (let n = 0; n < Se.length; n++) {
            const r = e.replace(new RegExp(Se[n], "g"), (e, ...t) => {
              const n = t.find(e => "object" == typeof e && void 0 !== e.replaceSection);
              if (void 0 === n) return e;
              const r = n.funcNum,
                o = n.replaceSection,
                s = Ae.get(Number(r));
              return void 0 === s ? e : e.replace(o, `${s} (${o})`);
            });
            if (r !== t) return r;
          }
          return t;
        } catch (t) {
          return console$1.debug(`MONO_WASM: failed to symbolicate: ${t}`), e;
        }
      }
      function xe(e) {
        let t = e;
        return t instanceof Error || (t = new Error(t)), Oe(t.stack);
      }
      function je(e, t, n, r, i) {
        const a = o.UTF8ToString(n),
          c = !!r,
          u = o.UTF8ToString(e),
          l = i,
          f = o.UTF8ToString(t),
          _ = `[MONO] ${a}`;
        if (s.logging && "function" === typeof s.logging.trace) return s.logging.trace(u, f, _, c, l), void 0;
        switch (f) {
          case "critical":
          case "error":
            console$1.error(xe(_));
            break;
          case "warning":
            console$1.log(_);
            break;
          case "message":
            console$1.log(_);
            break;
          case "info":
            console$1.info(_);
            break;
          case "debug":
            console$1.debug(_);
            break;
          default:
            console$1.log(_);
            break;
        }
      }
      let $e;
      function Ne(e, t, n) {
        const r = {
            log: t.log,
            error: t.error
          },
          o = t;
        function s(t, n, o) {
          return function (...s) {
            try {
              let r = s[0];
              if (void 0 === r) r = "undefined";else if (null === r) r = "null";else if ("function" === typeof r) r = r.toString();else if ("string" !== typeof r) try {
                r = JSON.stringify(r);
              } catch (e) {
                r = r.toString();
              }
              "string" === typeof r && "main" !== e && (r = `[${e}] ${r}`), n(o ? JSON.stringify({
                method: t,
                payload: r,
                arguments: s
              }) : [t + r, ...s.slice(1)]);
            } catch (e) {
              r.error(`proxyConsole failed: ${e}`);
            }
          };
        }
        const i = ["debug", "trace", "warn", "info", "error"];
        for (const e of i) "function" !== typeof o[e] && (o[e] = s(`console.${e}: `, t.log, false));
        const a = `${n}/console`.replace("https://", "wss://").replace("http://", "ws://");
        $e = new WebSocket(a), $e.addEventListener("open", () => {
          r.log(`browser: [${e}] Console websocket connected.`);
        }), $e.addEventListener("error", t => {
          r.error(`[${e}] websocket error: ${t}`, t);
        }), $e.addEventListener("close", t => {
          r.error(`[${e}] websocket closed: ${t}`, t);
        });
        const c = e => {
          $e.readyState === WebSocket.OPEN ? $e.send(e) : r.log(e);
        };
        for (const e of ["log", ...i]) o[e] = s(`console.${e}`, c, true);
      }
      function ke(e) {
        if (!b.mono_wasm_symbols_are_ready) {
          b.mono_wasm_symbols_are_ready = true;
          try {
            const t = undefined;
            o.FS_readFile(e, {
              flags: "r",
              encoding: "utf8"
            }).split(/[\r\n]/).forEach(e => {
              const t = e.split(/:/);
              t.length < 2 || (t[1] = t.splice(1).join(":"), Ae.set(Number(t[0]), t[1]));
            });
          } catch (t) {
            return 44 == t.errno || console$1.log(`MONO_WASM: Error loading symbol file ${e}: ${JSON.stringify(t)}`), void 0;
          }
        }
      }
      function Me(e) {
        if (!b.mono_wasm_bindings_is_ready) throw new Error("Assert failed: The runtime must be initialized.");
        const t = be(e);
        if (!t) throw new Error("Could not find assembly: " + e);
        let n = 0;
        1 == b.waitForDebugger && (n = 1);
        const r = M.mono_wasm_assembly_get_entry_point(t, n);
        if (!r) throw new Error("Could not find entry point for assembly: " + e);
        return r;
      }
      function Ie(e) {
        pc(e, false), De(1, e);
      }
      function De(e, t) {
        if (b.config.asyncFlushOnExit && 0 === e) throw _async(function () {
          return _continueIgnored(_finallyRethrows(function () {
            return _callIgnored(Ue);
          }, function (_wasThrown, _result) {
            Ce(e, t);
            return _rethrow(_wasThrown, _result);
          }));
        })(), b.ExitStatus ? new b.ExitStatus(e) : t || new Error("Stop with exit code " + e);
        Ce(e, t);
      }
      function Ce(e, t) {
        if (b.ExitStatus && (!t || t instanceof b.ExitStatus ? t = new b.ExitStatus(e) : t instanceof Error ? o.printErr(s.mono_wasm_stringify_as_error_with_stack(t)) : "string" == typeof t ? o.printErr(t) : o.printErr(JSON.stringify(t))), We(e, t), Pe(e), 0 !== e || !u) {
          if (!b.quit) throw t;
          b.quit(e, t);
        }
      }
      function Pe(e) {
        if (u && b.config.appendElementOnExit) {
          const t = _document.createElement("label");
          t.id = "tests_done", e && (t.style.background = "red"), t.innerHTML = e.toString(), _document.body.appendChild(t);
        }
      }
      function We(e, t) {
        if (b.config.logExitCode) if (0 != e && t && (t instanceof Error ? console$1.error(xe(t)) : "string" == typeof t ? console$1.error(t) : console$1.error(JSON.stringify(t))), $e) {
          const t = () => {
            0 == $e.bufferedAmount ? console$1.log("WASM EXIT " + e) : setTimeout$1(t, 100);
          };
          t();
        } else console$1.log("WASM EXIT " + e);
      }
      Se.push( /*#__PURE__*/_wrapRegExp(/at ([^:()]+:wasm\x2Dfunction\[(\d+)\]:0x[a-fA-F\d]+)((?![^)a-fA-F\d])|$)/, {
        replaceSection: 1,
        funcNum: 2
      })), Se.push( /*#__PURE__*/_wrapRegExp(/(?:WASM \[[\da-zA-Z]+\], (function #([\d]+) \(''\)))/, {
        replaceSection: 1,
        funcNum: 2
      })), Se.push( /*#__PURE__*/_wrapRegExp(/([a-z]+:\/\/[^ )]*:wasm\x2Dfunction\[(\d+)\]:0x[a-fA-F\d]+)/, {
        replaceSection: 1,
        funcNum: 2
      })), Se.push( /*#__PURE__*/_wrapRegExp(/(<[^ >]+>[.:]wasm\x2Dfunction\[([0-9]+)\])/, {
        replaceSection: 1,
        funcNum: 2
      }));
      const Fe = "function" === typeof globalThis.WeakRef;
      function Be(e) {
        return Fe ? new WeakRef(e) : {
          deref: () => e
        };
      }
      const He = "function" === typeof globalThis.FinalizationRegistry;
      let Ve;
      const ze = [],
        Le = [];
      let Je = 1;
      const qe = new Map();
      He && (Ve = new globalThis.FinalizationRegistry(rt));
      const Ge = Symbol.for("wasm js_owned_gc_handle"),
        Ye = Symbol.for("wasm cs_owned_js_handle");
      function Ze(e) {
        return 0 !== e && e !== x ? ze[e] : null;
      }
      function Xe(e) {
        return 0 !== e && e !== x ? Ze(e) : null;
      }
      function Qe(e) {
        if (e[Ye]) return e[Ye];
        const t = Le.length ? Le.pop() : Je++;
        return ze[t] = e, Object.isExtensible(e) && (e[Ye] = t), t;
      }
      function Ke(e) {
        const t = ze[e];
        if ("undefined" !== typeof t && null !== t) {
          if (globalThis === t) return;
          "undefined" !== typeof t[Ye] && (t[Ye] = void 0), ze[e] = void 0, Le.push(e);
        }
      }
      function et(e, t) {
        e[Ge] = t, He && Ve.register(e, t, e);
        const n = Be(e);
        qe.set(t, n);
      }
      function tt(e, t) {
        e && (t = e[Ge], e[Ge] = 0, He && Ve.unregister(e)), 0 !== t && qe.delete(t) && b.javaScriptExports.release_js_owned_object_by_gc_handle(t);
      }
      function nt(e) {
        const t = e[Ge];
        if (!(0 != t)) throw new Error("Assert failed: ObjectDisposedException");
        return t;
      }
      function rt(e) {
        tt(null, e);
      }
      function ot(e) {
        if (!e) return null;
        const t = qe.get(e);
        return t ? t.deref() : null;
      }
      const st = Symbol.for("wasm promise_control");
      function it(e, t) {
        let n = null;
        const r = new Promise$1(function (r, o) {
          n = {
            isDone: false,
            promise: null,
            resolve: t => {
              n.isDone || (n.isDone = true, r(t), e && e());
            },
            reject: e => {
              n.isDone || (n.isDone = true, o(e), t && t());
            }
          };
        });
        n.promise = r;
        const o = r;
        return o[st] = n, {
          promise: o,
          promise_control: n
        };
      }
      function at(e) {
        return e[st];
      }
      function ct(e) {
        return void 0 !== e[st];
      }
      function ut(e) {
        if (!ct(e)) throw new Error("Assert failed: Promise is not controllable");
      }
      const lt = ("object" === typeof Promise$1 || "function" === typeof Promise$1) && "function" === typeof Promise$1.resolve;
      function ft(e) {
        return Promise$1.resolve(e) === e || ("object" === typeof e || "function" === typeof e) && "function" === typeof e.then;
      }
      function _t(e) {
        const {
            promise: t,
            promise_control: n
          } = it();
        return e().then(e => n.resolve(e)).catch(e => n.reject(e)), t;
      }
      function dt(e) {
        const t = ot(e);
        if (!t) return;
        const n = t.promise;
        if (!!!n) throw new Error(`Assert failed: Expected Promise for GCHandle ${e}`);
        ut(n);
        at(n).reject("OperationCanceledException");
      }
      const mt = [],
        gt = 32768;
      let wt,
        ht,
        pt = null;
      function bt() {
        wt || (wt = o._malloc(gt), ht = wt);
      }
      const yt = "undefined" !== typeof BigInt && "undefined" !== typeof BigInt64Array;
      function vt() {
        bt(), mt.push(ht);
      }
      function Et() {
        if (!mt.length) throw new Error("No temp frames have been created at this point");
        ht = mt.pop();
      }
      function At(e, t, n) {
        if (!Number.isSafeInteger(e)) throw new Error(`Assert failed: Value is not an integer: ${e} (${typeof e})`);
        if (!(e >= t && e <= n)) throw new Error(`Assert failed: Overflow: value ${e} is out of ${t} ${n} range`);
      }
      function St(e, t) {
        e % 4 === 0 && t % 4 === 0 ? o.HEAP32.fill(0, e >>> 2, t >>> 2) : o.HEAP8.fill(0, e, t);
      }
      function Ot(e, t) {
        const n = !!t;
        "number" === typeof t && At(t, 0, 1), o.HEAP32[e >>> 2] = n ? 1 : 0;
      }
      function xt(e, t) {
        At(t, 0, 255), o.HEAPU8[e] = t;
      }
      function jt(e, t) {
        At(t, 0, 65535), o.HEAPU16[e >>> 1] = t;
      }
      function $t(e, t) {
        o.HEAPU32[e >>> 2] = t;
      }
      function Nt(e, t) {
        At(t, 0, 4294967295), o.HEAPU32[e >>> 2] = t;
      }
      function kt(e, t) {
        At(t, -128, 127), o.HEAP8[e] = t;
      }
      function Rt(e, t) {
        At(t, -32768, 32767), o.HEAP16[e >>> 1] = t;
      }
      function Tt(e, t) {
        o.HEAP32[e >>> 2] = t;
      }
      function Mt(e, t) {
        At(t, -2147483648, 2147483647), o.HEAP32[e >>> 2] = t;
      }
      function It(e) {
        if (0 !== e) switch (e) {
          case 1:
            throw new Error("value was not an integer");
          case 2:
            throw new Error("value out of range");
          default:
            throw new Error("unknown internal error");
        }
      }
      function Dt(e, t) {
        if (!Number.isSafeInteger(t)) throw new Error(`Assert failed: Value is not a safe integer: ${t} (${typeof t})`);
        It(M.mono_wasm_f64_to_i52(e, t));
      }
      function Ut(e, t) {
        if (!Number.isSafeInteger(t)) throw new Error(`Assert failed: Value is not a safe integer: ${t} (${typeof t})`);
        if (!(t >= 0)) throw new Error("Assert failed: Can't convert negative Number into UInt64");
        It(M.mono_wasm_f64_to_u52(e, t));
      }
      function Ct(e, t) {
        if (!yt) throw new Error("Assert failed: BigInt is not supported.");
        if (!("bigint" === typeof t)) throw new Error(`Assert failed: Value is not an bigint: ${t} (${typeof t})`);
        if (!(t >= Kt && t <= Qt)) throw new Error(`Assert failed: Overflow: value ${t} is out of ${Kt} ${Qt} range`);
        pt[e >>> 3] = t;
      }
      function Pt(e, t) {
        if (!("number" === typeof t)) throw new Error(`Assert failed: Value is not a Number: ${t} (${typeof t})`);
        o.HEAPF32[e >>> 2] = t;
      }
      function Wt(e, t) {
        if (!("number" === typeof t)) throw new Error(`Assert failed: Value is not a Number: ${t} (${typeof t})`);
        o.HEAPF64[e >>> 3] = t;
      }
      function Ft(e) {
        return !!o.HEAP32[e >>> 2];
      }
      function Bt(e) {
        return o.HEAPU8[e];
      }
      function Ht(e) {
        return o.HEAPU16[e >>> 1];
      }
      function Vt(e) {
        return o.HEAPU32[e >>> 2];
      }
      function zt(e) {
        return o.HEAP8[e];
      }
      function Lt(e) {
        return o.HEAP16[e >>> 1];
      }
      function Jt(e) {
        return o.HEAP32[e >>> 2];
      }
      function qt(e) {
        const t = M.mono_wasm_i52_to_f64(e, b._i52_error_scratch_buffer);
        return It(Jt(b._i52_error_scratch_buffer)), t;
      }
      function Gt(e) {
        const t = M.mono_wasm_u52_to_f64(e, b._i52_error_scratch_buffer);
        return It(Jt(b._i52_error_scratch_buffer)), t;
      }
      function Yt(e) {
        if (!yt) throw new Error("Assert failed: BigInt is not supported.");
        return pt[e >>> 3];
      }
      function Zt(e) {
        return o.HEAPF32[e >>> 2];
      }
      function Xt(e) {
        return o.HEAPF64[e >>> 3];
      }
      let Qt, Kt;
      function en(e) {
        yt && (Qt = BigInt("9223372036854775807"), Kt = BigInt("-9223372036854775808"), pt = new BigInt64Array(e));
      }
      function tn(e) {
        const t = o._malloc(e.length);
        return new Uint8Array(o.HEAPU8.buffer, t, e.length).set(e), t;
      }
      const nn = 8192;
      let rn = null,
        on = null,
        sn = 0;
      const an = [],
        cn = [];
      function un(e, t) {
        if (e <= 0) throw new Error("capacity >= 1");
        const n = 4 * (e |= 0),
          r = o._malloc(n);
        if (r % 4 !== 0) throw new Error("Malloc returned an unaligned offset");
        return St(r, n), new WasmRootBufferImpl(r, e, true, t);
      }
      function ln(e) {
        let t;
        if (!e) throw new Error("address must be a location in the native heap");
        return cn.length > 0 ? (t = cn.pop(), t._set_address(e)) : t = new wn(e), t;
      }
      function fn(e) {
        let t;
        if (an.length > 0) t = an.pop();else {
          const e = mn();
          t = new gn(rn, e);
        }
        if (void 0 !== e) {
          if ("number" !== typeof e) throw new Error("value must be an address in the managed heap");
          t.set(e);
        } else t.set(0);
        return t;
      }
      function _n(...e) {
        for (let t = 0; t < e.length; t++) R(e[t]) || e[t].release();
      }
      function dn(e) {
        void 0 !== e && (rn.set(e, 0), on[sn] = e, sn++);
      }
      function mn() {
        if (R(rn) || !on) {
          rn = un(nn, "js roots"), on = new Int32Array(nn), sn = nn;
          for (let e = 0; e < nn; e++) on[e] = nn - e - 1;
        }
        if (sn < 1) throw new Error("Out of scratch root space");
        const e = on[sn - 1];
        return sn--, e;
      }
      class WasmRootBufferImpl {
        constructor(e, t, n, r) {
          const o = 4 * t;
          this.__offset = e, this.__offset32 = e >>> 2, this.__count = t, this.length = t, this.__handle = M.mono_wasm_register_root(e, o, r || "noname"), this.__ownsAllocation = n;
        }
        _throw_index_out_of_range() {
          throw new Error("index out of range");
        }
        _check_in_range(e) {
          (e >= this.__count || e < 0) && this._throw_index_out_of_range();
        }
        get_address(e) {
          return this._check_in_range(e), this.__offset + 4 * e;
        }
        get_address_32(e) {
          return this._check_in_range(e), this.__offset32 + e;
        }
        get(e) {
          this._check_in_range(e);
          const t = this.get_address_32(e);
          return o.HEAPU32[t];
        }
        set(e, t) {
          const n = this.get_address(e);
          return M.mono_wasm_write_managed_pointer_unsafe(n, t), t;
        }
        copy_value_from_address(e, t) {
          const n = this.get_address(e);
          M.mono_wasm_copy_managed_pointer(n, t);
        }
        _unsafe_get(e) {
          return o.HEAPU32[this.__offset32 + e];
        }
        _unsafe_set(e, t) {
          const n = this.__offset + e;
          M.mono_wasm_write_managed_pointer_unsafe(n, t);
        }
        clear() {
          this.__offset && St(this.__offset, 4 * this.__count);
        }
        release() {
          this.__offset && this.__ownsAllocation && (M.mono_wasm_deregister_root(this.__offset), St(this.__offset, 4 * this.__count), o._free(this.__offset)), this.__handle = this.__offset = this.__count = this.__offset32 = 0;
        }
        toString() {
          return `[root buffer @${this.get_address(0)}, size ${this.__count} ]`;
        }
      }
      class gn {
        constructor(e, t) {
          this.__buffer = e, this.__index = t;
        }
        get_address() {
          return this.__buffer.get_address(this.__index);
        }
        get_address_32() {
          return this.__buffer.get_address_32(this.__index);
        }
        get address() {
          return this.__buffer.get_address(this.__index);
        }
        get() {
          return this.__buffer._unsafe_get(this.__index);
        }
        set(e) {
          const t = this.__buffer.get_address(this.__index);
          return M.mono_wasm_write_managed_pointer_unsafe(t, e), e;
        }
        copy_from(e) {
          const t = e.address,
            n = this.address;
          M.mono_wasm_copy_managed_pointer(n, t);
        }
        copy_to(e) {
          const t = this.address,
            n = e.address;
          M.mono_wasm_copy_managed_pointer(n, t);
        }
        copy_from_address(e) {
          const t = this.address;
          M.mono_wasm_copy_managed_pointer(t, e);
        }
        copy_to_address(e) {
          const t = this.address;
          M.mono_wasm_copy_managed_pointer(e, t);
        }
        get value() {
          return this.get();
        }
        set value(e) {
          this.set(e);
        }
        valueOf() {
          throw new Error("Implicit conversion of roots to pointers is no longer supported. Use .value or .address as appropriate");
        }
        clear() {
          this.set(0);
        }
        release() {
          if (!this.__buffer) throw new Error("No buffer");
          const e = 128;
          an.length > e ? (dn(this.__index), this.__buffer = null, this.__index = 0) : (this.set(0), an.push(this));
        }
        toString() {
          return `[root @${this.address}]`;
        }
      }
      class wn {
        constructor(e) {
          this.__external_address = 0, this.__external_address_32 = 0, this._set_address(e);
        }
        _set_address(e) {
          this.__external_address = e, this.__external_address_32 = e >>> 2;
        }
        get address() {
          return this.__external_address;
        }
        get_address() {
          return this.__external_address;
        }
        get_address_32() {
          return this.__external_address_32;
        }
        get() {
          return o.HEAPU32[this.__external_address_32];
        }
        set(e) {
          return M.mono_wasm_write_managed_pointer_unsafe(this.__external_address, e), e;
        }
        copy_from(e) {
          const t = e.address,
            n = this.__external_address;
          M.mono_wasm_copy_managed_pointer(n, t);
        }
        copy_to(e) {
          const t = this.__external_address,
            n = e.address;
          M.mono_wasm_copy_managed_pointer(n, t);
        }
        copy_from_address(e) {
          const t = this.__external_address;
          M.mono_wasm_copy_managed_pointer(t, e);
        }
        copy_to_address(e) {
          const t = this.__external_address;
          M.mono_wasm_copy_managed_pointer(e, t);
        }
        get value() {
          return this.get();
        }
        set value(e) {
          this.set(e);
        }
        valueOf() {
          throw new Error("Implicit conversion of roots to pointers is no longer supported. Use .value or .address as appropriate");
        }
        clear() {
          this.set(0);
        }
        release() {
          const e = 128;
          cn.length < e && cn.push(this);
        }
        toString() {
          return `[external root @${this.address}]`;
        }
      }
      const hn = new Map(),
        pn = new Map(),
        bn = Symbol.for("wasm bound_cs_function"),
        yn = Symbol.for("wasm bound_js_function"),
        vn = 16,
        En = 32;
      function Sn(e) {
        const n = o.stackAlloc(vn * e);
        if (!(n && n % 8 == 0)) throw new Error("Assert failed: Arg alignment");
        Cn(On(n, 0), wr.None);
        return Cn(On(n, 1), wr.None), n;
      }
      function On(e, t) {
        if (!e) throw new Error("Assert failed: Null args");
        return e + t * vn;
      }
      function xn(e) {
        if (!e) throw new Error("Assert failed: Null args");
        return Dn(e) !== wr.None;
      }
      function jn(e, t) {
        if (!e) throw new Error("Assert failed: Null signatures");
        return e + t * En + 8;
      }
      function $n(e) {
        if (!e) throw new Error("Assert failed: Null sig");
        return Vt(e);
      }
      function Nn(e) {
        if (!e) throw new Error("Assert failed: Null sig");
        return Vt(e + 16);
      }
      function kn(e) {
        if (!e) throw new Error("Assert failed: Null sig");
        return Vt(e + 20);
      }
      function Rn(e) {
        if (!e) throw new Error("Assert failed: Null sig");
        return Vt(e + 24);
      }
      function Tn(e) {
        if (!e) throw new Error("Assert failed: Null sig");
        return Vt(e + 28);
      }
      function Mn(e) {
        if (!e) throw new Error("Assert failed: Null signatures");
        return Jt(e + 4);
      }
      function In(e) {
        if (!e) throw new Error("Assert failed: Null signatures");
        return Jt(e);
      }
      function Dn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Vt(e + 12);
      }
      function Un(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Vt(e + 4);
      }
      function Cn(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Nt(e + 12, t);
      }
      function Pn(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Nt(e + 4, t);
      }
      function Wn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return !!Bt(e);
      }
      function Fn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Bt(e);
      }
      function Bn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Ht(e);
      }
      function Hn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Lt(e);
      }
      function Vn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Jt(e);
      }
      function zn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Vt(e);
      }
      function Ln(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Xt(e);
      }
      function Jn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Yt(e);
      }
      function qn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        const t = Xt(e);
        return new Date(t);
      }
      function Gn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Zt(e);
      }
      function Yn(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Xt(e);
      }
      function Zn(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        if (!("boolean" === typeof t)) throw new Error(`Assert failed: Value is not a Boolean: ${t} (${typeof t})`);
        xt(e, t ? 1 : 0);
      }
      function Xn(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        xt(e, t);
      }
      function Qn(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        jt(e, t);
      }
      function Kn(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Rt(e, t);
      }
      function er(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Mt(e, t);
      }
      function tr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Nt(e, t);
      }
      function nr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        if (!Number.isSafeInteger(t)) throw new Error(`Assert failed: Value is not an integer: ${t} (${typeof t})`);
        Wt(e, t);
      }
      function rr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Ct(e, t);
      }
      function or(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Wt(e, t.getTime());
      }
      function sr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Wt(e, t);
      }
      function ir(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Pt(e, t);
      }
      function ar(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Vt(e + 4);
      }
      function cr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Nt(e + 4, t);
      }
      function ur(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Vt(e + 4);
      }
      function lr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Nt(e + 4, t);
      }
      function fr(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return ln(e);
      }
      function _r(e) {
        if (!e) throw new Error("Assert failed: Null arg");
        return Jt(e + 8);
      }
      function dr(e, t) {
        if (!e) throw new Error("Assert failed: Null arg");
        Mt(e + 8, t);
      }
      class ManagedObject {
        dispose() {
          tt(this, 0);
        }
        get isDisposed() {
          return 0 === this[Ge];
        }
        toString() {
          return `CsObject(gc_handle: ${this[Ge]})`;
        }
      }
      class ManagedError extends Error {
        constructor(e) {
          super(e), this.superStack = Object.getOwnPropertyDescriptor(this, "stack"), Object.defineProperty(this, "stack", {
            get: this.getManageStack
          });
        }
        getSuperStack() {
          return this.superStack ? this.superStack.value : super.stack;
        }
        getManageStack() {
          const e = this[Ge];
          if (e) {
            const t = b.javaScriptExports.get_managed_stack_trace(e);
            if (t) return t + "\n" + this.getSuperStack();
          }
          return this.getSuperStack();
        }
        dispose() {
          tt(this, 0);
        }
        get isDisposed() {
          return 0 === this[Ge];
        }
      }
      function mr(e) {
        return e == wr.Byte ? 1 : e == wr.Int32 ? 4 : e == wr.Int52 || e == wr.Double ? 8 : e == wr.String || e == wr.Object || e == wr.JSObject ? vn : -1;
      }
      class gr {
        constructor(e, t, n) {
          this._pointer = e, this._length = t, this._viewType = n;
        }
        _unsafe_create_view() {
          const e = 0 == this._viewType ? new Uint8Array(o.HEAPU8.buffer, this._pointer, this._length) : 1 == this._viewType ? new Int32Array(o.HEAP32.buffer, this._pointer, this._length) : 2 == this._viewType ? new Float64Array(o.HEAPF64.buffer, this._pointer, this._length) : null;
          if (!e) throw new Error("NotImplementedException");
          return e;
        }
        set(e, t) {
          if (!!this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
          const n = this._unsafe_create_view();
          if (!(e && n && e.constructor === n.constructor)) throw new Error(`Assert failed: Expected ${n.constructor}`);
          n.set(e, t);
        }
        copyTo(e, t) {
          if (!!this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
          const n = this._unsafe_create_view();
          if (!(e && n && e.constructor === n.constructor)) throw new Error(`Assert failed: Expected ${n.constructor}`);
          const r = n.subarray(t);
          e.set(r);
        }
        slice(e, t) {
          if (!!this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
          return this._unsafe_create_view().slice(e, t);
        }
        get length() {
          if (!!this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
          return this._length;
        }
        get byteLength() {
          if (!!this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
          return 0 == this._viewType ? this._length : 1 == this._viewType ? this._length << 2 : 2 == this._viewType ? this._length << 3 : 0;
        }
      }
      class Span extends gr {
        constructor(e, t, n) {
          super(e, t, n), this.is_disposed = false;
        }
        dispose() {
          this.is_disposed = true;
        }
        get isDisposed() {
          return this.is_disposed;
        }
      }
      class ArraySegment extends gr {
        constructor(e, t, n) {
          super(e, t, n);
        }
        dispose() {
          tt(this, 0);
        }
        get isDisposed() {
          return 0 === this[Ge];
        }
      }
      var wr;
      (function (e) {
        e[e.None = 0] = "None", e[e.Void = 1] = "Void", e[e.Discard = 2] = "Discard", e[e.Boolean = 3] = "Boolean", e[e.Byte = 4] = "Byte", e[e.Char = 5] = "Char", e[e.Int16 = 6] = "Int16", e[e.Int32 = 7] = "Int32", e[e.Int52 = 8] = "Int52", e[e.BigInt64 = 9] = "BigInt64", e[e.Double = 10] = "Double", e[e.Single = 11] = "Single", e[e.IntPtr = 12] = "IntPtr", e[e.JSObject = 13] = "JSObject", e[e.Object = 14] = "Object", e[e.String = 15] = "String", e[e.Exception = 16] = "Exception", e[e.DateTime = 17] = "DateTime", e[e.DateTimeOffset = 18] = "DateTimeOffset", e[e.Nullable = 19] = "Nullable", e[e.Task = 20] = "Task", e[e.Array = 21] = "Array", e[e.ArraySegment = 22] = "ArraySegment", e[e.Span = 23] = "Span", e[e.Action = 24] = "Action", e[e.Function = 25] = "Function", e[e.JSException = 26] = "JSException";
      })(wr || (wr = {}));
      class hr {
        init_fields() {
          this.mono_wasm_string_decoder_buffer || (this.mono_text_decoder = null, this.mono_wasm_string_root = fn(), this.mono_wasm_string_decoder_buffer = o._malloc(12));
        }
        copy(e) {
          if (this.init_fields(), 0 === e) return null;
          this.mono_wasm_string_root.value = e;
          const t = this.copy_root(this.mono_wasm_string_root);
          return this.mono_wasm_string_root.value = 0, t;
        }
        copy_root(e) {
          if (this.init_fields(), 0 === e.value) return null;
          const t = this.mono_wasm_string_decoder_buffer + 0,
            n = this.mono_wasm_string_decoder_buffer + 4,
            r = this.mono_wasm_string_decoder_buffer + 8;
          let o;
          M.mono_wasm_string_get_data_ref(e.address, t, n, r);
          const s = Jt(n),
            i = Vt(t),
            a = Jt(r);
          if (a && (o = pr.get(e.value)), void 0 === o && (s && i ? (o = this.decode(i, i + s), a && pr.set(e.value, o)) : o = Sr), void 0 === o) throw new Error(`internal error when decoding string at location ${e.value}`);
          return o;
        }
        decode(e, t) {
          let n = "";
          if (this.mono_text_decoder) {
            const r = "undefined" !== typeof SharedArrayBuffer && o.HEAPU8.buffer instanceof SharedArrayBuffer ? o.HEAPU8.slice(e, t) : o.HEAPU8.subarray(e, t);
            n = this.mono_text_decoder.decode(r);
          } else for (let r = 0; r < t - e; r += 2) {
            const t = o.getValue(e + r, "i16");
            n += String.fromCharCode(t);
          }
          return n;
        }
      }
      const pr = new Map(),
        br = new Map();
      let yr = 0,
        vr = null,
        Er = 0;
      const Ar = new hr(),
        Sr = "";
      function Or(e) {
        return Ar.copy(e);
      }
      function xr(e) {
        return Ar.copy_root(e);
      }
      function jr(e) {
        if (0 === e.length) return Sr;
        const t = Tr(e),
          n = pr.get(t);
        if (R(n)) throw new Error("internal error: interned_string_table did not contain string after js_string_to_mono_string_interned");
        return n;
      }
      function $r(e, t, n) {
        if (!t.value) throw new Error("null pointer passed to _store_string_in_intern_table");
        const r = 8192;
        Er >= r && (vr = null), vr || (vr = un(r, "interned strings"), Er = 0);
        const o = vr,
          s = Er++;
        if (n && (M.mono_wasm_intern_string_ref(t.address), !t.value)) throw new Error("mono_wasm_intern_string_ref produced a null pointer");
        br.set(e, t.value), pr.set(t.value, e), 0 !== e.length || yr || (yr = t.value), o.copy_value_from_address(s, t.address);
      }
      function Nr(e, t) {
        let n;
        if ("symbol" === typeof e ? (n = e.description, "string" !== typeof n && (n = Symbol.keyFor(e)), "string" !== typeof n && (n = "<unknown Symbol>")) : "string" === typeof e && (n = e), "string" !== typeof n) throw new Error(`Argument to js_string_to_mono_string_interned must be a string but was ${e}`);
        if (0 === n.length && yr) return t.set(yr), void 0;
        const r = br.get(n);
        if (r) return t.set(r), void 0;
        Rr(n, t), $r(n, t, true);
      }
      function kr(e, t) {
        if (t.clear(), null !== e) if ("symbol" === typeof e) Nr(e, t);else {
          if ("string" !== typeof e) throw new Error("Expected string argument, got " + typeof e);
          if (0 === e.length) Nr(e, t);else {
            if (e.length <= 256) {
              const n = br.get(e);
              if (n) return t.set(n), void 0;
            }
            Rr(e, t);
          }
        }
      }
      function Rr(e, t) {
        const n = o._malloc(2 * (e.length + 1)),
          r = n >>> 1 | 0;
        for (let t = 0; t < e.length; t++) o.HEAP16[r + t] = e.charCodeAt(t);
        o.HEAP16[r + e.length] = 0, M.mono_wasm_string_from_utf16_ref(n, e.length, t.address), o._free(n);
      }
      function Tr(e) {
        const t = fn();
        try {
          return Nr(e, t), t.value;
        } finally {
          t.release();
        }
      }
      function Mr(e) {
        const t = fn();
        try {
          return kr(e, t), t.value;
        } finally {
          t.release();
        }
      }
      function Ir() {
        0 == pn.size && (pn.set(wr.Array, ro), pn.set(wr.Span, so), pn.set(wr.ArraySegment, io), pn.set(wr.Boolean, Ur), pn.set(wr.Byte, Cr), pn.set(wr.Char, Pr), pn.set(wr.Int16, Wr), pn.set(wr.Int32, Fr), pn.set(wr.Int52, Br), pn.set(wr.BigInt64, Hr), pn.set(wr.Double, Vr), pn.set(wr.Single, zr), pn.set(wr.IntPtr, Lr), pn.set(wr.DateTime, Jr), pn.set(wr.DateTimeOffset, qr), pn.set(wr.String, Gr), pn.set(wr.Exception, eo), pn.set(wr.JSException, eo), pn.set(wr.JSObject, to), pn.set(wr.Object, no), pn.set(wr.Task, Kr), pn.set(wr.Action, Xr), pn.set(wr.Function, Xr), pn.set(wr.None, Zr), pn.set(wr.Discard, Zr), pn.set(wr.Void, Zr));
      }
      function Dr(e, t, n, r, o, s) {
        let i = "",
          a = "",
          c = "";
        const u = "converter" + t;
        let l = "null",
          f = "null",
          _ = "null",
          d = "null",
          m = $n(e);
        if (m === wr.None || m === wr.Void) return {
          converters: i,
          call_body: c,
          marshaler_type: m
        };
        const g = Nn(e);
        if (g !== wr.None) {
          const e = pn.get(g);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${g} at ${t}`);
          m != wr.Nullable ? (d = "converter" + t + "_res", i += ", " + d, a += " " + wr[g], s[d] = e) : m = g;
        }
        const w = kn(e);
        if (w !== wr.None) {
          const e = hn.get(w);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${w} at ${t}`);
          l = "converter" + t + "_arg1", i += ", " + l, a += " " + wr[w], s[l] = e;
        }
        const h = Rn(e);
        if (h !== wr.None) {
          const e = hn.get(h);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${h} at ${t}`);
          f = "converter" + t + "_arg2", i += ", " + f, a += " " + wr[h], s[f] = e;
        }
        const p = Tn(e);
        if (p !== wr.None) {
          const e = hn.get(p);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${p} at ${t}`);
          _ = "converter" + t + "_arg3", i += ", " + _, a += " " + wr[p], s[_] = e;
        }
        const b = pn.get(m),
          y = wr[m];
        if (!(b && "function" === typeof b)) throw new Error(`Assert failed: Unknow converter for type ${y} (${m}) at ${t} `);
        return i += ", " + u, a += " " + y, s[u] = b, c = m == wr.Task ? `  ${u}(args + ${n}, ${o}, signature + ${r}, ${d}); // ${a} \n` : m == wr.Action || m == wr.Function ? `  ${u}(args + ${n}, ${o}, signature + ${r}, ${d}, ${l}, ${f}, ${f}); // ${a} \n` : `  ${u}(args + ${n}, ${o}, signature + ${r}); // ${a} \n`, {
          converters: i,
          call_body: c,
          marshaler_type: m
        };
      }
      function Ur(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Boolean), Zn(e, t));
      }
      function Cr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Byte), Xn(e, t));
      }
      function Pr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Char), Qn(e, t));
      }
      function Wr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Int16), Kn(e, t));
      }
      function Fr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Int32), er(e, t));
      }
      function Br(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Int52), nr(e, t));
      }
      function Hr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.BigInt64), rr(e, t));
      }
      function Vr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Double), sr(e, t));
      }
      function zr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.Single), ir(e, t));
      }
      function Lr(e, t) {
        null === t || void 0 === t ? Cn(e, wr.None) : (Cn(e, wr.IntPtr), tr(e, t));
      }
      function Jr(e, t) {
        if (null === t || void 0 === t) Cn(e, wr.None);else {
          if (!(t instanceof Date)) throw new Error("Assert failed: Value is not a Date");
          Cn(e, wr.DateTime), or(e, t);
        }
      }
      function qr(e, t) {
        if (null === t || void 0 === t) Cn(e, wr.None);else {
          if (!(t instanceof Date)) throw new Error("Assert failed: Value is not a Date");
          Cn(e, wr.DateTimeOffset), or(e, t);
        }
      }
      function Gr(e, t) {
        if (null === t || void 0 === t) Cn(e, wr.None);else {
          if (Cn(e, wr.String), !("string" === typeof t)) throw new Error("Assert failed: Value is not a String");
          Yr(e, t);
        }
      }
      function Yr(e, t) {
        const n = fr(e);
        try {
          kr(t, n);
        } finally {
          n.release();
        }
      }
      function Zr(e) {
        Cn(e, wr.None);
      }
      function Xr(e, t, n, r, o, s, i) {
        if (null === t || void 0 === t) return Cn(e, wr.None), void 0;
        if (!(t && t instanceof Function)) throw new Error("Assert failed: Value is not a Function");
        const a = e => {
          const n = On(e, 0),
            a = On(e, 1),
            c = On(e, 2),
            u = On(e, 3),
            l = On(e, 4);
          try {
            let e, n, f;
            o && (e = o(c)), s && (n = s(u)), i && (f = i(l));
            const _ = t(e, n, f);
            r && r(a, _);
          } catch (e) {
            eo(n, e);
          }
        };
        a[yn] = true;
        cr(e, Qe(a)), Cn(e, wr.Function);
      }
      class Qr {
        constructor(e) {
          this.promise = e;
        }
        dispose() {
          tt(this, 0);
        }
        get isDisposed() {
          return 0 === this[Ge];
        }
      }
      function Kr(e, t, n, r) {
        if (null === t || void 0 === t) return Cn(e, wr.None), void 0;
        if (!ft(t)) throw new Error("Assert failed: Value is not a Promise");
        const o = b.javaScriptExports.create_task_callback();
        lr(e, o), Cn(e, wr.Task);
        const s = new Qr(t);
        et(s, o), t.then(e => {
          b.javaScriptExports.complete_task(o, null, e, r || no), tt(s, o);
        }).catch(e => {
          b.javaScriptExports.complete_task(o, e, null, void 0), tt(s, o);
        });
      }
      function eo(e, t) {
        if (null === t || void 0 === t) Cn(e, wr.None);else if (t instanceof ManagedError) {
          Cn(e, wr.Exception);
          lr(e, nt(t));
        } else {
          if (!("object" === typeof t || "string" === typeof t)) throw new Error("Assert failed: Value is not an Error " + typeof t);
          Cn(e, wr.JSException);
          Yr(e, t.toString());
          const r = t[Ye];
          if (r) cr(e, r);else {
            cr(e, Qe(t));
          }
        }
      }
      function to(e, t) {
        if (void 0 === t || null === t) Cn(e, wr.None);else {
          if (!(void 0 === t[Ge])) throw new Error("Assert failed: JSObject proxy of ManagedObject proxy is not supported");
          if (!("function" === typeof t || "object" === typeof t)) throw new Error(`Assert failed: JSObject proxy of ${typeof t} is not supported`);
          Cn(e, wr.JSObject);
          cr(e, Qe(t));
        }
      }
      function no(e, t) {
        if (void 0 === t || null === t) Cn(e, wr.None);else {
          const n = t[Ge],
            r = typeof t;
          if (void 0 === n) {
            if ("string" === r || "symbol" === r) Cn(e, wr.String), Yr(e, t);else if ("number" === r) Cn(e, wr.Double), sr(e, t);else {
              if ("bigint" === r) throw new Error("NotImplementedException: bigint");
              if ("boolean" === r) Cn(e, wr.Boolean), Zn(e, t);else if (t instanceof Date) Cn(e, wr.DateTime), or(e, t);else if (t instanceof Error) {
                Cn(e, wr.JSException);
                cr(e, Qe(t));
              } else if (t instanceof Uint8Array) oo(e, t, wr.Byte);else if (t instanceof Float64Array) oo(e, t, wr.Double);else if (t instanceof Int32Array) oo(e, t, wr.Int32);else if (Array.isArray(t)) oo(e, t, wr.Object);else {
                if (t instanceof Int16Array || t instanceof Int8Array || t instanceof Uint8ClampedArray || t instanceof Uint16Array || t instanceof Uint32Array || t instanceof Float32Array) throw new Error("NotImplementedException: TypedArray");
                if (ft(t)) Kr(e, t);else {
                  if (t instanceof Span) throw new Error("NotImplementedException: Span");
                  if ("object" != r) throw new Error(`JSObject proxy is not supported for ${r} ${t}`);
                  {
                    const n = Qe(t);
                    Cn(e, wr.JSObject), cr(e, n);
                  }
                }
              }
            }
          } else {
            if (nt(t), t instanceof ArraySegment) throw new Error("NotImplementedException: ArraySegment");
            if (t instanceof ManagedError) Cn(e, wr.Exception), lr(e, n);else {
              if (!(t instanceof ManagedObject)) throw new Error("NotImplementedException " + r);
              Cn(e, wr.Object), lr(e, n);
            }
          }
        }
      }
      function ro(e, t, n) {
        if (!!!n) throw new Error("Assert failed: Expected valid sig parameter");
        oo(e, t, kn(n));
      }
      function oo(e, t, n) {
        if (null === t || void 0 === t) Cn(e, wr.None);else {
          const r = mr(n);
          if (!(-1 != r)) throw new Error(`Assert failed: Element type ${wr[n]} not supported`);
          const s = t.length,
            i = r * s,
            a = o._malloc(i);
          if (n == wr.String) {
            if (!Array.isArray(t)) throw new Error("Assert failed: Value is not an Array");
            St(a, i), M.mono_wasm_register_root(a, i, "marshal_array_to_cs");
            for (let e = 0; e < s; e++) {
              Gr(On(a, e), t[e]);
            }
          } else if (n == wr.Object) {
            if (!Array.isArray(t)) throw new Error("Assert failed: Value is not an Array");
            St(a, i), M.mono_wasm_register_root(a, i, "marshal_array_to_cs");
            for (let e = 0; e < s; e++) {
              no(On(a, e), t[e]);
            }
          } else if (n == wr.JSObject) {
            if (!Array.isArray(t)) throw new Error("Assert failed: Value is not an Array");
            St(a, i);
            for (let e = 0; e < s; e++) {
              to(On(a, e), t[e]);
            }
          } else if (n == wr.Byte) {
            if (!(Array.isArray(t) || t instanceof Uint8Array)) throw new Error("Assert failed: Value is not an Array or Uint8Array");
            o.HEAPU8.subarray(a, a + s).set(t);
          } else if (n == wr.Int32) {
            if (!(Array.isArray(t) || t instanceof Int32Array)) throw new Error("Assert failed: Value is not an Array or Int32Array");
            o.HEAP32.subarray(a >> 2, (a >> 2) + s).set(t);
          } else {
            if (n != wr.Double) throw new Error("not implemented");
            {
              if (!(Array.isArray(t) || t instanceof Float64Array)) throw new Error("Assert failed: Value is not an Array or Float64Array");
              o.HEAPF64.subarray(a >> 3, (a >> 3) + s).set(t);
            }
          }
          tr(e, a), Cn(e, wr.Array), Pn(e, n), dr(e, t.length);
        }
      }
      function so(e, t, n) {
        if (!!!n) throw new Error("Assert failed: Expected valid sig parameter");
        if (!!t.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
        ao(n, t._viewType), Cn(e, wr.Span), tr(e, t._pointer), dr(e, t.length);
      }
      function io(e, t, n) {
        if (!!!n) throw new Error("Assert failed: Expected valid sig parameter");
        const r = nt(t);
        if (!r) throw new Error("Assert failed: Only roundtrip of ArraySegment instance created by C#");
        ao(n, t._viewType), Cn(e, wr.ArraySegment), tr(e, t._pointer), dr(e, t.length), lr(e, r);
      }
      function ao(e, t) {
        const n = kn(e);
        if (n == wr.Byte) {
          if (!(0 == t)) throw new Error("Assert failed: Expected MemoryViewType.Byte");
        } else if (n == wr.Int32) {
          if (!(1 == t)) throw new Error("Assert failed: Expected MemoryViewType.Int32");
        } else {
          if (n != wr.Double) throw new Error(`NotImplementedException ${wr[n]} `);
          if (!(2 == t)) throw new Error("Assert failed: Expected MemoryViewType.Double");
        }
      }
      function co() {
        0 == hn.size && (hn.set(wr.Array, ko), hn.set(wr.Span, To), hn.set(wr.ArraySegment, Mo), hn.set(wr.Boolean, lo), hn.set(wr.Byte, fo), hn.set(wr.Char, _o), hn.set(wr.Int16, mo), hn.set(wr.Int32, go), hn.set(wr.Int52, wo), hn.set(wr.BigInt64, ho), hn.set(wr.Single, po), hn.set(wr.IntPtr, yo), hn.set(wr.Double, bo), hn.set(wr.String, xo), hn.set(wr.Exception, jo), hn.set(wr.JSException, jo), hn.set(wr.JSObject, $o), hn.set(wr.Object, No), hn.set(wr.DateTime, Eo), hn.set(wr.DateTimeOffset, Eo), hn.set(wr.Task, So), hn.set(wr.Action, Ao), hn.set(wr.Function, Ao), hn.set(wr.None, vo), hn.set(wr.Void, vo), hn.set(wr.Discard, vo));
      }
      function uo(e, t, n, r, o, s) {
        let i = "",
          a = "",
          c = "";
        const u = "converter" + t;
        let l = "null",
          f = "null",
          _ = "null",
          d = "null",
          m = $n(e);
        if (m === wr.None || m === wr.Void) return {
          converters: i,
          call_body: c,
          marshaler_type: m
        };
        const g = Nn(e);
        if (g !== wr.None) {
          const e = hn.get(g);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${g} at ${t}`);
          m != wr.Nullable ? (d = "converter" + t + "_res", i += ", " + d, a += " " + wr[g], s[d] = e) : m = g;
        }
        const w = kn(e);
        if (w !== wr.None) {
          const e = pn.get(w);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${w} at ${t}`);
          l = "converter" + t + "_arg1", i += ", " + l, a += " " + wr[w], s[l] = e;
        }
        const h = Rn(e);
        if (h !== wr.None) {
          const e = pn.get(h);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${h} at ${t}`);
          f = "converter" + t + "_arg2", i += ", " + f, a += " " + wr[h], s[f] = e;
        }
        const p = Tn(e);
        if (p !== wr.None) {
          const e = pn.get(p);
          if (!(e && "function" === typeof e)) throw new Error(`Assert failed: Unknow converter for type ${p} at ${t}`);
          _ = "converter" + t + "_arg3", i += ", " + _, a += " " + wr[p], s[_] = e;
        }
        const b = hn.get(m);
        if (!(b && "function" === typeof b)) throw new Error(`Assert failed: Unknow converter for type ${m} at ${t} `);
        return i += ", " + u, a += " " + wr[m], s[u] = b, c = m == wr.Task ? `  const ${o} = ${u}(args + ${n}, signature + ${r}, ${d}); // ${a} \n` : m == wr.Action || m == wr.Function ? `  const ${o} = ${u}(args + ${n}, signature + ${r}, ${d}, ${l}, ${f}, ${_}); // ${a} \n` : `  const ${o} = ${u}(args + ${n}, signature + ${r}); // ${a} \n`, {
          converters: i,
          call_body: c,
          marshaler_type: m
        };
      }
      function lo(e) {
        return Dn(e) == wr.None ? null : Wn(e);
      }
      function fo(e) {
        return Dn(e) == wr.None ? null : Fn(e);
      }
      function _o(e) {
        return Dn(e) == wr.None ? null : Bn(e);
      }
      function mo(e) {
        return Dn(e) == wr.None ? null : Hn(e);
      }
      function go(e) {
        return Dn(e) == wr.None ? null : Vn(e);
      }
      function wo(e) {
        return Dn(e) == wr.None ? null : Ln(e);
      }
      function ho(e) {
        return Dn(e) == wr.None ? null : Jn(e);
      }
      function po(e) {
        return Dn(e) == wr.None ? null : Gn(e);
      }
      function bo(e) {
        return Dn(e) == wr.None ? null : Yn(e);
      }
      function yo(e) {
        return Dn(e) == wr.None ? null : zn(e);
      }
      function vo() {
        return null;
      }
      function Eo(e) {
        return Dn(e) === wr.None ? null : qn(e);
      }
      function Ao(e, t, n, r, o, s) {
        if (Dn(e) === wr.None) return null;
        const a = ur(e);
        let c = ot(a);
        return null !== c && void 0 !== c || (c = (e, t, i) => b.javaScriptExports.call_delegate(a, e, t, i, n, r, o, s), et(c, a)), c;
      }
      function So(e, t, n) {
        const r = Dn(e);
        if (r === wr.None) return null;
        if (r !== wr.Task) {
          if (n || (n = hn.get(r)), !n) throw new Error(`Assert failed: Unknow sub_converter for type ${wr[r]} `);
          const t = n(e);
          return new Promise$1(e => e(t));
        }
        const o = ar(e);
        if (0 == o) return new Promise$1(e => e(void 0));
        const s = Ze(o);
        if (!!!s) throw new Error(`Assert failed: ERR28: promise not found for js_handle: ${o} `);
        ut(s);
        const i = at(s),
          a = i.resolve;
        return i.resolve = e => {
          const t = Dn(e);
          if (t === wr.None) return a(null), void 0;
          if (n || (n = hn.get(t)), !n) throw new Error(`Assert failed: Unknow sub_converter for type ${wr[t]}`);
          const r = n(e);
          a(r);
        }, s;
      }
      function Oo(e) {
        const t = On(e, 0),
          n = On(e, 1),
          r = On(e, 2),
          o = On(e, 3),
          s = Dn(t),
          i = Dn(o),
          a = ar(r);
        if (0 === a) {
          const {
              promise: e,
              promise_control: r
            } = it();
          if (cr(n, Qe(e)), s !== wr.None) {
            const e = jo(t);
            r.reject(e);
          } else if (i !== wr.Task) {
            const e = hn.get(i);
            if (!e) throw new Error(`Assert failed: Unknow sub_converter for type ${wr[i]} `);
            const t = e(o);
            r.resolve(t);
          }
        } else {
          const e = Ze(a);
          if (!!!e) throw new Error(`Assert failed: ERR25: promise not found for js_handle: ${a} `);
          ut(e);
          const n = at(e);
          if (s !== wr.None) {
            const e = jo(t);
            n.reject(e);
          } else i !== wr.Task && n.resolve(o);
        }
        Cn(n, wr.Task), Cn(t, wr.None);
      }
      function xo(e) {
        if (Dn(e) == wr.None) return null;
        const n = fr(e);
        try {
          const e = undefined;
          return xr(n);
        } finally {
          n.release();
        }
      }
      function jo(e) {
        const t = Dn(e);
        if (t == wr.None) return null;
        if (t == wr.JSException) {
          return Ze(ar(e));
        }
        const n = ur(e);
        let r = ot(n);
        if (null === r || void 0 === r) {
          const t = xo(e);
          r = new ManagedError(t), et(r, n);
        }
        return r;
      }
      function $o(e) {
        if (Dn(e) == wr.None) return null;
        return Ze(ar(e));
      }
      function No(e) {
        const t = Dn(e);
        if (t == wr.None) return null;
        if (t == wr.JSObject) {
          return Ze(ar(e));
        }
        if (t == wr.Array) {
          return Ro(e, Un(e));
        }
        if (t == wr.Object) {
          const t = ur(e);
          if (0 === t) return null;
          let n = ot(t);
          return n || (n = new ManagedObject(), et(n, t)), n;
        }
        const n = hn.get(t);
        if (!n) throw new Error(`Assert failed: Unknow converter for type ${wr[t]}`);
        return n(e);
      }
      function ko(e, t) {
        if (!!!t) throw new Error("Assert failed: Expected valid sig parameter");
        return Ro(e, kn(t));
      }
      function Ro(e, t) {
        if (Dn(e) == wr.None) return null;
        if (!(-1 != mr(t))) throw new Error(`Assert failed: Element type ${wr[t]} not supported`);
        const s = zn(e),
          i = _r(e);
        let a = null;
        if (t == wr.String) {
          a = new Array(i);
          for (let e = 0; e < i; e++) {
            const t = On(s, e);
            a[e] = xo(t);
          }
          M.mono_wasm_deregister_root(s);
        } else if (t == wr.Object) {
          a = new Array(i);
          for (let e = 0; e < i; e++) {
            const t = On(s, e);
            a[e] = No(t);
          }
          M.mono_wasm_deregister_root(s);
        } else if (t == wr.JSObject) {
          a = new Array(i);
          for (let e = 0; e < i; e++) {
            const t = On(s, e);
            a[e] = $o(t);
          }
        } else if (t == wr.Byte) {
          a = o.HEAPU8.subarray(s, s + i).slice();
        } else if (t == wr.Int32) {
          a = o.HEAP32.subarray(s >> 2, (s >> 2) + i).slice();
        } else {
          if (t != wr.Double) throw new Error(`NotImplementedException ${wr[t]} `);
          {
            a = o.HEAPF64.subarray(s >> 3, (s >> 3) + i).slice();
          }
        }
        return o._free(s), a;
      }
      function To(e, t) {
        if (!!!t) throw new Error("Assert failed: Expected valid sig parameter");
        const n = kn(t),
          r = zn(e),
          o = _r(e);
        let s = null;
        if (n == wr.Byte) s = new Span(r, o, 0);else if (n == wr.Int32) s = new Span(r, o, 1);else {
          if (n != wr.Double) throw new Error(`NotImplementedException ${wr[n]} `);
          s = new Span(r, o, 2);
        }
        return s;
      }
      function Mo(e, t) {
        if (!!!t) throw new Error("Assert failed: Expected valid sig parameter");
        const n = kn(t),
          r = zn(e),
          o = _r(e);
        let s = null;
        if (n == wr.Byte) s = new ArraySegment(r, o, 0);else if (n == wr.Int32) s = new ArraySegment(r, o, 1);else {
          if (n != wr.Double) throw new Error(`NotImplementedException ${wr[n]} `);
          s = new ArraySegment(r, o, 2);
        }
        return et(s, ur(e)), s;
      }
      let Io, Do;
      const Uo = {};
      function Co(e) {
        Io = e.mono, Do = e.binding;
      }
      const Po = Symbol.for("wasm type");
      function Wo(e) {
        return new Promise$1(t => setTimeout$1(t, e));
      }
      const Fo = it(),
        Bo = it();
      let Ho = 0,
        Vo = 0,
        zo = 0,
        Lo = 0;
      const Jo = [],
        qo = Object.create(null);
      let Go = 0,
        Yo;
      const Zo = {
          "js-module-threads": true
        },
        Xo = {
          dotnetwasm: true
        },
        Qo = {
          "js-module-threads": true,
          dotnetwasm: true
        };
      function Ko(e) {
        var t;
        const n = null === (t = b.config.assets) || void 0 === t ? void 0 : t.find(t => t.behavior == e);
        if (!n) throw new Error(`Assert failed: Can't find asset for ${e}`);
        return n.resolvedUrl || (n.resolvedUrl = os(n, "")), n;
      }
      function os(e, t) {
        if (!(null !== t && void 0 !== t)) throw new Error(`Assert failed: sourcePrefix must be provided for ${e.name}`);
        let n;
        const r = b.config.assemblyRootFolder;
        if (e.resolvedUrl) n = e.resolvedUrl;else {
          if ("" === t) {
            if ("assembly" === e.behavior || "pdb" === e.behavior) n = r ? r + "/" + e.name : e.name;else if ("resource" === e.behavior) {
              const t = "" !== e.culture ? `${e.culture}/${e.name}` : e.name;
              n = r ? r + "/" + t : t;
            } else n = e.name;
          } else n = t + e.name;
          n = b.locateFile(n);
        }
        if (!(n && "string" == typeof n)) throw new Error("Assert failed: attemptUrl need to be path or url string");
        return n;
      }
      function ss(e) {
        try {
          if ("function" === typeof o.downloadResource) {
            const t = o.downloadResource(e);
            if (t) return t;
          }
          const t = {};
          e.hash && (t.integrity = e.hash);
          const n = b.fetch_like(e.resolvedUrl, t);
          return {
            name: e.name,
            url: e.resolvedUrl,
            response: n
          };
        } catch (t) {
          const n = {
            ok: false,
            url: e.resolvedUrl,
            status: 500,
            statusText: "ERR29: " + t,
            arrayBuffer: () => {
              throw t;
            },
            json: () => {
              throw t;
            }
          };
          return {
            name: e.name,
            url: e.resolvedUrl,
            response: Promise$1.resolve(n)
          };
        }
      }
      function is(e, t, n) {
        b.diagnosticTracing && console$1.debug(`MONO_WASM: Loaded:${e.name} as ${e.behavior} size ${n.length} from ${t}`);
        const r = "string" === typeof e.virtualPath ? e.virtualPath : e.name;
        let s = null;
        switch (e.behavior) {
          case "dotnetwasm":
          case "js-module-threads":
            break;
          case "resource":
          case "assembly":
          case "pdb":
            Jo.push({
              url: t,
              file: r
            });
          case "heap":
          case "icu":
            s = tn(n), qo[r] = [s, n.length];
            break;
          case "vfs":
            {
              const e = r.lastIndexOf("/");
              let t = e > 0 ? r.substr(0, e) : null,
                s = e > 0 ? r.substr(e + 1) : r;
              s.startsWith("/") && (s = s.substr(1)), t ? (b.diagnosticTracing && console$1.debug(`MONO_WASM: Creating directory '${t}'`), o.FS_createPath("/", t, true, true)) : t = "/", b.diagnosticTracing && console$1.debug(`MONO_WASM: Creating file '${s}' in directory '${t}'`), cs(n, t) || o.FS_createDataFile(t, s, n, true, true, true);
              break;
            }
          default:
            throw new Error(`Unrecognized asset behavior:${e.behavior}, for asset ${e.name}`);
        }
        if ("assembly" === e.behavior) {
          if (!M.mono_wasm_add_assembly(r, s, n.length)) {
            const e = Jo.findIndex(e => e.file == r);
            Jo.splice(e, 1);
          }
        } else "icu" === e.behavior ? fe(s) || o.printErr(`MONO_WASM: Error loading ICU asset ${e.name}`) : "resource" === e.behavior && M.mono_wasm_add_satellite_assembly(r, e.culture, s, n.length);
        ++Vo;
      }
      function cs(e, t) {
        if (e.length < 8) return false;
        const n = new DataView(e.buffer);
        if (1651270004 != n.getUint32(0, true)) return false;
        const s = n.getUint32(4, true);
        if (0 == s || e.length < s + 8) return false;
        let i;
        try {
          const t = o.UTF8ArrayToString(e, 8, s);
          if (i = JSON.parse(t), !(i instanceof Array)) return false;
        } catch (e) {
          return false;
        }
        e = e.slice(s + 8);
        const a = new Set();
        i.filter(e => {
          const t = e[0],
            n = t.lastIndexOf("/"),
            r = t.slice(0, n + 1);
          a.add(r);
        }), a.forEach(e => {
          o.FS_createPath(t, e, true, true);
        });
        for (const n of i) {
          const r = n[0],
            s = n[1],
            i = e.slice(0, s);
          o.FS_createDataFile(t, r, i, true, true), e = e.slice(s);
        }
        return true;
      }
      function ls() {
        return Io.loaded_files;
      }
      let fs, _s;
      function ds(e) {
        const t = o;
        "undefined" === typeof globalThis.performance && (globalThis.performance = gs), "undefined" === typeof globalThis.URL && (globalThis.URL = class e {
          constructor(e) {
            this.url = e;
          }
          toString() {
            return this.url;
          }
        });
        const n = t.imports = o.imports || {},
          r = e => t => {
            const n = o.imports[t];
            return n || e(t);
          };
        n.require ? b.requirePromise = e.requirePromise = Promise$1.resolve(r(n.require)) : e.require ? b.requirePromise = e.requirePromise = Promise$1.resolve(r(e.require)) : e.requirePromise ? b.requirePromise = e.requirePromise.then(e => r(e)) : b.requirePromise = e.requirePromise = Promise$1.resolve(r(e => {
          throw new Error(`Please provide Module.imports.${e} or Module.imports.require`);
        })), b.scriptDirectory = e.scriptDirectory = bs(e), t.mainScriptUrlOrBlob = e.scriptUrl, t.__locateFile === t.locateFile ? t.locateFile = b.locateFile = e => Es(e) ? e : b.scriptDirectory + e : b.locateFile = t.locateFile, n.fetch ? e.fetch = b.fetch_like = n.fetch : e.fetch = b.fetch_like = ws, e.noExitRuntime = u;
        const s = e.updateGlobalBufferAndViews;
        e.updateGlobalBufferAndViews = e => {
          s(e), en(e);
        };
      }
      const gs = {
        now: function () {
          return Date.now();
        }
      };
      function hs(e) {
        return e.replace(/\\/g, "/").replace(/[?#].*/, "");
      }
      function ps(e) {
        return e.slice(0, e.lastIndexOf("/")) + "/";
      }
      function bs(e) {
        return l && (e.scriptUrl = self.location.href), e.scriptUrl || (e.scriptUrl = "./dotnet.js"), e.scriptUrl = hs(e.scriptUrl), ps(e.scriptUrl);
      }
      const ys = /^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,
        vs = /[a-zA-Z]:[\\/]/;
      function Es(e) {
        return a || c ? e.startsWith("/") || e.startsWith("\\") || -1 !== e.indexOf("///") || vs.test(e) : ys.test(e);
      }
      function As(e, t, n, r, o, s) {
        const i = ln(e),
          a = ln(t),
          c = ln(s);
        try {
          const e = In(n);
          if (!(1 === e)) throw new Error(`Assert failed: Signature version ${e} mismatch.`);
          const t = xr(i),
            o = xr(a);
          b.diagnosticTracing && console$1.debug(`MONO_WASM: Binding [JSImport] ${t} from ${o}`);
          const s = xs(t, o),
            u = Mn(n),
            l = {
              fn: s,
              marshal_exception_to_cs: eo,
              signature: n
            },
            f = "_bound_js_" + t.replace(/\./g, "_");
          let _ = `//# sourceURL=https://dotnet.generated.invalid/${f} \n`,
            d = "",
            m = "",
            g = "";
          for (let e = 0; e < u; e++) {
            const t = (e + 2) * vn,
              r = (e + 2) * En + 8,
              o = `arg${e}`,
              s = jn(n, e + 2),
              {
                converters: i,
                call_body: a
              } = uo(s, e + 2, t, r, o, l);
            d += i, m += a, g += "" === g ? o : `, ${o}`;
          }
          const {
            converters: w,
            call_body: h,
            marshaler_type: p
          } = Dr(jn(n, 1), 1, vn, 40, "js_result", l);
          d += w, _ += `const { signature, fn, marshal_exception_to_cs ${d} } = closure;\n`, _ += `return function ${f} (args) { try {\n`, _ += m, p === wr.Void ? (_ += `  const js_result = fn(${g});\n`, _ += `  if (js_result !== undefined) throw new Error('Function ${t} returned unexpected value, C# signature is void');\n`) : p === wr.Discard ? _ += `  fn(${g});\n` : (_ += `  const js_result = fn(${g});\n`, _ += h);
          for (let e = 0; e < u; e++) {
            const t = jn(n, e + 2),
              r = undefined;
            if ($n(t) == wr.Span) {
              const t = undefined;
              _ += `  ${`arg${e}`}.dispose();\n`;
            }
          }
          _ += "} catch (ex) {\n", _ += "  marshal_exception_to_cs(args, ex);\n", _ += "}}";
          const y = undefined,
            v = new Function("closure", _)(l);
          v[yn] = true;
          const E = undefined;
          Mt(r, Qe(v));
        } catch (e) {
          Us(o, e, c);
        } finally {
          c.release(), i.release();
        }
      }
      function Ss(e, t) {
        const n = Ze(e);
        if (!(n && "function" === typeof n && n[yn])) throw new Error(`Assert failed: Bound function handle expected ${e}`);
        n(t);
      }
      function Os(e, t) {
        Ms.set(e, t), b.diagnosticTracing && console$1.debug(`MONO_WASM: added module imports '${e}'`);
      }
      function xs(e, t) {
        if (!(e && "string" === typeof e)) throw new Error("Assert failed: function_name must be string");
        let n = i;
        const r = e.split(".");
        if (t) {
          if (n = Ms.get(t), !n) throw new Error(`Assert failed: ES6 module ${t} was not imported yet, please call JSHost.Import() first.`);
        } else "INTERNAL" === r[0] ? (n = s, r.shift()) : "globalThis" === r[0] && (n = globalThis, r.shift());
        for (let t = 0; t < r.length - 1; t++) {
          const o = r[t],
            s = n[o];
          if (!s) throw new Error(`Assert failed: ${o} not found while looking up ${e}`);
          n = s;
        }
        const a = n[r[r.length - 1]];
        if (!("function" === typeof a)) throw new Error(`Assert failed: ${e} must be a Function but was ${typeof a}`);
        return a.bind(n);
      }
      function js(e, t, n) {
        if (!e) throw new Error("Assert failed: Null reference");
        e[t] = n;
      }
      function $s(e, t) {
        if (!e) throw new Error("Assert failed: Null reference");
        return e[t];
      }
      function Ns(e, t) {
        if (!e) throw new Error("Assert failed: Null reference");
        return t in e;
      }
      function ks(e, t) {
        if (!e) throw new Error("Assert failed: Null reference");
        return typeof e[t];
      }
      function Rs() {
        return globalThis;
      }
      const Ts = new Map(),
        Ms = new Map();
      function Ds(e, t) {
        let n = "unknown exception";
        if (t) {
          n = t.toString();
          const e = t.stack;
          e && (e.startsWith(n) ? n = e : n += "\n" + e), n = Oe(n);
        }
        return e && o.setValue(e, 1, "i32"), n;
      }
      function Us(e, t, n) {
        kr(Ds(e, t), n);
      }
      const Cs = new Map();
      function Ps(e, t, n, r, s) {
        const i = ln(e),
          a = ln(s),
          c = o;
        try {
          const e = In(n);
          if (!(1 === e)) throw new Error(`Assert failed: Signature version ${e} mismatch.`);
          const r = Mn(n),
            o = xr(i);
          if (!o) throw new Error("Assert failed: fully_qualified_name must be string");
          b.diagnosticTracing && console$1.debug(`MONO_WASM: Binding [JSExport] ${o}`);
          const {
              assembly: s,
              namespace: u,
              classname: l,
              methodname: f
            } = Vs(o),
            _ = be(s);
          if (!_) throw new Error("Could not find assembly: " + s);
          const d = M.mono_wasm_assembly_find_class(_, u, l);
          if (!d) throw new Error("Could not find class: " + u + ":" + l + " in assembly " + s);
          const m = `__Wrapper_${f}_${t}`,
            g = M.mono_wasm_assembly_find_method(d, m, -1);
          if (!g) throw new Error(`Could not find method: ${m} in ${d} [${s}]`);
          const w = {
              method: g,
              signature: n,
              stackSave: c.stackSave,
              stackRestore: c.stackRestore,
              alloc_stack_frame: Sn,
              invoke_method_and_handle_exception: Ws
            },
            h = "_bound_cs_" + `${u}_${l}_${f}`.replace(/\./g, "_").replace(/\//g, "_");
          let p = `//# sourceURL=https://dotnet.generated.invalid/${h} \n`,
            y = "",
            v = "";
          for (let e = 0; e < r; e++) {
            const t = (e + 2) * vn,
              r = (e + 2) * En + 8,
              o = jn(n, e + 2),
              {
                converters: s,
                call_body: i
              } = Dr(o, e + 2, t, r, `arguments[${e}]`, w);
            v += s, y += i;
          }
          const {
            converters: E,
            call_body: A,
            marshaler_type: S
          } = uo(jn(n, 1), 1, vn, 40, "js_result", w);
          v += E, p += `const { method, signature, stackSave, stackRestore,  alloc_stack_frame, invoke_method_and_handle_exception ${v} } = closure;\n`, p += `return function ${h} () {\n`, p += "const sp = stackSave();\n", p += "try {\n", p += `  const args = alloc_stack_frame(${r + 2});\n`, p += y, p += "  invoke_method_and_handle_exception(method, args);\n", S !== wr.Void && S !== wr.Discard && (p += A), S !== wr.Void && S !== wr.Discard && (p += "  return js_result;\n"), p += "} finally {\n", p += "  stackRestore(sp);\n", p += "}}";
          const O = undefined,
            x = new Function("closure", p)(w);
          x[bn] = true, Cs.set(o, x), Bs(s, u, l, f, t, x);
        } catch (e) {
          o.printErr(e.toString()), Us(r, e, a);
        } finally {
          a.release(), i.release();
        }
      }
      function Ws(e, t) {
        const n = M.mono_wasm_invoke_method_bound(e, t);
        if (n) throw new Error("ERR24: Unexpected error: " + Or(n));
        if (xn(t)) {
          throw jo(On(t, 0));
        }
      }
      const Fs = new Map();
      function Bs(e, t, n, r, o, s) {
        const i = `${t}.${n}`.replace(/\//g, ".").split(".");
        let a,
          c = Fs.get(e);
        c || (c = {}, Fs.set(e, c), Fs.set(e + ".dll", c)), a = c;
        for (let e = 0; e < i.length; e++) {
          const t = i[e];
          if ("" != t) {
            let e = a[t];
            if ("undefined" === typeof e && (e = {}, a[t] = e), !e) throw new Error(`Assert failed: ${t} not found while looking up ${n}`);
            a = e;
          }
        }
        a[r] || (a[r] = s), a[`${r}.${o}`] = s;
      }
      function Vs(e) {
        const t = e.substring(e.indexOf("[") + 1, e.indexOf("]")).trim(),
          n = (e = e.substring(e.indexOf("]") + 1).trim()).substring(e.indexOf(":") + 1);
        let r = "",
          o = e = e.substring(0, e.indexOf(":")).trim();
        if (-1 != e.indexOf(".")) {
          const t = e.lastIndexOf(".");
          r = e.substring(0, t), o = e.substring(t + 1);
        }
        if (!t.trim()) throw new Error("No assembly name specified " + e);
        if (!o.trim()) throw new Error("No class name specified " + e);
        if (!n.trim()) throw new Error("No method name specified " + e);
        return {
          assembly: t,
          namespace: r,
          classname: o,
          methodname: n
        };
      }
      function zs() {
        const e = o,
          t = "System.Runtime.InteropServices.JavaScript";
        if (b.runtime_interop_module = M.mono_wasm_assembly_load(t), !b.runtime_interop_module) throw "Can't find bindings module assembly: " + t;
        if (b.runtime_interop_namespace = "System.Runtime.InteropServices.JavaScript", b.runtime_interop_exports_classname = "JavaScriptExports", b.runtime_interop_exports_class = M.mono_wasm_assembly_find_class(b.runtime_interop_module, b.runtime_interop_namespace, b.runtime_interop_exports_classname), !b.runtime_interop_exports_class) throw "Can't find " + b.runtime_interop_namespace + "." + b.runtime_interop_exports_classname + " class";
        const n = M.mono_wasm_assembly_find_method(b.runtime_interop_exports_class, "InstallSynchronizationContext", -1),
          r = Ls("CallEntrypoint");
        if (!r) throw new Error("Assert failed: Can't find CallEntrypoint method");
        const s = Ls("ReleaseJSOwnedObjectByGCHandle");
        if (!s) throw new Error("Assert failed: Can't find ReleaseJSOwnedObjectByGCHandle method");
        const i = Ls("CreateTaskCallback");
        if (!i) throw new Error("Assert failed: Can't find CreateTaskCallback method");
        const a = Ls("CompleteTask");
        if (!a) throw new Error("Assert failed: Can't find CompleteTask method");
        const c = Ls("CallDelegate");
        if (!c) throw new Error("Assert failed: Can't find CallDelegate method");
        const u = Ls("GetManagedStackTrace");
        if (!u) throw new Error("Assert failed: Can't find GetManagedStackTrace method");
        b.javaScriptExports.call_entry_point = (t, n) => {
          const o = e.stackSave();
          try {
            const s = Sn(4),
              i = On(s, 1),
              a = On(s, 2),
              c = On(s, 3);
            Lr(a, t), n && 0 == n.length && (n = void 0), oo(c, n, wr.String), Ws(r, s);
            const u = So(i, void 0, go);
            return u || Promise$1.resolve(0);
          } finally {
            e.stackRestore(o);
          }
        }, b.javaScriptExports.release_js_owned_object_by_gc_handle = t => {
          if (!t) throw new Error("Assert failed: Must be valid gc_handle");
          const n = e.stackSave();
          try {
            const r = Sn(3),
              o = On(r, 2);
            Cn(o, wr.Object), lr(o, t), Ws(s, r);
          } finally {
            e.stackRestore(n);
          }
        }, b.javaScriptExports.create_task_callback = () => {
          const t = e.stackSave();
          try {
            const n = Sn(2);
            Ws(i, n);
            const r = undefined;
            return ur(On(n, 1));
          } finally {
            e.stackRestore(t);
          }
        }, b.javaScriptExports.complete_task = (t, n, r, o) => {
          const s = e.stackSave();
          try {
            const i = Sn(5),
              c = On(i, 2);
            Cn(c, wr.Object), lr(c, t);
            const u = On(i, 3);
            if (n) eo(u, n);else {
              Cn(u, wr.None);
              const e = On(i, 4);
              if (!o) throw new Error("Assert failed: res_converter missing");
              o(e, r);
            }
            Ws(a, i);
          } finally {
            e.stackRestore(s);
          }
        }, b.javaScriptExports.call_delegate = (t, n, r, o, s, i, a, u) => {
          const l = e.stackSave();
          try {
            const f = Sn(6),
              _ = On(f, 2);
            if (Cn(_, wr.Object), lr(_, t), i) {
              const e = undefined;
              i(On(f, 3), n);
            }
            if (a) {
              const e = undefined;
              a(On(f, 4), r);
            }
            if (u) {
              const e = undefined;
              u(On(f, 5), o);
            }
            if (Ws(c, f), s) {
              const e = undefined;
              return s(On(f, 1));
            }
          } finally {
            e.stackRestore(l);
          }
        }, b.javaScriptExports.get_managed_stack_trace = t => {
          const n = e.stackSave();
          try {
            const r = Sn(3),
              o = On(r, 2);
            Cn(o, wr.Exception), lr(o, t), Ws(u, r);
            const s = undefined;
            return xo(On(r, 1));
          } finally {
            e.stackRestore(n);
          }
        }, n && (b.javaScriptExports.install_synchronization_context = () => {
          const t = e.stackSave();
          try {
            const r = Sn(2);
            Ws(n, r);
          } finally {
            e.stackRestore(t);
          }
        }, f || b.javaScriptExports.install_synchronization_context());
      }
      function Ls(e) {
        const t = M.mono_wasm_assembly_find_method(b.runtime_interop_exports_class, e, -1);
        if (!t) throw "Can't find method " + b.runtime_interop_namespace + "." + b.runtime_interop_exports_classname + "." + e;
        return t;
      }
      function Js(e, t, n, r, o, s, i) {
        const a = ln(i);
        try {
          const s = undefined;
          Qs(qs(e, t, n, r, o), a, true);
        } catch (e) {
          Us(s, String(e), a);
        } finally {
          a.release();
        }
      }
      function qs(e, t, n, r, o) {
        let s = null;
        switch (o) {
          case 5:
            s = new Int8Array(n - t);
            break;
          case 6:
            s = new Uint8Array(n - t);
            break;
          case 7:
            s = new Int16Array(n - t);
            break;
          case 8:
            s = new Uint16Array(n - t);
            break;
          case 9:
            s = new Int32Array(n - t);
            break;
          case 10:
            s = new Uint32Array(n - t);
            break;
          case 13:
            s = new Float32Array(n - t);
            break;
          case 14:
            s = new Float64Array(n - t);
            break;
          case 15:
            s = new Uint8ClampedArray(n - t);
            break;
          default:
            throw new Error("Unknown array type " + o);
        }
        return Gs(s, e, t, n, r), s;
      }
      function Gs(e, t, n, r, s) {
        if (Ys(e) && e.BYTES_PER_ELEMENT) {
          if (s !== e.BYTES_PER_ELEMENT) throw new Error("Inconsistent element sizes: TypedArray.BYTES_PER_ELEMENT '" + e.BYTES_PER_ELEMENT + "' sizeof managed element: '" + s + "'");
          let i = (r - n) * s;
          const a = e.length * e.BYTES_PER_ELEMENT;
          i > a && (i = a);
          const u = n * s;
          return new Uint8Array(e.buffer, 0, i).set(o.HEAPU8.subarray(t + u, t + u + i)), i;
        }
        throw new Error("Object '" + e + "' is not a typed array");
      }
      function Ys(e) {
        return "undefined" !== typeof SharedArrayBuffer ? e.buffer instanceof ArrayBuffer || e.buffer instanceof SharedArrayBuffer : e.buffer instanceof ArrayBuffer;
      }
      function Zs(e, t, n) {
        switch (true) {
          case null === t:
          case "undefined" === typeof t:
            return n.clear(), void 0;
          case "symbol" === typeof t:
          case "string" === typeof t:
            return Xi._create_uri_ref(t, n.address), void 0;
          default:
            return Ks(e, t, n), void 0;
        }
      }
      function Xs(e) {
        const t = fn();
        try {
          return Qs(e, t, false), t.value;
        } finally {
          t.release();
        }
      }
      function Qs(e, t, n) {
        if (R(t)) throw new Error("Expected (value, WasmRoot, boolean)");
        switch (true) {
          case null === e:
          case "undefined" === typeof e:
            return t.clear(), void 0;
          case "number" === typeof e:
            {
              let n;
              return (0 | e) === e ? (Tt(Uo._box_buffer, e), n = Uo._class_int32) : e >>> 0 === e ? ($t(Uo._box_buffer, e), n = Uo._class_uint32) : (Wt(Uo._box_buffer, e), n = Uo._class_double), M.mono_wasm_box_primitive_ref(n, Uo._box_buffer, 8, t.address), void 0;
            }
          case "string" === typeof e:
            return kr(e, t), void 0;
          case "symbol" === typeof e:
            return Nr(e, t), void 0;
          case "boolean" === typeof e:
            return Ot(Uo._box_buffer, e), M.mono_wasm_box_primitive_ref(Uo._class_boolean, Uo._box_buffer, 4, t.address), void 0;
          case true === ft(e):
            return si(e, t), void 0;
          case "Date" === e.constructor.name:
            return Xi._create_date_time_ref(e.getTime(), t.address), void 0;
          default:
            return Ks(n, e, t), void 0;
        }
      }
      function Ks(e, t, n) {
        if (n.clear(), null !== t && "undefined" !== typeof t) {
          if (void 0 !== t[Ge]) {
            return Ei(nt(t), n.address), void 0;
          }
          if (t[Ye] && (ai(t[Ye], e, n.address), n.value || delete t[Ye]), !n.value) {
            const r = t[Po],
              o = "undefined" === typeof r ? 0 : r,
              s = Qe(t);
            Xi._create_cs_owned_proxy_ref(s, o, e ? 1 : 0, n.address);
          }
        }
      }
      function ei(e) {
        const t = e.length * e.BYTES_PER_ELEMENT,
          n = o._malloc(t),
          r = new Uint8Array(o.HEAPU8.buffer, n, t);
        return r.set(new Uint8Array(e.buffer, e.byteOffset, t)), r;
      }
      function ti(e, t) {
        if (!Ys(e) || !e.BYTES_PER_ELEMENT) throw new Error("Object '" + e + "' is not a typed array");
        {
          const n = e[Po],
            r = ei(e);
          M.mono_wasm_typed_array_new_ref(r.byteOffset, e.length, e.BYTES_PER_ELEMENT, n, t.address), o._free(r.byteOffset);
        }
      }
      function ni(e) {
        const t = fn();
        try {
          return ti(e, t), t.value;
        } finally {
          t.release();
        }
      }
      function ri(e, t, n) {
        if ("number" !== typeof e) throw new Error(`Expected numeric value for enum argument, got '${e}'`);
        return 0 | e;
      }
      function oi(e, t, n) {
        const r = fn();
        t ? M.mono_wasm_string_array_new_ref(e.length, r.address) : M.mono_wasm_obj_array_new_ref(e.length, r.address);
        const o = fn(0),
          s = r.address,
          i = o.address;
        try {
          for (let r = 0; r < e.length; ++r) {
            let a = e[r];
            t && (a = a.toString()), Qs(a, o, n), M.mono_wasm_obj_array_set_ref(s, r, i);
          }
          return r.value;
        } finally {
          _n(r, o);
        }
      }
      function si(e, t) {
        if (!e) return t.clear(), null;
        const n = Qe(e),
          r = Xi._create_tcs(),
          o = {
            tcs_gc_handle: r
          };
        return et(o, r), e.then(e => {
          Xi._set_tcs_result_ref(r, e);
        }, e => {
          Xi._set_tcs_failure(r, e ? e.toString() : "");
        }).finally(() => {
          Ke(n), tt(o, r);
        }), Xi._get_tcs_task_ref(r, t.address), {
          then_js_handle: n
        };
      }
      function ii(e, t, n) {
        const r = ln(n);
        try {
          const n = Ze(e);
          if (R(n)) return Us(t, "ERR06: Invalid JS object handle '" + e + "'", r), void 0;
          ti(n, r);
        } catch (e) {
          Us(t, String(e), r);
        } finally {
          r.release();
        }
      }
      function ai(e, t, n) {
        if (0 === e || e === x) return Tt(n, 0), void 0;
        Xi._get_cs_owned_object_by_js_handle_ref(e, t ? 1 : 0, n);
      }
      const ci = Symbol.for("wasm delegate_invoke");
      function ui(e) {
        if (0 === e) return;
        const t = fn(e);
        try {
          return di(t);
        } finally {
          t.release();
        }
      }
      function li(e) {
        return Ze(Xi._get_cs_owned_object_js_handle_ref(e.address, 0));
      }
      function fi(e, t, n, r) {
        switch (t) {
          case 0:
            return null;
          case 26:
          case 27:
            throw new Error("int64 not available");
          case 3:
          case 29:
            return xr(e);
          case 4:
            throw new Error("no idea on how to unbox value types");
          case 5:
            return hi(e);
          case 6:
            return yi(e);
          case 7:
            return vi(e);
          case 10:
          case 11:
          case 12:
          case 13:
          case 14:
          case 15:
          case 16:
          case 17:
          case 18:
            throw new Error("Marshaling of primitive arrays are not supported.");
          case 20:
            return new Date(Xi._get_date_value_ref(e.address));
          case 21:
            return Xi._object_to_string_ref(e.address);
          case 22:
            return Xi._object_to_string_ref(e.address);
          case 23:
            return li(e);
          case 30:
            return;
          default:
            throw new Error(`no idea on how to unbox object of MarshalType ${t} at offset ${e.value} (root address is ${e.address})`);
        }
      }
      function _i(e, t, n) {
        if (t >= 512) throw new Error(`Got marshaling error ${t} when attempting to unbox object at address ${e.value} (root located at ${e.address})`);
        let r = 0;
        if ((4 === t || 7 == t) && (r = Vt(n), r < 1024)) throw new Error(`Got invalid MonoType ${r} for object at address ${e.value} (root located at ${e.address})`);
        return fi(e, t);
      }
      function di(e) {
        if (0 === e.value) return;
        const t = Uo._unbox_buffer,
          n = M.mono_wasm_try_unbox_primitive_and_get_type_ref(e.address, t, Uo._unbox_buffer_size);
        switch (n) {
          case 1:
            return Jt(t);
          case 25:
            return Vt(t);
          case 32:
            return Vt(t);
          case 24:
            return Zt(t);
          case 2:
            return Xt(t);
          case 8:
            return 0 !== Jt(t);
          case 28:
            return String.fromCharCode(Jt(t));
          case 0:
            return null;
          default:
            return _i(e, n, t);
        }
      }
      function mi(e) {
        if (0 === e) return null;
        const t = fn(e);
        try {
          return wi(t);
        } finally {
          t.release();
        }
      }
      function gi(e) {
        return Xi._is_simple_array_ref(e.address);
      }
      function wi(e) {
        if (0 === e.value) return null;
        const t = e.address,
          n = fn(),
          r = n.address;
        try {
          const o = M.mono_wasm_array_length(e.value),
            s = new Array(o);
          for (let e = 0; e < o; ++e) M.mono_wasm_array_get_ref(t, e, r), gi(n) ? s[e] = wi(n) : s[e] = di(n);
          return s;
        } finally {
          n.release();
        }
      }
      function hi(e) {
        if (0 === e.value) return null;
        return pi(Xi._get_js_owned_object_gc_handle_ref(e.address));
      }
      function pi(e) {
        let t = ot(e);
        if (t) nt(t);else {
          t = function (...e) {
            nt(t);
            return (0, t[ci])(...e);
          };
          const n = fn();
          Ei(e, n.address);
          try {
            if ("undefined" === typeof t[ci]) {
              const r = M.mono_wasm_get_delegate_invoke_ref(n.address),
                o = undefined,
                s = Li(r, Yi(r, n), true);
              if (t[ci] = s.bind({
                this_arg_gc_handle: e
              }), !t[ci]) throw new Error("System.Delegate Invoke method can not be resolved.");
            }
          } finally {
            n.release();
          }
          et(t, e);
        }
        return t;
      }
      function bi(e, t, n, r) {
        const o = ln(t),
          s = ln(e),
          i = ln(r);
        try {
          const e = xr(s);
          if (!e) return Us(n, "Invalid name @" + s.value, i), void 0;
          const t = globalThis[e];
          if (null === t || "undefined" === typeof t) return Us(n, "JavaScript host object '" + e + "' not found.", i), void 0;
          try {
            const e = wi(o),
              n = function (e, t) {
                let n = [];
                n[0] = e, t && (n = n.concat(t));
                const r = undefined,
                  o = undefined;
                return new (e.bind.apply(e, n))();
              },
              r = n(t, e),
              s = undefined;
            Qs(Qe(r), i, false);
          } catch (e) {
            return Us(n, e, i), void 0;
          }
        } finally {
          i.release(), o.release(), s.release();
        }
      }
      function yi(e) {
        if (0 === e.value) return null;
        if (!lt) throw new Error("Promises are not supported thus 'System.Threading.Tasks.Task' can not work in this context.");
        const t = Xi._get_js_owned_object_gc_handle_ref(e.address);
        let n = ot(t);
        if (!n) {
          const r = () => tt(n, t),
            {
              promise: o,
              promise_control: s
            } = it(r, r);
          n = o, Xi._setup_js_cont_ref(e.address, s), et(n, t);
        }
        return n;
      }
      function vi(e) {
        if (0 === e.value) return null;
        const t = Xi._try_get_cs_owned_object_js_handle_ref(e.address, 0);
        if (t) {
          if (t === x) throw new Error("Cannot access a disposed JSObject at " + e.value);
          return Ze(t);
        }
        const n = Xi._get_js_owned_object_gc_handle_ref(e.address);
        let r = ot(n);
        return R(r) && (r = new ManagedObject(), et(r, n)), r;
      }
      function Ei(e, t) {
        if (!e) return Tt(t, 0), void 0;
        Xi._get_js_owned_object_by_gc_handle_ref(e, t);
      }
      const Ai = new Map();
      function Si(e, t, n, r, s, i, a) {
        Et(), o.stackRestore(a), "object" === typeof r && (r.clear(), null !== t && null === t.scratchResultRoot ? t.scratchResultRoot = r : r.release()), "object" === typeof s && (s.clear(), null !== t && null === t.scratchExceptionRoot ? t.scratchExceptionRoot = s : s.release()), "object" === typeof i && (i.clear(), null !== t && null === t.scratchThisArgRoot ? t.scratchThisArgRoot = i : i.release());
      }
      function Oi(e, t) {
        if (!b.mono_wasm_bindings_is_ready) throw new Error("Assert failed: The runtime must be initialized.");
        const n = `${e}-${t}`;
        let r = Ai.get(n);
        if (void 0 === r) {
          const o = Gi(e);
          "undefined" === typeof t && (t = Yi(o, void 0)), r = Li(o, t, false, e), Ai.set(n, r);
        }
        return r;
      }
      function xi(e, t) {
        const n = Me(e);
        "string" !== typeof t && (t = Yi(n, void 0));
        const r = Li(n, t, false, "_" + e + "__entrypoint");
        return _async(function (...e) {
          return e.length > 0 && Array.isArray(e[0]) && (e[0] = oi(e[0], true, false)), r(...e);
        });
      }
      function ji(e, t, n) {
        if (!b.mono_wasm_bindings_is_ready) throw new Error("Assert failed: The runtime must be initialized.");
        return t || (t = [[]]), xi(e, n)(...t);
      }
      function $i(e, t, n, r, o) {
        const s = ln(n),
          i = ln(t),
          a = ln(o);
        try {
          const t = xr(i);
          if (!t || "string" !== typeof t) return Us(r, "ERR12: Invalid method name object @" + i.value, a), void 0;
          const n = Xe(e);
          if (R(n)) return Us(r, "ERR13: Invalid JS object handle '" + e + "' while invoking '" + t + "'", a), void 0;
          const o = wi(s);
          try {
            const e = n[t];
            if ("undefined" === typeof e) throw new Error("Method: '" + t + "' not found for: '" + Object.prototype.toString.call(n) + "'");
            const r = undefined;
            Qs(e.apply(n, o), a, true);
          } catch (e) {
            Us(r, e, a);
          }
        } finally {
          s.release(), i.release(), a.release();
        }
      }
      function Ni(e, t, n, r) {
        const o = ln(t),
          s = ln(r);
        try {
          const t = xr(o);
          if (!t) return Us(n, "Invalid property name object '" + o.value + "'", s), void 0;
          const r = Ze(e);
          if (R(r)) return Us(n, "ERR01: Invalid JS object handle '" + e + "' while geting '" + t + "'", s), void 0;
          const i = undefined;
          Qs(r[t], s, true);
        } catch (e) {
          Us(n, e, s);
        } finally {
          s.release(), o.release();
        }
      }
      function ki(e, t, n, r, o, s, i) {
        const a = ln(n),
          c = ln(t),
          u = ln(i);
        try {
          const n = xr(c);
          if (!n) return Us(s, "Invalid property name object '" + t + "'", u), void 0;
          const i = Ze(e);
          if (R(i)) return Us(s, "ERR02: Invalid JS object handle '" + e + "' while setting '" + n + "'", u), void 0;
          let l = false;
          const f = di(a);
          if (r) i[n] = f, l = true;else {
            if (l = false, !r && !Object.prototype.hasOwnProperty.call(i, n)) return Qs(false, u, false), void 0;
            true === o ? Object.prototype.hasOwnProperty.call(i, n) && (i[n] = f, l = true) : (i[n] = f, l = true);
          }
          Qs(l, u, false);
        } catch (e) {
          Us(s, e, u);
        } finally {
          u.release(), c.release(), a.release();
        }
      }
      function Ri(e, t, n, r) {
        const o = ln(r);
        try {
          const r = Ze(e);
          if (R(r)) return Us(n, "ERR03: Invalid JS object handle '" + e + "' while getting [" + t + "]", o), void 0;
          const s = undefined;
          Qs(r[t], o, true);
        } catch (e) {
          Us(n, e, o);
        } finally {
          o.release();
        }
      }
      function Ti(e, t, n, r, o) {
        const s = ln(n),
          i = ln(o);
        try {
          const n = Ze(e);
          if (R(n)) return Us(r, "ERR04: Invalid JS object handle '" + e + "' while setting [" + t + "]", i), void 0;
          const o = di(s);
          n[t] = o, i.clear();
        } catch (e) {
          Us(r, e, i);
        } finally {
          i.release(), s.release();
        }
      }
      function Mi(e, t, n) {
        const r = ln(e),
          i = ln(n);
        try {
          const e = xr(r);
          let n;
          if (n = e ? "Module" == e ? o : "INTERNAL" == e ? s : globalThis[e] : globalThis, null === n || void 0 === typeof n) return Us(t, "Global object '" + e + "' not found.", i), void 0;
          Qs(n, i, true);
        } catch (e) {
          Us(t, e, i);
        } finally {
          i.release(), r.release();
        }
      }
      function Ii(e, t, n, r, o) {
        try {
          const e = globalThis.Blazor;
          if (!e) throw new Error("The blazor.webassembly.js library is not loaded.");
          return e._internal.invokeJSFromDotNet(t, n, r, o);
        } catch (t) {
          const n = t.message + "\n" + t.stack,
            r = fn();
          return kr(n, r), r.copy_to_address(e), r.release(), 0;
        }
      }
      const Di = /[^A-Za-z0-9_$]/g,
        Ui = new Map(),
        Ci = new Map(),
        Pi = new Map();
      function Wi(e, t, n, r) {
        let o = null,
          s = null,
          i = null;
        if (r) {
          i = Object.keys(r), s = new Array(i.length);
          for (let e = 0, t = i.length; e < t; e++) s[e] = r[i[e]];
        }
        return o = Fi(e, t, n, i).apply(null, s), o;
      }
      function Fi(e, t, n, r) {
        const o = '"use strict";\r\n';
        let s = "",
          i = "";
        e ? (s = "//# sourceURL=https://dotnet.generated.invalid/" + e + "\r\n", i = e) : i = "unnamed";
        let a = "function " + i + "(" + t.join(", ") + ") {\r\n" + n + "\r\n};\r\n";
        const c = /\r(\n?)/g;
        a = s + o + a.replace(c, "\r\n    ") + `    return ${i};\r\n`;
        let u = null,
          l = null;
        return l = r ? r.concat([a]) : [a], u = Function.apply(Function, l), u;
      }
      function Bi() {
        const e = Ui;
        e.set("m", {
          steps: [{}],
          size: 0
        }), e.set("s", {
          steps: [{
            convert_root: kr.bind(Do)
          }],
          size: 0,
          needs_root: true
        }), e.set("S", {
          steps: [{
            convert_root: Nr.bind(Do)
          }],
          size: 0,
          needs_root: true
        }), e.set("o", {
          steps: [{
            convert_root: Qs.bind(Do)
          }],
          size: 0,
          needs_root: true
        }), e.set("u", {
          steps: [{
            convert_root: Zs.bind(Do, false)
          }],
          size: 0,
          needs_root: true
        }), e.set("R", {
          steps: [{
            convert_root: Qs.bind(Do),
            byref: true
          }],
          size: 0,
          needs_root: true
        }), e.set("j", {
          steps: [{
            convert: ri.bind(Do),
            indirect: "i32"
          }],
          size: 8
        }), e.set("b", {
          steps: [{
            indirect: "bool"
          }],
          size: 8
        }), e.set("i", {
          steps: [{
            indirect: "i32"
          }],
          size: 8
        }), e.set("I", {
          steps: [{
            indirect: "u32"
          }],
          size: 8
        }), e.set("l", {
          steps: [{
            indirect: "i52"
          }],
          size: 8
        }), e.set("L", {
          steps: [{
            indirect: "u52"
          }],
          size: 8
        }), e.set("f", {
          steps: [{
            indirect: "float"
          }],
          size: 8
        }), e.set("d", {
          steps: [{
            indirect: "double"
          }],
          size: 8
        });
      }
      function Hi(e) {
        const t = [];
        let n = 0,
          r = false,
          o = false,
          s = -1,
          i = false;
        for (let a = 0; a < e.length; ++a) {
          const c = e[a];
          if (a === e.length - 1) {
            if ("!" === c) {
              r = true;
              continue;
            }
            "m" === c && (o = true, s = e.length - 1);
          } else if ("!" === c) throw new Error("! must be at the end of the signature");
          const u = Ui.get(c);
          if (!u) throw new Error("Unknown parameter type " + c);
          const l = Object.create(u.steps[0]);
          l.size = u.size, u.needs_root && (i = true), l.needs_root = u.needs_root, l.key = c, t.push(l), n += u.size;
        }
        return {
          steps: t,
          size: n,
          args_marshal: e,
          is_result_definitely_unmarshaled: r,
          is_result_possibly_unmarshaled: o,
          result_unmarshaled_if_argc: s,
          needs_root_buffer: i
        };
      }
      function Vi(e) {
        let t = Ci.get(e);
        return t || (t = Hi(e), Ci.set(e, t)), t;
      }
      function zi(e) {
        const t = Vi(e);
        if ("string" !== typeof t.args_marshal) throw new Error("Corrupt converter for '" + e + "'");
        if (t.compiled_function && t.compiled_variadic_function) return t;
        const n = e.replace("!", "_result_unmarshaled");
        t.name = n;
        let r = [],
          s = ["method"];
        const i = {
          Module: o,
          setI32: Mt,
          setU32: Nt,
          setF32: Pt,
          setF64: Wt,
          setU52: Ut,
          setI52: Dt,
          setB32: Ot,
          setI32_unchecked: Tt,
          setU32_unchecked: $t,
          scratchValueRoot: t.scratchValueRoot,
          stackAlloc: o.stackAlloc,
          _zero_region: St
        };
        let a = 0;
        const c = 8 * ((4 * e.length + 7) / 8 | 0),
          u = t.size + 4 * e.length + 16;
        r.push("if (!method) throw new Error('no method provided');", `const buffer = stackAlloc(${u});`, `_zero_region(buffer, ${u});`, `const indirectStart = buffer + ${c};`, "");
        for (let e = 0; e < t.steps.length; e++) {
          const n = t.steps[e],
            c = "step" + e,
            u = "value" + e,
            l = "arg" + e,
            f = `(indirectStart + ${a})`;
          if (s.push(l), n.convert_root) {
            if (!!n.indirect) throw new Error("Assert failed: converter step cannot both be rooted and indirect");
            if (!t.scratchValueRoot) {
              const e = o.stackSave();
              t.scratchValueRoot = ln(e), i.scratchValueRoot = t.scratchValueRoot;
            }
            i[c] = n.convert_root, r.push(`scratchValueRoot._set_address(${f});`), r.push(`${c}(${l}, scratchValueRoot);`), n.byref ? r.push(`let ${u} = ${f};`) : r.push(`let ${u} = scratchValueRoot.value;`);
          } else n.convert ? (i[c] = n.convert, r.push(`let ${u} = ${c}(${l}, method, ${e});`)) : r.push(`let ${u} = ${l};`);
          if (n.needs_root && !n.convert_root && (r.push("if (!rootBuffer) throw new Error('no root buffer provided');"), r.push(`rootBuffer.set (${e}, ${u});`)), n.indirect) {
            switch (n.indirect) {
              case "bool":
                r.push(`setB32(${f}, ${u});`);
                break;
              case "u32":
                r.push(`setU32(${f}, ${u});`);
                break;
              case "i32":
                r.push(`setI32(${f}, ${u});`);
                break;
              case "float":
                r.push(`setF32(${f}, ${u});`);
                break;
              case "double":
                r.push(`setF64(${f}, ${u});`);
                break;
              case "i52":
                r.push(`setI52(${f}, ${u});`);
                break;
              case "u52":
                r.push(`setU52(${f}, ${u});`);
                break;
              default:
                throw new Error("Unimplemented indirect type: " + n.indirect);
            }
            r.push(`setU32_unchecked(buffer + (${e} * 4), ${f});`), a += n.size;
          } else r.push(`setU32_unchecked(buffer + (${e} * 4), ${u});`), a += 4;
          r.push("");
        }
        r.push("return buffer;");
        let l = r.join("\r\n"),
          f = null,
          _ = null;
        try {
          f = Wi("converter_" + n, s, l, i), t.compiled_function = f;
        } catch (e) {
          throw t.compiled_function = null, console$1.log("MONO_WASM: compiling converter failed for", l, "with error", e), e;
        }
        s = ["method", "args"];
        const d = {
          converter: f
        };
        r = ["return converter(", "  method,"];
        for (let e = 0; e < t.steps.length; e++) r.push("  args[" + e + (e == t.steps.length - 1 ? "]" : "], "));
        r.push(");"), l = r.join("\r\n");
        try {
          _ = Wi("variadic_converter_" + n, s, l, d), t.compiled_variadic_function = _;
        } catch (e) {
          throw t.compiled_variadic_function = null, console$1.log("MONO_WASM: compiling converter failed for", l, "with error", e), e;
        }
        return t.scratchRootBuffer = null, t.scratchBuffer = 0, t;
      }
      function Li(e, t, n, r) {
        if ("string" !== typeof t) throw new Error("args_marshal argument invalid, expected string");
        const s = `managed_${e}_${t}`;
        let i = Pi.get(s);
        if (i) return i;
        r || (r = s);
        let a = null;
        "string" === typeof t && (a = zi(t));
        const c = 128,
          u = o._malloc(c),
          l = {
            method: e,
            converter: a,
            scratchRootBuffer: null,
            scratchBuffer: 0,
            scratchResultRoot: fn(),
            scratchExceptionRoot: fn(),
            scratchThisArgRoot: fn()
          },
          f = {
            Module: o,
            mono_wasm_new_root: fn,
            get_js_owned_object_by_gc_handle_ref: Ei,
            _create_temp_frame: vt,
            _handle_exception_for_call: Ji,
            _teardown_after_call: Si,
            mono_wasm_try_unbox_primitive_and_get_type_ref: M.mono_wasm_try_unbox_primitive_and_get_type_ref,
            _unbox_mono_obj_root_with_known_nonprimitive_type: _i,
            invoke_method_ref: M.mono_wasm_invoke_method_ref,
            method: e,
            token: l,
            unbox_buffer: u,
            unbox_buffer_size: c,
            getB32: Ft,
            getI32: Jt,
            getU32: Vt,
            getF32: Zt,
            getF64: Xt,
            stackSave: o.stackSave
          },
          _ = a ? "converter_" + a.name : "";
        a && (f[_] = a);
        const d = [],
          m = ["_create_temp_frame();", "let resultRoot = token.scratchResultRoot, exceptionRoot = token.scratchExceptionRoot, thisArgRoot = token.scratchThisArgRoot , sp = stackSave();", "token.scratchResultRoot = null;", "token.scratchExceptionRoot = null;", "token.scratchThisArgRoot = null;", "if (resultRoot === null)", "\tresultRoot = mono_wasm_new_root ();", "if (exceptionRoot === null)", "\texceptionRoot = mono_wasm_new_root ();", "if (thisArgRoot === null)", "\tthisArgRoot = mono_wasm_new_root ();", ""];
        if (a) {
          m.push(`let buffer = ${_}.compiled_function(`, "    method,");
          for (let e = 0; e < a.steps.length; e++) {
            const t = "arg" + e;
            d.push(t), m.push("    " + t + (e == a.steps.length - 1 ? "" : ", "));
          }
          m.push(");");
        } else m.push("let buffer = 0;");
        if (a && a.is_result_definitely_unmarshaled ? m.push("let is_result_marshaled = false;") : a && a.is_result_possibly_unmarshaled ? m.push(`let is_result_marshaled = arguments.length !== ${a.result_unmarshaled_if_argc};`) : m.push("let is_result_marshaled = true;"), m.push("", "", ""), n ? (m.push("get_js_owned_object_by_gc_handle_ref(this.this_arg_gc_handle, thisArgRoot.address);"), m.push("invoke_method_ref (method, thisArgRoot.address, buffer, exceptionRoot.address, resultRoot.address);")) : m.push("invoke_method_ref (method, 0, buffer, exceptionRoot.address, resultRoot.address);"), m.push(`_handle_exception_for_call (${_}, token, buffer, resultRoot, exceptionRoot, thisArgRoot, sp);`, "", "let resultPtr = resultRoot.value, result = undefined;"), !a) throw new Error("No converter");
        a.is_result_possibly_unmarshaled && m.push("if (!is_result_marshaled) "), (a.is_result_definitely_unmarshaled || a.is_result_possibly_unmarshaled) && m.push("    result = resultPtr;"), a.is_result_definitely_unmarshaled || m.push("if (is_result_marshaled) {", "    let resultType = mono_wasm_try_unbox_primitive_and_get_type_ref (resultRoot.address, unbox_buffer, unbox_buffer_size);", "    switch (resultType) {", "    case 1:", "        result = getI32(unbox_buffer); break;", "    case 32:", "    case 25:", "        result = getU32(unbox_buffer); break;", "    case 24:", "        result = getF32(unbox_buffer); break;", "    case 2:", "        result = getF64(unbox_buffer); break;", "    case 8:", "        result = getB32(unbox_buffer); break;", "    case 28:", "        result = String.fromCharCode(getI32(unbox_buffer)); break;", "    case 0:", "        result = null; break;", "    default:", "        result = _unbox_mono_obj_root_with_known_nonprimitive_type (resultRoot, resultType, unbox_buffer); break;", "    }", "}");
        let g = r.replace(Di, "_");
        n && (g += "_this"), m.push(`_teardown_after_call (${_}, token, buffer, resultRoot, exceptionRoot, thisArgRoot, sp);`, "return result;");
        return i = Wi(g, d, m.join("\r\n"), f), Pi.set(s, i), i;
      }
      function Ji(e, t, n, r, o, s, i) {
        const a = qi(r, o);
        if (a) throw Si(e, t, n, r, o, s, i), a;
      }
      function qi(e, t) {
        if (0 === t.value) return null;
        const n = xr(e);
        return new Error(n);
      }
      function Gi(e) {
        const {
            assembly: t,
            namespace: n,
            classname: r,
            methodname: o
          } = Vs(e),
          s = M.mono_wasm_assembly_load(t);
        if (!s) throw new Error("Could not find assembly: " + t);
        const i = M.mono_wasm_assembly_find_class(s, n, r);
        if (!i) throw new Error("Could not find class: " + n + ":" + r + " in assembly " + t);
        const a = M.mono_wasm_assembly_find_method(i, o, -1);
        if (!a) throw new Error("Could not find method: " + o);
        return a;
      }
      function Yi(e, t) {
        return Xi._get_call_sig_ref(e, t ? t.address : Uo._null_root.address);
      }
      const Zi = [[true, "_get_cs_owned_object_by_js_handle_ref", "GetCSOwnedObjectByJSHandleRef", "iim"], [true, "_get_cs_owned_object_js_handle_ref", "GetCSOwnedObjectJSHandleRef", "mi"], [true, "_try_get_cs_owned_object_js_handle_ref", "TryGetCSOwnedObjectJSHandleRef", "mi"], [false, "_create_cs_owned_proxy_ref", "CreateCSOwnedProxyRef", "iiim"], [false, "_get_js_owned_object_by_gc_handle_ref", "GetJSOwnedObjectByGCHandleRef", "im"], [true, "_get_js_owned_object_gc_handle_ref", "GetJSOwnedObjectGCHandleRef", "m"], [true, "_create_tcs", "CreateTaskSource", ""], [true, "_set_tcs_result_ref", "SetTaskSourceResultRef", "iR"], [true, "_set_tcs_failure", "SetTaskSourceFailure", "is"], [true, "_get_tcs_task_ref", "GetTaskSourceTaskRef", "im"], [true, "_setup_js_cont_ref", "SetupJSContinuationRef", "mo"], [true, "_object_to_string_ref", "ObjectToStringRef", "m"], [true, "_get_date_value_ref", "GetDateValueRef", "m"], [true, "_create_date_time_ref", "CreateDateTimeRef", "dm"], [true, "_create_uri_ref", "CreateUriRef", "sm"], [true, "_is_simple_array_ref", "IsSimpleArrayRef", "m"], [false, "_get_call_sig_ref", "GetCallSignatureRef", "im"]],
        Xi = {};
      function Qi(e, t) {
        return Li(ea(e), t, false, "BINDINGS_" + e);
      }
      function Ki() {
        Object.prototype[Po] = 0, Array.prototype[Po] = 1, ArrayBuffer.prototype[Po] = 2, DataView.prototype[Po] = 3, Function.prototype[Po] = 4, Uint8Array.prototype[Po] = 11;
        const e = 65536;
        if (Uo._unbox_buffer_size = 65536, Uo._box_buffer = o._malloc(e), Uo._unbox_buffer = o._malloc(Uo._unbox_buffer_size), Uo._class_int32 = Ee("System", "Int32"), Uo._class_uint32 = Ee("System", "UInt32"), Uo._class_double = Ee("System", "Double"), Uo._class_boolean = Ee("System", "Boolean"), Uo._null_root = fn(), Bi(), Uo.runtime_legacy_exports_classname = "LegacyExports", Uo.runtime_legacy_exports_class = M.mono_wasm_assembly_find_class(b.runtime_interop_module, b.runtime_interop_namespace, Uo.runtime_legacy_exports_classname), !Uo.runtime_legacy_exports_class) throw "Can't find " + b.runtime_interop_namespace + "." + b.runtime_interop_exports_classname + " class";
        for (const e of Zi) {
          const t = Xi,
            [n, r, o, s] = e;
          if (n) t[r] = function (...e) {
            const n = Qi(o, s);
            return t[r] = n, n(...e);
          };else {
            const e = Qi(o, s);
            t[r] = e;
          }
        }
      }
      function ea(e) {
        const t = M.mono_wasm_assembly_find_method(Uo.runtime_legacy_exports_class, e, -1);
        if (!t) throw "Can't find method " + b.runtime_interop_namespace + "." + Uo.runtime_legacy_exports_classname + "." + e;
        return t;
      }
      function ta() {
        return "undefined" !== typeof Response && "body" in Response.prototype && "function" === typeof ReadableStream;
      }
      function na() {
        return new AbortController();
      }
      function ra(e) {
        e.abort();
      }
      function oa(e) {
        e.__abort_controller.abort(), e.__reader && e.__reader.cancel();
      }
      function sa(e, t, n, r, o, s, i, a) {
        return ia(e, t, n, r, o, s, new Span(i, a, 0).slice());
      }
      function ia(e, t, n, r, o, s, i) {
        if (!(e && "string" === typeof e)) throw new Error("Assert failed: expected url string");
        if (!(t && n && Array.isArray(t) && Array.isArray(n) && t.length === n.length)) throw new Error("Assert failed: expected headerNames and headerValues arrays");
        if (!(r && o && Array.isArray(r) && Array.isArray(o) && r.length === o.length)) throw new Error("Assert failed: expected headerNames and headerValues arrays");
        const a = new Headers();
        for (let e = 0; e < t.length; e++) a.append(t[e], n[e]);
        const c = {
          body: i,
          headers: a,
          signal: s.signal
        };
        for (let e = 0; e < r.length; e++) c[r[e]] = o[e];
        return _t(_async(function () {
          return _await$1(fetch(e, c), function (t) {
            return t.__abort_controller = s, t;
          });
        }));
      }
      function aa(e) {
        if (!e.__headerNames) {
          e.__headerNames = [], e.__headerValues = [];
          const t = e.headers.entries();
          for (const n of t) e.__headerNames.push(n[0]), e.__headerValues.push(n[1]);
        }
      }
      function ca(e) {
        return aa(e), e.__headerNames;
      }
      function ua(e) {
        return aa(e), e.__headerValues;
      }
      function la(e) {
        return _t(_async(function () {
          return _await$1(e.arrayBuffer(), function (t) {
            return e.__buffer = t, e.__source_offset = 0, t.byteLength;
          });
        }));
      }
      function fa(e, t) {
        if (!e.__buffer) throw new Error("Assert failed: expected resoved arrayBuffer");
        if (e.__source_offset == e.__buffer.byteLength) return 0;
        const n = new Uint8Array(e.__buffer, e.__source_offset);
        t.set(n, 0);
        const r = Math.min(t.byteLength, n.byteLength);
        return e.__source_offset += r, r;
      }
      let da = 0,
        ma = false,
        ga = 0,
        wa;
      if (globalThis.navigator) {
        const e = globalThis.navigator;
        e.userAgentData && e.userAgentData.brands ? ma = e.userAgentData.brands.some(e => "Chromium" == e.brand) : e.userAgent && (ma = e.userAgent.includes("Chrome"));
      }
      function ha() {
        for (; ga > 0;) --ga, M.mono_background_exec();
      }
      function pa() {
        if (!ma) return;
        const e = new Date().valueOf(),
          t = e + 36e4,
          r = 1e3;
        for (let n = Math.max(e + 1e3, da); n < t; n += r) {
          setTimeout$1(() => {
            M.mono_set_timeout_exec(), ga++, ha();
          }, n - e);
        }
        da = t;
      }
      function ba() {
        ++ga, setTimeout$1(ha, 0);
      }
      function ya(e) {
        function mono_wasm_set_timeout_exec() {
          M.mono_set_timeout_exec();
        }
        wa && (clearTimeout$1(wa), wa = void 0), wa = setTimeout$1(mono_wasm_set_timeout_exec, e);
      }
      class va {
        constructor() {
          this.queue = [], this.offset = 0;
        }
        getLength() {
          return this.queue.length - this.offset;
        }
        isEmpty() {
          return 0 == this.queue.length;
        }
        enqueue(e) {
          this.queue.push(e);
        }
        dequeue() {
          if (0 === this.queue.length) return;
          const e = this.queue[this.offset];
          return this.queue[this.offset] = null, 2 * ++this.offset >= this.queue.length && (this.queue = this.queue.slice(this.offset), this.offset = 0), e;
        }
        peek() {
          return this.queue.length > 0 ? this.queue[this.offset] : void 0;
        }
        drain(e) {
          for (; this.getLength();) {
            e(this.dequeue());
          }
        }
      }
      const Ea = Symbol.for("wasm ws_pending_send_buffer"),
        Aa = Symbol.for("wasm ws_pending_send_buffer_offset"),
        Sa = Symbol.for("wasm ws_pending_send_buffer_type"),
        Oa = Symbol.for("wasm ws_pending_receive_event_queue"),
        xa = Symbol.for("wasm ws_pending_receive_promise_queue"),
        ja = Symbol.for("wasm ws_pending_open_promise"),
        $a = Symbol.for("wasm ws_pending_close_promises"),
        Na = Symbol.for("wasm ws_pending_send_promises"),
        ka = Symbol.for("wasm ws_is_aborted"),
        Ra = Symbol.for("wasm ws_receive_status_ptr");
      let Ta = false,
        Ma,
        Ia;
      const Da = 65536,
        Ua = new Uint8Array();
      function Ca(e, t, n, r) {
        if (!(e && "string" === typeof e)) throw new Error("Assert failed: ERR12: Invalid uri " + typeof e);
        const o = new globalThis.WebSocket(e, t || void 0),
          {
            promise_control: s
          } = it();
        o[Oa] = new va(), o[xa] = new va(), o[ja] = s, o[Na] = [], o[$a] = [], o[Ra] = n, o.binaryType = "arraybuffer";
        const i = () => {
            o[ka] || (s.resolve(o), pa());
          },
          a = e => {
            o[ka] || (za(o, e), pa());
          },
          c = e => {
            if (o.removeEventListener("message", a), o[ka]) return;
            r && r(e.code, e.reason), s.reject(e.reason);
            for (const e of o[$a]) e.resolve();
            o[xa].drain(e => {
              Mt(n, 0), Mt(n + 4, 2), Mt(n + 8, 1), e.resolve();
            });
          },
          u = e => {
            s.reject(e.message || "WebSocket error");
          };
        return o.addEventListener("message", a), o.addEventListener("open", i, {
          once: true
        }), o.addEventListener("close", c, {
          once: true
        }), o.addEventListener("error", u, {
          once: true
        }), o;
      }
      function Pa(e) {
        if (!!!e) throw new Error("Assert failed: ERR17: expected ws instance");
        return e[ja].promise;
      }
      function Wa(e, t, n, r, s) {
        if (!!!e) throw new Error("Assert failed: ERR17: expected ws instance");
        const a = Ja(e, new Uint8Array(o.HEAPU8.buffer, t, n), r, s);
        return s && a ? Va(e, a) : null;
      }
      function Fa(e, t, n) {
        if (!!!e) throw new Error("Assert failed: ERR18: expected ws instance");
        const r = e[Oa],
          o = e[xa],
          s = e.readyState;
        if (s != WebSocket.OPEN && s != WebSocket.CLOSING) throw new Error("InvalidState: The WebSocket is not connected.");
        if (r.getLength()) {
          if (!(0 == o.getLength())) throw new Error("Assert failed: ERR20: Invalid WS state");
          return La(e, r, t, n), null;
        }
        const {
            promise: i,
            promise_control: a
          } = it(),
          c = a;
        return c.buffer_ptr = t, c.buffer_length = n, o.enqueue(c), i;
      }
      function Ba(e, t, n, r) {
        if (!!!e) throw new Error("Assert failed: ERR19: expected ws instance");
        if (e.readyState == WebSocket.CLOSED) return null;
        if (r) {
          const {
            promise: r,
            promise_control: o
          } = it();
          return e[$a].push(o), "string" === typeof n ? e.close(t, n) : e.close(t), r;
        }
        return Ta || (Ta = true, console$1.log("WARNING: Web browsers do not support closing the output side of a WebSocket. CloseOutputAsync has closed the socket and discarded any incoming messages.")), "string" === typeof n ? e.close(t, n) : e.close(t), null;
      }
      function Ha(e) {
        if (!!!e) throw new Error("Assert failed: ERR18: expected ws instance");
        e[ka] = true;
        const t = e[ja];
        t && t.reject("OperationCanceledException");
        for (const t of e[$a]) t.reject("OperationCanceledException");
        for (const t of e[Na]) t.reject("OperationCanceledException");
        e[xa].drain(e => {
          e.reject("OperationCanceledException");
        }), e.close(1e3, "Connection was aborted.");
      }
      function Va(e, t) {
        if (e.send(t), e[Ea] = null, e.bufferedAmount < Da) return null;
        const {
            promise: n,
            promise_control: r
          } = it(),
          o = e[Na];
        o.push(r);
        let s = 1;
        const i = () => {
          if (0 === e.bufferedAmount) r.resolve();else if (e.readyState != WebSocket.OPEN) r.reject("InvalidState: The WebSocket is not connected.");else if (!r.isDone) return globalThis.setTimeout(i, s), s = Math.min(1.5 * s, 1e3), void 0;
          const t = o.indexOf(r);
          t > -1 && o.splice(t, 1);
        };
        return globalThis.setTimeout(i, 0), n;
      }
      function za(e, t) {
        const n = e[Oa],
          r = e[xa];
        if ("string" === typeof t.data) void 0 === Ia && (Ia = new TextEncoder$1()), n.enqueue({
          type: 0,
          data: Ia.encode(t.data),
          offset: 0
        });else {
          if ("ArrayBuffer" !== t.data.constructor.name) throw new Error("ERR19: WebSocket receive expected ArrayBuffer");
          n.enqueue({
            type: 1,
            data: new Uint8Array(t.data),
            offset: 0
          });
        }
        if (r.getLength() && n.getLength() > 1) throw new Error("ERR21: Invalid WS state");
        for (; r.getLength() && n.getLength();) {
          const t = r.dequeue();
          La(e, n, t.buffer_ptr, t.buffer_length), t.resolve();
        }
        pa();
      }
      function La(e, t, n, r) {
        const s = t.peek(),
          i = Math.min(r, s.data.length - s.offset);
        if (i > 0) {
          const e = s.data.subarray(s.offset, s.offset + i);
          new Uint8Array(o.HEAPU8.buffer, n, r).set(e, 0), s.offset += i;
        }
        const a = s.data.length === s.offset ? 1 : 0;
        a && t.dequeue();
        const c = e[Ra];
        Mt(c, i), Mt(c + 4, s.type), Mt(c + 8, a);
      }
      function Ja(e, t, n, r) {
        let o = e[Ea],
          s = 0;
        const i = t.byteLength;
        if (o) {
          if (s = e[Aa], n = e[Sa], 0 !== i) {
            if (s + i > o.length) {
              const n = new Uint8Array(1.5 * (s + i + 50));
              n.set(o, 0), n.subarray(s).set(t), e[Ea] = o = n;
            } else o.subarray(s).set(t);
            s += i, e[Aa] = s;
          }
        } else r ? 0 !== i && (o = t, s = i) : (0 !== i && (o = t.slice(), s = i, e[Aa] = s, e[Ea] = o), e[Sa] = n);
        if (r) {
          if (0 == s || null == o) return Ua;
          if (0 === n) {
            void 0 === Ma && (Ma = new TextDecoder$1("utf-8", {
              fatal: false
            }));
            const e = "undefined" !== typeof SharedArrayBuffer && o instanceof SharedArrayBuffer ? o.slice(0, s) : o.subarray(0, s);
            return Ma.decode(e);
          }
          return o.subarray(0, s);
        }
        return null;
      }
      function qa() {
        return {
          mono_wasm_exit: e => {
            o.printErr("MONO_WASM: early exit " + e);
          },
          mono_wasm_enable_on_demand_gc: M.mono_wasm_enable_on_demand_gc,
          mono_profiler_init_aot: M.mono_profiler_init_aot,
          mono_wasm_exec_regression: M.mono_wasm_exec_regression,
          mono_method_resolve: Gi,
          mono_intern_string: jr,
          logging: void 0,
          mono_wasm_stringify_as_error_with_stack: xe,
          mono_wasm_get_loaded_files: ls,
          mono_wasm_send_dbg_command_with_parms: q,
          mono_wasm_send_dbg_command: G,
          mono_wasm_get_dbg_command_info: Y,
          mono_wasm_get_details: ie,
          mono_wasm_release_object: ce,
          mono_wasm_call_function_on: oe,
          mono_wasm_debugger_resume: Z,
          mono_wasm_detach_debugger: X,
          mono_wasm_raise_debug_event: K,
          mono_wasm_change_debugger_log_level: Q,
          mono_wasm_debugger_attached: te,
          mono_wasm_runtime_is_ready: b.mono_wasm_runtime_is_ready,
          get_property: $s,
          set_property: js,
          has_property: Ns,
          get_typeof_property: ks,
          get_global_this: Rs,
          get_dotnet_instance: () => _,
          dynamic_import: Is,
          mono_wasm_cancel_promise: dt,
          ws_wasm_create: Ca,
          ws_wasm_open: Pa,
          ws_wasm_send: Wa,
          ws_wasm_receive: Fa,
          ws_wasm_close: Ba,
          ws_wasm_abort: Ha,
          http_wasm_supports_streaming_response: ta,
          http_wasm_create_abort_controler: na,
          http_wasm_abort_request: ra,
          http_wasm_abort_response: oa,
          http_wasm_fetch: ia,
          http_wasm_fetch_bytes: sa,
          http_wasm_get_response_header_names: ca,
          http_wasm_get_response_header_values: ua,
          http_wasm_get_response_bytes: fa,
          http_wasm_get_response_length: la,
          http_wasm_get_streamed_response_bytes: _a
        };
      }
      function Ga(e) {
        Object.assign(e, {
          mono_wasm_exit: M.mono_wasm_exit,
          mono_wasm_enable_on_demand_gc: M.mono_wasm_enable_on_demand_gc,
          mono_profiler_init_aot: M.mono_profiler_init_aot,
          mono_wasm_exec_regression: M.mono_wasm_exec_regression
        });
      }
      function Ya() {
        return {
          mono_wasm_setenv: Oc,
          mono_wasm_load_bytes_into_heap: tn,
          mono_wasm_load_icu_data: fe,
          mono_wasm_runtime_ready: mono_wasm_runtime_ready,
          mono_wasm_load_data_archive: cs,
          mono_wasm_load_config: Rc,
          mono_load_runtime_and_bcl_args: Ic,
          mono_wasm_new_root_buffer: un,
          mono_wasm_new_root: fn,
          mono_wasm_new_external_root: ln,
          mono_wasm_release_roots: _n,
          mono_run_main: Te,
          mono_run_main_and_exit: Re,
          mono_wasm_add_assembly: null,
          mono_wasm_load_runtime: Nc,
          config: b.config,
          loaded_files: [],
          setB32: Ot,
          setI8: kt,
          setI16: Rt,
          setI32: Mt,
          setI52: Dt,
          setU52: Ut,
          setI64Big: Ct,
          setU8: xt,
          setU16: jt,
          setU32: Nt,
          setF32: Pt,
          setF64: Wt,
          getB32: Ft,
          getI8: zt,
          getI16: Lt,
          getI32: Jt,
          getI52: qt,
          getU52: Gt,
          getI64Big: Yt,
          getU8: Bt,
          getU16: Ht,
          getU32: Vt,
          getF32: Zt,
          getF64: Xt
        };
      }
      function Za(e) {
        Object.assign(e, {
          mono_wasm_add_assembly: M.mono_wasm_add_assembly
        });
      }
      function Xa() {
        return {
          bind_static_method: Oi,
          call_assembly_entry_point: ji,
          mono_obj_array_new: null,
          mono_obj_array_set: null,
          js_string_to_mono_string: Mr,
          js_typed_array_to_array: ni,
          mono_array_to_js_array: mi,
          js_to_mono_obj: Xs,
          conv_string: Or,
          unbox_mono_obj: ui,
          mono_obj_array_new_ref: null,
          mono_obj_array_set_ref: null,
          js_string_to_mono_string_root: kr,
          js_typed_array_to_array_root: ti,
          js_to_mono_obj_root: Qs,
          conv_string_root: xr,
          unbox_mono_obj_root: di,
          mono_array_root_to_js_array: wi
        };
      }
      function Qa(e) {
        Object.assign(e, {
          mono_obj_array_new: M.mono_wasm_obj_array_new,
          mono_obj_array_set: M.mono_wasm_obj_array_set,
          mono_obj_array_new_ref: M.mono_wasm_obj_array_new_ref,
          mono_obj_array_set_ref: M.mono_wasm_obj_array_set_ref
        });
      }
      function Ka() {}
      let tc,
        nc = false,
        rc = false;
      const oc = it(),
        sc = it(),
        ic = it(),
        ac = it(),
        cc = it(),
        uc = it(),
        lc = it(),
        fc = it();
      function _c(e, t) {
        const n = e.instantiateWasm,
          r = e.preInit ? "function" === typeof e.preInit ? [e.preInit] : e.preInit : [],
          o = e.preRun ? "function" === typeof e.preRun ? [e.preRun] : e.preRun : [],
          s = e.postRun ? "function" === typeof e.postRun ? [e.postRun] : e.postRun : [],
          i = e.onRuntimeInitialized ? e.onRuntimeInitialized : () => {};
        rc = !e.configSrc && (!e.config || !e.config.assets || -1 == e.config.assets.findIndex(e => "assembly" === e.behavior)), e.instantiateWasm = (e, t) => dc(e, t, n), e.preInit = [() => mc(r)], e.preRun = [() => gc(o)], e.onRuntimeInitialized = () => wc(i), e.postRun = [() => hc(s)], e.ready = e.ready.then(_async(function () {
          return _await$1(fc.promise, function () {
            return t;
          });
        })), e.onAbort || (e.onAbort = () => Ie);
      }
      function dc(e, t, n) {
        if (o.configSrc || o.config || n || o.print("MONO_WASM: configSrc nor config was specified"), tc = o.config ? b.config = o.config : b.config = o.config = {}, b.diagnosticTracing = !!tc.diagnosticTracing, n) {
          return n(e, (e, n) => {
            sc.promise_control.resolve(), t(e, n);
          });
        }
        return jc(e, t), [];
      }
      function mc(e) {
        o.addRunDependency("mono_pre_init");
        try {
          bc(), b.diagnosticTracing && console$1.debug("MONO_WASM: preInit"), ic.promise_control.resolve(), e.forEach(e => e());
        } catch (e) {
          throw Sc("MONO_WASM: user preInint() failed", e), pc(e, true), e;
        }
        _async(function () {
          return _continue$1(_catch$1(function () {
            return _call$1(yc, function () {
              const _rc = rc;
              return _await$1(_rc || vc(), function (_vc) {
                _vc;
              }, _rc);
            });
          }, function (e) {
            throw pc(e, true), e;
          }), function (_result2) {
            ac.promise_control.resolve(), o.removeRunDependency("mono_pre_init");
          });
        })();
      }
      function pc(e, t) {
        b.diagnosticTracing && console$1.trace("MONO_WASM: abort_startup"), sc.promise_control.reject(e), ic.promise_control.reject(e), ac.promise_control.reject(e), cc.promise_control.reject(e), uc.promise_control.reject(e), lc.promise_control.reject(e), fc.promise_control.reject(e), t && De(1, e);
      }
      function bc() {
        o.addRunDependency("mono_wasm_pre_init_essential"), b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_pre_init_essential"), I(), Ga(s), Za(Io), Qa(Do), o.removeRunDependency("mono_wasm_pre_init_essential");
      }
      function Sc(e, t) {
        o.printErr(`${e}: ${JSON.stringify(t)}`), t.stack && (o.printErr("MONO_WASM: Stacktrace: \n"), o.printErr(t.stack));
      }
      function Oc(e, t) {
        M.mono_wasm_setenv(e, t);
      }
      function xc(e) {
        if (!Array.isArray(e)) throw new Error("Expected runtimeOptions to be an array of strings");
        const t = o._malloc(4 * e.length);
        let n = 0;
        for (let r = 0; r < e.length; ++r) {
          const s = e[r];
          if ("string" !== typeof s) throw new Error("Expected runtimeOptions to be an array of strings");
          o.setValue(t + 4 * n, M.mono_wasm_strdup(s), "i32"), n += 1;
        }
        M.mono_wasm_parse_runtime_options(e.length, t);
      }
      function Nc(e, t) {
        if (b.diagnosticTracing && console$1.debug("MONO_WASM: mono_wasm_load_runtime"), !b.mono_wasm_load_runtime_done) {
          b.mono_wasm_load_runtime_done = true;
          try {
            void 0 == t && (t = 0, tc && tc.debugLevel && (t = 0 + t)), M.mono_wasm_load_runtime(e || "unused", t), b.waitForDebugger = tc.waitForDebugger, b.mono_wasm_bindings_is_ready || kc();
          } catch (e) {
            if (Sc("MONO_WASM: mono_wasm_load_runtime () failed", e), pc(e, false), c || a) {
              (0, M.mono_wasm_exit)(1);
            }
            throw e;
          }
        }
      }
      function kc() {
        if (b.diagnosticTracing && console$1.debug("MONO_WASM: bindings_init"), !b.mono_wasm_bindings_is_ready) {
          b.mono_wasm_bindings_is_ready = true;
          try {
            zs(), Ki(), co(), Ir(), b._i52_error_scratch_buffer = o._malloc(4);
          } catch (e) {
            throw Sc("MONO_WASM: Error in bindings_init", e), e;
          }
        }
      }
      function Tc(e, t, n, r, s) {
        if (true !== b.mono_wasm_runtime_is_ready) return;
        const i = 0 !== e ? o.UTF8ToString(e).concat(".dll") : "",
          c = D(new Uint8Array(o.HEAPU8.buffer, t, n));
        let u;
        if (r) {
          u = D(new Uint8Array(o.HEAPU8.buffer, r, s));
        }
        K({
          eventName: "AssemblyLoaded",
          assembly_name: i,
          assembly_b64: c,
          pdb_b64: u
        });
      }
      function Mc(e, t) {
        const n = t.length + 1,
          r = o._malloc(4 * n);
        let s = 0;
        o.setValue(r + 4 * s, M.mono_wasm_strdup(e), "i32"), s += 1;
        for (let e = 0; e < t.length; ++e) o.setValue(r + 4 * s, M.mono_wasm_strdup(t[e]), "i32"), s += 1;
        M.mono_wasm_set_main_args(n, r);
      }
      var Dc, Uc;
      ((function (e) {
        e[e.Sending = 0] = "Sending", e[e.Closed = 1] = "Closed", e[e.Error = 2] = "Error";
      }))(Dc || (Dc = {})), function (e) {
        e[e.Idle = 0] = "Idle", e[e.PartialCommand = 1] = "PartialCommand", e[e.Error = 2] = "Error";
      }(Uc || (Uc = {}));
      const Cc = void 0;
      function Pc() {
        return {
          mono_set_timeout: ya,
          mono_wasm_asm_loaded: Tc,
          mono_wasm_fire_debugger_agent_message: mono_wasm_fire_debugger_agent_message,
          mono_wasm_debugger_log: ue,
          mono_wasm_add_dbg_command_received: L,
          schedule_background_exec: ba,
          mono_wasm_invoke_js_blazor: Ii,
          mono_wasm_trace_logger: je,
          mono_wasm_set_entrypoint_breakpoint: ne,
          mono_wasm_event_pipe_early_startup_callback: Ka,
          mono_wasm_invoke_js_with_args_ref: $i,
          mono_wasm_get_object_property_ref: Ni,
          mono_wasm_set_object_property_ref: ki,
          mono_wasm_get_by_index_ref: Ri,
          mono_wasm_set_by_index_ref: Ti,
          mono_wasm_get_global_object_ref: Mi,
          mono_wasm_create_cs_owned_object_ref: bi,
          mono_wasm_release_cs_owned_object: Ke,
          mono_wasm_typed_array_to_array_ref: ii,
          mono_wasm_typed_array_from_ref: Js,
          mono_wasm_bind_js_function: As,
          mono_wasm_invoke_bound_function: Ss,
          mono_wasm_bind_cs_function: Ps,
          mono_wasm_marshal_promise: Oo,
          mono_wasm_load_icu_data: fe,
          mono_wasm_get_icudt_name: _e,
          ...Cc
        };
      }
      class Wc {
        constructor() {
          this.moduleConfig = {
            disableDotnet6Compatibility: true,
            configSrc: "./mono-config.json",
            config: b.config
          };
        }
        withModuleConfig(e) {
          try {
            return Object.assign(this.moduleConfig, e), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withConsoleForwarding() {
          try {
            const e = {
              forwardConsoleLogsToWS: true
            };
            return Object.assign(this.moduleConfig.config, e), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withAsyncFlushOnExit() {
          try {
            const e = {
              asyncFlushOnExit: true
            };
            return Object.assign(this.moduleConfig.config, e), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withExitCodeLogging() {
          try {
            const e = {
              logExitCode: true
            };
            return Object.assign(this.moduleConfig.config, e), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withElementOnExit() {
          try {
            const e = {
              appendElementOnExit: true
            };
            return Object.assign(this.moduleConfig.config, e), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withWaitingForDebugger(e) {
          try {
            const t = {
              waitForDebugger: e
            };
            return Object.assign(this.moduleConfig.config, t), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withConfig(e) {
          try {
            const t = {
              ...e
            };
            return t.assets = [...(this.moduleConfig.config.assets || []), ...(t.assets || [])], t.environmentVariables = {
              ...(this.moduleConfig.config.environmentVariables || {}),
              ...(t.environmentVariables || {})
            }, Object.assign(this.moduleConfig.config, t), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withConfigSrc(e) {
          try {
            if (!(e && "string" === typeof e)) throw new Error("Assert failed: must be file path or URL");
            return Object.assign(this.moduleConfig, {
              configSrc: e
            }), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withVirtualWorkingDirectory(e) {
          try {
            if (!(e && "string" === typeof e)) throw new Error("Assert failed: must be directory path");
            return this.virtualWorkingDirectory = e, this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withEnvironmentVariable(e, t) {
          try {
            return this.moduleConfig.config.environmentVariables[e] = t, this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withEnvironmentVariables(e) {
          try {
            if (!(e && "object" === typeof e)) throw new Error("Assert failed: must be dictionary object");
            return Object.assign(this.moduleConfig.config.environmentVariables, e), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withDiagnosticTracing(e) {
          try {
            if (!("boolean" === typeof e)) throw new Error("Assert failed: must be boolean");
            return this.moduleConfig.config.diagnosticTracing = e, this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withDebugging(e) {
          try {
            if (!(e && "number" === typeof e)) throw new Error("Assert failed: must be number");
            return this.moduleConfig.config.debugLevel = e, this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withApplicationArguments(...e) {
          try {
            if (!(e && Array.isArray(e))) throw new Error("Assert failed: must be array of strings");
            return this.applicationArguments = e, this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withRuntimeOptions(e) {
          try {
            if (!(e && Array.isArray(e))) throw new Error("Assert failed: must be array of strings");
            return Object.assign(this.moduleConfig, {
              runtimeOptions: e
            }), this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withMainAssembly(e) {
          try {
            return this.moduleConfig.config.mainAssemblyName = e, this;
          } catch (e) {
            throw De(1, e), e;
          }
        }
        withApplicationArgumentsFromQuery() {
          try {
            if ("undefined" != typeof globalThis.URLSearchParams) {
              const e = undefined,
                t = new URLSearchParams(window.location.search).getAll("arg");
              return this.withApplicationArguments(...t);
            }
            throw new Error("URLSearchParams is supported");
          } catch (e) {
            throw De(1, e), e;
          }
        }
        create() {
          const _this = this;
          return _call$1(function () {
            let _exit3 = false;
            return _await$1(_catch$1(function () {
              return _invoke(function () {
                if (!_this.instance) {
                  return _invoke(function () {
                    if (u && !f && _this.moduleConfig.config.forwardConsoleLogsToWS && "undefined" != typeof globalThis.WebSocket && Ne("main", globalThis.console, globalThis.location.origin), a) {
                      return _await$1(Promise.resolve().then(function () { return _polyfillNode_process; }), function (e) {
                        if (e.versions.node.split(".")[0] < 14) throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}'`);
                      });
                    }
                  }, function (_result3) {
                    if (_exit3) ;
                    if (!_this.moduleConfig) throw new Error("Assert failed: Null moduleConfig");
                    if (!_this.moduleConfig.config) throw new Error("Assert failed: Null moduleConfig.config");
                    return _await$1(m(_this.moduleConfig), function (_m) {
                      _this.instance = _m;
                    });
                  });
                }
              }, function (_result4) {
                if (_exit3) ;
                if (_this.virtualWorkingDirectory) {
                  const e = _this.instance.Module.FS,
                    t = e.stat(_this.virtualWorkingDirectory);
                  if (!(t && e.isDir(t.mode))) throw new Error(`Assert failed: Could not find working directory ${_this.virtualWorkingDirectory}`);
                  e.chdir(_this.virtualWorkingDirectory);
                }
                return _this.instance;
              });
            }, function (e) {
              throw De(1, e), e;
            }));
          });
        }
        run() {
          const _this2 = this;
          return _call$1(function () {
            let _exit4 = false;
            return _await$1(_catch$1(function () {
              let _exit5 = false;
              if (!_this2.moduleConfig.config) throw new Error("Assert failed: Null moduleConfig.config");
              const _this2$instance = _this2.instance;
              return _await$1(_this2$instance || _this2.create(), function (_this2$create) {
                if (_this2$create, !_this2.moduleConfig.config.mainAssemblyName) throw new Error("Assert failed: Null moduleConfig.config.mainAssemblyName");
                return _invoke(function () {
                  if (!_this2.applicationArguments) return _invokeIgnored(function () {
                    if (a) {
                      return _await$1(Promise.resolve().then(function () { return _polyfillNode_process; }), function (e) {
                        _this2.applicationArguments = e.argv.slice(2);
                      });
                    } else _this2.applicationArguments = [];
                  });
                }, function () {
                  return _this2.instance.runMainAndExit(_this2.moduleConfig.config.mainAssemblyName, _this2.applicationArguments);
                });
              }, _this2$instance);
            }, function (e) {
              throw De(1, e), e;
            }));
          });
        }
      }
      const Fc = new Wc();
      function Bc() {
        return {
          runMain: Te,
          runMainAndExit: Re,
          setEnvironmentVariable: Oc,
          getAssemblyExports: Hs,
          setModuleImports: Os,
          getConfig: () => b.config,
          setHeapB32: Ot,
          setHeapU8: xt,
          setHeapU16: jt,
          setHeapU32: Nt,
          setHeapI8: kt,
          setHeapI16: Rt,
          setHeapI32: Mt,
          setHeapI52: Dt,
          setHeapU52: Ut,
          setHeapI64Big: Ct,
          setHeapF32: Pt,
          setHeapF64: Wt,
          getHeapB32: Ft,
          getHeapU8: Bt,
          getHeapU16: Ht,
          getHeapU32: Vt,
          getHeapI8: zt,
          getHeapI16: Lt,
          getHeapI32: Jt,
          getHeapI52: qt,
          getHeapU52: Gt,
          getHeapI64Big: Yt,
          getHeapF32: Zt,
          getHeapF64: Xt
        };
      }
      function Hc() {
        return {
          dotnet: Fc,
          exit: De
        };
      }
      const Vc = Lc,
        zc = qc;
      function Lc(n, o, s, i) {
        const a = o.module,
          c = globalThis;
        g(n, o), Co(o), ds(s), Object.assign(o.mono, Ya()), Object.assign(o.binding, Xa()), Object.assign(o.internal, qa()), Object.assign(o.internal, qa());
        const u = Bc();
        if (e.__linker_exports = Pc(), Object.assign(_, {
          MONO: o.mono,
          BINDING: o.binding,
          INTERNAL: o.internal,
          IMPORTS: o.marshaled_imports,
          Module: a,
          runtimeBuildInfo: {
            productVersion: t,
            buildConfiguration: r
          },
          ...u
        }), Object.assign(i, u), o.module.__undefinedConfig && (a.disableDotnet6Compatibility = true, a.configSrc = "./mono-config.json"), a.print || (a.print = console$1.log.bind(console$1)), a.printErr || (a.printErr = console$1.error.bind(console$1)), "undefined" === typeof a.disableDotnet6Compatibility && (a.disableDotnet6Compatibility = true), n.isGlobal || !a.disableDotnet6Compatibility) {
          Object.assign(a, _), a.mono_bind_static_method = (e, t) => (console$1.log("MONO_WASM: Module.mono_bind_static_method is obsolete, please use [JSExportAttribute] interop instead"), Oi(e, t));
          const e = (e, t) => {
            if ("undefined" !== typeof c[e]) return;
            let n;
            Object.defineProperty(globalThis, e, {
              get: () => {
                if (R(n)) {
                  const r = new Error().stack,
                    o = r ? r.substr(r.indexOf("\n", 8) + 1) : "";
                  console$1.log(`MONO_WASM: global ${e} is obsolete, please use Module.${e} instead ${o}`), n = t();
                }
                return n;
              }
            });
          };
          c.MONO = o.mono, c.BINDING = o.binding, c.INTERNAL = o.internal, n.isGlobal || (c.Module = a), e("cwrap", () => a.cwrap), e("addRunDependency", () => a.addRunDependency), e("removeRunDependency", () => a.removeRunDependency);
        }
        let l;
        return c.getDotnetRuntime ? l = c.getDotnetRuntime.__list : (c.getDotnetRuntime = e => c.getDotnetRuntime.__list.getRuntime(e), c.getDotnetRuntime.__list = l = new Jc()), l.registerRuntime(_), _c(a, _), _;
      }
      e.__linker_exports = null;
      class Jc {
        constructor() {
          this.list = {};
        }
        registerRuntime(e) {
          return e.runtimeId = Object.keys(this.list).length, this.list[e.runtimeId] = Be(e), e.runtimeId;
        }
        getRuntime(e) {
          const t = this.list[e];
          return t ? t.deref() : void 0;
        }
      }
      function qc(e, t) {
        w(t), Object.assign(d, Hc()), h(e);
      }
      return e.__initializeImportsAndExports = Vc, e.__setEmscriptenEntrypoint = zc, e.moduleExports = d, Object.defineProperty(e, "__esModule", {
        value: true
      }), e;
    }({});
    var createDotnetRuntime = (() => {
      var _scriptDir = (_document.currentScript && _document.currentScript.src || new URL('bootloader.js', _document.baseURI).href);
      return function (createDotnetRuntime) {
        createDotnetRuntime = createDotnetRuntime || {};
        var Module = typeof createDotnetRuntime != "undefined" ? createDotnetRuntime : {};
        var readyPromiseResolve, readyPromiseReject;
        Module["ready"] = new Promise$1(function (resolve, reject) {
          readyPromiseResolve = resolve;
          readyPromiseReject = reject;
        });
        var require = require || undefined;
        var __dirname = __dirname || "";
        var __callbackAPI = {
          MONO: MONO,
          BINDING: BINDING,
          INTERNAL: INTERNAL,
          IMPORTS: IMPORTS
        };
        if (typeof createDotnetRuntime === "function") {
          __callbackAPI.Module = Module = {
            ready: Module.ready
          };
          const extension = createDotnetRuntime(__callbackAPI);
          if (extension.ready) {
            throw new Error("MONO_WASM: Module.ready couldn't be redefined.");
          }
          Object.assign(Module, extension);
          createDotnetRuntime = Module;
          if (!createDotnetRuntime.locateFile) createDotnetRuntime.locateFile = createDotnetRuntime.__locateFile = path => scriptDirectory + path;
        } else if (typeof createDotnetRuntime === "object") {
          __callbackAPI.Module = Module = {
            ready: Module.ready,
            __undefinedConfig: Object.keys(createDotnetRuntime).length === 1
          };
          Object.assign(Module, createDotnetRuntime);
          createDotnetRuntime = Module;
          if (!createDotnetRuntime.locateFile) createDotnetRuntime.locateFile = createDotnetRuntime.__locateFile = path => scriptDirectory + path;
        } else {
          throw new Error("MONO_WASM: Can't use moduleFactory callback of createDotnetRuntime function.");
        }
        var moduleOverrides = Object.assign({}, Module);
        var thisProgram = "./this.program";
        var quit_ = (status, toThrow) => {
          throw toThrow;
        };
        var ENVIRONMENT_IS_WEB = typeof window == "object";
        var ENVIRONMENT_IS_WORKER = typeof importScripts == "function";
        var ENVIRONMENT_IS_NODE = typeof browser$1 == "object" && typeof browser$1.versions == "object" && typeof browser$1.versions.node == "string";
        var ENVIRONMENT_IS_SHELL = !ENVIRONMENT_IS_WEB && !ENVIRONMENT_IS_NODE && !ENVIRONMENT_IS_WORKER;
        var scriptDirectory = "";
        function locateFile(path) {
          if (Module["locateFile"]) {
            return Module["locateFile"](path, scriptDirectory);
          }
          return scriptDirectory + path;
        }
        var read_, readAsync, readBinary;
        function logExceptionOnExit(e) {
          if (e instanceof ExitStatus) return;
          let toLog = e;
          err("exiting due to exception: " + toLog);
        }
        var fs;
        var nodePath;
        var requireNodeFS;
        if (ENVIRONMENT_IS_NODE) {
          if (ENVIRONMENT_IS_WORKER) {
            scriptDirectory = require("path").dirname(scriptDirectory) + "/";
          } else {
            scriptDirectory = __dirname + "/";
          }
          requireNodeFS = () => {
            if (!nodePath) {
              fs = require("fs");
              nodePath = require("path");
            }
          };
          read_ = function shell_read(filename, binary) {
            requireNodeFS();
            filename = nodePath["normalize"](filename);
            return fs.readFileSync(filename, binary ? undefined : "utf8");
          };
          readBinary = filename => {
            var ret = read_(filename, true);
            if (!ret.buffer) {
              ret = new Uint8Array(ret);
            }
            return ret;
          };
          readAsync = (filename, onload, onerror) => {
            requireNodeFS();
            filename = nodePath["normalize"](filename);
            fs.readFile(filename, function (err, data) {
              if (err) onerror(err);else onload(data.buffer);
            });
          };
          if (browser$1["argv"].length > 1) {
            thisProgram = browser$1["argv"][1].replace(/\\/g, "/");
          }
          browser$1["argv"].slice(2);
          browser$1["on"]("uncaughtException", function (ex) {
            if (!(ex instanceof ExitStatus)) {
              throw ex;
            }
          });
          browser$1["on"]("unhandledRejection", function (reason) {
            throw reason;
          });
          quit_ = (status, toThrow) => {
            if (keepRuntimeAlive()) {
              browser$1["exitCode"] = status;
              throw toThrow;
            }
            logExceptionOnExit(toThrow);
            browser$1["exit"](status);
          };
          Module["inspect"] = function () {
            return "[Emscripten Module object]";
          };
        } else if (ENVIRONMENT_IS_SHELL) {
          if (typeof read != "undefined") {
            read_ = function shell_read(f) {
              return read(f);
            };
          }
          readBinary = function readBinary(f) {
            let data;
            if (typeof readbuffer == "function") {
              return new Uint8Array(readbuffer(f));
            }
            data = read(f, "binary");
            assert(typeof data == "object");
            return data;
          };
          readAsync = function readAsync(f, onload, onerror) {
            setTimeout$1(() => onload(readBinary(f)), 0);
          };
          if (typeof scriptArgs != "undefined") {
            scriptArgs;
          }
          if (typeof quit == "function") {
            quit_ = (status, toThrow) => {
              logExceptionOnExit(toThrow);
              quit(status);
            };
          }
          if (typeof print != "undefined") {
            if (typeof console$1 == "undefined") console$1 = {};
            console$1.log = print;
            console$1.log = console$1.error = typeof printErr != "undefined" ? printErr : print;
          }
        } else if (ENVIRONMENT_IS_WEB || ENVIRONMENT_IS_WORKER) {
          if (ENVIRONMENT_IS_WORKER) {
            scriptDirectory = self.location.href;
          } else if (typeof document != "undefined" && _document.currentScript) {
            scriptDirectory = _document.currentScript.src;
          }
          if (_scriptDir) {
            scriptDirectory = _scriptDir;
          }
          if (scriptDirectory.indexOf("blob:") !== 0) {
            scriptDirectory = scriptDirectory.substr(0, scriptDirectory.replace(/[?#].*/, "").lastIndexOf("/") + 1);
          } else {
            scriptDirectory = "";
          }
          {
            read_ = url => {
              var xhr = new XMLHttpRequest();
              xhr.open("GET", url, false);
              xhr.send(null);
              return xhr.responseText;
            };
            if (ENVIRONMENT_IS_WORKER) {
              readBinary = url => {
                var xhr = new XMLHttpRequest();
                xhr.open("GET", url, false);
                xhr.responseType = "arraybuffer";
                xhr.send(null);
                return new Uint8Array(xhr.response);
              };
            }
            readAsync = (url, onload, onerror) => {
              var xhr = new XMLHttpRequest();
              xhr.open("GET", url, true);
              xhr.responseType = "arraybuffer";
              xhr.onload = () => {
                if (xhr.status == 200 || xhr.status == 0 && xhr.response) {
                  onload(xhr.response);
                  return;
                }
                onerror();
              };
              xhr.onerror = onerror;
              xhr.send(null);
            };
          }
        } else ;
        var out = Module["print"] || console$1.log.bind(console$1);
        var err = Module["printErr"] || console$1.warn.bind(console$1);
        Object.assign(Module, moduleOverrides);
        moduleOverrides = null;
        if (Module["arguments"]) Module["arguments"];
        if (Module["thisProgram"]) thisProgram = Module["thisProgram"];
        if (Module["quit"]) quit_ = Module["quit"];
        var tempRet0 = 0;
        var setTempRet0 = value => {
          tempRet0 = value;
        };
        var getTempRet0 = () => tempRet0;
        var wasmBinary;
        if (Module["wasmBinary"]) wasmBinary = Module["wasmBinary"];
        var noExitRuntime = Module["noExitRuntime"] || true;
        if (typeof WebAssembly != "object") {
          abort("no native wasm support detected");
        }
        var wasmMemory;
        var ABORT = false;
        function assert(condition, text) {
          if (!condition) {
            abort(text);
          }
        }
        function getCFunc(ident) {
          var func = Module["_" + ident];
          return func;
        }
        function ccall(ident, returnType, argTypes, args, opts) {
          var toC = {
            "string": function (str) {
              var ret = 0;
              if (str !== null && str !== undefined && str !== 0) {
                var len = (str.length << 2) + 1;
                ret = stackAlloc(len);
                stringToUTF8(str, ret, len);
              }
              return ret;
            },
            "array": function (arr) {
              var ret = stackAlloc(arr.length);
              writeArrayToMemory(arr, ret);
              return ret;
            }
          };
          function convertReturnValue(ret) {
            if (returnType === "string") {
              return UTF8ToString(ret);
            }
            if (returnType === "boolean") return Boolean(ret);
            return ret;
          }
          var func = getCFunc(ident);
          var cArgs = [];
          var stack = 0;
          if (args) {
            for (var i = 0; i < args.length; i++) {
              var converter = toC[argTypes[i]];
              if (converter) {
                if (stack === 0) stack = stackSave();
                cArgs[i] = converter(args[i]);
              } else {
                cArgs[i] = args[i];
              }
            }
          }
          var ret = func.apply(null, cArgs);
          function onDone(ret) {
            if (stack !== 0) stackRestore(stack);
            return convertReturnValue(ret);
          }
          ret = onDone(ret);
          return ret;
        }
        function cwrap(ident, returnType, argTypes, opts) {
          argTypes = argTypes || [];
          var numericArgs = argTypes.every(function (type) {
            return type === "number";
          });
          var numericRet = returnType !== "string";
          if (numericRet && numericArgs && !opts) {
            return getCFunc(ident);
          }
          return function () {
            return ccall(ident, returnType, argTypes, arguments);
          };
        }
        var UTF8Decoder = undefined;
        function UTF8ArrayToString(heapOrArray, idx, maxBytesToRead) {
          var endIdx = idx + maxBytesToRead;
          var endPtr = idx;
          while (heapOrArray[endPtr] && !(endPtr >= endIdx)) ++endPtr;
          if (endPtr - idx > 16 && heapOrArray.buffer && UTF8Decoder) {
            return UTF8Decoder.decode(heapOrArray.subarray(idx, endPtr));
          } else {
            var str = "";
            while (idx < endPtr) {
              var u0 = heapOrArray[idx++];
              if (!(u0 & 128)) {
                str += String.fromCharCode(u0);
                continue;
              }
              var u1 = heapOrArray[idx++] & 63;
              if ((u0 & 224) == 192) {
                str += String.fromCharCode((u0 & 31) << 6 | u1);
                continue;
              }
              var u2 = heapOrArray[idx++] & 63;
              if ((u0 & 240) == 224) {
                u0 = (u0 & 15) << 12 | u1 << 6 | u2;
              } else {
                u0 = (u0 & 7) << 18 | u1 << 12 | u2 << 6 | heapOrArray[idx++] & 63;
              }
              if (u0 < 65536) {
                str += String.fromCharCode(u0);
              } else {
                var ch = u0 - 65536;
                str += String.fromCharCode(55296 | ch >> 10, 56320 | ch & 1023);
              }
            }
          }
          return str;
        }
        function UTF8ToString(ptr, maxBytesToRead) {
          return ptr ? UTF8ArrayToString(HEAPU8, ptr, maxBytesToRead) : "";
        }
        function stringToUTF8Array(str, heap, outIdx, maxBytesToWrite) {
          if (!(maxBytesToWrite > 0)) return 0;
          var startIdx = outIdx;
          var endIdx = outIdx + maxBytesToWrite - 1;
          for (var i = 0; i < str.length; ++i) {
            var u = str.charCodeAt(i);
            if (u >= 55296 && u <= 57343) {
              var u1 = str.charCodeAt(++i);
              u = 65536 + ((u & 1023) << 10) | u1 & 1023;
            }
            if (u <= 127) {
              if (outIdx >= endIdx) break;
              heap[outIdx++] = u;
            } else if (u <= 2047) {
              if (outIdx + 1 >= endIdx) break;
              heap[outIdx++] = 192 | u >> 6;
              heap[outIdx++] = 128 | u & 63;
            } else if (u <= 65535) {
              if (outIdx + 2 >= endIdx) break;
              heap[outIdx++] = 224 | u >> 12;
              heap[outIdx++] = 128 | u >> 6 & 63;
              heap[outIdx++] = 128 | u & 63;
            } else {
              if (outIdx + 3 >= endIdx) break;
              heap[outIdx++] = 240 | u >> 18;
              heap[outIdx++] = 128 | u >> 12 & 63;
              heap[outIdx++] = 128 | u >> 6 & 63;
              heap[outIdx++] = 128 | u & 63;
            }
          }
          heap[outIdx] = 0;
          return outIdx - startIdx;
        }
        function stringToUTF8(str, outPtr, maxBytesToWrite) {
          return stringToUTF8Array(str, HEAPU8, outPtr, maxBytesToWrite);
        }
        function lengthBytesUTF8(str) {
          var len = 0;
          for (var i = 0; i < str.length; ++i) {
            var u = str.charCodeAt(i);
            if (u >= 55296 && u <= 57343) u = 65536 + ((u & 1023) << 10) | str.charCodeAt(++i) & 1023;
            if (u <= 127) ++len;else if (u <= 2047) len += 2;else if (u <= 65535) len += 3;else len += 4;
          }
          return len;
        }
        typeof TextDecoder$1 != "undefined" ? new TextDecoder$1("utf-16le") : undefined;
        function allocateUTF8(str) {
          var size = lengthBytesUTF8(str) + 1;
          var ret = _malloc(size);
          if (ret) stringToUTF8Array(str, HEAP8, ret, size);
          return ret;
        }
        function writeArrayToMemory(array, buffer) {
          HEAP8.set(array, buffer);
        }
        function writeAsciiToMemory(str, buffer, dontAddNull) {
          for (var i = 0; i < str.length; ++i) {
            HEAP8[buffer++ >> 0] = str.charCodeAt(i);
          }
          if (!dontAddNull) HEAP8[buffer >> 0] = 0;
        }
        var buffer, HEAP8, HEAPU8, HEAP16, HEAP32, HEAPU32, HEAPF32, HEAPF64;
        function updateGlobalBufferAndViews(buf) {
          buffer = buf;
          Module["HEAP8"] = HEAP8 = new Int8Array(buf);
          Module["HEAP16"] = HEAP16 = new Int16Array(buf);
          Module["HEAP32"] = HEAP32 = new Int32Array(buf);
          Module["HEAPU8"] = HEAPU8 = new Uint8Array(buf);
          Module["HEAPU16"] = new Uint16Array(buf);
          Module["HEAPU32"] = HEAPU32 = new Uint32Array(buf);
          Module["HEAPF32"] = HEAPF32 = new Float32Array(buf);
          Module["HEAPF64"] = HEAPF64 = new Float64Array(buf);
        }
        Module["INITIAL_MEMORY"] || 536870912;
        var wasmTable;
        var __ATPRERUN__ = [];
        var __ATINIT__ = [];
        var __ATPOSTRUN__ = [];
        function keepRuntimeAlive() {
          return noExitRuntime;
        }
        function preRun() {
          if (Module["preRun"]) {
            if (typeof Module["preRun"] == "function") Module["preRun"] = [Module["preRun"]];
            while (Module["preRun"].length) {
              addOnPreRun(Module["preRun"].shift());
            }
          }
          callRuntimeCallbacks(__ATPRERUN__);
        }
        function initRuntime() {
          if (!Module["noFSInit"] && !FS.init.initialized) FS.init();
          FS.ignorePermissions = false;
          callRuntimeCallbacks(__ATINIT__);
        }
        function postRun() {
          if (Module["postRun"]) {
            if (typeof Module["postRun"] == "function") Module["postRun"] = [Module["postRun"]];
            while (Module["postRun"].length) {
              addOnPostRun(Module["postRun"].shift());
            }
          }
          callRuntimeCallbacks(__ATPOSTRUN__);
        }
        function addOnPreRun(cb) {
          __ATPRERUN__.unshift(cb);
        }
        function addOnInit(cb) {
          __ATINIT__.unshift(cb);
        }
        function addOnPostRun(cb) {
          __ATPOSTRUN__.unshift(cb);
        }
        var runDependencies = 0;
        var dependenciesFulfilled = null;
        function getUniqueRunDependency(id) {
          return id;
        }
        function addRunDependency(id) {
          runDependencies++;
          if (Module["monitorRunDependencies"]) {
            Module["monitorRunDependencies"](runDependencies);
          }
        }
        function removeRunDependency(id) {
          runDependencies--;
          if (Module["monitorRunDependencies"]) {
            Module["monitorRunDependencies"](runDependencies);
          }
          if (runDependencies == 0) {
            if (dependenciesFulfilled) {
              var callback = dependenciesFulfilled;
              dependenciesFulfilled = null;
              callback();
            }
          }
        }
        function abort(what) {
          {
            if (Module["onAbort"]) {
              Module["onAbort"](what);
            }
          }
          what = "Aborted(" + what + ")";
          err(what);
          ABORT = true;
          what += ". Build with -sASSERTIONS for more info.";
          var e = new WebAssembly.RuntimeError(what);
          readyPromiseReject(e);
          throw e;
        }
        var dataURIPrefix = "data:application/octet-stream;base64,";
        function isDataURI(filename) {
          return filename.startsWith(dataURIPrefix);
        }
        function isFileURI(filename) {
          return filename.startsWith("file://");
        }
        var wasmBinaryFile;
        if (Module["locateFile"]) {
          wasmBinaryFile = "dotnet.wasm";
          if (!isDataURI(wasmBinaryFile)) {
            wasmBinaryFile = locateFile(wasmBinaryFile);
          }
        } else {
          wasmBinaryFile = new URL("dotnet.wasm", (_document.currentScript && _document.currentScript.src || new URL('bootloader.js', _document.baseURI).href)).toString();
        }
        function getBinary(file) {
          try {
            if (file == wasmBinaryFile && wasmBinary) {
              return new Uint8Array(wasmBinary);
            }
            if (readBinary) {
              return readBinary(file);
            } else {
              throw "both async and sync fetching of the wasm failed";
            }
          } catch (err) {
            abort(err);
          }
        }
        function getBinaryPromise() {
          if (!wasmBinary && (ENVIRONMENT_IS_WEB || ENVIRONMENT_IS_WORKER)) {
            if (typeof fetch == "function" && !isFileURI(wasmBinaryFile)) {
              return fetch(wasmBinaryFile, {
                credentials: "same-origin"
              }).then(function (response) {
                if (!response["ok"]) {
                  throw "failed to load wasm binary file at '" + wasmBinaryFile + "'";
                }
                return response["arrayBuffer"]();
              }).catch(function () {
                return getBinary(wasmBinaryFile);
              });
            } else {
              if (readAsync) {
                return new Promise$1(function (resolve, reject) {
                  readAsync(wasmBinaryFile, function (response) {
                    resolve(new Uint8Array(response));
                  }, reject);
                });
              }
            }
          }
          return Promise$1.resolve().then(function () {
            return getBinary(wasmBinaryFile);
          });
        }
        function createWasm() {
          var info = {
            "env": asmLibraryArg,
            "wasi_snapshot_preview1": asmLibraryArg
          };
          function receiveInstance(instance, module) {
            var exports = instance.exports;
            Module["asm"] = exports;
            wasmMemory = Module["asm"]["memory"];
            updateGlobalBufferAndViews(wasmMemory.buffer);
            wasmTable = Module["asm"]["__indirect_function_table"];
            addOnInit(Module["asm"]["__wasm_call_ctors"]);
            removeRunDependency();
          }
          addRunDependency();
          function receiveInstantiationResult(result) {
            receiveInstance(result["instance"]);
          }
          function instantiateArrayBuffer(receiver) {
            return getBinaryPromise().then(function (binary) {
              return _WebAssemblyInstantiate(binary, info);
            }).then(function (instance) {
              return instance;
            }).then(receiver, function (reason) {
              err("failed to asynchronously prepare wasm: " + reason);
              abort(reason);
            });
          }
          function instantiateAsync() {
            if (!wasmBinary && typeof WebAssembly.instantiateStreaming == "function" && !isDataURI(wasmBinaryFile) && !isFileURI(wasmBinaryFile) && typeof fetch == "function") {
              return fetch(wasmBinaryFile, {
                credentials: "same-origin"
              }).then(function (response) {
                var result = WebAssembly.instantiateStreaming(response, info);
                return result.then(receiveInstantiationResult, function (reason) {
                  err("wasm streaming compile failed: " + reason);
                  err("falling back to ArrayBuffer instantiation");
                  return instantiateArrayBuffer(receiveInstantiationResult);
                });
              });
            } else {
              return instantiateArrayBuffer(receiveInstantiationResult);
            }
          }
          if (Module["instantiateWasm"]) {
            try {
              var exports = Module["instantiateWasm"](info, receiveInstance);
              return exports;
            } catch (e) {
              err("Module.instantiateWasm callback failed with error: " + e);
              return false;
            }
          }
          instantiateAsync().catch(readyPromiseReject);
          return {};
        }
        var tempDouble;
        var tempI64;
        function callRuntimeCallbacks(callbacks) {
          while (callbacks.length > 0) {
            var callback = callbacks.shift();
            if (typeof callback == "function") {
              callback(Module);
              continue;
            }
            var func = callback.func;
            if (typeof func == "number") {
              if (callback.arg === undefined) {
                getWasmTableEntry(func)();
              } else {
                getWasmTableEntry(func)(callback.arg);
              }
            } else {
              func(callback.arg === undefined ? null : callback.arg);
            }
          }
        }
        function getValue(ptr, type = "i8") {
          if (type.endsWith("*")) type = "i32";
          switch (type) {
            case "i1":
              return HEAP8[ptr >> 0];
            case "i8":
              return HEAP8[ptr >> 0];
            case "i16":
              return HEAP16[ptr >> 1];
            case "i32":
              return HEAP32[ptr >> 2];
            case "i64":
              return HEAP32[ptr >> 2];
            case "float":
              return HEAPF32[ptr >> 2];
            case "double":
              return Number(HEAPF64[ptr >> 3]);
            default:
              abort("invalid type for getValue: " + type);
          }
          return null;
        }
        var wasmTableMirror = [];
        function getWasmTableEntry(funcPtr) {
          var func = wasmTableMirror[funcPtr];
          if (!func) {
            if (funcPtr >= wasmTableMirror.length) wasmTableMirror.length = funcPtr + 1;
            wasmTableMirror[funcPtr] = func = wasmTable.get(funcPtr);
          }
          return func;
        }
        function setValue(ptr, value, type = "i8") {
          if (type.endsWith("*")) type = "i32";
          switch (type) {
            case "i1":
              HEAP8[ptr >> 0] = value;
              break;
            case "i8":
              HEAP8[ptr >> 0] = value;
              break;
            case "i16":
              HEAP16[ptr >> 1] = value;
              break;
            case "i32":
              HEAP32[ptr >> 2] = value;
              break;
            case "i64":
              tempI64 = [value >>> 0, (tempDouble = value, +Math.abs(tempDouble) >= 1 ? tempDouble > 0 ? (Math.min(+Math.floor(tempDouble / 4294967296), 4294967295) | 0) >>> 0 : ~~+Math.ceil((tempDouble - +(~~tempDouble >>> 0)) / 4294967296) >>> 0 : 0)], HEAP32[ptr >> 2] = tempI64[0], HEAP32[ptr + 4 >> 2] = tempI64[1];
              break;
            case "float":
              HEAPF32[ptr >> 2] = value;
              break;
            case "double":
              HEAPF64[ptr >> 3] = value;
              break;
            default:
              abort("invalid type for setValue: " + type);
          }
        }
        function ___assert_fail(condition, filename, line, func) {
          abort("Assertion failed: " + UTF8ToString(condition) + ", at: " + [filename ? UTF8ToString(filename) : "unknown filename", line, func ? UTF8ToString(func) : "unknown function"]);
        }
        function ___cxa_allocate_exception(size) {
          return _malloc(size + 24) + 24;
        }
        var exceptionCaught = [];
        function exception_addRef(info) {
          info.add_ref();
        }
        function ___cxa_begin_catch(ptr) {
          var info = new ExceptionInfo(ptr);
          if (!info.get_caught()) {
            info.set_caught(true);
          }
          info.set_rethrown(false);
          exceptionCaught.push(info);
          exception_addRef(info);
          return info.get_exception_ptr();
        }
        var exceptionLast = 0;
        function ExceptionInfo(excPtr) {
          this.excPtr = excPtr;
          this.ptr = excPtr - 24;
          this.set_type = function (type) {
            HEAPU32[this.ptr + 4 >> 2] = type;
          };
          this.get_type = function () {
            return HEAPU32[this.ptr + 4 >> 2];
          };
          this.set_destructor = function (destructor) {
            HEAPU32[this.ptr + 8 >> 2] = destructor;
          };
          this.get_destructor = function () {
            return HEAPU32[this.ptr + 8 >> 2];
          };
          this.set_refcount = function (refcount) {
            HEAP32[this.ptr >> 2] = refcount;
          };
          this.set_caught = function (caught) {
            caught = caught ? 1 : 0;
            HEAP8[this.ptr + 12 >> 0] = caught;
          };
          this.get_caught = function () {
            return HEAP8[this.ptr + 12 >> 0] != 0;
          };
          this.set_rethrown = function (rethrown) {
            rethrown = rethrown ? 1 : 0;
            HEAP8[this.ptr + 13 >> 0] = rethrown;
          };
          this.get_rethrown = function () {
            return HEAP8[this.ptr + 13 >> 0] != 0;
          };
          this.init = function (type, destructor) {
            this.set_adjusted_ptr(0);
            this.set_type(type);
            this.set_destructor(destructor);
            this.set_refcount(0);
            this.set_caught(false);
            this.set_rethrown(false);
          };
          this.add_ref = function () {
            var value = HEAP32[this.ptr >> 2];
            HEAP32[this.ptr >> 2] = value + 1;
          };
          this.release_ref = function () {
            var prev = HEAP32[this.ptr >> 2];
            HEAP32[this.ptr >> 2] = prev - 1;
            return prev === 1;
          };
          this.set_adjusted_ptr = function (adjustedPtr) {
            HEAPU32[this.ptr + 16 >> 2] = adjustedPtr;
          };
          this.get_adjusted_ptr = function () {
            return HEAPU32[this.ptr + 16 >> 2];
          };
          this.get_exception_ptr = function () {
            var isPointer = ___cxa_is_pointer_type(this.get_type());
            if (isPointer) {
              return HEAPU32[this.excPtr >> 2];
            }
            var adjusted = this.get_adjusted_ptr();
            if (adjusted !== 0) return adjusted;
            return this.excPtr;
          };
        }
        function ___cxa_free_exception(ptr) {
          return _free(new ExceptionInfo(ptr).ptr);
        }
        function exception_decRef(info) {
          if (info.release_ref() && !info.get_rethrown()) {
            var destructor = info.get_destructor();
            if (destructor) {
              getWasmTableEntry(destructor)(info.excPtr);
            }
            ___cxa_free_exception(info.excPtr);
          }
        }
        function ___cxa_end_catch() {
          _setThrew(0);
          var info = exceptionCaught.pop();
          exception_decRef(info);
          exceptionLast = 0;
        }
        function ___resumeException(ptr) {
          if (!exceptionLast) {
            exceptionLast = ptr;
          }
          throw ptr;
        }
        function ___cxa_find_matching_catch_3() {
          var thrown = exceptionLast;
          if (!thrown) {
            setTempRet0(0);
            return 0;
          }
          var info = new ExceptionInfo(thrown);
          info.set_adjusted_ptr(thrown);
          var thrownType = info.get_type();
          if (!thrownType) {
            setTempRet0(0);
            return thrown;
          }
          var typeArray = Array.prototype.slice.call(arguments);
          for (var i = 0; i < typeArray.length; i++) {
            var caughtType = typeArray[i];
            if (caughtType === 0 || caughtType === thrownType) {
              break;
            }
            var adjusted_ptr_addr = info.ptr + 16;
            if (___cxa_can_catch(caughtType, thrownType, adjusted_ptr_addr)) {
              setTempRet0(caughtType);
              return thrown;
            }
          }
          setTempRet0(thrownType);
          return thrown;
        }
        function ___cxa_throw(ptr, type, destructor) {
          var info = new ExceptionInfo(ptr);
          info.init(type, destructor);
          exceptionLast = ptr;
          throw ptr;
        }
        var PATH = {
          isAbs: path => path.charAt(0) === "/",
          splitPath: filename => {
            var splitPathRe = /^(\/?|)([\s\S]*?)((?:\.{1,2}|[^\/]+?|)(\.[^.\/]*|))(?:[\/]*)$/;
            return splitPathRe.exec(filename).slice(1);
          },
          normalizeArray: (parts, allowAboveRoot) => {
            var up = 0;
            for (var i = parts.length - 1; i >= 0; i--) {
              var last = parts[i];
              if (last === ".") {
                parts.splice(i, 1);
              } else if (last === "..") {
                parts.splice(i, 1);
                up++;
              } else if (up) {
                parts.splice(i, 1);
                up--;
              }
            }
            if (allowAboveRoot) {
              for (; up; up--) {
                parts.unshift("..");
              }
            }
            return parts;
          },
          normalize: path => {
            var isAbsolute = PATH.isAbs(path),
              trailingSlash = path.substr(-1) === "/";
            path = PATH.normalizeArray(path.split("/").filter(p => !!p), !isAbsolute).join("/");
            if (!path && !isAbsolute) {
              path = ".";
            }
            if (path && trailingSlash) {
              path += "/";
            }
            return (isAbsolute ? "/" : "") + path;
          },
          dirname: path => {
            var result = PATH.splitPath(path),
              root = result[0],
              dir = result[1];
            if (!root && !dir) {
              return ".";
            }
            if (dir) {
              dir = dir.substr(0, dir.length - 1);
            }
            return root + dir;
          },
          basename: path => {
            if (path === "/") return "/";
            path = PATH.normalize(path);
            path = path.replace(/\/$/, "");
            var lastSlash = path.lastIndexOf("/");
            if (lastSlash === -1) return path;
            return path.substr(lastSlash + 1);
          },
          join: function () {
            var paths = Array.prototype.slice.call(arguments, 0);
            return PATH.normalize(paths.join("/"));
          },
          join2: (l, r) => {
            return PATH.normalize(l + "/" + r);
          }
        };
        function getRandomDevice() {
          if (typeof crypto == "object" && typeof crypto["getRandomValues"] == "function") {
            var randomBuffer = new Uint8Array(1);
            return function () {
              crypto.getRandomValues(randomBuffer);
              return randomBuffer[0];
            };
          } else if (ENVIRONMENT_IS_NODE) {
            try {
              var crypto_module = require("crypto");
              return function () {
                return crypto_module["randomBytes"](1)[0];
              };
            } catch (e) {}
          }
          return function () {
            abort("randomDevice");
          };
        }
        var PATH_FS = {
          resolve: function () {
            var resolvedPath = "",
              resolvedAbsolute = false;
            for (var i = arguments.length - 1; i >= -1 && !resolvedAbsolute; i--) {
              var path = i >= 0 ? arguments[i] : FS.cwd();
              if (typeof path != "string") {
                throw new TypeError("Arguments to path.resolve must be strings");
              } else if (!path) {
                return "";
              }
              resolvedPath = path + "/" + resolvedPath;
              resolvedAbsolute = PATH.isAbs(path);
            }
            resolvedPath = PATH.normalizeArray(resolvedPath.split("/").filter(p => !!p), !resolvedAbsolute).join("/");
            return (resolvedAbsolute ? "/" : "") + resolvedPath || ".";
          },
          relative: (from, to) => {
            from = PATH_FS.resolve(from).substr(1);
            to = PATH_FS.resolve(to).substr(1);
            function trim(arr) {
              var start = 0;
              for (; start < arr.length; start++) {
                if (arr[start] !== "") break;
              }
              var end = arr.length - 1;
              for (; end >= 0; end--) {
                if (arr[end] !== "") break;
              }
              if (start > end) return [];
              return arr.slice(start, end - start + 1);
            }
            var fromParts = trim(from.split("/"));
            var toParts = trim(to.split("/"));
            var length = Math.min(fromParts.length, toParts.length);
            var samePartsLength = length;
            for (var i = 0; i < length; i++) {
              if (fromParts[i] !== toParts[i]) {
                samePartsLength = i;
                break;
              }
            }
            var outputParts = [];
            for (var i = samePartsLength; i < fromParts.length; i++) {
              outputParts.push("..");
            }
            outputParts = outputParts.concat(toParts.slice(samePartsLength));
            return outputParts.join("/");
          }
        };
        var TTY = {
          ttys: [],
          init: function () {},
          shutdown: function () {},
          register: function (dev, ops) {
            TTY.ttys[dev] = {
              input: [],
              output: [],
              ops: ops
            };
            FS.registerDevice(dev, TTY.stream_ops);
          },
          stream_ops: {
            open: function (stream) {
              var tty = TTY.ttys[stream.node.rdev];
              if (!tty) {
                throw new FS.ErrnoError(43);
              }
              stream.tty = tty;
              stream.seekable = false;
            },
            close: function (stream) {
              stream.tty.ops.flush(stream.tty);
            },
            flush: function (stream) {
              stream.tty.ops.flush(stream.tty);
            },
            read: function (stream, buffer, offset, length, pos) {
              if (!stream.tty || !stream.tty.ops.get_char) {
                throw new FS.ErrnoError(60);
              }
              var bytesRead = 0;
              for (var i = 0; i < length; i++) {
                var result;
                try {
                  result = stream.tty.ops.get_char(stream.tty);
                } catch (e) {
                  throw new FS.ErrnoError(29);
                }
                if (result === undefined && bytesRead === 0) {
                  throw new FS.ErrnoError(6);
                }
                if (result === null || result === undefined) break;
                bytesRead++;
                buffer[offset + i] = result;
              }
              if (bytesRead) {
                stream.node.timestamp = Date.now();
              }
              return bytesRead;
            },
            write: function (stream, buffer, offset, length, pos) {
              if (!stream.tty || !stream.tty.ops.put_char) {
                throw new FS.ErrnoError(60);
              }
              try {
                for (var i = 0; i < length; i++) {
                  stream.tty.ops.put_char(stream.tty, buffer[offset + i]);
                }
              } catch (e) {
                throw new FS.ErrnoError(29);
              }
              if (length) {
                stream.node.timestamp = Date.now();
              }
              return i;
            }
          },
          default_tty_ops: {
            get_char: function (tty) {
              if (!tty.input.length) {
                var result = null;
                if (ENVIRONMENT_IS_NODE) {
                  var BUFSIZE = 256;
                  var buf = Buffer.alloc(BUFSIZE);
                  var bytesRead = 0;
                  try {
                    bytesRead = fs.readSync(browser$1.stdin.fd, buf, 0, BUFSIZE, -1);
                  } catch (e) {
                    if (e.toString().includes("EOF")) bytesRead = 0;else throw e;
                  }
                  if (bytesRead > 0) {
                    result = buf.slice(0, bytesRead).toString("utf-8");
                  } else {
                    result = null;
                  }
                } else if (typeof window != "undefined" && typeof window.prompt == "function") {
                  result = window.prompt("Input: ");
                  if (result !== null) {
                    result += "\n";
                  }
                } else if (typeof readline == "function") {
                  result = readline();
                  if (result !== null) {
                    result += "\n";
                  }
                }
                if (!result) {
                  return null;
                }
                tty.input = intArrayFromString(result, true);
              }
              return tty.input.shift();
            },
            put_char: function (tty, val) {
              if (val === null || val === 10) {
                out(UTF8ArrayToString(tty.output, 0));
                tty.output = [];
              } else {
                if (val != 0) tty.output.push(val);
              }
            },
            flush: function (tty) {
              if (tty.output && tty.output.length > 0) {
                out(UTF8ArrayToString(tty.output, 0));
                tty.output = [];
              }
            }
          },
          default_tty1_ops: {
            put_char: function (tty, val) {
              if (val === null || val === 10) {
                err(UTF8ArrayToString(tty.output, 0));
                tty.output = [];
              } else {
                if (val != 0) tty.output.push(val);
              }
            },
            flush: function (tty) {
              if (tty.output && tty.output.length > 0) {
                err(UTF8ArrayToString(tty.output, 0));
                tty.output = [];
              }
            }
          }
        };
        function zeroMemory(address, size) {
          HEAPU8.fill(0, address, address + size);
        }
        function alignMemory(size, alignment) {
          return Math.ceil(size / alignment) * alignment;
        }
        function mmapAlloc(size) {
          size = alignMemory(size, 65536);
          var ptr = _emscripten_builtin_memalign(65536, size);
          if (!ptr) return 0;
          zeroMemory(ptr, size);
          return ptr;
        }
        var MEMFS = {
          ops_table: null,
          mount: function (mount) {
            return MEMFS.createNode(null, "/", 16384 | 511, 0);
          },
          createNode: function (parent, name, mode, dev) {
            if (FS.isBlkdev(mode) || FS.isFIFO(mode)) {
              throw new FS.ErrnoError(63);
            }
            if (!MEMFS.ops_table) {
              MEMFS.ops_table = {
                dir: {
                  node: {
                    getattr: MEMFS.node_ops.getattr,
                    setattr: MEMFS.node_ops.setattr,
                    lookup: MEMFS.node_ops.lookup,
                    mknod: MEMFS.node_ops.mknod,
                    rename: MEMFS.node_ops.rename,
                    unlink: MEMFS.node_ops.unlink,
                    rmdir: MEMFS.node_ops.rmdir,
                    readdir: MEMFS.node_ops.readdir,
                    symlink: MEMFS.node_ops.symlink
                  },
                  stream: {
                    llseek: MEMFS.stream_ops.llseek
                  }
                },
                file: {
                  node: {
                    getattr: MEMFS.node_ops.getattr,
                    setattr: MEMFS.node_ops.setattr
                  },
                  stream: {
                    llseek: MEMFS.stream_ops.llseek,
                    read: MEMFS.stream_ops.read,
                    write: MEMFS.stream_ops.write,
                    allocate: MEMFS.stream_ops.allocate,
                    mmap: MEMFS.stream_ops.mmap,
                    msync: MEMFS.stream_ops.msync
                  }
                },
                link: {
                  node: {
                    getattr: MEMFS.node_ops.getattr,
                    setattr: MEMFS.node_ops.setattr,
                    readlink: MEMFS.node_ops.readlink
                  },
                  stream: {}
                },
                chrdev: {
                  node: {
                    getattr: MEMFS.node_ops.getattr,
                    setattr: MEMFS.node_ops.setattr
                  },
                  stream: FS.chrdev_stream_ops
                }
              };
            }
            var node = FS.createNode(parent, name, mode, dev);
            if (FS.isDir(node.mode)) {
              node.node_ops = MEMFS.ops_table.dir.node;
              node.stream_ops = MEMFS.ops_table.dir.stream;
              node.contents = {};
            } else if (FS.isFile(node.mode)) {
              node.node_ops = MEMFS.ops_table.file.node;
              node.stream_ops = MEMFS.ops_table.file.stream;
              node.usedBytes = 0;
              node.contents = null;
            } else if (FS.isLink(node.mode)) {
              node.node_ops = MEMFS.ops_table.link.node;
              node.stream_ops = MEMFS.ops_table.link.stream;
            } else if (FS.isChrdev(node.mode)) {
              node.node_ops = MEMFS.ops_table.chrdev.node;
              node.stream_ops = MEMFS.ops_table.chrdev.stream;
            }
            node.timestamp = Date.now();
            if (parent) {
              parent.contents[name] = node;
              parent.timestamp = node.timestamp;
            }
            return node;
          },
          getFileDataAsTypedArray: function (node) {
            if (!node.contents) return new Uint8Array(0);
            if (node.contents.subarray) return node.contents.subarray(0, node.usedBytes);
            return new Uint8Array(node.contents);
          },
          expandFileStorage: function (node, newCapacity) {
            var prevCapacity = node.contents ? node.contents.length : 0;
            if (prevCapacity >= newCapacity) return;
            var CAPACITY_DOUBLING_MAX = 1024 * 1024;
            newCapacity = Math.max(newCapacity, prevCapacity * (prevCapacity < CAPACITY_DOUBLING_MAX ? 2 : 1.125) >>> 0);
            if (prevCapacity != 0) newCapacity = Math.max(newCapacity, 256);
            var oldContents = node.contents;
            node.contents = new Uint8Array(newCapacity);
            if (node.usedBytes > 0) node.contents.set(oldContents.subarray(0, node.usedBytes), 0);
          },
          resizeFileStorage: function (node, newSize) {
            if (node.usedBytes == newSize) return;
            if (newSize == 0) {
              node.contents = null;
              node.usedBytes = 0;
            } else {
              var oldContents = node.contents;
              node.contents = new Uint8Array(newSize);
              if (oldContents) {
                node.contents.set(oldContents.subarray(0, Math.min(newSize, node.usedBytes)));
              }
              node.usedBytes = newSize;
            }
          },
          node_ops: {
            getattr: function (node) {
              var attr = {};
              attr.dev = FS.isChrdev(node.mode) ? node.id : 1;
              attr.ino = node.id;
              attr.mode = node.mode;
              attr.nlink = 1;
              attr.uid = 0;
              attr.gid = 0;
              attr.rdev = node.rdev;
              if (FS.isDir(node.mode)) {
                attr.size = 4096;
              } else if (FS.isFile(node.mode)) {
                attr.size = node.usedBytes;
              } else if (FS.isLink(node.mode)) {
                attr.size = node.link.length;
              } else {
                attr.size = 0;
              }
              attr.atime = new Date(node.timestamp);
              attr.mtime = new Date(node.timestamp);
              attr.ctime = new Date(node.timestamp);
              attr.blksize = 4096;
              attr.blocks = Math.ceil(attr.size / attr.blksize);
              return attr;
            },
            setattr: function (node, attr) {
              if (attr.mode !== undefined) {
                node.mode = attr.mode;
              }
              if (attr.timestamp !== undefined) {
                node.timestamp = attr.timestamp;
              }
              if (attr.size !== undefined) {
                MEMFS.resizeFileStorage(node, attr.size);
              }
            },
            lookup: function (parent, name) {
              throw FS.genericErrors[44];
            },
            mknod: function (parent, name, mode, dev) {
              return MEMFS.createNode(parent, name, mode, dev);
            },
            rename: function (old_node, new_dir, new_name) {
              if (FS.isDir(old_node.mode)) {
                var new_node;
                try {
                  new_node = FS.lookupNode(new_dir, new_name);
                } catch (e) {}
                if (new_node) {
                  for (var i in new_node.contents) {
                    throw new FS.ErrnoError(55);
                  }
                }
              }
              delete old_node.parent.contents[old_node.name];
              old_node.parent.timestamp = Date.now();
              old_node.name = new_name;
              new_dir.contents[new_name] = old_node;
              new_dir.timestamp = old_node.parent.timestamp;
              old_node.parent = new_dir;
            },
            unlink: function (parent, name) {
              delete parent.contents[name];
              parent.timestamp = Date.now();
            },
            rmdir: function (parent, name) {
              var node = FS.lookupNode(parent, name);
              for (var i in node.contents) {
                throw new FS.ErrnoError(55);
              }
              delete parent.contents[name];
              parent.timestamp = Date.now();
            },
            readdir: function (node) {
              var entries = [".", ".."];
              for (var key in node.contents) {
                if (!node.contents.hasOwnProperty(key)) {
                  continue;
                }
                entries.push(key);
              }
              return entries;
            },
            symlink: function (parent, newname, oldpath) {
              var node = MEMFS.createNode(parent, newname, 511 | 40960, 0);
              node.link = oldpath;
              return node;
            },
            readlink: function (node) {
              if (!FS.isLink(node.mode)) {
                throw new FS.ErrnoError(28);
              }
              return node.link;
            }
          },
          stream_ops: {
            read: function (stream, buffer, offset, length, position) {
              var contents = stream.node.contents;
              if (position >= stream.node.usedBytes) return 0;
              var size = Math.min(stream.node.usedBytes - position, length);
              if (size > 8 && contents.subarray) {
                buffer.set(contents.subarray(position, position + size), offset);
              } else {
                for (var i = 0; i < size; i++) buffer[offset + i] = contents[position + i];
              }
              return size;
            },
            write: function (stream, buffer, offset, length, position, canOwn) {
              if (buffer.buffer === HEAP8.buffer) {
                canOwn = false;
              }
              if (!length) return 0;
              var node = stream.node;
              node.timestamp = Date.now();
              if (buffer.subarray && (!node.contents || node.contents.subarray)) {
                if (canOwn) {
                  node.contents = buffer.subarray(offset, offset + length);
                  node.usedBytes = length;
                  return length;
                } else if (node.usedBytes === 0 && position === 0) {
                  node.contents = buffer.slice(offset, offset + length);
                  node.usedBytes = length;
                  return length;
                } else if (position + length <= node.usedBytes) {
                  node.contents.set(buffer.subarray(offset, offset + length), position);
                  return length;
                }
              }
              MEMFS.expandFileStorage(node, position + length);
              if (node.contents.subarray && buffer.subarray) {
                node.contents.set(buffer.subarray(offset, offset + length), position);
              } else {
                for (var i = 0; i < length; i++) {
                  node.contents[position + i] = buffer[offset + i];
                }
              }
              node.usedBytes = Math.max(node.usedBytes, position + length);
              return length;
            },
            llseek: function (stream, offset, whence) {
              var position = offset;
              if (whence === 1) {
                position += stream.position;
              } else if (whence === 2) {
                if (FS.isFile(stream.node.mode)) {
                  position += stream.node.usedBytes;
                }
              }
              if (position < 0) {
                throw new FS.ErrnoError(28);
              }
              return position;
            },
            allocate: function (stream, offset, length) {
              MEMFS.expandFileStorage(stream.node, offset + length);
              stream.node.usedBytes = Math.max(stream.node.usedBytes, offset + length);
            },
            mmap: function (stream, length, position, prot, flags) {
              if (!FS.isFile(stream.node.mode)) {
                throw new FS.ErrnoError(43);
              }
              var ptr;
              var allocated;
              var contents = stream.node.contents;
              if (!(flags & 2) && contents.buffer === buffer) {
                allocated = false;
                ptr = contents.byteOffset;
              } else {
                if (position > 0 || position + length < contents.length) {
                  if (contents.subarray) {
                    contents = contents.subarray(position, position + length);
                  } else {
                    contents = Array.prototype.slice.call(contents, position, position + length);
                  }
                }
                allocated = true;
                ptr = mmapAlloc(length);
                if (!ptr) {
                  throw new FS.ErrnoError(48);
                }
                HEAP8.set(contents, ptr);
              }
              return {
                ptr: ptr,
                allocated: allocated
              };
            },
            msync: function (stream, buffer, offset, length, mmapFlags) {
              if (!FS.isFile(stream.node.mode)) {
                throw new FS.ErrnoError(43);
              }
              if (mmapFlags & 2) {
                return 0;
              }
              MEMFS.stream_ops.write(stream, buffer, 0, length, offset, false);
              return 0;
            }
          }
        };
        function asyncLoad(url, onload, onerror, noRunDep) {
          var dep = !noRunDep ? getUniqueRunDependency("al " + url) : "";
          readAsync(url, function (arrayBuffer) {
            assert(arrayBuffer, 'Loading data file "' + url + '" failed (no arrayBuffer).');
            onload(new Uint8Array(arrayBuffer));
            if (dep) removeRunDependency();
          }, function (event) {
            if (onerror) {
              onerror();
            } else {
              throw 'Loading data file "' + url + '" failed.';
            }
          });
          if (dep) addRunDependency();
        }
        var FS = {
          root: null,
          mounts: [],
          devices: {},
          streams: [],
          nextInode: 1,
          nameTable: null,
          currentPath: "/",
          initialized: false,
          ignorePermissions: true,
          ErrnoError: null,
          genericErrors: {},
          filesystems: null,
          syncFSRequests: 0,
          lookupPath: (path, opts = {}) => {
            path = PATH_FS.resolve(FS.cwd(), path);
            if (!path) return {
              path: "",
              node: null
            };
            var defaults = {
              follow_mount: true,
              recurse_count: 0
            };
            opts = Object.assign(defaults, opts);
            if (opts.recurse_count > 8) {
              throw new FS.ErrnoError(32);
            }
            var parts = PATH.normalizeArray(path.split("/").filter(p => !!p), false);
            var current = FS.root;
            var current_path = "/";
            for (var i = 0; i < parts.length; i++) {
              var islast = i === parts.length - 1;
              if (islast && opts.parent) {
                break;
              }
              current = FS.lookupNode(current, parts[i]);
              current_path = PATH.join2(current_path, parts[i]);
              if (FS.isMountpoint(current)) {
                if (!islast || islast && opts.follow_mount) {
                  current = current.mounted.root;
                }
              }
              if (!islast || opts.follow) {
                var count = 0;
                while (FS.isLink(current.mode)) {
                  var link = FS.readlink(current_path);
                  current_path = PATH_FS.resolve(PATH.dirname(current_path), link);
                  var lookup = FS.lookupPath(current_path, {
                    recurse_count: opts.recurse_count + 1
                  });
                  current = lookup.node;
                  if (count++ > 40) {
                    throw new FS.ErrnoError(32);
                  }
                }
              }
            }
            return {
              path: current_path,
              node: current
            };
          },
          getPath: node => {
            var path;
            while (true) {
              if (FS.isRoot(node)) {
                var mount = node.mount.mountpoint;
                if (!path) return mount;
                return mount[mount.length - 1] !== "/" ? mount + "/" + path : mount + path;
              }
              path = path ? node.name + "/" + path : node.name;
              node = node.parent;
            }
          },
          hashName: (parentid, name) => {
            var hash = 0;
            for (var i = 0; i < name.length; i++) {
              hash = (hash << 5) - hash + name.charCodeAt(i) | 0;
            }
            return (parentid + hash >>> 0) % FS.nameTable.length;
          },
          hashAddNode: node => {
            var hash = FS.hashName(node.parent.id, node.name);
            node.name_next = FS.nameTable[hash];
            FS.nameTable[hash] = node;
          },
          hashRemoveNode: node => {
            var hash = FS.hashName(node.parent.id, node.name);
            if (FS.nameTable[hash] === node) {
              FS.nameTable[hash] = node.name_next;
            } else {
              var current = FS.nameTable[hash];
              while (current) {
                if (current.name_next === node) {
                  current.name_next = node.name_next;
                  break;
                }
                current = current.name_next;
              }
            }
          },
          lookupNode: (parent, name) => {
            var errCode = FS.mayLookup(parent);
            if (errCode) {
              throw new FS.ErrnoError(errCode, parent);
            }
            var hash = FS.hashName(parent.id, name);
            for (var node = FS.nameTable[hash]; node; node = node.name_next) {
              var nodeName = node.name;
              if (node.parent.id === parent.id && nodeName === name) {
                return node;
              }
            }
            return FS.lookup(parent, name);
          },
          createNode: (parent, name, mode, rdev) => {
            var node = new FS.FSNode(parent, name, mode, rdev);
            FS.hashAddNode(node);
            return node;
          },
          destroyNode: node => {
            FS.hashRemoveNode(node);
          },
          isRoot: node => {
            return node === node.parent;
          },
          isMountpoint: node => {
            return !!node.mounted;
          },
          isFile: mode => {
            return (mode & 61440) === 32768;
          },
          isDir: mode => {
            return (mode & 61440) === 16384;
          },
          isLink: mode => {
            return (mode & 61440) === 40960;
          },
          isChrdev: mode => {
            return (mode & 61440) === 8192;
          },
          isBlkdev: mode => {
            return (mode & 61440) === 24576;
          },
          isFIFO: mode => {
            return (mode & 61440) === 4096;
          },
          isSocket: mode => {
            return (mode & 49152) === 49152;
          },
          flagModes: {
            "r": 0,
            "r+": 2,
            "w": 577,
            "w+": 578,
            "a": 1089,
            "a+": 1090
          },
          modeStringToFlags: str => {
            var flags = FS.flagModes[str];
            if (typeof flags == "undefined") {
              throw new Error("Unknown file open mode: " + str);
            }
            return flags;
          },
          flagsToPermissionString: flag => {
            var perms = ["r", "w", "rw"][flag & 3];
            if (flag & 512) {
              perms += "w";
            }
            return perms;
          },
          nodePermissions: (node, perms) => {
            if (FS.ignorePermissions) {
              return 0;
            }
            if (perms.includes("r") && !(node.mode & 292)) {
              return 2;
            } else if (perms.includes("w") && !(node.mode & 146)) {
              return 2;
            } else if (perms.includes("x") && !(node.mode & 73)) {
              return 2;
            }
            return 0;
          },
          mayLookup: dir => {
            var errCode = FS.nodePermissions(dir, "x");
            if (errCode) return errCode;
            if (!dir.node_ops.lookup) return 2;
            return 0;
          },
          mayCreate: (dir, name) => {
            try {
              var node = FS.lookupNode(dir, name);
              return 20;
            } catch (e) {}
            return FS.nodePermissions(dir, "wx");
          },
          mayDelete: (dir, name, isdir) => {
            var node;
            try {
              node = FS.lookupNode(dir, name);
            } catch (e) {
              return e.errno;
            }
            var errCode = FS.nodePermissions(dir, "wx");
            if (errCode) {
              return errCode;
            }
            if (isdir) {
              if (!FS.isDir(node.mode)) {
                return 54;
              }
              if (FS.isRoot(node) || FS.getPath(node) === FS.cwd()) {
                return 10;
              }
            } else {
              if (FS.isDir(node.mode)) {
                return 31;
              }
            }
            return 0;
          },
          mayOpen: (node, flags) => {
            if (!node) {
              return 44;
            }
            if (FS.isLink(node.mode)) {
              return 32;
            } else if (FS.isDir(node.mode)) {
              if (FS.flagsToPermissionString(flags) !== "r" || flags & 512) {
                return 31;
              }
            }
            return FS.nodePermissions(node, FS.flagsToPermissionString(flags));
          },
          MAX_OPEN_FDS: 4096,
          nextfd: (fd_start = 0, fd_end = FS.MAX_OPEN_FDS) => {
            for (var fd = fd_start; fd <= fd_end; fd++) {
              if (!FS.streams[fd]) {
                return fd;
              }
            }
            throw new FS.ErrnoError(33);
          },
          getStream: fd => FS.streams[fd],
          createStream: (stream, fd_start, fd_end) => {
            if (!FS.FSStream) {
              FS.FSStream = function () {
                this.shared = {};
              };
              FS.FSStream.prototype = {
                object: {
                  get: function () {
                    return this.node;
                  },
                  set: function (val) {
                    this.node = val;
                  }
                },
                isRead: {
                  get: function () {
                    return (this.flags & 2097155) !== 1;
                  }
                },
                isWrite: {
                  get: function () {
                    return (this.flags & 2097155) !== 0;
                  }
                },
                isAppend: {
                  get: function () {
                    return this.flags & 1024;
                  }
                },
                flags: {
                  get: function () {
                    return this.shared.flags;
                  },
                  set: function (val) {
                    this.shared.flags = val;
                  }
                },
                position: {
                  get function() {
                    return this.shared.position;
                  },
                  set: function (val) {
                    this.shared.position = val;
                  }
                }
              };
            }
            stream = Object.assign(new FS.FSStream(), stream);
            var fd = FS.nextfd(fd_start, fd_end);
            stream.fd = fd;
            FS.streams[fd] = stream;
            return stream;
          },
          closeStream: fd => {
            FS.streams[fd] = null;
          },
          chrdev_stream_ops: {
            open: stream => {
              var device = FS.getDevice(stream.node.rdev);
              stream.stream_ops = device.stream_ops;
              if (stream.stream_ops.open) {
                stream.stream_ops.open(stream);
              }
            },
            llseek: () => {
              throw new FS.ErrnoError(70);
            }
          },
          major: dev => dev >> 8,
          minor: dev => dev & 255,
          makedev: (ma, mi) => ma << 8 | mi,
          registerDevice: (dev, ops) => {
            FS.devices[dev] = {
              stream_ops: ops
            };
          },
          getDevice: dev => FS.devices[dev],
          getMounts: mount => {
            var mounts = [];
            var check = [mount];
            while (check.length) {
              var m = check.pop();
              mounts.push(m);
              check.push.apply(check, m.mounts);
            }
            return mounts;
          },
          syncfs: (populate, callback) => {
            if (typeof populate == "function") {
              callback = populate;
              populate = false;
            }
            FS.syncFSRequests++;
            if (FS.syncFSRequests > 1) {
              err("warning: " + FS.syncFSRequests + " FS.syncfs operations in flight at once, probably just doing extra work");
            }
            var mounts = FS.getMounts(FS.root.mount);
            var completed = 0;
            function doCallback(errCode) {
              FS.syncFSRequests--;
              return callback(errCode);
            }
            function done(errCode) {
              if (errCode) {
                if (!done.errored) {
                  done.errored = true;
                  return doCallback(errCode);
                }
                return;
              }
              if (++completed >= mounts.length) {
                doCallback(null);
              }
            }
            mounts.forEach(mount => {
              if (!mount.type.syncfs) {
                return done(null);
              }
              mount.type.syncfs(mount, populate, done);
            });
          },
          mount: (type, opts, mountpoint) => {
            var root = mountpoint === "/";
            var pseudo = !mountpoint;
            var node;
            if (root && FS.root) {
              throw new FS.ErrnoError(10);
            } else if (!root && !pseudo) {
              var lookup = FS.lookupPath(mountpoint, {
                follow_mount: false
              });
              mountpoint = lookup.path;
              node = lookup.node;
              if (FS.isMountpoint(node)) {
                throw new FS.ErrnoError(10);
              }
              if (!FS.isDir(node.mode)) {
                throw new FS.ErrnoError(54);
              }
            }
            var mount = {
              type: type,
              opts: opts,
              mountpoint: mountpoint,
              mounts: []
            };
            var mountRoot = type.mount(mount);
            mountRoot.mount = mount;
            mount.root = mountRoot;
            if (root) {
              FS.root = mountRoot;
            } else if (node) {
              node.mounted = mount;
              if (node.mount) {
                node.mount.mounts.push(mount);
              }
            }
            return mountRoot;
          },
          unmount: mountpoint => {
            var lookup = FS.lookupPath(mountpoint, {
              follow_mount: false
            });
            if (!FS.isMountpoint(lookup.node)) {
              throw new FS.ErrnoError(28);
            }
            var node = lookup.node;
            var mount = node.mounted;
            var mounts = FS.getMounts(mount);
            Object.keys(FS.nameTable).forEach(hash => {
              var current = FS.nameTable[hash];
              while (current) {
                var next = current.name_next;
                if (mounts.includes(current.mount)) {
                  FS.destroyNode(current);
                }
                current = next;
              }
            });
            node.mounted = null;
            var idx = node.mount.mounts.indexOf(mount);
            node.mount.mounts.splice(idx, 1);
          },
          lookup: (parent, name) => {
            return parent.node_ops.lookup(parent, name);
          },
          mknod: (path, mode, dev) => {
            var lookup = FS.lookupPath(path, {
              parent: true
            });
            var parent = lookup.node;
            var name = PATH.basename(path);
            if (!name || name === "." || name === "..") {
              throw new FS.ErrnoError(28);
            }
            var errCode = FS.mayCreate(parent, name);
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            if (!parent.node_ops.mknod) {
              throw new FS.ErrnoError(63);
            }
            return parent.node_ops.mknod(parent, name, mode, dev);
          },
          create: (path, mode) => {
            mode = mode !== undefined ? mode : 438;
            mode &= 4095;
            mode |= 32768;
            return FS.mknod(path, mode, 0);
          },
          mkdir: (path, mode) => {
            mode = mode !== undefined ? mode : 511;
            mode &= 511 | 512;
            mode |= 16384;
            return FS.mknod(path, mode, 0);
          },
          mkdirTree: (path, mode) => {
            var dirs = path.split("/");
            var d = "";
            for (var i = 0; i < dirs.length; ++i) {
              if (!dirs[i]) continue;
              d += "/" + dirs[i];
              try {
                FS.mkdir(d, mode);
              } catch (e) {
                if (e.errno != 20) throw e;
              }
            }
          },
          mkdev: (path, mode, dev) => {
            if (typeof dev == "undefined") {
              dev = mode;
              mode = 438;
            }
            mode |= 8192;
            return FS.mknod(path, mode, dev);
          },
          symlink: (oldpath, newpath) => {
            if (!PATH_FS.resolve(oldpath)) {
              throw new FS.ErrnoError(44);
            }
            var lookup = FS.lookupPath(newpath, {
              parent: true
            });
            var parent = lookup.node;
            if (!parent) {
              throw new FS.ErrnoError(44);
            }
            var newname = PATH.basename(newpath);
            var errCode = FS.mayCreate(parent, newname);
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            if (!parent.node_ops.symlink) {
              throw new FS.ErrnoError(63);
            }
            return parent.node_ops.symlink(parent, newname, oldpath);
          },
          rename: (old_path, new_path) => {
            var old_dirname = PATH.dirname(old_path);
            var new_dirname = PATH.dirname(new_path);
            var old_name = PATH.basename(old_path);
            var new_name = PATH.basename(new_path);
            var lookup, old_dir, new_dir;
            lookup = FS.lookupPath(old_path, {
              parent: true
            });
            old_dir = lookup.node;
            lookup = FS.lookupPath(new_path, {
              parent: true
            });
            new_dir = lookup.node;
            if (!old_dir || !new_dir) throw new FS.ErrnoError(44);
            if (old_dir.mount !== new_dir.mount) {
              throw new FS.ErrnoError(75);
            }
            var old_node = FS.lookupNode(old_dir, old_name);
            var relative = PATH_FS.relative(old_path, new_dirname);
            if (relative.charAt(0) !== ".") {
              throw new FS.ErrnoError(28);
            }
            relative = PATH_FS.relative(new_path, old_dirname);
            if (relative.charAt(0) !== ".") {
              throw new FS.ErrnoError(55);
            }
            var new_node;
            try {
              new_node = FS.lookupNode(new_dir, new_name);
            } catch (e) {}
            if (old_node === new_node) {
              return;
            }
            var isdir = FS.isDir(old_node.mode);
            var errCode = FS.mayDelete(old_dir, old_name, isdir);
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            errCode = new_node ? FS.mayDelete(new_dir, new_name, isdir) : FS.mayCreate(new_dir, new_name);
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            if (!old_dir.node_ops.rename) {
              throw new FS.ErrnoError(63);
            }
            if (FS.isMountpoint(old_node) || new_node && FS.isMountpoint(new_node)) {
              throw new FS.ErrnoError(10);
            }
            if (new_dir !== old_dir) {
              errCode = FS.nodePermissions(old_dir, "w");
              if (errCode) {
                throw new FS.ErrnoError(errCode);
              }
            }
            FS.hashRemoveNode(old_node);
            try {
              old_dir.node_ops.rename(old_node, new_dir, new_name);
            } catch (e) {
              throw e;
            } finally {
              FS.hashAddNode(old_node);
            }
          },
          rmdir: path => {
            var lookup = FS.lookupPath(path, {
              parent: true
            });
            var parent = lookup.node;
            var name = PATH.basename(path);
            var node = FS.lookupNode(parent, name);
            var errCode = FS.mayDelete(parent, name, true);
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            if (!parent.node_ops.rmdir) {
              throw new FS.ErrnoError(63);
            }
            if (FS.isMountpoint(node)) {
              throw new FS.ErrnoError(10);
            }
            parent.node_ops.rmdir(parent, name);
            FS.destroyNode(node);
          },
          readdir: path => {
            var lookup = FS.lookupPath(path, {
              follow: true
            });
            var node = lookup.node;
            if (!node.node_ops.readdir) {
              throw new FS.ErrnoError(54);
            }
            return node.node_ops.readdir(node);
          },
          unlink: path => {
            var lookup = FS.lookupPath(path, {
              parent: true
            });
            var parent = lookup.node;
            if (!parent) {
              throw new FS.ErrnoError(44);
            }
            var name = PATH.basename(path);
            var node = FS.lookupNode(parent, name);
            var errCode = FS.mayDelete(parent, name, false);
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            if (!parent.node_ops.unlink) {
              throw new FS.ErrnoError(63);
            }
            if (FS.isMountpoint(node)) {
              throw new FS.ErrnoError(10);
            }
            parent.node_ops.unlink(parent, name);
            FS.destroyNode(node);
          },
          readlink: path => {
            var lookup = FS.lookupPath(path);
            var link = lookup.node;
            if (!link) {
              throw new FS.ErrnoError(44);
            }
            if (!link.node_ops.readlink) {
              throw new FS.ErrnoError(28);
            }
            return PATH_FS.resolve(FS.getPath(link.parent), link.node_ops.readlink(link));
          },
          stat: (path, dontFollow) => {
            var lookup = FS.lookupPath(path, {
              follow: !dontFollow
            });
            var node = lookup.node;
            if (!node) {
              throw new FS.ErrnoError(44);
            }
            if (!node.node_ops.getattr) {
              throw new FS.ErrnoError(63);
            }
            return node.node_ops.getattr(node);
          },
          lstat: path => {
            return FS.stat(path, true);
          },
          chmod: (path, mode, dontFollow) => {
            var node;
            if (typeof path == "string") {
              var lookup = FS.lookupPath(path, {
                follow: !dontFollow
              });
              node = lookup.node;
            } else {
              node = path;
            }
            if (!node.node_ops.setattr) {
              throw new FS.ErrnoError(63);
            }
            node.node_ops.setattr(node, {
              mode: mode & 4095 | node.mode & ~4095,
              timestamp: Date.now()
            });
          },
          lchmod: (path, mode) => {
            FS.chmod(path, mode, true);
          },
          fchmod: (fd, mode) => {
            var stream = FS.getStream(fd);
            if (!stream) {
              throw new FS.ErrnoError(8);
            }
            FS.chmod(stream.node, mode);
          },
          chown: (path, uid, gid, dontFollow) => {
            var node;
            if (typeof path == "string") {
              var lookup = FS.lookupPath(path, {
                follow: !dontFollow
              });
              node = lookup.node;
            } else {
              node = path;
            }
            if (!node.node_ops.setattr) {
              throw new FS.ErrnoError(63);
            }
            node.node_ops.setattr(node, {
              timestamp: Date.now()
            });
          },
          lchown: (path, uid, gid) => {
            FS.chown(path, uid, gid, true);
          },
          fchown: (fd, uid, gid) => {
            var stream = FS.getStream(fd);
            if (!stream) {
              throw new FS.ErrnoError(8);
            }
            FS.chown(stream.node, uid, gid);
          },
          truncate: (path, len) => {
            if (len < 0) {
              throw new FS.ErrnoError(28);
            }
            var node;
            if (typeof path == "string") {
              var lookup = FS.lookupPath(path, {
                follow: true
              });
              node = lookup.node;
            } else {
              node = path;
            }
            if (!node.node_ops.setattr) {
              throw new FS.ErrnoError(63);
            }
            if (FS.isDir(node.mode)) {
              throw new FS.ErrnoError(31);
            }
            if (!FS.isFile(node.mode)) {
              throw new FS.ErrnoError(28);
            }
            var errCode = FS.nodePermissions(node, "w");
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            node.node_ops.setattr(node, {
              size: len,
              timestamp: Date.now()
            });
          },
          ftruncate: (fd, len) => {
            var stream = FS.getStream(fd);
            if (!stream) {
              throw new FS.ErrnoError(8);
            }
            if ((stream.flags & 2097155) === 0) {
              throw new FS.ErrnoError(28);
            }
            FS.truncate(stream.node, len);
          },
          utime: (path, atime, mtime) => {
            var lookup = FS.lookupPath(path, {
              follow: true
            });
            var node = lookup.node;
            node.node_ops.setattr(node, {
              timestamp: Math.max(atime, mtime)
            });
          },
          open: (path, flags, mode) => {
            if (path === "") {
              throw new FS.ErrnoError(44);
            }
            flags = typeof flags == "string" ? FS.modeStringToFlags(flags) : flags;
            mode = typeof mode == "undefined" ? 438 : mode;
            if (flags & 64) {
              mode = mode & 4095 | 32768;
            } else {
              mode = 0;
            }
            var node;
            if (typeof path == "object") {
              node = path;
            } else {
              path = PATH.normalize(path);
              try {
                var lookup = FS.lookupPath(path, {
                  follow: !(flags & 131072)
                });
                node = lookup.node;
              } catch (e) {}
            }
            var created = false;
            if (flags & 64) {
              if (node) {
                if (flags & 128) {
                  throw new FS.ErrnoError(20);
                }
              } else {
                node = FS.mknod(path, mode, 0);
                created = true;
              }
            }
            if (!node) {
              throw new FS.ErrnoError(44);
            }
            if (FS.isChrdev(node.mode)) {
              flags &= ~512;
            }
            if (flags & 65536 && !FS.isDir(node.mode)) {
              throw new FS.ErrnoError(54);
            }
            if (!created) {
              var errCode = FS.mayOpen(node, flags);
              if (errCode) {
                throw new FS.ErrnoError(errCode);
              }
            }
            if (flags & 512 && !created) {
              FS.truncate(node, 0);
            }
            flags &= ~(128 | 512 | 131072);
            var stream = FS.createStream({
              node: node,
              path: FS.getPath(node),
              flags: flags,
              seekable: true,
              position: 0,
              stream_ops: node.stream_ops,
              ungotten: [],
              error: false
            });
            if (stream.stream_ops.open) {
              stream.stream_ops.open(stream);
            }
            if (Module["logReadFiles"] && !(flags & 1)) {
              if (!FS.readFiles) FS.readFiles = {};
              if (!(path in FS.readFiles)) {
                FS.readFiles[path] = 1;
              }
            }
            return stream;
          },
          close: stream => {
            if (FS.isClosed(stream)) {
              throw new FS.ErrnoError(8);
            }
            if (stream.getdents) stream.getdents = null;
            try {
              if (stream.stream_ops.close) {
                stream.stream_ops.close(stream);
              }
            } catch (e) {
              throw e;
            } finally {
              FS.closeStream(stream.fd);
            }
            stream.fd = null;
          },
          isClosed: stream => {
            return stream.fd === null;
          },
          llseek: (stream, offset, whence) => {
            if (FS.isClosed(stream)) {
              throw new FS.ErrnoError(8);
            }
            if (!stream.seekable || !stream.stream_ops.llseek) {
              throw new FS.ErrnoError(70);
            }
            if (whence != 0 && whence != 1 && whence != 2) {
              throw new FS.ErrnoError(28);
            }
            stream.position = stream.stream_ops.llseek(stream, offset, whence);
            stream.ungotten = [];
            return stream.position;
          },
          read: (stream, buffer, offset, length, position) => {
            if (length < 0 || position < 0) {
              throw new FS.ErrnoError(28);
            }
            if (FS.isClosed(stream)) {
              throw new FS.ErrnoError(8);
            }
            if ((stream.flags & 2097155) === 1) {
              throw new FS.ErrnoError(8);
            }
            if (FS.isDir(stream.node.mode)) {
              throw new FS.ErrnoError(31);
            }
            if (!stream.stream_ops.read) {
              throw new FS.ErrnoError(28);
            }
            var seeking = typeof position != "undefined";
            if (!seeking) {
              position = stream.position;
            } else if (!stream.seekable) {
              throw new FS.ErrnoError(70);
            }
            var bytesRead = stream.stream_ops.read(stream, buffer, offset, length, position);
            if (!seeking) stream.position += bytesRead;
            return bytesRead;
          },
          write: (stream, buffer, offset, length, position, canOwn) => {
            if (length < 0 || position < 0) {
              throw new FS.ErrnoError(28);
            }
            if (FS.isClosed(stream)) {
              throw new FS.ErrnoError(8);
            }
            if ((stream.flags & 2097155) === 0) {
              throw new FS.ErrnoError(8);
            }
            if (FS.isDir(stream.node.mode)) {
              throw new FS.ErrnoError(31);
            }
            if (!stream.stream_ops.write) {
              throw new FS.ErrnoError(28);
            }
            if (stream.seekable && stream.flags & 1024) {
              FS.llseek(stream, 0, 2);
            }
            var seeking = typeof position != "undefined";
            if (!seeking) {
              position = stream.position;
            } else if (!stream.seekable) {
              throw new FS.ErrnoError(70);
            }
            var bytesWritten = stream.stream_ops.write(stream, buffer, offset, length, position, canOwn);
            if (!seeking) stream.position += bytesWritten;
            return bytesWritten;
          },
          allocate: (stream, offset, length) => {
            if (FS.isClosed(stream)) {
              throw new FS.ErrnoError(8);
            }
            if (offset < 0 || length <= 0) {
              throw new FS.ErrnoError(28);
            }
            if ((stream.flags & 2097155) === 0) {
              throw new FS.ErrnoError(8);
            }
            if (!FS.isFile(stream.node.mode) && !FS.isDir(stream.node.mode)) {
              throw new FS.ErrnoError(43);
            }
            if (!stream.stream_ops.allocate) {
              throw new FS.ErrnoError(138);
            }
            stream.stream_ops.allocate(stream, offset, length);
          },
          mmap: (stream, length, position, prot, flags) => {
            if ((prot & 2) !== 0 && (flags & 2) === 0 && (stream.flags & 2097155) !== 2) {
              throw new FS.ErrnoError(2);
            }
            if ((stream.flags & 2097155) === 1) {
              throw new FS.ErrnoError(2);
            }
            if (!stream.stream_ops.mmap) {
              throw new FS.ErrnoError(43);
            }
            return stream.stream_ops.mmap(stream, length, position, prot, flags);
          },
          msync: (stream, buffer, offset, length, mmapFlags) => {
            if (!stream || !stream.stream_ops.msync) {
              return 0;
            }
            return stream.stream_ops.msync(stream, buffer, offset, length, mmapFlags);
          },
          munmap: stream => 0,
          ioctl: (stream, cmd, arg) => {
            if (!stream.stream_ops.ioctl) {
              throw new FS.ErrnoError(59);
            }
            return stream.stream_ops.ioctl(stream, cmd, arg);
          },
          readFile: (path, opts = {}) => {
            opts.flags = opts.flags || 0;
            opts.encoding = opts.encoding || "binary";
            if (opts.encoding !== "utf8" && opts.encoding !== "binary") {
              throw new Error('Invalid encoding type "' + opts.encoding + '"');
            }
            var ret;
            var stream = FS.open(path, opts.flags);
            var stat = FS.stat(path);
            var length = stat.size;
            var buf = new Uint8Array(length);
            FS.read(stream, buf, 0, length, 0);
            if (opts.encoding === "utf8") {
              ret = UTF8ArrayToString(buf, 0);
            } else if (opts.encoding === "binary") {
              ret = buf;
            }
            FS.close(stream);
            return ret;
          },
          writeFile: (path, data, opts = {}) => {
            opts.flags = opts.flags || 577;
            var stream = FS.open(path, opts.flags, opts.mode);
            if (typeof data == "string") {
              var buf = new Uint8Array(lengthBytesUTF8(data) + 1);
              var actualNumBytes = stringToUTF8Array(data, buf, 0, buf.length);
              FS.write(stream, buf, 0, actualNumBytes, undefined, opts.canOwn);
            } else if (ArrayBuffer.isView(data)) {
              FS.write(stream, data, 0, data.byteLength, undefined, opts.canOwn);
            } else {
              throw new Error("Unsupported data type");
            }
            FS.close(stream);
          },
          cwd: () => FS.currentPath,
          chdir: path => {
            var lookup = FS.lookupPath(path, {
              follow: true
            });
            if (lookup.node === null) {
              throw new FS.ErrnoError(44);
            }
            if (!FS.isDir(lookup.node.mode)) {
              throw new FS.ErrnoError(54);
            }
            var errCode = FS.nodePermissions(lookup.node, "x");
            if (errCode) {
              throw new FS.ErrnoError(errCode);
            }
            FS.currentPath = lookup.path;
          },
          createDefaultDirectories: () => {
            FS.mkdir("/tmp");
            FS.mkdir("/home");
            FS.mkdir("/home/web_user");
          },
          createDefaultDevices: () => {
            FS.mkdir("/dev");
            FS.registerDevice(FS.makedev(1, 3), {
              read: () => 0,
              write: (stream, buffer, offset, length, pos) => length
            });
            FS.mkdev("/dev/null", FS.makedev(1, 3));
            TTY.register(FS.makedev(5, 0), TTY.default_tty_ops);
            TTY.register(FS.makedev(6, 0), TTY.default_tty1_ops);
            FS.mkdev("/dev/tty", FS.makedev(5, 0));
            FS.mkdev("/dev/tty1", FS.makedev(6, 0));
            var random_device = getRandomDevice();
            FS.createDevice("/dev", "random", random_device);
            FS.createDevice("/dev", "urandom", random_device);
            FS.mkdir("/dev/shm");
            FS.mkdir("/dev/shm/tmp");
          },
          createSpecialDirectories: () => {
            FS.mkdir("/proc");
            var proc_self = FS.mkdir("/proc/self");
            FS.mkdir("/proc/self/fd");
            FS.mount({
              mount: () => {
                var node = FS.createNode(proc_self, "fd", 16384 | 511, 73);
                node.node_ops = {
                  lookup: (parent, name) => {
                    var fd = +name;
                    var stream = FS.getStream(fd);
                    if (!stream) throw new FS.ErrnoError(8);
                    var ret = {
                      parent: null,
                      mount: {
                        mountpoint: "fake"
                      },
                      node_ops: {
                        readlink: () => stream.path
                      }
                    };
                    ret.parent = ret;
                    return ret;
                  }
                };
                return node;
              }
            }, {}, "/proc/self/fd");
          },
          createStandardStreams: () => {
            if (Module["stdin"]) {
              FS.createDevice("/dev", "stdin", Module["stdin"]);
            } else {
              FS.symlink("/dev/tty", "/dev/stdin");
            }
            if (Module["stdout"]) {
              FS.createDevice("/dev", "stdout", null, Module["stdout"]);
            } else {
              FS.symlink("/dev/tty", "/dev/stdout");
            }
            if (Module["stderr"]) {
              FS.createDevice("/dev", "stderr", null, Module["stderr"]);
            } else {
              FS.symlink("/dev/tty1", "/dev/stderr");
            }
            FS.open("/dev/stdin", 0);
            FS.open("/dev/stdout", 1);
            FS.open("/dev/stderr", 1);
          },
          ensureErrnoError: () => {
            if (FS.ErrnoError) return;
            FS.ErrnoError = function ErrnoError(errno, node) {
              this.node = node;
              this.setErrno = function (errno) {
                this.errno = errno;
              };
              this.setErrno(errno);
              this.message = "FS error";
            };
            FS.ErrnoError.prototype = new Error();
            FS.ErrnoError.prototype.constructor = FS.ErrnoError;
            [44].forEach(code => {
              FS.genericErrors[code] = new FS.ErrnoError(code);
              FS.genericErrors[code].stack = "<generic error, no stack>";
            });
          },
          staticInit: () => {
            FS.ensureErrnoError();
            FS.nameTable = new Array(4096);
            FS.mount(MEMFS, {}, "/");
            FS.createDefaultDirectories();
            FS.createDefaultDevices();
            FS.createSpecialDirectories();
            FS.filesystems = {
              "MEMFS": MEMFS
            };
          },
          init: (input, output, error) => {
            FS.init.initialized = true;
            FS.ensureErrnoError();
            Module["stdin"] = input || Module["stdin"];
            Module["stdout"] = output || Module["stdout"];
            Module["stderr"] = error || Module["stderr"];
            FS.createStandardStreams();
          },
          quit: () => {
            FS.init.initialized = false;
            for (var i = 0; i < FS.streams.length; i++) {
              var stream = FS.streams[i];
              if (!stream) {
                continue;
              }
              FS.close(stream);
            }
          },
          getMode: (canRead, canWrite) => {
            var mode = 0;
            if (canRead) mode |= 292 | 73;
            if (canWrite) mode |= 146;
            return mode;
          },
          findObject: (path, dontResolveLastLink) => {
            var ret = FS.analyzePath(path, dontResolveLastLink);
            if (ret.exists) {
              return ret.object;
            } else {
              return null;
            }
          },
          analyzePath: (path, dontResolveLastLink) => {
            try {
              var lookup = FS.lookupPath(path, {
                follow: !dontResolveLastLink
              });
              path = lookup.path;
            } catch (e) {}
            var ret = {
              isRoot: false,
              exists: false,
              error: 0,
              name: null,
              path: null,
              object: null,
              parentExists: false,
              parentPath: null,
              parentObject: null
            };
            try {
              var lookup = FS.lookupPath(path, {
                parent: true
              });
              ret.parentExists = true;
              ret.parentPath = lookup.path;
              ret.parentObject = lookup.node;
              ret.name = PATH.basename(path);
              lookup = FS.lookupPath(path, {
                follow: !dontResolveLastLink
              });
              ret.exists = true;
              ret.path = lookup.path;
              ret.object = lookup.node;
              ret.name = lookup.node.name;
              ret.isRoot = lookup.path === "/";
            } catch (e) {
              ret.error = e.errno;
            }
            return ret;
          },
          createPath: (parent, path, canRead, canWrite) => {
            parent = typeof parent == "string" ? parent : FS.getPath(parent);
            var parts = path.split("/").reverse();
            while (parts.length) {
              var part = parts.pop();
              if (!part) continue;
              var current = PATH.join2(parent, part);
              try {
                FS.mkdir(current);
              } catch (e) {}
              parent = current;
            }
            return current;
          },
          createFile: (parent, name, properties, canRead, canWrite) => {
            var path = PATH.join2(typeof parent == "string" ? parent : FS.getPath(parent), name);
            var mode = FS.getMode(canRead, canWrite);
            return FS.create(path, mode);
          },
          createDataFile: (parent, name, data, canRead, canWrite, canOwn) => {
            var path = name;
            if (parent) {
              parent = typeof parent == "string" ? parent : FS.getPath(parent);
              path = name ? PATH.join2(parent, name) : parent;
            }
            var mode = FS.getMode(canRead, canWrite);
            var node = FS.create(path, mode);
            if (data) {
              if (typeof data == "string") {
                var arr = new Array(data.length);
                for (var i = 0, len = data.length; i < len; ++i) arr[i] = data.charCodeAt(i);
                data = arr;
              }
              FS.chmod(node, mode | 146);
              var stream = FS.open(node, 577);
              FS.write(stream, data, 0, data.length, 0, canOwn);
              FS.close(stream);
              FS.chmod(node, mode);
            }
            return node;
          },
          createDevice: (parent, name, input, output) => {
            var path = PATH.join2(typeof parent == "string" ? parent : FS.getPath(parent), name);
            var mode = FS.getMode(!!input, !!output);
            if (!FS.createDevice.major) FS.createDevice.major = 64;
            var dev = FS.makedev(FS.createDevice.major++, 0);
            FS.registerDevice(dev, {
              open: stream => {
                stream.seekable = false;
              },
              close: stream => {
                if (output && output.buffer && output.buffer.length) {
                  output(10);
                }
              },
              read: (stream, buffer, offset, length, pos) => {
                var bytesRead = 0;
                for (var i = 0; i < length; i++) {
                  var result;
                  try {
                    result = input();
                  } catch (e) {
                    throw new FS.ErrnoError(29);
                  }
                  if (result === undefined && bytesRead === 0) {
                    throw new FS.ErrnoError(6);
                  }
                  if (result === null || result === undefined) break;
                  bytesRead++;
                  buffer[offset + i] = result;
                }
                if (bytesRead) {
                  stream.node.timestamp = Date.now();
                }
                return bytesRead;
              },
              write: (stream, buffer, offset, length, pos) => {
                for (var i = 0; i < length; i++) {
                  try {
                    output(buffer[offset + i]);
                  } catch (e) {
                    throw new FS.ErrnoError(29);
                  }
                }
                if (length) {
                  stream.node.timestamp = Date.now();
                }
                return i;
              }
            });
            return FS.mkdev(path, mode, dev);
          },
          forceLoadFile: obj => {
            if (obj.isDevice || obj.isFolder || obj.link || obj.contents) return true;
            if (typeof XMLHttpRequest != "undefined") {
              throw new Error("Lazy loading should have been performed (contents set) in createLazyFile, but it was not. Lazy loading only works in web workers. Use --embed-file or --preload-file in emcc on the main thread.");
            } else if (read_) {
              try {
                obj.contents = intArrayFromString(read_(obj.url), true);
                obj.usedBytes = obj.contents.length;
              } catch (e) {
                throw new FS.ErrnoError(29);
              }
            } else {
              throw new Error("Cannot load without read() or XMLHttpRequest.");
            }
          },
          createLazyFile: (parent, name, url, canRead, canWrite) => {
            function LazyUint8Array() {
              this.lengthKnown = false;
              this.chunks = [];
            }
            LazyUint8Array.prototype.get = function LazyUint8Array_get(idx) {
              if (idx > this.length - 1 || idx < 0) {
                return undefined;
              }
              var chunkOffset = idx % this.chunkSize;
              var chunkNum = idx / this.chunkSize | 0;
              return this.getter(chunkNum)[chunkOffset];
            };
            LazyUint8Array.prototype.setDataGetter = function LazyUint8Array_setDataGetter(getter) {
              this.getter = getter;
            };
            LazyUint8Array.prototype.cacheLength = function LazyUint8Array_cacheLength() {
              var xhr = new XMLHttpRequest();
              xhr.open("HEAD", url, false);
              xhr.send(null);
              if (!(xhr.status >= 200 && xhr.status < 300 || xhr.status === 304)) throw new Error("Couldn't load " + url + ". Status: " + xhr.status);
              var datalength = Number(xhr.getResponseHeader("Content-length"));
              var header;
              var hasByteServing = (header = xhr.getResponseHeader("Accept-Ranges")) && header === "bytes";
              var usesGzip = (header = xhr.getResponseHeader("Content-Encoding")) && header === "gzip";
              var chunkSize = 1024 * 1024;
              if (!hasByteServing) chunkSize = datalength;
              var doXHR = (from, to) => {
                if (from > to) throw new Error("invalid range (" + from + ", " + to + ") or no bytes requested!");
                if (to > datalength - 1) throw new Error("only " + datalength + " bytes available! programmer error!");
                var xhr = new XMLHttpRequest();
                xhr.open("GET", url, false);
                if (datalength !== chunkSize) xhr.setRequestHeader("Range", "bytes=" + from + "-" + to);
                xhr.responseType = "arraybuffer";
                if (xhr.overrideMimeType) {
                  xhr.overrideMimeType("text/plain; charset=x-user-defined");
                }
                xhr.send(null);
                if (!(xhr.status >= 200 && xhr.status < 300 || xhr.status === 304)) throw new Error("Couldn't load " + url + ". Status: " + xhr.status);
                if (xhr.response !== undefined) {
                  return new Uint8Array(xhr.response || []);
                } else {
                  return intArrayFromString(xhr.responseText || "", true);
                }
              };
              var lazyArray = this;
              lazyArray.setDataGetter(chunkNum => {
                var start = chunkNum * chunkSize;
                var end = (chunkNum + 1) * chunkSize - 1;
                end = Math.min(end, datalength - 1);
                if (typeof lazyArray.chunks[chunkNum] == "undefined") {
                  lazyArray.chunks[chunkNum] = doXHR(start, end);
                }
                if (typeof lazyArray.chunks[chunkNum] == "undefined") throw new Error("doXHR failed!");
                return lazyArray.chunks[chunkNum];
              });
              if (usesGzip || !datalength) {
                chunkSize = datalength = 1;
                datalength = this.getter(0).length;
                chunkSize = datalength;
                out("LazyFiles on gzip forces download of the whole file when length is accessed");
              }
              this._length = datalength;
              this._chunkSize = chunkSize;
              this.lengthKnown = true;
            };
            if (typeof XMLHttpRequest != "undefined") {
              if (!ENVIRONMENT_IS_WORKER) throw "Cannot do synchronous binary XHRs outside webworkers in modern browsers. Use --embed-file or --preload-file in emcc";
              var lazyArray = new LazyUint8Array();
              Object.defineProperties(lazyArray, {
                length: {
                  get: function () {
                    if (!this.lengthKnown) {
                      this.cacheLength();
                    }
                    return this._length;
                  }
                },
                chunkSize: {
                  get: function () {
                    if (!this.lengthKnown) {
                      this.cacheLength();
                    }
                    return this._chunkSize;
                  }
                }
              });
              var properties = {
                isDevice: false,
                contents: lazyArray
              };
            } else {
              var properties = {
                isDevice: false,
                url: url
              };
            }
            var node = FS.createFile(parent, name, properties, canRead, canWrite);
            if (properties.contents) {
              node.contents = properties.contents;
            } else if (properties.url) {
              node.contents = null;
              node.url = properties.url;
            }
            Object.defineProperties(node, {
              usedBytes: {
                get: function () {
                  return this.contents.length;
                }
              }
            });
            var stream_ops = {};
            var keys = Object.keys(node.stream_ops);
            keys.forEach(key => {
              var fn = node.stream_ops[key];
              stream_ops[key] = function forceLoadLazyFile() {
                FS.forceLoadFile(node);
                return fn.apply(null, arguments);
              };
            });
            stream_ops.read = (stream, buffer, offset, length, position) => {
              FS.forceLoadFile(node);
              var contents = stream.node.contents;
              if (position >= contents.length) return 0;
              var size = Math.min(contents.length - position, length);
              if (contents.slice) {
                for (var i = 0; i < size; i++) {
                  buffer[offset + i] = contents[position + i];
                }
              } else {
                for (var i = 0; i < size; i++) {
                  buffer[offset + i] = contents.get(position + i);
                }
              }
              return size;
            };
            node.stream_ops = stream_ops;
            return node;
          },
          createPreloadedFile: (parent, name, url, canRead, canWrite, onload, onerror, dontCreateFile, canOwn, preFinish) => {
            var fullname = name ? PATH_FS.resolve(PATH.join2(parent, name)) : parent;
            function processData(byteArray) {
              function finish(byteArray) {
                if (preFinish) preFinish();
                if (!dontCreateFile) {
                  FS.createDataFile(parent, name, byteArray, canRead, canWrite, canOwn);
                }
                if (onload) onload();
                removeRunDependency();
              }
              if (Browser.handledByPreloadPlugin(byteArray, fullname, finish, () => {
                if (onerror) onerror();
                removeRunDependency();
              })) {
                return;
              }
              finish(byteArray);
            }
            addRunDependency();
            if (typeof url == "string") {
              asyncLoad(url, byteArray => processData(byteArray), onerror);
            } else {
              processData(url);
            }
          },
          indexedDB: () => {
            return window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;
          },
          DB_NAME: () => {
            return "EM_FS_" + window.location.pathname;
          },
          DB_VERSION: 20,
          DB_STORE_NAME: "FILE_DATA",
          saveFilesToDB: (paths, onload, onerror) => {
            onload = onload || (() => {});
            onerror = onerror || (() => {});
            var indexedDB = FS.indexedDB();
            try {
              var openRequest = indexedDB.open(FS.DB_NAME(), FS.DB_VERSION);
            } catch (e) {
              return onerror(e);
            }
            openRequest.onupgradeneeded = () => {
              out("creating db");
              var db = openRequest.result;
              db.createObjectStore(FS.DB_STORE_NAME);
            };
            openRequest.onsuccess = () => {
              var db = openRequest.result;
              var transaction = db.transaction([FS.DB_STORE_NAME], "readwrite");
              var files = transaction.objectStore(FS.DB_STORE_NAME);
              var ok = 0,
                fail = 0,
                total = paths.length;
              function finish() {
                if (fail == 0) onload();else onerror();
              }
              paths.forEach(path => {
                var putRequest = files.put(FS.analyzePath(path).object.contents, path);
                putRequest.onsuccess = () => {
                  ok++;
                  if (ok + fail == total) finish();
                };
                putRequest.onerror = () => {
                  fail++;
                  if (ok + fail == total) finish();
                };
              });
              transaction.onerror = onerror;
            };
            openRequest.onerror = onerror;
          },
          loadFilesFromDB: (paths, onload, onerror) => {
            onload = onload || (() => {});
            onerror = onerror || (() => {});
            var indexedDB = FS.indexedDB();
            try {
              var openRequest = indexedDB.open(FS.DB_NAME(), FS.DB_VERSION);
            } catch (e) {
              return onerror(e);
            }
            openRequest.onupgradeneeded = onerror;
            openRequest.onsuccess = () => {
              var db = openRequest.result;
              try {
                var transaction = db.transaction([FS.DB_STORE_NAME], "readonly");
              } catch (e) {
                onerror(e);
                return;
              }
              var files = transaction.objectStore(FS.DB_STORE_NAME);
              var ok = 0,
                fail = 0,
                total = paths.length;
              function finish() {
                if (fail == 0) onload();else onerror();
              }
              paths.forEach(path => {
                var getRequest = files.get(path);
                getRequest.onsuccess = () => {
                  if (FS.analyzePath(path).exists) {
                    FS.unlink(path);
                  }
                  FS.createDataFile(PATH.dirname(path), PATH.basename(path), getRequest.result, true, true, true);
                  ok++;
                  if (ok + fail == total) finish();
                };
                getRequest.onerror = () => {
                  fail++;
                  if (ok + fail == total) finish();
                };
              });
              transaction.onerror = onerror;
            };
            openRequest.onerror = onerror;
          }
        };
        var SYSCALLS = {
          DEFAULT_POLLMASK: 5,
          calculateAt: function (dirfd, path, allowEmpty) {
            if (PATH.isAbs(path)) {
              return path;
            }
            var dir;
            if (dirfd === -100) {
              dir = FS.cwd();
            } else {
              var dirstream = FS.getStream(dirfd);
              if (!dirstream) throw new FS.ErrnoError(8);
              dir = dirstream.path;
            }
            if (path.length == 0) {
              if (!allowEmpty) {
                throw new FS.ErrnoError(44);
              }
              return dir;
            }
            return PATH.join2(dir, path);
          },
          doStat: function (func, path, buf) {
            try {
              var stat = func(path);
            } catch (e) {
              if (e && e.node && PATH.normalize(path) !== PATH.normalize(FS.getPath(e.node))) {
                return -54;
              }
              throw e;
            }
            HEAP32[buf >> 2] = stat.dev;
            HEAP32[buf + 4 >> 2] = 0;
            HEAP32[buf + 8 >> 2] = stat.ino;
            HEAP32[buf + 12 >> 2] = stat.mode;
            HEAP32[buf + 16 >> 2] = stat.nlink;
            HEAP32[buf + 20 >> 2] = stat.uid;
            HEAP32[buf + 24 >> 2] = stat.gid;
            HEAP32[buf + 28 >> 2] = stat.rdev;
            HEAP32[buf + 32 >> 2] = 0;
            tempI64 = [stat.size >>> 0, (tempDouble = stat.size, +Math.abs(tempDouble) >= 1 ? tempDouble > 0 ? (Math.min(+Math.floor(tempDouble / 4294967296), 4294967295) | 0) >>> 0 : ~~+Math.ceil((tempDouble - +(~~tempDouble >>> 0)) / 4294967296) >>> 0 : 0)], HEAP32[buf + 40 >> 2] = tempI64[0], HEAP32[buf + 44 >> 2] = tempI64[1];
            HEAP32[buf + 48 >> 2] = 4096;
            HEAP32[buf + 52 >> 2] = stat.blocks;
            HEAP32[buf + 56 >> 2] = stat.atime.getTime() / 1e3 | 0;
            HEAP32[buf + 60 >> 2] = 0;
            HEAP32[buf + 64 >> 2] = stat.mtime.getTime() / 1e3 | 0;
            HEAP32[buf + 68 >> 2] = 0;
            HEAP32[buf + 72 >> 2] = stat.ctime.getTime() / 1e3 | 0;
            HEAP32[buf + 76 >> 2] = 0;
            tempI64 = [stat.ino >>> 0, (tempDouble = stat.ino, +Math.abs(tempDouble) >= 1 ? tempDouble > 0 ? (Math.min(+Math.floor(tempDouble / 4294967296), 4294967295) | 0) >>> 0 : ~~+Math.ceil((tempDouble - +(~~tempDouble >>> 0)) / 4294967296) >>> 0 : 0)], HEAP32[buf + 80 >> 2] = tempI64[0], HEAP32[buf + 84 >> 2] = tempI64[1];
            return 0;
          },
          doMsync: function (addr, stream, len, flags, offset) {
            var buffer = HEAPU8.slice(addr, addr + len);
            FS.msync(stream, buffer, offset, len, flags);
          },
          varargs: undefined,
          get: function () {
            SYSCALLS.varargs += 4;
            var ret = HEAP32[SYSCALLS.varargs - 4 >> 2];
            return ret;
          },
          getStr: function (ptr) {
            var ret = UTF8ToString(ptr);
            return ret;
          },
          getStreamFromFD: function (fd) {
            var stream = FS.getStream(fd);
            if (!stream) throw new FS.ErrnoError(8);
            return stream;
          }
        };
        function ___syscall_faccessat(dirfd, path, amode, flags) {
          try {
            path = SYSCALLS.getStr(path);
            path = SYSCALLS.calculateAt(dirfd, path);
            if (amode & ~7) {
              return -28;
            }
            var lookup = FS.lookupPath(path, {
              follow: true
            });
            var node = lookup.node;
            if (!node) {
              return -44;
            }
            var perms = "";
            if (amode & 4) perms += "r";
            if (amode & 2) perms += "w";
            if (amode & 1) perms += "x";
            if (perms && FS.nodePermissions(node, perms)) {
              return -2;
            }
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_fadvise64(fd, offset, len, advice) {
          return 0;
        }
        function setErrNo(value) {
          HEAP32[___errno_location() >> 2] = value;
          return value;
        }
        function ___syscall_fcntl64(fd, cmd, varargs) {
          SYSCALLS.varargs = varargs;
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            switch (cmd) {
              case 0:
                {
                  var arg = SYSCALLS.get();
                  if (arg < 0) {
                    return -28;
                  }
                  var newStream;
                  newStream = FS.createStream(stream, arg);
                  return newStream.fd;
                }
              case 1:
              case 2:
                return 0;
              case 3:
                return stream.flags;
              case 4:
                {
                  var arg = SYSCALLS.get();
                  stream.flags |= arg;
                  return 0;
                }
              case 5:
                {
                  var arg = SYSCALLS.get();
                  var offset = 0;
                  HEAP16[arg + offset >> 1] = 2;
                  return 0;
                }
              case 6:
              case 7:
                return 0;
              case 16:
              case 8:
                return -28;
              case 9:
                setErrNo(28);
                return -1;
              default:
                {
                  return -28;
                }
            }
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_fstat64(fd, buf) {
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            return SYSCALLS.doStat(FS.stat, stream.path, buf);
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_statfs64(path, size, buf) {
          try {
            path = SYSCALLS.getStr(path);
            HEAP32[buf + 4 >> 2] = 4096;
            HEAP32[buf + 40 >> 2] = 4096;
            HEAP32[buf + 8 >> 2] = 1e6;
            HEAP32[buf + 12 >> 2] = 5e5;
            HEAP32[buf + 16 >> 2] = 5e5;
            HEAP32[buf + 20 >> 2] = FS.nextInode;
            HEAP32[buf + 24 >> 2] = 1e6;
            HEAP32[buf + 28 >> 2] = 42;
            HEAP32[buf + 44 >> 2] = 2;
            HEAP32[buf + 36 >> 2] = 255;
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_fstatfs64(fd, size, buf) {
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            return ___syscall_statfs64(0, size, buf);
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function convertI32PairToI53Checked(lo, hi) {
          return hi + 2097152 >>> 0 < 4194305 - !!lo ? (lo >>> 0) + hi * 4294967296 : NaN;
        }
        function ___syscall_ftruncate64(fd, length_low, length_high) {
          try {
            var length = convertI32PairToI53Checked(length_low, length_high);
            if (isNaN(length)) return -61;
            FS.ftruncate(fd, length);
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_getcwd(buf, size) {
          try {
            if (size === 0) return -28;
            var cwd = FS.cwd();
            var cwdLengthInBytes = lengthBytesUTF8(cwd) + 1;
            if (size < cwdLengthInBytes) return -68;
            stringToUTF8(cwd, buf, size);
            return cwdLengthInBytes;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_getdents64(fd, dirp, count) {
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            if (!stream.getdents) {
              stream.getdents = FS.readdir(stream.path);
            }
            var struct_size = 280;
            var pos = 0;
            var off = FS.llseek(stream, 0, 1);
            var idx = Math.floor(off / struct_size);
            while (idx < stream.getdents.length && pos + struct_size <= count) {
              var id;
              var type;
              var name = stream.getdents[idx];
              if (name === ".") {
                id = stream.node.id;
                type = 4;
              } else if (name === "..") {
                var lookup = FS.lookupPath(stream.path, {
                  parent: true
                });
                id = lookup.node.id;
                type = 4;
              } else {
                var child = FS.lookupNode(stream.node, name);
                id = child.id;
                type = FS.isChrdev(child.mode) ? 2 : FS.isDir(child.mode) ? 4 : FS.isLink(child.mode) ? 10 : 8;
              }
              tempI64 = [id >>> 0, (tempDouble = id, +Math.abs(tempDouble) >= 1 ? tempDouble > 0 ? (Math.min(+Math.floor(tempDouble / 4294967296), 4294967295) | 0) >>> 0 : ~~+Math.ceil((tempDouble - +(~~tempDouble >>> 0)) / 4294967296) >>> 0 : 0)], HEAP32[dirp + pos >> 2] = tempI64[0], HEAP32[dirp + pos + 4 >> 2] = tempI64[1];
              tempI64 = [(idx + 1) * struct_size >>> 0, (tempDouble = (idx + 1) * struct_size, +Math.abs(tempDouble) >= 1 ? tempDouble > 0 ? (Math.min(+Math.floor(tempDouble / 4294967296), 4294967295) | 0) >>> 0 : ~~+Math.ceil((tempDouble - +(~~tempDouble >>> 0)) / 4294967296) >>> 0 : 0)], HEAP32[dirp + pos + 8 >> 2] = tempI64[0], HEAP32[dirp + pos + 12 >> 2] = tempI64[1];
              HEAP16[dirp + pos + 16 >> 1] = 280;
              HEAP8[dirp + pos + 18 >> 0] = type;
              stringToUTF8(name, dirp + pos + 19, 256);
              pos += struct_size;
              idx += 1;
            }
            FS.llseek(stream, idx * struct_size, 0);
            return pos;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_ioctl(fd, op, varargs) {
          SYSCALLS.varargs = varargs;
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            switch (op) {
              case 21509:
              case 21505:
                {
                  if (!stream.tty) return -59;
                  return 0;
                }
              case 21510:
              case 21511:
              case 21512:
              case 21506:
              case 21507:
              case 21508:
                {
                  if (!stream.tty) return -59;
                  return 0;
                }
              case 21519:
                {
                  if (!stream.tty) return -59;
                  var argp = SYSCALLS.get();
                  HEAP32[argp >> 2] = 0;
                  return 0;
                }
              case 21520:
                {
                  if (!stream.tty) return -59;
                  return -28;
                }
              case 21531:
                {
                  var argp = SYSCALLS.get();
                  return FS.ioctl(stream, op, argp);
                }
              case 21523:
                {
                  if (!stream.tty) return -59;
                  return 0;
                }
              case 21524:
                {
                  if (!stream.tty) return -59;
                  return 0;
                }
              default:
                abort("bad ioctl syscall " + op);
            }
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_lstat64(path, buf) {
          try {
            path = SYSCALLS.getStr(path);
            return SYSCALLS.doStat(FS.lstat, path, buf);
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_newfstatat(dirfd, path, buf, flags) {
          try {
            path = SYSCALLS.getStr(path);
            var nofollow = flags & 256;
            var allowEmpty = flags & 4096;
            flags = flags & ~4352;
            path = SYSCALLS.calculateAt(dirfd, path, allowEmpty);
            return SYSCALLS.doStat(nofollow ? FS.lstat : FS.stat, path, buf);
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_openat(dirfd, path, flags, varargs) {
          SYSCALLS.varargs = varargs;
          try {
            path = SYSCALLS.getStr(path);
            path = SYSCALLS.calculateAt(dirfd, path);
            var mode = varargs ? SYSCALLS.get() : 0;
            return FS.open(path, flags, mode).fd;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_readlinkat(dirfd, path, buf, bufsize) {
          try {
            path = SYSCALLS.getStr(path);
            path = SYSCALLS.calculateAt(dirfd, path);
            if (bufsize <= 0) return -28;
            var ret = FS.readlink(path);
            var len = Math.min(bufsize, lengthBytesUTF8(ret));
            var endChar = HEAP8[buf + len];
            stringToUTF8(ret, buf, bufsize + 1);
            HEAP8[buf + len] = endChar;
            return len;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_stat64(path, buf) {
          try {
            path = SYSCALLS.getStr(path);
            return SYSCALLS.doStat(FS.stat, path, buf);
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function ___syscall_unlinkat(dirfd, path, flags) {
          try {
            path = SYSCALLS.getStr(path);
            path = SYSCALLS.calculateAt(dirfd, path);
            if (flags === 0) {
              FS.unlink(path);
            } else if (flags === 512) {
              FS.rmdir(path);
            } else {
              abort("Invalid flags passed to unlinkat");
            }
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function __emscripten_date_now() {
          return Date.now();
        }
        var nowIsMonotonic = true;
        function __emscripten_get_now_is_monotonic() {
          return nowIsMonotonic;
        }
        function __localtime_js(time, tmPtr) {
          var date = new Date(HEAP32[time >> 2] * 1e3);
          HEAP32[tmPtr >> 2] = date.getSeconds();
          HEAP32[tmPtr + 4 >> 2] = date.getMinutes();
          HEAP32[tmPtr + 8 >> 2] = date.getHours();
          HEAP32[tmPtr + 12 >> 2] = date.getDate();
          HEAP32[tmPtr + 16 >> 2] = date.getMonth();
          HEAP32[tmPtr + 20 >> 2] = date.getFullYear() - 1900;
          HEAP32[tmPtr + 24 >> 2] = date.getDay();
          var start = new Date(date.getFullYear(), 0, 1);
          var yday = (date.getTime() - start.getTime()) / (1e3 * 60 * 60 * 24) | 0;
          HEAP32[tmPtr + 28 >> 2] = yday;
          HEAP32[tmPtr + 36 >> 2] = -(date.getTimezoneOffset() * 60);
          var summerOffset = new Date(date.getFullYear(), 6, 1).getTimezoneOffset();
          var winterOffset = start.getTimezoneOffset();
          var dst = (summerOffset != winterOffset && date.getTimezoneOffset() == Math.min(winterOffset, summerOffset)) | 0;
          HEAP32[tmPtr + 32 >> 2] = dst;
        }
        function __mmap_js(len, prot, flags, fd, off, allocated) {
          try {
            var stream = FS.getStream(fd);
            if (!stream) return -8;
            var res = FS.mmap(stream, len, off, prot, flags);
            var ptr = res.ptr;
            HEAP32[allocated >> 2] = res.allocated;
            return ptr;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function __munmap_js(addr, len, prot, flags, fd, offset) {
          try {
            var stream = FS.getStream(fd);
            if (stream) {
              if (prot & 2) {
                SYSCALLS.doMsync(addr, stream, len, flags, offset);
              }
              FS.munmap(stream);
            }
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return -e.errno;
          }
        }
        function _tzset_impl(timezone, daylight, tzname) {
          var currentYear = new Date().getFullYear();
          var winter = new Date(currentYear, 0, 1);
          var summer = new Date(currentYear, 6, 1);
          var winterOffset = winter.getTimezoneOffset();
          var summerOffset = summer.getTimezoneOffset();
          var stdTimezoneOffset = Math.max(winterOffset, summerOffset);
          HEAP32[timezone >> 2] = stdTimezoneOffset * 60;
          HEAP32[daylight >> 2] = Number(winterOffset != summerOffset);
          function extractZone(date) {
            var match = date.toTimeString().match(/\(([A-Za-z ]+)\)$/);
            return match ? match[1] : "GMT";
          }
          var winterName = extractZone(winter);
          var summerName = extractZone(summer);
          var winterNamePtr = allocateUTF8(winterName);
          var summerNamePtr = allocateUTF8(summerName);
          if (summerOffset < winterOffset) {
            HEAPU32[tzname >> 2] = winterNamePtr;
            HEAPU32[tzname + 4 >> 2] = summerNamePtr;
          } else {
            HEAPU32[tzname >> 2] = summerNamePtr;
            HEAPU32[tzname + 4 >> 2] = winterNamePtr;
          }
        }
        function __tzset_js(timezone, daylight, tzname) {
          if (__tzset_js.called) return;
          __tzset_js.called = true;
          _tzset_impl(timezone, daylight, tzname);
        }
        function _abort() {
          abort("");
        }
        var DOTNETENTROPY = {
          batchedQuotaMax: 65536,
          getBatchedRandomValues: function (buffer, bufferLength) {
            const needTempBuf = typeof SharedArrayBuffer !== "undefined" && Module.HEAPU8.buffer instanceof SharedArrayBuffer;
            const buf = needTempBuf ? new ArrayBuffer(bufferLength) : Module.HEAPU8.buffer;
            const offset = needTempBuf ? 0 : buffer;
            for (let i = 0; i < bufferLength; i += this.batchedQuotaMax) {
              const view = new Uint8Array(buf, offset + i, Math.min(bufferLength - i, this.batchedQuotaMax));
              crypto.getRandomValues(view);
            }
            if (needTempBuf) {
              const heapView = new Uint8Array(Module.HEAPU8.buffer, buffer, bufferLength);
              heapView.set(new Uint8Array(buf));
            }
          }
        };
        function _dotnet_browser_entropy(buffer, bufferLength) {
          if (typeof crypto === "object" && typeof crypto["getRandomValues"] === "function") {
            DOTNETENTROPY.getBatchedRandomValues(buffer, bufferLength);
            return 0;
          } else {
            return -1;
          }
        }
        function getHeapMax() {
          return 2147483648;
        }
        function _emscripten_get_heap_max() {
          return getHeapMax();
        }
        var _emscripten_get_now;
        if (ENVIRONMENT_IS_NODE) {
          _emscripten_get_now = () => {
            var t = browser$1["hrtime"]();
            return t[0] * 1e3 + t[1] / 1e6;
          };
        } else if (typeof dateNow != "undefined") {
          _emscripten_get_now = dateNow;
        } else _emscripten_get_now = () => performance.now();
        function _emscripten_get_now_res() {
          if (ENVIRONMENT_IS_NODE) {
            return 1;
          } else if (typeof dateNow != "undefined") {
            return 1e3;
          } else return 1e3;
        }
        function _emscripten_memcpy_big(dest, src, num) {
          HEAPU8.copyWithin(dest, src, src + num);
        }
        function emscripten_realloc_buffer(size) {
          try {
            wasmMemory.grow(size - buffer.byteLength + 65535 >>> 16);
            updateGlobalBufferAndViews(wasmMemory.buffer);
            return 1;
          } catch (e) {}
        }
        function _emscripten_resize_heap(requestedSize) {
          var oldSize = HEAPU8.length;
          requestedSize = requestedSize >>> 0;
          var maxHeapSize = getHeapMax();
          if (requestedSize > maxHeapSize) {
            return false;
          }
          let alignUp = (x, multiple) => x + (multiple - x % multiple) % multiple;
          for (var cutDown = 1; cutDown <= 4; cutDown *= 2) {
            var overGrownHeapSize = oldSize * (1 + .2 / cutDown);
            overGrownHeapSize = Math.min(overGrownHeapSize, requestedSize + 100663296);
            var newSize = Math.min(maxHeapSize, alignUp(Math.max(requestedSize, overGrownHeapSize), 65536));
            var replacement = emscripten_realloc_buffer(newSize);
            if (replacement) {
              return true;
            }
          }
          return false;
        }
        var ENV = {};
        function getExecutableName() {
          return thisProgram || "./this.program";
        }
        function getEnvStrings() {
          if (!getEnvStrings.strings) {
            var lang = (typeof navigator == "object" && navigator.languages && navigator.languages[0] || "C").replace("-", "_") + ".UTF-8";
            var env = {
              "USER": "web_user",
              "LOGNAME": "web_user",
              "PATH": "/",
              "PWD": "/",
              "HOME": "/home/web_user",
              "LANG": lang,
              "_": getExecutableName()
            };
            for (var x in ENV) {
              if (ENV[x] === undefined) delete env[x];else env[x] = ENV[x];
            }
            var strings = [];
            for (var x in env) {
              strings.push(x + "=" + env[x]);
            }
            getEnvStrings.strings = strings;
          }
          return getEnvStrings.strings;
        }
        function _environ_get(__environ, environ_buf) {
          var bufSize = 0;
          getEnvStrings().forEach(function (string, i) {
            var ptr = environ_buf + bufSize;
            HEAPU32[__environ + i * 4 >> 2] = ptr;
            writeAsciiToMemory(string, ptr);
            bufSize += string.length + 1;
          });
          return 0;
        }
        function _environ_sizes_get(penviron_count, penviron_buf_size) {
          var strings = getEnvStrings();
          HEAPU32[penviron_count >> 2] = strings.length;
          var bufSize = 0;
          strings.forEach(function (string) {
            bufSize += string.length + 1;
          });
          HEAPU32[penviron_buf_size >> 2] = bufSize;
          return 0;
        }
        function _exit(status) {
          exit(status);
        }
        function _fd_close(fd) {
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            FS.close(stream);
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return e.errno;
          }
        }
        function doReadv(stream, iov, iovcnt, offset) {
          var ret = 0;
          for (var i = 0; i < iovcnt; i++) {
            var ptr = HEAPU32[iov >> 2];
            var len = HEAPU32[iov + 4 >> 2];
            iov += 8;
            var curr = FS.read(stream, HEAP8, ptr, len, offset);
            if (curr < 0) return -1;
            ret += curr;
            if (curr < len) break;
          }
          return ret;
        }
        function _fd_pread(fd, iov, iovcnt, offset_low, offset_high, pnum) {
          try {
            var offset = convertI32PairToI53Checked(offset_low, offset_high);
            if (isNaN(offset)) return 61;
            var stream = SYSCALLS.getStreamFromFD(fd);
            var num = doReadv(stream, iov, iovcnt, offset);
            HEAP32[pnum >> 2] = num;
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return e.errno;
          }
        }
        function _fd_read(fd, iov, iovcnt, pnum) {
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            var num = doReadv(stream, iov, iovcnt);
            HEAP32[pnum >> 2] = num;
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return e.errno;
          }
        }
        function _fd_seek(fd, offset_low, offset_high, whence, newOffset) {
          try {
            var offset = convertI32PairToI53Checked(offset_low, offset_high);
            if (isNaN(offset)) return 61;
            var stream = SYSCALLS.getStreamFromFD(fd);
            FS.llseek(stream, offset, whence);
            tempI64 = [stream.position >>> 0, (tempDouble = stream.position, +Math.abs(tempDouble) >= 1 ? tempDouble > 0 ? (Math.min(+Math.floor(tempDouble / 4294967296), 4294967295) | 0) >>> 0 : ~~+Math.ceil((tempDouble - +(~~tempDouble >>> 0)) / 4294967296) >>> 0 : 0)], HEAP32[newOffset >> 2] = tempI64[0], HEAP32[newOffset + 4 >> 2] = tempI64[1];
            if (stream.getdents && offset === 0 && whence === 0) stream.getdents = null;
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return e.errno;
          }
        }
        function doWritev(stream, iov, iovcnt, offset) {
          var ret = 0;
          for (var i = 0; i < iovcnt; i++) {
            var ptr = HEAPU32[iov >> 2];
            var len = HEAPU32[iov + 4 >> 2];
            iov += 8;
            var curr = FS.write(stream, HEAP8, ptr, len, offset);
            if (curr < 0) return -1;
            ret += curr;
          }
          return ret;
        }
        function _fd_write(fd, iov, iovcnt, pnum) {
          try {
            var stream = SYSCALLS.getStreamFromFD(fd);
            var num = doWritev(stream, iov, iovcnt);
            HEAPU32[pnum >> 2] = num;
            return 0;
          } catch (e) {
            if (typeof FS == "undefined" || !(e instanceof FS.ErrnoError)) throw e;
            return e.errno;
          }
        }
        function _getTempRet0() {
          return getTempRet0();
        }
        function _llvm_eh_typeid_for(type) {
          return type;
        }
        function _mono_set_timeout() {
          return __dotnet_runtime.__linker_exports.mono_set_timeout.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_bind_cs_function() {
          return __dotnet_runtime.__linker_exports.mono_wasm_bind_cs_function.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_bind_js_function() {
          return __dotnet_runtime.__linker_exports.mono_wasm_bind_js_function.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_create_cs_owned_object_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_create_cs_owned_object_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_get_by_index_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_get_by_index_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_get_global_object_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_get_global_object_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_get_object_property_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_get_object_property_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_invoke_bound_function() {
          return __dotnet_runtime.__linker_exports.mono_wasm_invoke_bound_function.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_invoke_js_blazor() {
          return __dotnet_runtime.__linker_exports.mono_wasm_invoke_js_blazor.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_invoke_js_with_args_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_invoke_js_with_args_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_marshal_promise() {
          return __dotnet_runtime.__linker_exports.mono_wasm_marshal_promise.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_release_cs_owned_object() {
          return __dotnet_runtime.__linker_exports.mono_wasm_release_cs_owned_object.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_set_by_index_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_set_by_index_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_set_entrypoint_breakpoint() {
          return __dotnet_runtime.__linker_exports.mono_wasm_set_entrypoint_breakpoint.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_set_object_property_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_set_object_property_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_trace_logger() {
          return __dotnet_runtime.__linker_exports.mono_wasm_trace_logger.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_typed_array_from_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_typed_array_from_ref.apply(__dotnet_runtime, arguments);
        }
        function _mono_wasm_typed_array_to_array_ref() {
          return __dotnet_runtime.__linker_exports.mono_wasm_typed_array_to_array_ref.apply(__dotnet_runtime, arguments);
        }
        function _schedule_background_exec() {
          return __dotnet_runtime.__linker_exports.schedule_background_exec.apply(__dotnet_runtime, arguments);
        }
        function _setTempRet0(val) {
          setTempRet0(val);
        }
        function __isLeapYear(year) {
          return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
        }
        function __arraySum(array, index) {
          var sum = 0;
          for (var i = 0; i <= index; sum += array[i++]) {}
          return sum;
        }
        var __MONTH_DAYS_LEAP = [31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        var __MONTH_DAYS_REGULAR = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
        function __addDays(date, days) {
          var newDate = new Date(date.getTime());
          while (days > 0) {
            var leap = __isLeapYear(newDate.getFullYear());
            var currentMonth = newDate.getMonth();
            var daysInCurrentMonth = (leap ? __MONTH_DAYS_LEAP : __MONTH_DAYS_REGULAR)[currentMonth];
            if (days > daysInCurrentMonth - newDate.getDate()) {
              days -= daysInCurrentMonth - newDate.getDate() + 1;
              newDate.setDate(1);
              if (currentMonth < 11) {
                newDate.setMonth(currentMonth + 1);
              } else {
                newDate.setMonth(0);
                newDate.setFullYear(newDate.getFullYear() + 1);
              }
            } else {
              newDate.setDate(newDate.getDate() + days);
              return newDate;
            }
          }
          return newDate;
        }
        function _strftime(s, maxsize, format, tm) {
          var tm_zone = HEAP32[tm + 40 >> 2];
          var date = {
            tm_sec: HEAP32[tm >> 2],
            tm_min: HEAP32[tm + 4 >> 2],
            tm_hour: HEAP32[tm + 8 >> 2],
            tm_mday: HEAP32[tm + 12 >> 2],
            tm_mon: HEAP32[tm + 16 >> 2],
            tm_year: HEAP32[tm + 20 >> 2],
            tm_wday: HEAP32[tm + 24 >> 2],
            tm_yday: HEAP32[tm + 28 >> 2],
            tm_isdst: HEAP32[tm + 32 >> 2],
            tm_gmtoff: HEAP32[tm + 36 >> 2],
            tm_zone: tm_zone ? UTF8ToString(tm_zone) : ""
          };
          var pattern = UTF8ToString(format);
          var EXPANSION_RULES_1 = {
            "%c": "%a %b %d %H:%M:%S %Y",
            "%D": "%m/%d/%y",
            "%F": "%Y-%m-%d",
            "%h": "%b",
            "%r": "%I:%M:%S %p",
            "%R": "%H:%M",
            "%T": "%H:%M:%S",
            "%x": "%m/%d/%y",
            "%X": "%H:%M:%S",
            "%Ec": "%c",
            "%EC": "%C",
            "%Ex": "%m/%d/%y",
            "%EX": "%H:%M:%S",
            "%Ey": "%y",
            "%EY": "%Y",
            "%Od": "%d",
            "%Oe": "%e",
            "%OH": "%H",
            "%OI": "%I",
            "%Om": "%m",
            "%OM": "%M",
            "%OS": "%S",
            "%Ou": "%u",
            "%OU": "%U",
            "%OV": "%V",
            "%Ow": "%w",
            "%OW": "%W",
            "%Oy": "%y"
          };
          for (var rule in EXPANSION_RULES_1) {
            pattern = pattern.replace(new RegExp(rule, "g"), EXPANSION_RULES_1[rule]);
          }
          var WEEKDAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
          var MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
          function leadingSomething(value, digits, character) {
            var str = typeof value == "number" ? value.toString() : value || "";
            while (str.length < digits) {
              str = character[0] + str;
            }
            return str;
          }
          function leadingNulls(value, digits) {
            return leadingSomething(value, digits, "0");
          }
          function compareByDay(date1, date2) {
            function sgn(value) {
              return value < 0 ? -1 : value > 0 ? 1 : 0;
            }
            var compare;
            if ((compare = sgn(date1.getFullYear() - date2.getFullYear())) === 0) {
              if ((compare = sgn(date1.getMonth() - date2.getMonth())) === 0) {
                compare = sgn(date1.getDate() - date2.getDate());
              }
            }
            return compare;
          }
          function getFirstWeekStartDate(janFourth) {
            switch (janFourth.getDay()) {
              case 0:
                return new Date(janFourth.getFullYear() - 1, 11, 29);
              case 1:
                return janFourth;
              case 2:
                return new Date(janFourth.getFullYear(), 0, 3);
              case 3:
                return new Date(janFourth.getFullYear(), 0, 2);
              case 4:
                return new Date(janFourth.getFullYear(), 0, 1);
              case 5:
                return new Date(janFourth.getFullYear() - 1, 11, 31);
              case 6:
                return new Date(janFourth.getFullYear() - 1, 11, 30);
            }
          }
          function getWeekBasedYear(date) {
            var thisDate = __addDays(new Date(date.tm_year + 1900, 0, 1), date.tm_yday);
            var janFourthThisYear = new Date(thisDate.getFullYear(), 0, 4);
            var janFourthNextYear = new Date(thisDate.getFullYear() + 1, 0, 4);
            var firstWeekStartThisYear = getFirstWeekStartDate(janFourthThisYear);
            var firstWeekStartNextYear = getFirstWeekStartDate(janFourthNextYear);
            if (compareByDay(firstWeekStartThisYear, thisDate) <= 0) {
              if (compareByDay(firstWeekStartNextYear, thisDate) <= 0) {
                return thisDate.getFullYear() + 1;
              } else {
                return thisDate.getFullYear();
              }
            } else {
              return thisDate.getFullYear() - 1;
            }
          }
          var EXPANSION_RULES_2 = {
            "%a": function (date) {
              return WEEKDAYS[date.tm_wday].substring(0, 3);
            },
            "%A": function (date) {
              return WEEKDAYS[date.tm_wday];
            },
            "%b": function (date) {
              return MONTHS[date.tm_mon].substring(0, 3);
            },
            "%B": function (date) {
              return MONTHS[date.tm_mon];
            },
            "%C": function (date) {
              var year = date.tm_year + 1900;
              return leadingNulls(year / 100 | 0, 2);
            },
            "%d": function (date) {
              return leadingNulls(date.tm_mday, 2);
            },
            "%e": function (date) {
              return leadingSomething(date.tm_mday, 2, " ");
            },
            "%g": function (date) {
              return getWeekBasedYear(date).toString().substring(2);
            },
            "%G": function (date) {
              return getWeekBasedYear(date);
            },
            "%H": function (date) {
              return leadingNulls(date.tm_hour, 2);
            },
            "%I": function (date) {
              var twelveHour = date.tm_hour;
              if (twelveHour == 0) twelveHour = 12;else if (twelveHour > 12) twelveHour -= 12;
              return leadingNulls(twelveHour, 2);
            },
            "%j": function (date) {
              return leadingNulls(date.tm_mday + __arraySum(__isLeapYear(date.tm_year + 1900) ? __MONTH_DAYS_LEAP : __MONTH_DAYS_REGULAR, date.tm_mon - 1), 3);
            },
            "%m": function (date) {
              return leadingNulls(date.tm_mon + 1, 2);
            },
            "%M": function (date) {
              return leadingNulls(date.tm_min, 2);
            },
            "%n": function () {
              return "\n";
            },
            "%p": function (date) {
              if (date.tm_hour >= 0 && date.tm_hour < 12) {
                return "AM";
              } else {
                return "PM";
              }
            },
            "%S": function (date) {
              return leadingNulls(date.tm_sec, 2);
            },
            "%t": function () {
              return "\t";
            },
            "%u": function (date) {
              return date.tm_wday || 7;
            },
            "%U": function (date) {
              var days = date.tm_yday + 7 - date.tm_wday;
              return leadingNulls(Math.floor(days / 7), 2);
            },
            "%V": function (date) {
              var val = Math.floor((date.tm_yday + 7 - (date.tm_wday + 6) % 7) / 7);
              if ((date.tm_wday + 371 - date.tm_yday - 2) % 7 <= 2) {
                val++;
              }
              if (!val) {
                val = 52;
                var dec31 = (date.tm_wday + 7 - date.tm_yday - 1) % 7;
                if (dec31 == 4 || dec31 == 5 && __isLeapYear(date.tm_year % 400 - 1)) {
                  val++;
                }
              } else if (val == 53) {
                var jan1 = (date.tm_wday + 371 - date.tm_yday) % 7;
                if (jan1 != 4 && (jan1 != 3 || !__isLeapYear(date.tm_year))) val = 1;
              }
              return leadingNulls(val, 2);
            },
            "%w": function (date) {
              return date.tm_wday;
            },
            "%W": function (date) {
              var days = date.tm_yday + 7 - (date.tm_wday + 6) % 7;
              return leadingNulls(Math.floor(days / 7), 2);
            },
            "%y": function (date) {
              return (date.tm_year + 1900).toString().substring(2);
            },
            "%Y": function (date) {
              return date.tm_year + 1900;
            },
            "%z": function (date) {
              var off = date.tm_gmtoff;
              var ahead = off >= 0;
              off = Math.abs(off) / 60;
              off = off / 60 * 100 + off % 60;
              return (ahead ? "+" : "-") + String("0000" + off).slice(-4);
            },
            "%Z": function (date) {
              return date.tm_zone;
            },
            "%%": function () {
              return "%";
            }
          };
          pattern = pattern.replace(/%%/g, "\0\0");
          for (var rule in EXPANSION_RULES_2) {
            if (pattern.includes(rule)) {
              pattern = pattern.replace(new RegExp(rule, "g"), EXPANSION_RULES_2[rule](date));
            }
          }
          pattern = pattern.replace(/\0\0/g, "%");
          var bytes = intArrayFromString(pattern, false);
          if (bytes.length > maxsize) {
            return 0;
          }
          writeArrayToMemory(bytes, s);
          return bytes.length - 1;
        }
        var FSNode = function (parent, name, mode, rdev) {
          if (!parent) {
            parent = this;
          }
          this.parent = parent;
          this.mount = parent.mount;
          this.mounted = null;
          this.id = FS.nextInode++;
          this.name = name;
          this.mode = mode;
          this.node_ops = {};
          this.stream_ops = {};
          this.rdev = rdev;
        };
        var readMode = 292 | 73;
        var writeMode = 146;
        Object.defineProperties(FSNode.prototype, {
          read: {
            get: function () {
              return (this.mode & readMode) === readMode;
            },
            set: function (val) {
              val ? this.mode |= readMode : this.mode &= ~readMode;
            }
          },
          write: {
            get: function () {
              return (this.mode & writeMode) === writeMode;
            },
            set: function (val) {
              val ? this.mode |= writeMode : this.mode &= ~writeMode;
            }
          },
          isFolder: {
            get: function () {
              return FS.isDir(this.mode);
            }
          },
          isDevice: {
            get: function () {
              return FS.isChrdev(this.mode);
            }
          }
        });
        FS.FSNode = FSNode;
        FS.staticInit();
        Module["FS_createPath"] = FS.createPath;
        Module["FS_createDataFile"] = FS.createDataFile;
        Module["FS_readFile"] = FS.readFile;
        Module["FS_createPath"] = FS.createPath;
        Module["FS_createDataFile"] = FS.createDataFile;
        Module["FS_createPreloadedFile"] = FS.createPreloadedFile;
        Module["FS_unlink"] = FS.unlink;
        Module["FS_createLazyFile"] = FS.createLazyFile;
        Module["FS_createDevice"] = FS.createDevice;
        let __dotnet_replacement_PThread = undefined;
        let __dotnet_replacements = {
          scriptUrl: (_document.currentScript && _document.currentScript.src || new URL('bootloader.js', _document.baseURI).href),
          fetch: globalThis.fetch,
          require: require,
          updateGlobalBufferAndViews: updateGlobalBufferAndViews,
          pthreadReplacements: __dotnet_replacement_PThread
        };
        if (ENVIRONMENT_IS_NODE) {
          __dotnet_replacements.requirePromise = Promise.resolve().then(function () { return _polyfillNode_module$1; }).then(mod => mod.createRequire((_document.currentScript && _document.currentScript.src || new URL('bootloader.js', _document.baseURI).href)));
        }
        let __dotnet_exportedAPI = __dotnet_runtime.__initializeImportsAndExports({
          isGlobal: false,
          isNode: ENVIRONMENT_IS_NODE,
          isWorker: ENVIRONMENT_IS_WORKER,
          isShell: ENVIRONMENT_IS_SHELL,
          isWeb: ENVIRONMENT_IS_WEB,
          isPThread: false,
          quit_: quit_,
          ExitStatus: ExitStatus,
          requirePromise: __dotnet_replacements.requirePromise
        }, {
          mono: MONO,
          binding: BINDING,
          internal: INTERNAL,
          module: Module,
          marshaled_imports: IMPORTS
        }, __dotnet_replacements, __callbackAPI);
        updateGlobalBufferAndViews = __dotnet_replacements.updateGlobalBufferAndViews;
        var fetch = __dotnet_replacements.fetch;
        _scriptDir = __dirname = scriptDirectory = __dotnet_replacements.scriptDirectory;
        if (ENVIRONMENT_IS_NODE) {
          __dotnet_replacements.requirePromise.then(someRequire => {
            require = someRequire;
          });
        }
        var noExitRuntime = __dotnet_replacements.noExitRuntime;
        function intArrayFromString(stringy, dontAddNull, length) {
          var len = length > 0 ? length : lengthBytesUTF8(stringy) + 1;
          var u8array = new Array(len);
          var numBytesWritten = stringToUTF8Array(stringy, u8array, 0, u8array.length);
          if (dontAddNull) u8array.length = numBytesWritten;
          return u8array;
        }
        var asmLibraryArg = {
          "__assert_fail": ___assert_fail,
          "__cxa_allocate_exception": ___cxa_allocate_exception,
          "__cxa_begin_catch": ___cxa_begin_catch,
          "__cxa_end_catch": ___cxa_end_catch,
          "__cxa_find_matching_catch_3": ___cxa_find_matching_catch_3,
          "__cxa_throw": ___cxa_throw,
          "__resumeException": ___resumeException,
          "__syscall_faccessat": ___syscall_faccessat,
          "__syscall_fadvise64": ___syscall_fadvise64,
          "__syscall_fcntl64": ___syscall_fcntl64,
          "__syscall_fstat64": ___syscall_fstat64,
          "__syscall_fstatfs64": ___syscall_fstatfs64,
          "__syscall_ftruncate64": ___syscall_ftruncate64,
          "__syscall_getcwd": ___syscall_getcwd,
          "__syscall_getdents64": ___syscall_getdents64,
          "__syscall_ioctl": ___syscall_ioctl,
          "__syscall_lstat64": ___syscall_lstat64,
          "__syscall_newfstatat": ___syscall_newfstatat,
          "__syscall_openat": ___syscall_openat,
          "__syscall_readlinkat": ___syscall_readlinkat,
          "__syscall_stat64": ___syscall_stat64,
          "__syscall_unlinkat": ___syscall_unlinkat,
          "_emscripten_date_now": __emscripten_date_now,
          "_emscripten_get_now_is_monotonic": __emscripten_get_now_is_monotonic,
          "_localtime_js": __localtime_js,
          "_mmap_js": __mmap_js,
          "_munmap_js": __munmap_js,
          "_tzset_js": __tzset_js,
          "abort": _abort,
          "dotnet_browser_entropy": _dotnet_browser_entropy,
          "emscripten_get_heap_max": _emscripten_get_heap_max,
          "emscripten_get_now": _emscripten_get_now,
          "emscripten_get_now_res": _emscripten_get_now_res,
          "emscripten_memcpy_big": _emscripten_memcpy_big,
          "emscripten_resize_heap": _emscripten_resize_heap,
          "environ_get": _environ_get,
          "environ_sizes_get": _environ_sizes_get,
          "exit": _exit,
          "fd_close": _fd_close,
          "fd_pread": _fd_pread,
          "fd_read": _fd_read,
          "fd_seek": _fd_seek,
          "fd_write": _fd_write,
          "getTempRet0": _getTempRet0,
          "invoke_vi": invoke_vi,
          "llvm_eh_typeid_for": _llvm_eh_typeid_for,
          "mono_set_timeout": _mono_set_timeout,
          "mono_wasm_bind_cs_function": _mono_wasm_bind_cs_function,
          "mono_wasm_bind_js_function": _mono_wasm_bind_js_function,
          "mono_wasm_create_cs_owned_object_ref": _mono_wasm_create_cs_owned_object_ref,
          "mono_wasm_get_by_index_ref": _mono_wasm_get_by_index_ref,
          "mono_wasm_get_global_object_ref": _mono_wasm_get_global_object_ref,
          "mono_wasm_get_object_property_ref": _mono_wasm_get_object_property_ref,
          "mono_wasm_invoke_bound_function": _mono_wasm_invoke_bound_function,
          "mono_wasm_invoke_js_blazor": _mono_wasm_invoke_js_blazor,
          "mono_wasm_invoke_js_with_args_ref": _mono_wasm_invoke_js_with_args_ref,
          "mono_wasm_marshal_promise": _mono_wasm_marshal_promise,
          "mono_wasm_release_cs_owned_object": _mono_wasm_release_cs_owned_object,
          "mono_wasm_set_by_index_ref": _mono_wasm_set_by_index_ref,
          "mono_wasm_set_entrypoint_breakpoint": _mono_wasm_set_entrypoint_breakpoint,
          "mono_wasm_set_object_property_ref": _mono_wasm_set_object_property_ref,
          "mono_wasm_trace_logger": _mono_wasm_trace_logger,
          "mono_wasm_typed_array_from_ref": _mono_wasm_typed_array_from_ref,
          "mono_wasm_typed_array_to_array_ref": _mono_wasm_typed_array_to_array_ref,
          "schedule_background_exec": _schedule_background_exec,
          "setTempRet0": _setTempRet0,
          "strftime": _strftime
        };
        createWasm();
        Module["___wasm_call_ctors"] = function () {
          return (Module["___wasm_call_ctors"] = Module["asm"]["__wasm_call_ctors"]).apply(null, arguments);
        };
        Module["_ScreepsDotNet_InitNative_World"] = function () {
          return (Module["_ScreepsDotNet_InitNative_World"] = Module["asm"]["ScreepsDotNet_InitNative_World"]).apply(null, arguments);
        };
        var _malloc = Module["_malloc"] = function () {
          return (_malloc = Module["_malloc"] = Module["asm"]["malloc"]).apply(null, arguments);
        };
        Module["_mono_wasm_register_root"] = function () {
          return (Module["_mono_wasm_register_root"] = Module["asm"]["mono_wasm_register_root"]).apply(null, arguments);
        };
        Module["_mono_wasm_deregister_root"] = function () {
          return (Module["_mono_wasm_deregister_root"] = Module["asm"]["mono_wasm_deregister_root"]).apply(null, arguments);
        };
        Module["_mono_wasm_typed_array_new_ref"] = function () {
          return (Module["_mono_wasm_typed_array_new_ref"] = Module["asm"]["mono_wasm_typed_array_new_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_unbox_enum"] = function () {
          return (Module["_mono_wasm_unbox_enum"] = Module["asm"]["mono_wasm_unbox_enum"]).apply(null, arguments);
        };
        Module["_mono_wasm_add_assembly"] = function () {
          return (Module["_mono_wasm_add_assembly"] = Module["asm"]["mono_wasm_add_assembly"]).apply(null, arguments);
        };
        Module["_mono_wasm_add_satellite_assembly"] = function () {
          return (Module["_mono_wasm_add_satellite_assembly"] = Module["asm"]["mono_wasm_add_satellite_assembly"]).apply(null, arguments);
        };
        Module["_mono_wasm_setenv"] = function () {
          return (Module["_mono_wasm_setenv"] = Module["asm"]["mono_wasm_setenv"]).apply(null, arguments);
        };
        Module["_mono_wasm_getenv"] = function () {
          return (Module["_mono_wasm_getenv"] = Module["asm"]["mono_wasm_getenv"]).apply(null, arguments);
        };
        Module["_mono_wasm_register_bundled_satellite_assemblies"] = function () {
          return (Module["_mono_wasm_register_bundled_satellite_assemblies"] = Module["asm"]["mono_wasm_register_bundled_satellite_assemblies"]).apply(null, arguments);
        };
        var _free = Module["_free"] = function () {
          return (_free = Module["_free"] = Module["asm"]["free"]).apply(null, arguments);
        };
        Module["_mono_wasm_load_runtime"] = function () {
          return (Module["_mono_wasm_load_runtime"] = Module["asm"]["mono_wasm_load_runtime"]).apply(null, arguments);
        };
        Module["_mono_wasm_assembly_load"] = function () {
          return (Module["_mono_wasm_assembly_load"] = Module["asm"]["mono_wasm_assembly_load"]).apply(null, arguments);
        };
        Module["_mono_wasm_get_corlib"] = function () {
          return (Module["_mono_wasm_get_corlib"] = Module["asm"]["mono_wasm_get_corlib"]).apply(null, arguments);
        };
        Module["_mono_wasm_assembly_find_class"] = function () {
          return (Module["_mono_wasm_assembly_find_class"] = Module["asm"]["mono_wasm_assembly_find_class"]).apply(null, arguments);
        };
        Module["_mono_wasm_runtime_run_module_cctor"] = function () {
          return (Module["_mono_wasm_runtime_run_module_cctor"] = Module["asm"]["mono_wasm_runtime_run_module_cctor"]).apply(null, arguments);
        };
        Module["_mono_wasm_assembly_find_method"] = function () {
          return (Module["_mono_wasm_assembly_find_method"] = Module["asm"]["mono_wasm_assembly_find_method"]).apply(null, arguments);
        };
        Module["_mono_wasm_get_delegate_invoke_ref"] = function () {
          return (Module["_mono_wasm_get_delegate_invoke_ref"] = Module["asm"]["mono_wasm_get_delegate_invoke_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_box_primitive_ref"] = function () {
          return (Module["_mono_wasm_box_primitive_ref"] = Module["asm"]["mono_wasm_box_primitive_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_invoke_method_ref"] = function () {
          return (Module["_mono_wasm_invoke_method_ref"] = Module["asm"]["mono_wasm_invoke_method_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_invoke_method_bound"] = function () {
          return (Module["_mono_wasm_invoke_method_bound"] = Module["asm"]["mono_wasm_invoke_method_bound"]).apply(null, arguments);
        };
        Module["_mono_wasm_assembly_get_entry_point"] = function () {
          return (Module["_mono_wasm_assembly_get_entry_point"] = Module["asm"]["mono_wasm_assembly_get_entry_point"]).apply(null, arguments);
        };
        Module["_mono_wasm_string_get_utf8"] = function () {
          return (Module["_mono_wasm_string_get_utf8"] = Module["asm"]["mono_wasm_string_get_utf8"]).apply(null, arguments);
        };
        Module["_mono_wasm_string_from_js"] = function () {
          return (Module["_mono_wasm_string_from_js"] = Module["asm"]["mono_wasm_string_from_js"]).apply(null, arguments);
        };
        Module["_mono_wasm_string_from_utf16_ref"] = function () {
          return (Module["_mono_wasm_string_from_utf16_ref"] = Module["asm"]["mono_wasm_string_from_utf16_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_get_obj_class"] = function () {
          return (Module["_mono_wasm_get_obj_class"] = Module["asm"]["mono_wasm_get_obj_class"]).apply(null, arguments);
        };
        Module["_mono_wasm_get_obj_type"] = function () {
          return (Module["_mono_wasm_get_obj_type"] = Module["asm"]["mono_wasm_get_obj_type"]).apply(null, arguments);
        };
        Module["_mono_wasm_try_unbox_primitive_and_get_type_ref"] = function () {
          return (Module["_mono_wasm_try_unbox_primitive_and_get_type_ref"] = Module["asm"]["mono_wasm_try_unbox_primitive_and_get_type_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_array_length"] = function () {
          return (Module["_mono_wasm_array_length"] = Module["asm"]["mono_wasm_array_length"]).apply(null, arguments);
        };
        Module["_mono_wasm_array_get"] = function () {
          return (Module["_mono_wasm_array_get"] = Module["asm"]["mono_wasm_array_get"]).apply(null, arguments);
        };
        Module["_mono_wasm_array_get_ref"] = function () {
          return (Module["_mono_wasm_array_get_ref"] = Module["asm"]["mono_wasm_array_get_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_obj_array_new_ref"] = function () {
          return (Module["_mono_wasm_obj_array_new_ref"] = Module["asm"]["mono_wasm_obj_array_new_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_obj_array_new"] = function () {
          return (Module["_mono_wasm_obj_array_new"] = Module["asm"]["mono_wasm_obj_array_new"]).apply(null, arguments);
        };
        Module["_mono_wasm_obj_array_set"] = function () {
          return (Module["_mono_wasm_obj_array_set"] = Module["asm"]["mono_wasm_obj_array_set"]).apply(null, arguments);
        };
        Module["_mono_wasm_obj_array_set_ref"] = function () {
          return (Module["_mono_wasm_obj_array_set_ref"] = Module["asm"]["mono_wasm_obj_array_set_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_string_array_new_ref"] = function () {
          return (Module["_mono_wasm_string_array_new_ref"] = Module["asm"]["mono_wasm_string_array_new_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_exec_regression"] = function () {
          return (Module["_mono_wasm_exec_regression"] = Module["asm"]["mono_wasm_exec_regression"]).apply(null, arguments);
        };
        Module["_mono_wasm_exit"] = function () {
          return (Module["_mono_wasm_exit"] = Module["asm"]["mono_wasm_exit"]).apply(null, arguments);
        };
        Module["_mono_wasm_set_main_args"] = function () {
          return (Module["_mono_wasm_set_main_args"] = Module["asm"]["mono_wasm_set_main_args"]).apply(null, arguments);
        };
        Module["_mono_wasm_strdup"] = function () {
          return (Module["_mono_wasm_strdup"] = Module["asm"]["mono_wasm_strdup"]).apply(null, arguments);
        };
        Module["_mono_wasm_parse_runtime_options"] = function () {
          return (Module["_mono_wasm_parse_runtime_options"] = Module["asm"]["mono_wasm_parse_runtime_options"]).apply(null, arguments);
        };
        Module["_mono_wasm_enable_on_demand_gc"] = function () {
          return (Module["_mono_wasm_enable_on_demand_gc"] = Module["asm"]["mono_wasm_enable_on_demand_gc"]).apply(null, arguments);
        };
        Module["_mono_wasm_intern_string_ref"] = function () {
          return (Module["_mono_wasm_intern_string_ref"] = Module["asm"]["mono_wasm_intern_string_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_string_get_data_ref"] = function () {
          return (Module["_mono_wasm_string_get_data_ref"] = Module["asm"]["mono_wasm_string_get_data_ref"]).apply(null, arguments);
        };
        Module["_mono_wasm_string_get_data"] = function () {
          return (Module["_mono_wasm_string_get_data"] = Module["asm"]["mono_wasm_string_get_data"]).apply(null, arguments);
        };
        Module["_mono_wasm_class_get_type"] = function () {
          return (Module["_mono_wasm_class_get_type"] = Module["asm"]["mono_wasm_class_get_type"]).apply(null, arguments);
        };
        Module["_mono_wasm_type_get_class"] = function () {
          return (Module["_mono_wasm_type_get_class"] = Module["asm"]["mono_wasm_type_get_class"]).apply(null, arguments);
        };
        Module["_mono_wasm_get_type_name"] = function () {
          return (Module["_mono_wasm_get_type_name"] = Module["asm"]["mono_wasm_get_type_name"]).apply(null, arguments);
        };
        Module["_mono_wasm_get_type_aqn"] = function () {
          return (Module["_mono_wasm_get_type_aqn"] = Module["asm"]["mono_wasm_get_type_aqn"]).apply(null, arguments);
        };
        Module["_mono_wasm_write_managed_pointer_unsafe"] = function () {
          return (Module["_mono_wasm_write_managed_pointer_unsafe"] = Module["asm"]["mono_wasm_write_managed_pointer_unsafe"]).apply(null, arguments);
        };
        Module["_mono_wasm_copy_managed_pointer"] = function () {
          return (Module["_mono_wasm_copy_managed_pointer"] = Module["asm"]["mono_wasm_copy_managed_pointer"]).apply(null, arguments);
        };
        Module["_mono_wasm_i52_to_f64"] = function () {
          return (Module["_mono_wasm_i52_to_f64"] = Module["asm"]["mono_wasm_i52_to_f64"]).apply(null, arguments);
        };
        Module["_mono_wasm_u52_to_f64"] = function () {
          return (Module["_mono_wasm_u52_to_f64"] = Module["asm"]["mono_wasm_u52_to_f64"]).apply(null, arguments);
        };
        Module["_mono_wasm_f64_to_u52"] = function () {
          return (Module["_mono_wasm_f64_to_u52"] = Module["asm"]["mono_wasm_f64_to_u52"]).apply(null, arguments);
        };
        Module["_mono_wasm_f64_to_i52"] = function () {
          return (Module["_mono_wasm_f64_to_i52"] = Module["asm"]["mono_wasm_f64_to_i52"]).apply(null, arguments);
        };
        Module["_mono_wasm_send_dbg_command_with_parms"] = function () {
          return (Module["_mono_wasm_send_dbg_command_with_parms"] = Module["asm"]["mono_wasm_send_dbg_command_with_parms"]).apply(null, arguments);
        };
        Module["_mono_wasm_send_dbg_command"] = function () {
          return (Module["_mono_wasm_send_dbg_command"] = Module["asm"]["mono_wasm_send_dbg_command"]).apply(null, arguments);
        };
        Module["_mono_wasm_event_pipe_enable"] = function () {
          return (Module["_mono_wasm_event_pipe_enable"] = Module["asm"]["mono_wasm_event_pipe_enable"]).apply(null, arguments);
        };
        Module["_mono_wasm_event_pipe_session_start_streaming"] = function () {
          return (Module["_mono_wasm_event_pipe_session_start_streaming"] = Module["asm"]["mono_wasm_event_pipe_session_start_streaming"]).apply(null, arguments);
        };
        Module["_mono_wasm_event_pipe_session_disable"] = function () {
          return (Module["_mono_wasm_event_pipe_session_disable"] = Module["asm"]["mono_wasm_event_pipe_session_disable"]).apply(null, arguments);
        };
        Module["_memset"] = function () {
          return (Module["_memset"] = Module["asm"]["memset"]).apply(null, arguments);
        };
        var ___errno_location = Module["___errno_location"] = function () {
          return (___errno_location = Module["___errno_location"] = Module["asm"]["__errno_location"]).apply(null, arguments);
        };
        Module["_mono_background_exec"] = function () {
          return (Module["_mono_background_exec"] = Module["asm"]["mono_background_exec"]).apply(null, arguments);
        };
        Module["_mono_print_method_from_ip"] = function () {
          return (Module["_mono_print_method_from_ip"] = Module["asm"]["mono_print_method_from_ip"]).apply(null, arguments);
        };
        Module["_mono_set_timeout_exec"] = function () {
          return (Module["_mono_set_timeout_exec"] = Module["asm"]["mono_set_timeout_exec"]).apply(null, arguments);
        };
        Module["___dl_seterr"] = function () {
          return (Module["___dl_seterr"] = Module["asm"]["__dl_seterr"]).apply(null, arguments);
        };
        Module["_htonl"] = function () {
          return (Module["_htonl"] = Module["asm"]["htonl"]).apply(null, arguments);
        };
        Module["_htons"] = function () {
          return (Module["_htons"] = Module["asm"]["htons"]).apply(null, arguments);
        };
        var _emscripten_builtin_memalign = Module["_emscripten_builtin_memalign"] = function () {
          return (_emscripten_builtin_memalign = Module["_emscripten_builtin_memalign"] = Module["asm"]["emscripten_builtin_memalign"]).apply(null, arguments);
        };
        Module["_ntohs"] = function () {
          return (Module["_ntohs"] = Module["asm"]["ntohs"]).apply(null, arguments);
        };
        Module["_memalign"] = function () {
          return (Module["_memalign"] = Module["asm"]["memalign"]).apply(null, arguments);
        };
        var _setThrew = Module["_setThrew"] = function () {
          return (_setThrew = Module["_setThrew"] = Module["asm"]["setThrew"]).apply(null, arguments);
        };
        var stackSave = Module["stackSave"] = function () {
          return (stackSave = Module["stackSave"] = Module["asm"]["stackSave"]).apply(null, arguments);
        };
        var stackRestore = Module["stackRestore"] = function () {
          return (stackRestore = Module["stackRestore"] = Module["asm"]["stackRestore"]).apply(null, arguments);
        };
        var stackAlloc = Module["stackAlloc"] = function () {
          return (stackAlloc = Module["stackAlloc"] = Module["asm"]["stackAlloc"]).apply(null, arguments);
        };
        var ___cxa_can_catch = Module["___cxa_can_catch"] = function () {
          return (___cxa_can_catch = Module["___cxa_can_catch"] = Module["asm"]["__cxa_can_catch"]).apply(null, arguments);
        };
        var ___cxa_is_pointer_type = Module["___cxa_is_pointer_type"] = function () {
          return (___cxa_is_pointer_type = Module["___cxa_is_pointer_type"] = Module["asm"]["__cxa_is_pointer_type"]).apply(null, arguments);
        };
        Module["dynCall_jiiiiiiiii"] = function () {
          return (Module["dynCall_jiiiiiiiii"] = Module["asm"]["dynCall_jiiiiiiiii"]).apply(null, arguments);
        };
        Module["dynCall_vj"] = function () {
          return (Module["dynCall_vj"] = Module["asm"]["dynCall_vj"]).apply(null, arguments);
        };
        Module["dynCall_iji"] = function () {
          return (Module["dynCall_iji"] = Module["asm"]["dynCall_iji"]).apply(null, arguments);
        };
        Module["dynCall_ij"] = function () {
          return (Module["dynCall_ij"] = Module["asm"]["dynCall_ij"]).apply(null, arguments);
        };
        Module["dynCall_iij"] = function () {
          return (Module["dynCall_iij"] = Module["asm"]["dynCall_iij"]).apply(null, arguments);
        };
        Module["dynCall_jj"] = function () {
          return (Module["dynCall_jj"] = Module["asm"]["dynCall_jj"]).apply(null, arguments);
        };
        Module["dynCall_iiijiiiii"] = function () {
          return (Module["dynCall_iiijiiiii"] = Module["asm"]["dynCall_iiijiiiii"]).apply(null, arguments);
        };
        Module["dynCall_j"] = function () {
          return (Module["dynCall_j"] = Module["asm"]["dynCall_j"]).apply(null, arguments);
        };
        Module["dynCall_jd"] = function () {
          return (Module["dynCall_jd"] = Module["asm"]["dynCall_jd"]).apply(null, arguments);
        };
        Module["dynCall_jf"] = function () {
          return (Module["dynCall_jf"] = Module["asm"]["dynCall_jf"]).apply(null, arguments);
        };
        Module["dynCall_jiji"] = function () {
          return (Module["dynCall_jiji"] = Module["asm"]["dynCall_jiji"]).apply(null, arguments);
        };
        Module["dynCall_iijj"] = function () {
          return (Module["dynCall_iijj"] = Module["asm"]["dynCall_iijj"]).apply(null, arguments);
        };
        Module["dynCall_iijji"] = function () {
          return (Module["dynCall_iijji"] = Module["asm"]["dynCall_iijji"]).apply(null, arguments);
        };
        Module["dynCall_iiiij"] = function () {
          return (Module["dynCall_iiiij"] = Module["asm"]["dynCall_iiiij"]).apply(null, arguments);
        };
        Module["dynCall_viij"] = function () {
          return (Module["dynCall_viij"] = Module["asm"]["dynCall_viij"]).apply(null, arguments);
        };
        Module["dynCall_ji"] = function () {
          return (Module["dynCall_ji"] = Module["asm"]["dynCall_ji"]).apply(null, arguments);
        };
        Module["dynCall_jijj"] = function () {
          return (Module["dynCall_jijj"] = Module["asm"]["dynCall_jijj"]).apply(null, arguments);
        };
        Module["dynCall_jij"] = function () {
          return (Module["dynCall_jij"] = Module["asm"]["dynCall_jij"]).apply(null, arguments);
        };
        function invoke_vi(index, a1) {
          var sp = stackSave();
          try {
            getWasmTableEntry(index)(a1);
          } catch (e) {
            stackRestore(sp);
            if (e !== e + 0) throw e;
            _setThrew(1, 0);
          }
        }
        Module["ccall"] = ccall;
        Module["cwrap"] = cwrap;
        Module["UTF8ArrayToString"] = UTF8ArrayToString;
        Module["UTF8ToString"] = UTF8ToString;
        Module["addRunDependency"] = addRunDependency;
        Module["removeRunDependency"] = removeRunDependency;
        Module["FS_createPath"] = FS.createPath;
        Module["FS_createDataFile"] = FS.createDataFile;
        Module["FS_createPreloadedFile"] = FS.createPreloadedFile;
        Module["FS_createLazyFile"] = FS.createLazyFile;
        Module["FS_createDevice"] = FS.createDevice;
        Module["FS_unlink"] = FS.unlink;
        Module["print"] = out;
        Module["setValue"] = setValue;
        Module["getValue"] = getValue;
        Module["FS"] = FS;
        var calledRun;
        function ExitStatus(status) {
          this.name = "ExitStatus";
          this.message = "Program terminated with exit(" + status + ")";
          this.status = status;
        }
        dependenciesFulfilled = function runCaller() {
          if (!calledRun) run();
          if (!calledRun) dependenciesFulfilled = runCaller;
        };
        function run(args) {
          if (runDependencies > 0) {
            return;
          }
          preRun();
          if (runDependencies > 0) {
            return;
          }
          function doRun() {
            if (calledRun) return;
            calledRun = true;
            Module["calledRun"] = true;
            if (ABORT) return;
            initRuntime();
            readyPromiseResolve(Module);
            if (Module["onRuntimeInitialized"]) Module["onRuntimeInitialized"]();
            postRun();
          }
          if (Module["setStatus"]) {
            Module["setStatus"]("Running...");
            setTimeout$1(function () {
              setTimeout$1(function () {
                Module["setStatus"]("");
              }, 1);
              doRun();
            }, 1);
          } else {
            doRun();
          }
        }
        Module["run"] = run;
        function exit(status, implicit) {
          procExit(status);
        }
        function procExit(code) {
          if (!keepRuntimeAlive()) {
            if (Module["onExit"]) Module["onExit"](code);
            ABORT = true;
          }
          quit_(code, new ExitStatus(code));
        }
        if (Module["preInit"]) {
          if (typeof Module["preInit"] == "function") Module["preInit"] = [Module["preInit"]];
          while (Module["preInit"].length > 0) {
            Module["preInit"].pop()();
          }
        }
        run();
        createDotnetRuntime.ready = createDotnetRuntime.ready.then(() => {
          return __dotnet_exportedAPI;
        });
        return createDotnetRuntime.ready;
      };
    })();
    const MONO = {},
      BINDING = {},
      INTERNAL = {},
      IMPORTS = {};

    // TODO duplicated from emscripten, so we can use them in the __setEmscriptenEntrypoint
    var ENVIRONMENT_IS_WEB = typeof window == 'object';
    var ENVIRONMENT_IS_WORKER = typeof importScripts == 'function';
    var ENVIRONMENT_IS_NODE = typeof browser$1 == 'object' && typeof browser$1.versions == 'object' && typeof browser$1.versions.node == 'string';
    var ENVIRONMENT_IS_SHELL = !ENVIRONMENT_IS_WEB && !ENVIRONMENT_IS_NODE && !ENVIRONMENT_IS_WORKER;
    __dotnet_runtime.__setEmscriptenEntrypoint(createDotnetRuntime, {
      isNode: ENVIRONMENT_IS_NODE,
      isShell: ENVIRONMENT_IS_SHELL,
      isWeb: ENVIRONMENT_IS_WEB,
      isWorker: ENVIRONMENT_IS_WORKER
    });
    __dotnet_runtime.moduleExports.dotnet;
    __dotnet_runtime.moduleExports.exit;

    function _empty() {}
    function _awaitIgnored(value, direct) {
      if (!direct) {
        return value && value.then ? value.then(_empty) : Promise$1.resolve();
      }
    }
    function _catch(body, recover) {
      try {
        var result = body();
      } catch (e) {
        return recover(e);
      }
      if (result && result.then) {
        return result.then(void 0, recover);
      }
      return result;
    }
    function _continue(value, then) {
      return value && value.then ? value.then(then) : then(value);
    }
    function _await(value, then, direct) {
      if (direct) {
        return then ? then(value) : value;
      }
      if (!value || !value.then) {
        value = Promise$1.resolve(value);
      }
      return then ? value.then(then) : value;
    }
    function _call(body, then, direct) {
      if (direct) {
        return then ? then(body()) : body();
      }
      try {
        var result = Promise$1.resolve(body());
        return then ? result.then(then) : result;
      } catch (e) {
        return Promise$1.reject(e);
      }
    }
    class DotNet {
      get ready() {
        return this._ready;
      }
      get isTickBarrier() {
        return this._tickBarrier != null && this._tickBarrier > this.tickIndex;
      }
      constructor(manifest, env) {
        this.fileMap = {};
        this.tickIndex = 0;
        this.imports = {};
        this.verboseLogging = false;
        this._ready = false;
        this.startSetupRuntime = false;
        this.customRuntimeSetupFnList = [];
        this.manifest = manifest.manifest;
        this.monoConfig = manifest.config;
        this.isArena = env === 'arena';
        this.isWorld = env === 'world';
      }
      setModuleImports(moduleName, imports) {
        this.imports[moduleName] = imports;
      }
      setVerboseLogging(verboseLogging) {
        this.verboseLogging = verboseLogging;
      }
      setPerfFn(perfFn) {
        this.perfFn = perfFn;
      }
      addCustomRuntimeSetupFunction(setupFn) {
        this.customRuntimeSetupFnList.push(setupFn);
      }
      getExports() {
        return this.exports;
      }
      init() {
        if (this.isArena) {
          setSuppressedLogMode(true);
        }
        this.decodeManifest();
        this.createRuntime();
      }
      loop(loopFn) {
        if (this.isArena) {
          setSuppressedLogMode(false);
        }
        try {
          ++this.tickIndex;
          let profiler = this.profile();
          this.runPendingAsyncActions();
          if (loopFn) {
            loopFn();
          }
          profiler = this.profile(profiler, 'loop');
        } finally {
          if (this.isArena) {
            setSuppressedLogMode(true);
          }
        }
      }
      profile(marker, blockName) {
        if (!this.perfFn) {
          return 0;
        }
        const cpuTime = this.perfFn();
        if (blockName == null || marker == null) {
          return cpuTime;
        }
        const delta = cpuTime - marker;
        log('PROFILE', blockName, `${(delta / 100000 | 0) / 10} ms`);
        return cpuTime;
      }
      profileAccum(marker, blockName) {
        if (!this.perfFn) {
          return 0;
        }
        if (blockName == null) {
          return this.perfFn() - marker;
        }
        log('PROFILE', blockName, `${(marker / 100000 | 0) / 10} ms`);
        return marker;
      }
      decodeManifest() {
        let profiler = this.profile();
        let profilerB64 = 0,
          profilerInflate = 0;
        let totalBytes = 0;
        for (const entry of this.manifest) {
          const profilerB64Marker = this.profile();
          let fileDataRaw;
          if ('b64' in entry) {
            fileDataRaw = toBytes(entry.b64);
          } else if ('b32768' in entry) {
            fileDataRaw = decode(entry.b32768);
          } else {
            log(`entry '${entry.path}' does not contain b64 or b32768 data`);
            continue;
          }
          profilerB64 += this.profileAccum(profilerB64Marker);
          if (entry.compressed) {
            const profilerInflateMarker = this.profile();
            const fileData = new Uint8Array(entry.originalSize);
            inflateSync(fileDataRaw, {
              out: fileData
            });
            profilerInflate += this.profileAccum(profilerInflateMarker);
            this.fileMap[entry.path] = fileData;
            totalBytes += fileData.length;
          } else {
            this.fileMap[entry.path] = fileDataRaw;
            totalBytes += fileDataRaw.length;
          }
        }
        log(`loaded ${this.manifest.length} items from the manifest, totalling ${totalBytes / 1024 | 0} KiB of data`);
        profiler = this.profile(profiler, 'decodeManifest');
        profilerB64 = this.profileAccum(profilerB64, 'decodeManifest (b64)');
        profilerInflate = this.profileAccum(profilerInflate, 'decodeManifest (inflate)');
      }
      createRuntime() {
        debug(`creating dotnet runtime...`);
        let profiler = this.profile();
        createDotnetRuntime(api => {
          return {
            config: {
              ...this.monoConfig,
              diagnosticTracing: this.verboseLogging
            },
            imports: {},
            downloadResource: request => ({
              name: request.name,
              url: request.resolvedUrl,
              response: Promise$1.resolve(this.downloadResource(request.resolvedUrl))
            }),
            preRun: () => {
              profiler = this.profile(profiler, 'preRun');
              if (this.isWorld) {
                this.tickBarrier();
              }
            },
            onRuntimeInitialized: () => {
              profiler = this.profile(profiler, 'onRuntimeInitialized');
            },
            onDotnetReady: () => {
              profiler = this.profile(profiler, 'onDotnetReady');
            }
          };
        }).then(x => {
          this.runtimeApi = x;
          return this.setupRuntime();
        });
        this.runPendingAsyncActions();
      }
      downloadResource(url) {
        if (this.fileMap[url]) {
          //log(`got downloadResource for '${request.resolvedUrl}' - found in file map`);
          return {
            ok: true,
            url,
            status: 200,
            statusText: 'ok',
            arrayBuffer: () => Promise$1.resolve(this.fileMap[url]),
            json: () => Promise$1.reject('json not yet supported')
          };
        } else {
          //log(`got downloadResource for '${request.resolvedUrl!}' - NOT found in file map`);
          return {
            ok: false,
            url,
            status: 404,
            statusText: 'not found',
            arrayBuffer: () => undefined,
            json: () => undefined
          };
        }
      }
      setupRuntime() {
        const _this = this;
        return _call(function () {
          if (!_this.runtimeApi) {
            _this.pendingError = new Error(`Tried to setupRuntime when runtimeApi was not set`);
            return _await();
          }
          if (_this.startSetupRuntime) {
            _this.pendingError = new Error(`Tried to setupRuntime when it was already called`);
            return _await();
          }
          _this.startSetupRuntime = true;
          let profiler = _this.profile();
          debug(`setting up dotnet runtime...`);
          for (const setupFn of _this.customRuntimeSetupFnList) {
            setupFn(_this.runtimeApi);
          }
          for (const moduleName in _this.imports) {
            _this.runtimeApi.setModuleImports(moduleName, _this.imports[moduleName]);
          }
          profiler = _this.profile(profiler, 'setModuleImports');
          return _await(_this.runtimeApi.getAssemblyExports(_this.monoConfig.mainAssemblyName), function (_this$runtimeApi$getA) {
            _this.exports = _this$runtimeApi$getA;
            if (_this.exports) {
              debug(`exports: ${Object.keys(_this.exports)}`);
            } else {
              debug(`failed to retrieve exports`);
            }
            profiler = _this.profile(profiler, 'getAssemblyExports');
            return _continue(_catch(function () {
              return _awaitIgnored(_this.runtimeApi.runMain(_this.monoConfig.mainAssemblyName, []));
            }, function (err) {
              error(`got error when running Program.Main(): ${err.stack}`);
            }), function () {
              profiler = _this.profile(profiler, 'runMain');
              _this._ready = true;
            });
          });
        });
      }
      runPendingAsyncActions() {
        if (this.isTickBarrier) {
          debug(`refusing runPendingAsyncActions as tick barrier is in place`);
          return;
        }
        let numTimersProcessed;
        do {
          numTimersProcessed = advanceFrame();
        } while (numTimersProcessed > 0 && !this.isTickBarrier && !this.pendingError);
        if (this.pendingError) {
          throw this.pendingError;
        }
      }
      tickBarrier() {
        //debug(`TICK BARRIER`);
        this._tickBarrier = this.tickIndex + 1;
        cancelAdvanceFrame();
      }
    }

    var _polyfillNode_module = {};

    var _polyfillNode_module$1 = /*#__PURE__*/Object.freeze({
        __proto__: null,
        default: _polyfillNode_module
    });

    exports.DotNet = DotNet;

    return exports;

})({});

module.exports = bootloader;
