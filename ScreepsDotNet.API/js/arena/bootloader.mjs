import * as utils from 'game/utils';
import * as prototypes from 'game/prototypes';
import * as constants from 'game/constants';
import * as pathFinder from 'game/path-finder';
import * as visual from 'game/visual';
import { arenaInfo } from 'game';
var bootloader = (function (exports) {
  'use strict';

  function _arrayLikeToArray(r, a) {
    (null == a || a > r.length) && (a = r.length);
    for (var e = 0, n = Array(a); e < a; e++) n[e] = r[e];
    return n;
  }
  function _arrayWithHoles(r) {
    if (Array.isArray(r)) return r;
  }
  function _assertThisInitialized(e) {
    if (void 0 === e) throw new ReferenceError("this hasn't been initialised - super() hasn't been called");
    return e;
  }
  function _classCallCheck(a, n) {
    if (!(a instanceof n)) throw new TypeError("Cannot call a class as a function");
  }
  function _defineProperties(e, r) {
    for (var t = 0; t < r.length; t++) {
      var o = r[t];
      o.enumerable = o.enumerable || !1, o.configurable = !0, "value" in o && (o.writable = !0), Object.defineProperty(e, _toPropertyKey(o.key), o);
    }
  }
  function _createClass(e, r, t) {
    return r && _defineProperties(e.prototype, r), t && _defineProperties(e, t), Object.defineProperty(e, "prototype", {
      writable: !1
    }), e;
  }
  function _createForOfIteratorHelper(r, e) {
    var t = "undefined" != typeof Symbol && r[Symbol.iterator] || r["@@iterator"];
    if (!t) {
      if (Array.isArray(r) || (t = _unsupportedIterableToArray(r)) || e && r && "number" == typeof r.length) {
        t && (r = t);
        var n = 0,
          F = function () {};
        return {
          s: F,
          n: function () {
            return n >= r.length ? {
              done: !0
            } : {
              done: !1,
              value: r[n++]
            };
          },
          e: function (r) {
            throw r;
          },
          f: F
        };
      }
      throw new TypeError("Invalid attempt to iterate non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method.");
    }
    var o,
      a = !0,
      u = !1;
    return {
      s: function () {
        t = t.call(r);
      },
      n: function () {
        var r = t.next();
        return a = r.done, r;
      },
      e: function (r) {
        u = !0, o = r;
      },
      f: function () {
        try {
          a || null == t.return || t.return();
        } finally {
          if (u) throw o;
        }
      }
    };
  }
  function _createSuper(t) {
    var r = _isNativeReflectConstruct();
    return function () {
      var e,
        o = _getPrototypeOf(t);
      if (r) {
        var s = _getPrototypeOf(this).constructor;
        e = Reflect.construct(o, arguments, s);
      } else e = o.apply(this, arguments);
      return _possibleConstructorReturn(this, e);
    };
  }
  function _defineProperty(e, r, t) {
    return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, {
      value: t,
      enumerable: !0,
      configurable: !0,
      writable: !0
    }) : e[r] = t, e;
  }
  function _get() {
    return _get = "undefined" != typeof Reflect && Reflect.get ? Reflect.get.bind() : function (e, t, r) {
      var p = _superPropBase(e, t);
      if (p) {
        var n = Object.getOwnPropertyDescriptor(p, t);
        return n.get ? n.get.call(arguments.length < 3 ? e : r) : n.value;
      }
    }, _get.apply(null, arguments);
  }
  function _getPrototypeOf(t) {
    return _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf.bind() : function (t) {
      return t.__proto__ || Object.getPrototypeOf(t);
    }, _getPrototypeOf(t);
  }
  function _inherits(t, e) {
    if ("function" != typeof e && null !== e) throw new TypeError("Super expression must either be null or a function");
    t.prototype = Object.create(e && e.prototype, {
      constructor: {
        value: t,
        writable: !0,
        configurable: !0
      }
    }), Object.defineProperty(t, "prototype", {
      writable: !1
    }), e && _setPrototypeOf(t, e);
  }
  function _isNativeReflectConstruct() {
    try {
      var t = !Boolean.prototype.valueOf.call(Reflect.construct(Boolean, [], function () {}));
    } catch (t) {}
    return (_isNativeReflectConstruct = function () {
      return !!t;
    })();
  }
  function _iterableToArrayLimit(r, l) {
    var t = null == r ? null : "undefined" != typeof Symbol && r[Symbol.iterator] || r["@@iterator"];
    if (null != t) {
      var e,
        n,
        i,
        u,
        a = [],
        f = !0,
        o = !1;
      try {
        if (i = (t = t.call(r)).next, 0 === l) {
          if (Object(t) !== t) return;
          f = !1;
        } else for (; !(f = (e = i.call(t)).done) && (a.push(e.value), a.length !== l); f = !0);
      } catch (r) {
        o = !0, n = r;
      } finally {
        try {
          if (!f && null != t.return && (u = t.return(), Object(u) !== u)) return;
        } finally {
          if (o) throw n;
        }
      }
      return a;
    }
  }
  function _nonIterableRest() {
    throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method.");
  }
  function ownKeys(e, r) {
    var t = Object.keys(e);
    if (Object.getOwnPropertySymbols) {
      var o = Object.getOwnPropertySymbols(e);
      r && (o = o.filter(function (r) {
        return Object.getOwnPropertyDescriptor(e, r).enumerable;
      })), t.push.apply(t, o);
    }
    return t;
  }
  function _objectSpread2(e) {
    for (var r = 1; r < arguments.length; r++) {
      var t = null != arguments[r] ? arguments[r] : {};
      r % 2 ? ownKeys(Object(t), !0).forEach(function (r) {
        _defineProperty(e, r, t[r]);
      }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(e, Object.getOwnPropertyDescriptors(t)) : ownKeys(Object(t)).forEach(function (r) {
        Object.defineProperty(e, r, Object.getOwnPropertyDescriptor(t, r));
      });
    }
    return e;
  }
  function _possibleConstructorReturn(t, e) {
    if (e && ("object" == typeof e || "function" == typeof e)) return e;
    if (void 0 !== e) throw new TypeError("Derived constructors may only return object or undefined");
    return _assertThisInitialized(t);
  }
  function _setPrototypeOf(t, e) {
    return _setPrototypeOf = Object.setPrototypeOf ? Object.setPrototypeOf.bind() : function (t, e) {
      return t.__proto__ = e, t;
    }, _setPrototypeOf(t, e);
  }
  function _slicedToArray(r, e) {
    return _arrayWithHoles(r) || _iterableToArrayLimit(r, e) || _unsupportedIterableToArray(r, e) || _nonIterableRest();
  }
  function _superPropBase(t, o) {
    for (; !{}.hasOwnProperty.call(t, o) && null !== (t = _getPrototypeOf(t)););
    return t;
  }
  function _toPrimitive(t, r) {
    if ("object" != typeof t || !t) return t;
    var e = t[Symbol.toPrimitive];
    if (void 0 !== e) {
      var i = e.call(t, r || "default");
      if ("object" != typeof i) return i;
      throw new TypeError("@@toPrimitive must return a primitive value.");
    }
    return ("string" === r ? String : Number)(t);
  }
  function _toPropertyKey(t) {
    var i = _toPrimitive(t, "string");
    return "symbol" == typeof i ? i : i + "";
  }
  function _typeof(o) {
    "@babel/helpers - typeof";

    return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (o) {
      return typeof o;
    } : function (o) {
      return o && "function" == typeof Symbol && o.constructor === Symbol && o !== Symbol.prototype ? "symbol" : typeof o;
    }, _typeof(o);
  }
  function _unsupportedIterableToArray(r, a) {
    if (r) {
      if ("string" == typeof r) return _arrayLikeToArray(r, a);
      var t = {}.toString.call(r).slice(8, -1);
      return "Object" === t && r.constructor && (t = r.constructor.name), "Map" === t || "Set" === t ? Array.from(r) : "Arguments" === t || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(t) ? _arrayLikeToArray(r, a) : void 0;
    }
  }

  (function (r) {
    function x() {}
    function y() {}
    var z = String.fromCharCode,
      v = {}.toString,
      A = v.call(r.SharedArrayBuffer),
      B = v(),
      q = r.Uint8Array,
      t = q || Array,
      w = q ? ArrayBuffer : t,
      C = w.isView || function (g) {
        return g && "length" in g;
      },
      D = v.call(w.prototype);
    w = y.prototype;
    var E = r.TextEncoder,
      a = new (q ? Uint16Array : t)(32);
    x.prototype.decode = function (g) {
      if (!C(g)) {
        var l = v.call(g);
        if (l !== D && l !== A && l !== B) throw TypeError("Failed to execute 'decode' on 'TextDecoder': The provided value is not of type '(ArrayBuffer or ArrayBufferView)'");
        g = q ? new t(g) : g || [];
      }
      for (var f = l = "", b = 0, c = g.length | 0, u = c - 32 | 0, e, d, h = 0, p = 0, m, k = 0, n = -1; b < c;) {
        for (e = b <= u ? 32 : c - b | 0; k < e; b = b + 1 | 0, k = k + 1 | 0) {
          d = g[b] & 255;
          switch (d >> 4) {
            case 15:
              m = g[b = b + 1 | 0] & 255;
              if (2 !== m >> 6 || 247 < d) {
                b = b - 1 | 0;
                break;
              }
              h = (d & 7) << 6 | m & 63;
              p = 5;
              d = 256;
            case 14:
              m = g[b = b + 1 | 0] & 255, h <<= 6, h |= (d & 15) << 6 | m & 63, p = 2 === m >> 6 ? p + 4 | 0 : 24, d = d + 256 & 768;
            case 13:
            case 12:
              m = g[b = b + 1 | 0] & 255, h <<= 6, h |= (d & 31) << 6 | m & 63, p = p + 7 | 0, b < c && 2 === m >> 6 && h >> p && 1114112 > h ? (d = h, h = h - 65536 | 0, 0 <= h && (n = (h >> 10) + 55296 | 0, d = (h & 1023) + 56320 | 0, 31 > k ? (a[k] = n, k = k + 1 | 0, n = -1) : (m = n, n = d, d = m))) : (d >>= 8, b = b - d - 1 | 0, d = 65533), h = p = 0, e = b <= u ? 32 : c - b | 0;
            default:
              a[k] = d;
              continue;
            case 11:
            case 10:
            case 9:
            case 8:
          }
          a[k] = 65533;
        }
        f += z(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16], a[17], a[18], a[19], a[20], a[21], a[22], a[23], a[24], a[25], a[26], a[27], a[28], a[29], a[30], a[31]);
        32 > k && (f = f.slice(0, k - 32 | 0));
        if (b < c) {
          if (a[0] = n, k = ~n >>> 31, n = -1, f.length < l.length) continue;
        } else -1 !== n && (f += z(n));
        l += f;
        f = "";
      }
      return l;
    };
    w.encode = function (g) {
      g = void 0 === g ? "" : "" + g;
      var l = g.length | 0,
        f = new t((l << 1) + 8 | 0),
        b,
        c = 0,
        u = !q;
      for (b = 0; b < l; b = b + 1 | 0, c = c + 1 | 0) {
        var e = g.charCodeAt(b) | 0;
        if (127 >= e) f[c] = e;else {
          if (2047 >= e) f[c] = 192 | e >> 6;else {
            a: {
              if (55296 <= e) if (56319 >= e) {
                var d = g.charCodeAt(b = b + 1 | 0) | 0;
                if (56320 <= d && 57343 >= d) {
                  e = (e << 10) + d - 56613888 | 0;
                  if (65535 < e) {
                    f[c] = 240 | e >> 18;
                    f[c = c + 1 | 0] = 128 | e >> 12 & 63;
                    f[c = c + 1 | 0] = 128 | e >> 6 & 63;
                    f[c = c + 1 | 0] = 128 | e & 63;
                    continue;
                  }
                  break a;
                }
                e = 65533;
              } else 57343 >= e && (e = 65533);
              !u && b << 1 < c && b << 1 < (c - 7 | 0) && (u = !0, d = new t(3 * l), d.set(f), f = d);
            }
            f[c] = 224 | e >> 12;
            f[c = c + 1 | 0] = 128 | e >> 6 & 63;
          }
          f[c = c + 1 | 0] = 128 | e & 63;
        }
      }
      return q ? f.subarray(0, c) : f.slice(0, c);
    };
    E || (r.TextDecoder = x, r.TextEncoder = y);
  })("" + void 0 == (typeof global === "undefined" ? "undefined" : _typeof(global)) ? "" + void 0 == (typeof self === "undefined" ? "undefined" : _typeof(self)) ? global : self : global); //AnonyCo

  var lookup = new Uint8Array([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 62, 0, 62, 0, 63, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 0, 0, 0, 0, 63, 0, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51]);
  function base64Decode(source, target) {
    var sourceLength = source.length;
    var paddingLength = source[sourceLength - 2] === '=' ? 2 : source[sourceLength - 1] === '=' ? 1 : 0;
    var baseLength = sourceLength - paddingLength & 0xfffffffc;
    var tmp;
    var i = 0;
    var byteIndex = 0;
    for (; i < baseLength; i += 4) {
      tmp = lookup[source.charCodeAt(i)] << 18 | lookup[source.charCodeAt(i + 1)] << 12 | lookup[source.charCodeAt(i + 2)] << 6 | lookup[source.charCodeAt(i + 3)];
      target[byteIndex++] = tmp >> 16 & 0xFF;
      target[byteIndex++] = tmp >> 8 & 0xFF;
      target[byteIndex++] = tmp & 0xFF;
    }
    if (paddingLength === 1) {
      tmp = lookup[source.charCodeAt(i)] << 10 | lookup[source.charCodeAt(i + 1)] << 4 | lookup[source.charCodeAt(i + 2)] >> 2;
      target[byteIndex++] = tmp >> 8 & 0xFF;
      target[byteIndex++] = tmp & 0xFF;
    }
    if (paddingLength === 2) {
      tmp = lookup[source.charCodeAt(i)] << 2 | lookup[source.charCodeAt(i + 1)] >> 4;
      target[byteIndex++] = tmp & 0xFF;
    }
  }

  /**
    Base32768 is a binary-to-text encoding optimised for UTF-16-encoded text.
    (e.g. Windows, Java, JavaScript)
  */

  // Z is a number, usually a uint15 but sometimes a uint7

  var BITS_PER_CHAR = 15; // Base32768 is a 15-bit encoding
  var BITS_PER_BYTE = 8;
  var pairStrings = ['ҠҿԀԟڀڿݠޟ߀ߟကဟႠႿᄀᅟᆀᆟᇠሿበቿዠዿጠጿᎠᏟᐠᙟᚠᛟកសᠠᡟᣀᣟᦀᦟ᧠᧿ᨠᨿᯀᯟᰀᰟᴀᴟ⇠⇿⋀⋟⍀⏟␀␟─❟➀➿⠀⥿⦠⦿⨠⩟⪀⪿⫠⭟ⰀⰟⲀⳟⴀⴟⵀⵟ⺠⻟㇀㇟㐀䶟䷀龿ꀀꑿ꒠꒿ꔀꗿꙀꙟꚠꛟ꜀ꝟꞀꞟꡀꡟ', 'ƀƟɀʟ'];
  var lookupD = {};
  pairStrings.forEach(function (pairString, r) {
    // Decompression
    var encodeRepertoire = [];
    pairString.match(/(?:[\0-\t\x0B\f\x0E-\u2027\u202A-\uD7FF\uE000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?:[^\uD800-\uDBFF]|^)[\uDC00-\uDFFF])(?:[\0-\t\x0B\f\x0E-\u2027\u202A-\uD7FF\uE000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?:[^\uD800-\uDBFF]|^)[\uDC00-\uDFFF])/g).forEach(function (pair) {
      var first = pair.codePointAt(0);
      var last = pair.codePointAt(1);
      for (var codePoint = first; codePoint <= last; codePoint++) {
        encodeRepertoire.push(String.fromCodePoint(codePoint));
      }
    });
    var numZBits = BITS_PER_CHAR - BITS_PER_BYTE * r; // 0 -> 15, 1 -> 7
    encodeRepertoire.forEach(function (chr, z) {
      lookupD[chr] = [numZBits, z];
    });
  });
  var decode = function decode(str) {
    var length = str.length;

    // This length is a guess. There's a chance we allocate one more byte here
    // than we actually need. But we can count and slice it off later
    var uint8Array = new Uint8Array(Math.floor(length * BITS_PER_CHAR / BITS_PER_BYTE));
    var numUint8s = 0;
    var uint8 = 0;
    var numUint8Bits = 0;
    for (var i = 0; i < length; i++) {
      var chr = str.charAt(i);
      if (!(chr in lookupD)) {
        throw new Error("Unrecognised Base32768 character: ".concat(chr));
      }
      var _lookupD$chr = _slicedToArray(lookupD[chr], 2),
        numZBits = _lookupD$chr[0],
        z = _lookupD$chr[1];
      if (numZBits !== BITS_PER_CHAR && i !== length - 1) {
        throw new Error('Secondary character found before end of input at position ' + String(i));
      }

      // Take most significant bit first
      for (var j = numZBits - 1; j >= 0; j--) {
        var bit = z >> j & 1;
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
  var freb = function freb(eb, start) {
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
    var x = (i & 0xAAAA) >> 1 | (i & 0x5555) << 1;
    x = (x & 0xCCCC) >> 2 | (x & 0x3333) << 2;
    x = (x & 0xF0F0) >> 4 | (x & 0x0F0F) << 4;
    rev[i] = ((x & 0xFF00) >> 8 | (x & 0x00FF) << 8) >> 1;
  }
  // create huffman tree from u8 "map": index -> code length for code index
  // mb (max bits) must be at most 15
  // TODO: optimize/split up?
  var hMap = function hMap(cd, mb, r) {
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
  var max = function max(a) {
    var m = a[0];
    for (var i = 1; i < a.length; ++i) {
      if (a[i] > m) m = a[i];
    }
    return m;
  };
  // read d, starting at bit p and mask with m
  var bits = function bits(d, p, m) {
    var o = p / 8 | 0;
    return (d[o] | d[o + 1] << 8) >> (p & 7) & m;
  };
  // read d, starting at bit p continuing for at least 16 bits
  var bits16 = function bits16(d, p) {
    var o = p / 8 | 0;
    return (d[o] | d[o + 1] << 8 | d[o + 2] << 16) >> (p & 7);
  };
  // get end of byte
  var shft = function shft(p) {
    return (p + 7) / 8 | 0;
  };
  // typed array slice - allows garbage collector to free original reference,
  // while being more compatible than .slice
  var slc = function slc(v, s, e) {
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
  var err = function err(ind, msg, nt) {
    var e = new Error(msg || ec[ind]);
    e.code = ind;
    if (Error.captureStackTrace) Error.captureStackTrace(e, err);
    if (!nt) throw e;
    return e;
  };
  // expands raw DEFLATE data
  var inflt = function inflt(dat, st, buf, dict) {
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
    var cbuf = function cbuf(l) {
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
    var _final = st.f || 0,
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
        _final = bits(dat, pos, 1);
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
          st.b = bt += l, st.p = pos = t * 8, st.f = _final;
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
      st.l = lm, st.p = lpos, st.b = bt, st.f = _final;
      if (lm) _final = 1, st.m = lbt, st.d = dm, st.n = dbt;
    } while (!_final);
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

  var CLR_OBJECT_HANDLE = Symbol('clr-object-handle');
  function hasId(obj) {
    // TODO: Are we going to have an issue here with pojo's with ids? e.g. an object from Memory which is just { id: 'xyz' }
    // We could address this by including an instanceof check here against known prototypes that go stale across ticks
    // e.g. World's GameObject
    return 'id' in obj;
  }
  var ObjectInterop = /*#__PURE__*/function () {
    function ObjectInterop() {
      _classCallCheck(this, ObjectInterop);
      _defineProperty(this, "_objectTrackingList", []);
      _defineProperty(this, "_objectTrackingListById", new Map());
      _defineProperty(this, "_nonExtensibleObjectTrackingMap", new WeakMap());
      _defineProperty(this, "_freeObjectHandleList", []);
      _defineProperty(this, "_nextObjectHandle", 0);
      _defineProperty(this, "_numBeginTrackingObjects", 0);
      _defineProperty(this, "_numReleaseTrackingObjects", 0);
      _defineProperty(this, "_numTotalTrackingObjects", 0);
    }
    _createClass(ObjectInterop, [{
      key: "numBeginTrackingObjects",
      get: function get() {
        return this._numBeginTrackingObjects;
      }
    }, {
      key: "numReleaseTrackingObjects",
      get: function get() {
        return this._numReleaseTrackingObjects;
      }
    }, {
      key: "numTotalTrackingObjects",
      get: function get() {
        return this._numTotalTrackingObjects;
      }
    }, {
      key: "loop",
      value: function loop() {
        this._numBeginTrackingObjects = 0;
        this._numReleaseTrackingObjects = 0;
      }
    }, {
      key: "releaseObjectHandle",
      value: function releaseObjectHandle(objectHandle) {
        var obj = this._objectTrackingList[objectHandle];
        if (obj == null) {
          return;
        }
        this._objectTrackingList[objectHandle] = undefined;
        if (hasId(obj) && this._objectTrackingListById.get(obj.id) === obj) {
          this._objectTrackingListById["delete"](obj.id);
        }
        this.clearObjectHandle(obj);
        ++this._numReleaseTrackingObjects;
        --this._numTotalTrackingObjects;
        this._freeObjectHandleList.push(objectHandle);
      }
    }, {
      key: "getObjectByHandle",
      value: function getObjectByHandle(objectHandle) {
        return this._objectTrackingList[objectHandle];
      }
    }, {
      key: "getObjectHandle",
      value: function getObjectHandle(obj) {
        var _obj$CLR_OBJECT_HANDL;
        return (_obj$CLR_OBJECT_HANDL = obj[CLR_OBJECT_HANDLE]) !== null && _obj$CLR_OBJECT_HANDL !== void 0 ? _obj$CLR_OBJECT_HANDL : this._nonExtensibleObjectTrackingMap.get(obj);
      }
    }, {
      key: "allocateObjectHandle",
      value: function allocateObjectHandle() {
        var _this$_freeObjectHand;
        return (_this$_freeObjectHand = this._freeObjectHandleList.pop()) !== null && _this$_freeObjectHand !== void 0 ? _this$_freeObjectHand : this._nextObjectHandle++;
      }
    }, {
      key: "assignObjectHandle",
      value: function assignObjectHandle(obj, newObjectHandle) {
        if (newObjectHandle == null) {
          newObjectHandle = this.allocateObjectHandle();
          ++this._numBeginTrackingObjects;
          ++this._numTotalTrackingObjects;
        }
        if (Object.isExtensible(obj)) {
          obj[CLR_OBJECT_HANDLE] = newObjectHandle;
        } else {
          this._nonExtensibleObjectTrackingMap.set(obj, newObjectHandle);
        }
        this._objectTrackingList[newObjectHandle] = obj;
        if (hasId(obj)) {
          this._objectTrackingListById.set(obj.id, obj);
        }
        return newObjectHandle;
      }
    }, {
      key: "clearObjectHandle",
      value: function clearObjectHandle(obj) {
        if (Object.isExtensible(obj)) {
          obj[CLR_OBJECT_HANDLE] = undefined;
        } else {
          this._nonExtensibleObjectTrackingMap["delete"](obj);
        }
      }
    }, {
      key: "replaceObject",
      value: function replaceObject(p0, newObj) {
        var objectHandle = typeof p0 === 'number' ? p0 : this.getObjectHandle(p0);
        if (objectHandle == null) {
          return;
        }
        var oldObj = typeof p0 === 'number' ? this._objectTrackingList[objectHandle] : p0;
        if (oldObj != null) {
          this.clearObjectHandle(oldObj);
        }
        return this.assignObjectHandle(newObj, objectHandle);
      }
    }, {
      key: "getOrAssignObjectHandle",
      value: function getOrAssignObjectHandle(obj) {
        var _objectHandle;
        var objectHandle = this.getObjectHandle(obj);
        if (objectHandle == null) {
          // It doesn't - if it has an id, see if we're already tracking a stale version of the game object
          if (hasId(obj)) {
            var previousVersion = this._objectTrackingListById.get(obj.id);
            if (previousVersion != null && previousVersion !== obj) {
              // Replace the previous version with this one and reuse the tracking id
              objectHandle = this.replaceObject(previousVersion, obj);
            }
          }
        }
        return (_objectHandle = objectHandle) !== null && _objectHandle !== void 0 ? _objectHandle : this.assignObjectHandle(obj);
      }
    }, {
      key: "visitTrackedObjects",
      value: function visitTrackedObjects(visitor) {
        for (var i = 0; i < this._nextObjectHandle; ++i) {
          var obj = this._objectTrackingList[i];
          if (obj == null) {
            continue;
          }
          visitor(obj);
        }
      }
    }]);
    return ObjectInterop;
  }();

  var INTEROP_VALUE_TYPE_NAMES = ['void', 'bool', 'byte', 'sbyte', 'ushort', 'short', 'uint', 'int', 'ulong', 'long', 'float', 'double', 'void*', 'char*', 'JSObject', '[]', 'Name', 'struct'];
  function stringifyParamSpec(paramSpec) {
    if (paramSpec.type === 15 /* InteropValueType.Array */ && paramSpec.elementSpec) {
      return "".concat(stringifyParamSpec(paramSpec.elementSpec), "[]");
    }
    return "".concat(INTEROP_VALUE_TYPE_NAMES[paramSpec.type]).concat(paramSpec.nullable ? '?' : '');
  }
  var EXCEPTION_PARAM_SPEC = {
    type: 13 /* InteropValueType.String */,
    nullable: false,
    nullAsUndefined: false
  };
  var IMPORT_BINDING_SCOPE = {
    EXCEPTION_PARAM_SPEC: EXCEPTION_PARAM_SPEC
  };
  var Interop = /*#__PURE__*/function () {
    function Interop(profileFn) {
      _classCallCheck(this, Interop);
      _defineProperty(this, "interopImport", void 0);
      _defineProperty(this, "_profileFn", void 0);
      _defineProperty(this, "_imports", {});
      _defineProperty(this, "_objects", new ObjectInterop());
      _defineProperty(this, "_boundImportList", []);
      _defineProperty(this, "_boundRawImportList", []);
      _defineProperty(this, "_boundImportSymbolList", []);
      _defineProperty(this, "_nameList", []);
      _defineProperty(this, "_nameTable", {});
      _defineProperty(this, "_structList", []);
      _defineProperty(this, "_memory", void 0);
      _defineProperty(this, "_malloc", void 0);
      _defineProperty(this, "_free", void 0);
      _defineProperty(this, "_numBoundImportInvokes", 0);
      _defineProperty(this, "_numImportBinds", 0);
      _defineProperty(this, "_timeInInterop", 0);
      _defineProperty(this, "_timeInJsUserCode", 0);
      _defineProperty(this, "_transientBufferPtr", 0);
      _defineProperty(this, "_transientBufferSz", 0);
      _defineProperty(this, "_transientBufferHead", 0);
      this._profileFn = profileFn;
      this.interopImport = {};
      this.interopImport['bind-import'] = this.js_bind_import.bind(this);
      this.interopImport['invoke-import'] = this.js_invoke_import.bind(this);
      this.interopImport['release-object-reference'] = this.js_release_object_reference.bind(this);
      this.interopImport['set-name'] = this.js_set_name.bind(this);
      this.interopImport['define-struct'] = this.js_define_struct.bind(this);
      this.interopImport['invoke-i-i'] = this.js_invoke_i_i.bind(this);
      this.interopImport['invoke-i-ii'] = this.js_invoke_i_ii.bind(this);
      this.interopImport['invoke-i-iii'] = this.js_invoke_i_iii.bind(this);
      this.interopImport['invoke-i-o'] = this.js_invoke_i_o.bind(this);
      this.interopImport['invoke-i-oi'] = this.js_invoke_i_oi.bind(this);
      this.interopImport['invoke-i-on'] = this.js_invoke_i_on.bind(this);
      this.interopImport['invoke-i-oii'] = this.js_invoke_i_oii.bind(this);
      this.interopImport['invoke-i-oo'] = this.js_invoke_i_oo.bind(this);
      this.interopImport['invoke-i-ooi'] = this.js_invoke_i_ooi.bind(this);
      this.interopImport['invoke-i-ooii'] = this.js_invoke_i_ooii.bind(this);
      this.interopImport['invoke-d-v'] = this.js_invoke_d_v.bind(this);
    }
    _createClass(Interop, [{
      key: "objects",
      get: function get() {
        return this._objects;
      }
    }, {
      key: "memory",
      get: function get() {
        return this._memory;
      },
      set: function set(value) {
        this._memory = value;
      }
    }, {
      key: "malloc",
      get: function get() {
        return this._malloc;
      },
      set: function set(value) {
        this._malloc = value;
      }
    }, {
      key: "free",
      get: function get() {
        return this._malloc;
      },
      set: function set(value) {
        this._free = value;
      }
    }, {
      key: "setImports",
      value: function setImports(moduleName, importTable) {
        this._imports[moduleName] = importTable;
      }
    }, {
      key: "loop",
      value: function loop() {
        this._numBoundImportInvokes = 0;
        this._numImportBinds = 0;
        this._timeInInterop = 0;
        this._timeInJsUserCode = 0;
        this._objects.loop();
        this._transientBufferHead = 0;
      }
    }, {
      key: "buildProfilerString",
      value: function buildProfilerString() {
        var phrases = ["".concat((this._timeInInterop * 100 | 0) / 100, " ms in interop"), "".concat((this._timeInJsUserCode * 100 | 0) / 100, " ms in screeps api"), "".concat(this._numBoundImportInvokes, " js interop calls"), "".concat(this._objects.numTotalTrackingObjects, " +").concat(this._objects.numBeginTrackingObjects, " -").concat(this._objects.numReleaseTrackingObjects, " tracked js objects")];
        if (this._numBoundImportInvokes > 0) {
          phrases.push("".concat(this._boundImportList.length, " +").concat(this._numImportBinds, " bound imports"));
        }
        return phrases.join(', ');
      }
    }, {
      key: "resolveImport",
      value: function resolveImport(moduleName, importTable, importName) {
        var segments = importName.split('.');
        var currentValue = importTable;
        var _iterator = _createForOfIteratorHelper(segments),
          _step;
        try {
          for (_iterator.s(); !(_step = _iterator.n()).done;) {
            var segment = _step.value;
            if (currentValue == null || _typeof(currentValue) !== 'object') {
              throw new Error("unable to resolve import '".concat(importName, "' from module '").concat(moduleName, "' (one or more keys along the path did not resolve to an object)"));
            }
            currentValue = currentValue[segment];
          }
        } catch (err) {
          _iterator.e(err);
        } finally {
          _iterator.f();
        }
        if (currentValue == null || typeof currentValue !== 'function') {
          throw new Error("unable to resolve import '".concat(importName, "' from module '").concat(moduleName, "' (the path did not resolve to a function)"));
        }
        return currentValue;
      }
    }, {
      key: "js_bind_import",
      value: function js_bind_import(moduleNamePtr, importNamePtr, functionSpecPtr) {
        this.memory.flush();
        var moduleName = this.stringToJs(moduleNamePtr);
        var importTable = this._imports[moduleName];
        if (!importTable) {
          throw new Error("unknown import module '".concat(moduleName, "'"));
        }
        var importName = this.stringToJs(importNamePtr);
        var importFunction = this.resolveImport(moduleName, importTable, importName);
        this._boundRawImportList.push(importFunction);
        var functionSpec = this.functionSpecToJs(functionSpecPtr);
        var importIndex = this._boundImportList.length;
        var boundImportFunction = this.createImportBinding(importFunction, functionSpec, importIndex);
        this._boundImportList.push(boundImportFunction);
        this._boundImportSymbolList.push({
          fullName: "".concat(moduleName, "::").concat(importName),
          functionSpec: functionSpec
        });
        ++this._numImportBinds;
        // console.log(this.stringifyImportBindingForDisplay(importIndex));
        return importIndex;
      }
    }, {
      key: "js_invoke_import",
      value: function js_invoke_import(importIndex, paramsBufferPtr) {
        var boundImportFunction = this._boundImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(paramsBufferPtr);
      }
    }, {
      key: "js_release_object_reference",
      value: function js_release_object_reference(objectHandle) {
        this._objects.releaseObjectHandle(objectHandle);
      }
    }, {
      key: "js_set_name",
      value: function js_set_name(nameIndex, valuePtr) {
        this.memory.flush();
        var value = this.stringToJs(valuePtr);
        this._nameList[nameIndex] = value;
        this._nameTable[value] = nameIndex;
      }
    }, {
      key: "js_define_struct",
      value: function js_define_struct(numFields, fieldsPtr) {
        this.memory.flush();
        var spec = {
          fieldSpecs: []
        };
        spec.fieldSpecs.length = numFields;
        try {
          this._memory.enterConstrainedRange(fieldsPtr, numFields * 8);
          for (var i = 0; i < numFields; ++i) {
            var fieldName = this.stringToJs(this._memory.readI32(fieldsPtr));
            fieldsPtr += 4;
            var paramSpec = this.paramSpecToJs(fieldsPtr);
            fieldsPtr += 4;
            spec.fieldSpecs[i] = {
              fieldName: fieldName,
              paramSpec: paramSpec
            };
          }
          return this._structList.push(spec) - 1;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "js_invoke_i_i",
      value: function js_invoke_i_i(importIndex, p0) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(p0);
      }
    }, {
      key: "js_invoke_i_ii",
      value: function js_invoke_i_ii(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(p0, p1);
      }
    }, {
      key: "js_invoke_i_iii",
      value: function js_invoke_i_iii(importIndex, p0, p1, p2) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(p0, p1, p2);
      }
    }, {
      key: "js_invoke_i_o",
      value: function js_invoke_i_o(importIndex, p0) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0));
      }
    }, {
      key: "js_invoke_i_oi",
      value: function js_invoke_i_oi(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), p1);
      }
    }, {
      key: "js_invoke_i_on",
      value: function js_invoke_i_on(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._nameList[p1]);
      }
    }, {
      key: "js_invoke_i_oii",
      value: function js_invoke_i_oii(importIndex, p0, p1, p2) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), p1, p2);
      }
    }, {
      key: "js_invoke_i_oo",
      value: function js_invoke_i_oo(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._objects.getObjectByHandle(p1));
      }
    }, {
      key: "js_invoke_i_ooi",
      value: function js_invoke_i_ooi(importIndex, p0, p1, p2) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._objects.getObjectByHandle(p1), p2);
      }
    }, {
      key: "js_invoke_i_ooii",
      value: function js_invoke_i_ooii(importIndex, p0, p1, p2, p3) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._objects.getObjectByHandle(p1), p2, p3);
      }
    }, {
      key: "js_invoke_d_v",
      value: function js_invoke_d_v(importIndex) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        this.memory.flush();
        return boundImportFunction();
      }
    }, {
      key: "createImportBinding",
      value: function createImportBinding(importFunction, functionSpec, importIndex) {
        var lines = [];
        lines.push("var t0 = this._profileFn();");
        lines.push("this._memory.flush();");
        lines.push("this._memory.enterConstrainedRange(paramsBufferPtr, ".concat((functionSpec.paramSpecs.length + 2) * 16, ");"));
        lines.push("var returnValPtr = paramsBufferPtr;");
        lines.push("var exceptionValPtr = paramsBufferPtr + 16;");
        lines.push("var argsPtr = exceptionValPtr + 16;");
        var paramList = '';
        if (functionSpec.paramSpecs.length > 0) {
          var paramListArr = [];
          for (var i = 0; i < functionSpec.paramSpecs.length; ++i) {
            lines.push("var arg".concat(i, ";"));
            paramListArr.push("arg".concat(i));
          }
          paramList = paramListArr.join(', ');
          lines.push("try {");
          for (var _i = 0; _i < functionSpec.paramSpecs.length; ++_i) {
            lines.push("  arg".concat(_i, " = this.marshalToJs(argsPtr, functionSpec.paramSpecs[").concat(_i, "]);"));
            lines.push("  argsPtr += 16;");
          }
          lines.push("} catch (err) {");
          lines.push("  this._memory.exitConstrainedRange();");
          lines.push("  throw new Error(this.stringifyImportBindingForDisplay(".concat(importIndex, ") + ': ' + err.stack);"));
          lines.push("}");
        }
        lines.push("var t1 = this._profileFn();");
        lines.push("this._timeInInterop += (t1 - t0);");
        lines.push("var returnVal;");
        lines.push("try {");
        lines.push("  returnVal = importFunction(".concat(paramList, ");"));
        lines.push("  this._memory.flush();");
        lines.push("  this.marshalToClr(returnValPtr, functionSpec.returnSpec, returnVal);");
        lines.push("  return 1;");
        lines.push("} catch (err) {");
        lines.push("  this.marshalToClr(exceptionValPtr, scope.EXCEPTION_PARAM_SPEC, err.stack);");
        lines.push("} finally {");
        lines.push("  var t2 = this._profileFn();");
        lines.push("  this._timeInJsUserCode += (t2 - t1);");
        lines.push("  this._memory.exitConstrainedRange();");
        lines.push("}");
        var compiler = Function("return function import_binding_".concat(importIndex, "(scope, importFunction, functionSpec, paramsBufferPtr) {\n").concat(lines.join('\n'), "\n};"));
        return compiler().bind(this, IMPORT_BINDING_SCOPE, importFunction, functionSpec);
      }
    }, {
      key: "marshalToJs",
      value: function marshalToJs(valuePtr, paramSpec) {
        var _paramSpec$elementSpe, _INTEROP_VALUE_TYPE_N2;
        var valueType = this._memory.readU8(valuePtr + 12);
        if (valueType === 0 /* InteropValueType.Void */ && paramSpec.nullable) {
          return paramSpec.nullAsUndefined ? undefined : null;
        }
        if (paramSpec.type === 15 /* InteropValueType.Array */ && ((_paramSpec$elementSpe = paramSpec.elementSpec) === null || _paramSpec$elementSpe === void 0 ? void 0 : _paramSpec$elementSpe.type) === 13 /* InteropValueType.String */ && valueType === 15 /* InteropValueType.Array */) {
          return this.stringArrayToJs(this._memory.readI32(valuePtr), this._memory.readI32(valuePtr + 8), paramSpec.elementSpec);
        }
        if (paramSpec.type === 7 /* InteropValueType.I32 */ && valueType === 12 /* InteropValueType.Pointer */) {
          return this._memory.readI32(valuePtr);
        }
        if (valueType !== paramSpec.type) {
          var _INTEROP_VALUE_TYPE_N;
          throw new Error("failed to marshal ".concat(stringifyParamSpec(paramSpec), " from '").concat((_INTEROP_VALUE_TYPE_N = INTEROP_VALUE_TYPE_NAMES[valueType]) !== null && _INTEROP_VALUE_TYPE_N !== void 0 ? _INTEROP_VALUE_TYPE_N : 'unknown', "'"));
        }
        switch (paramSpec.type) {
          case 0 /* InteropValueType.Void */:
            return undefined;
          case 1 /* InteropValueType.U1 */:
            return this._memory.readU8(valuePtr) !== 0;
          case 2 /* InteropValueType.U8 */:
            return this._memory.readU8(valuePtr);
          case 3 /* InteropValueType.I8 */:
            return this._memory.readI8(valuePtr);
          case 4 /* InteropValueType.U16 */:
            return this._memory.readU16(valuePtr);
          case 5 /* InteropValueType.I16 */:
            return this._memory.readI16(valuePtr);
          case 6 /* InteropValueType.U32 */:
            return this._memory.readU32(valuePtr);
          case 7 /* InteropValueType.I32 */:
            return this._memory.readI32(valuePtr);
          case 8 /* InteropValueType.U64 */:
            return this._memory.readU64(valuePtr);
          case 9 /* InteropValueType.I64 */:
            return this._memory.readI64(valuePtr);
          case 10 /* InteropValueType.F32 */:
            return this._memory.readF32(valuePtr);
          case 11 /* InteropValueType.F64 */:
            return this._memory.readF64(valuePtr);
          case 12 /* InteropValueType.Pointer */:
            return this._memory.getDataView(this._memory.readI32(valuePtr), this._memory.readI32(valuePtr + 8));
          case 13 /* InteropValueType.String */:
            return this.stringToJs(this._memory.readI32(valuePtr));
          case 14 /* InteropValueType.Object */:
            return this._objects.getObjectByHandle(this._memory.readI32(valuePtr + 4));
          case 15 /* InteropValueType.Array */:
            if (paramSpec.elementSpec == null) {
              throw new Error("malformed param spec (array with no element spec)");
            }
            return this.arrayToJs(this._memory.readI32(valuePtr), this._memory.readI32(valuePtr + 8), paramSpec.elementSpec);
          case 16 /* InteropValueType.Name */:
            return this._nameList[this._memory.readI32(valuePtr)];
          case 17 /* InteropValueType.Struct */:
            return this.structToJs(this._memory.readI32(valuePtr), this._memory.readI32(valuePtr + 4), this._memory.readI32(valuePtr + 8));
          default:
            throw new Error("failed to marshal ".concat(stringifyParamSpec(paramSpec), " from '").concat((_INTEROP_VALUE_TYPE_N2 = INTEROP_VALUE_TYPE_NAMES[valueType]) !== null && _INTEROP_VALUE_TYPE_N2 !== void 0 ? _INTEROP_VALUE_TYPE_N2 : 'unknown', "'"));
        }
      }
    }, {
      key: "marshalToClr",
      value: function marshalToClr(valuePtr, paramSpec, value) {
        if (value == null) {
          if (paramSpec.nullable || paramSpec.type === 0 /* InteropValueType.Void */) {
            this._memory.writeU8(valuePtr, 0 /* InteropValueType.Void */);
            return;
          }
          throw new Error("failed to marshal null as '".concat(stringifyParamSpec(paramSpec), "'"));
        }
        switch (paramSpec.type) {
          case 0 /* InteropValueType.Void */:
            this._memory.writeU8(valuePtr + 12, 0 /* InteropValueType.Void */);
            break;
          case 1 /* InteropValueType.U1 */:
            this._memory.writeU8(valuePtr, value ? 1 : 0);
            this._memory.writeU8(valuePtr + 12, 1 /* InteropValueType.U1 */);
            break;
          case 2 /* InteropValueType.U8 */:
          case 3 /* InteropValueType.I8 */:
          case 4 /* InteropValueType.U16 */:
          case 5 /* InteropValueType.I16 */:
          case 6 /* InteropValueType.U32 */:
          case 7 /* InteropValueType.I32 */:
          case 8 /* InteropValueType.U64 */:
          case 9 /* InteropValueType.I64 */:
          case 10 /* InteropValueType.F32 */:
          case 11 /* InteropValueType.F64 */:
            if (typeof value === 'number') {
              this.marshalNumericToClr(valuePtr, paramSpec, value);
              break;
            }
            if (value instanceof BigInt) {
              throw new Error("failed to marshal BigInt as '".concat(stringifyParamSpec(paramSpec), "' (not yet implemented)"));
            }
            throw new Error("failed to marshal non-numeric as '".concat(stringifyParamSpec(paramSpec), "'"));
          // case InteropValueType.Ptr: return;
          case 13 /* InteropValueType.String */:
            this._memory.writeI32(valuePtr, this.stringToClr(typeof value === 'string' ? value : "".concat(value)));
            this._memory.writeU8(valuePtr + 12, 13 /* InteropValueType.String */);
            break;
          case 14 /* InteropValueType.Object */:
            if (_typeof(value) !== 'object' && typeof value !== 'function') {
              throw new Error("failed to marshal ".concat(_typeof(value), " as '").concat(stringifyParamSpec(paramSpec), "' (not an object)"));
            }
            this._memory.writeI32(valuePtr + 4, this._objects.getOrAssignObjectHandle(value));
            this._memory.writeU8(valuePtr + 12, 14 /* InteropValueType.Object */);
            break;
          case 15 /* InteropValueType.Array */:
            if (paramSpec.elementSpec == null) {
              throw new Error("malformed param spec (array with no element spec)");
            }
            if (!Array.isArray(value)) {
              //value = [value];
              // TODO: We could have a param spec flag that wraps single values in arrays in case we need to support apis that sometimes returns an array and sometimes a single value
              throw new Error("failed to marshal ".concat(_typeof(value), " as '").concat(stringifyParamSpec(paramSpec), "' (not an array)"));
            }
            if (paramSpec.elementSpec.type === 13 /* InteropValueType.String */) {
              this._memory.writeI32(valuePtr, this.stringArrayToClr(value, paramSpec.elementSpec));
            } else {
              this._memory.writeI32(valuePtr, this.arrayToClr(value, paramSpec.elementSpec));
            }
            this._memory.writeI32(valuePtr + 8, value.length);
            this._memory.writeU8(valuePtr + 12, 15 /* InteropValueType.Array */);
            break;
          case 16 /* InteropValueType.Name */:
            var valueAsStr = typeof value === 'string' ? value : "".concat(value);
            var nameIndex = this._nameTable[valueAsStr];
            if (nameIndex == null) {
              this._memory.writeI32(valuePtr, this.stringToClr(valueAsStr));
              this._memory.writeU8(valuePtr + 12, 13 /* InteropValueType.String */);
            } else {
              this._memory.writeI32(valuePtr, nameIndex);
              this._memory.writeU8(valuePtr + 12, 16 /* InteropValueType.Name */);
            }

            break;
          case 17 /* InteropValueType.Struct */:
            if (_typeof(value) !== 'object' && typeof value !== 'function') {
              throw new Error("failed to marshal ".concat(_typeof(value), " as '").concat(stringifyParamSpec(paramSpec), "' (not an object)"));
            }
            if (this._memory.readU8(valuePtr + 12) !== 17 /* InteropValueType.Struct */) {
              throw new Error("failed to marshal ".concat(_typeof(value), " as '").concat(stringifyParamSpec(paramSpec), "' (return InteropValue was not initialised correctly))"));
            }
            this.structToClr(this._memory.readI32(valuePtr + 4), this._memory.readI32(valuePtr), this._memory.readI32(valuePtr + 8), value);
            break;
          default:
            throw new Error("failed to marshal '".concat(_typeof(value), "' as '").concat(stringifyParamSpec(paramSpec), "' (not yet implemented)"));
        }
      }
    }, {
      key: "marshalNumericToClr",
      value: function marshalNumericToClr(valuePtr, paramSpec, value) {
        switch (paramSpec.type) {
          case 2 /* InteropValueType.U8 */:
            this._memory.writeU8(valuePtr, value);
            break;
          case 3 /* InteropValueType.I8 */:
            this._memory.writeI8(valuePtr, value);
            break;
          case 4 /* InteropValueType.U16 */:
            this._memory.writeU16(valuePtr, value);
            break;
          case 5 /* InteropValueType.I16 */:
            this._memory.writeI16(valuePtr, value);
            break;
          case 6 /* InteropValueType.U32 */:
            this._memory.writeU32(valuePtr, value);
            break;
          case 7 /* InteropValueType.I32 */:
            this._memory.writeI32(valuePtr, value);
            break;
          // case InteropValueType.U64: break;
          // case InteropValueType.I64: break;
          case 10 /* InteropValueType.F32 */:
            this._memory.writeF32(valuePtr, value);
            break;
          case 11 /* InteropValueType.F64 */:
            this._memory.writeF64(valuePtr, value);
            break;
          default:
            throw new Error("failed to marshal numeric as '".concat(stringifyParamSpec(paramSpec), "' (not yet implemented)"));
        }
        this._memory.writeU8(valuePtr + 12, paramSpec.type);
      }
    }, {
      key: "stringToJs",
      value: function stringToJs(stringPtr) {
        try {
          this._memory.enterConstrainedRange(stringPtr, 2 * 2 * 1024 * 1024); // assuming they will never try and copy a string larger than 2m characters to JS
          return this._memory.readNullTerminatedString(stringPtr);
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "stringToClr",
      value: function stringToClr(str) {
        var strPtr = this.allocateTransient((str.length + 1) * 2);
        try {
          this._memory.flush();
          this._memory.enterConstrainedRange(strPtr, (str.length + 1) * 2);
          this._memory.writeString(strPtr, str, true);
          return strPtr;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "arrayToJs",
      value: function arrayToJs(arrayPtr, arrayLen, elementSpec) {
        try {
          this._memory.enterConstrainedRange(arrayPtr, arrayLen * 16);
          var result = [];
          result.length = arrayLen;
          for (var i = 0; i < arrayLen; ++i) {
            result[i] = this.marshalToJs(arrayPtr, elementSpec);
            arrayPtr += 16;
          }
          return result;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "arrayToClr",
      value: function arrayToClr(value, elementSpec) {
        var arrPtr = this.allocateTransient(value.length * 16);
        try {
          this._memory.enterConstrainedRange(arrPtr, value.length * 16);
          this._memory.flush();
          var elPtr = arrPtr;
          for (var i = 0; i < value.length; ++i) {
            this.marshalToClr(elPtr, elementSpec, value[i]);
            elPtr += 16;
          }
          return arrPtr;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "stringArrayToJs",
      value: function stringArrayToJs(arrayPtr, arrayLen, elementSpec) {
        try {
          this._memory.enterConstrainedRange(arrayPtr, 2 * 2 * 1024 * 1024);
          var result = [];
          result.length = arrayLen;
          for (var i = 0; i < arrayLen; ++i) {
            var code = void 0;
            if (elementSpec.nullable) {
              code = this._memory.readU16(arrayPtr);
              arrayPtr += 2;
              if (code === 0) {
                result[i] = elementSpec.nullAsUndefined ? undefined : null;
                break;
              }
            }
            var str = this._memory.readNullTerminatedString(arrayPtr);
            arrayPtr += (str.length + 1) * 2;
            result[i] = str;
          }
          return result;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "stringArrayToClr",
      value: function stringArrayToClr(value, elementSpec) {
        var bufferSize = 0;
        var tmp = [];
        tmp.length = value.length;
        for (var i = 0; i < value.length; ++i) {
          var element = value[i];
          if (elementSpec.nullable) {
            ++bufferSize;
            if (element == null) {
              continue;
            }
          }
          var str = typeof element === 'string' ? element : "".concat(element);
          bufferSize += str.length + 1;
          tmp[i] = str;
        }
        var strPtr = this.allocateTransient(bufferSize * 2);
        try {
          this._memory.flush();
          this._memory.enterConstrainedRange(strPtr, bufferSize * 2);
          var charPtr = strPtr;
          for (var _i2 = 0; _i2 < value.length; ++_i2) {
            var _element = tmp[_i2];
            if (elementSpec.nullable) {
              this._memory.writeU16(charPtr, _element != null ? 1 : 0);
              charPtr += 2;
              if (_element == null) {
                continue;
              }
            }
            this._memory.writeString(charPtr, _element, true);
            charPtr += (_element.length + 1) * 2;
          }
          return strPtr;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "structToClr",
      value: function structToClr(structIndex, fieldPtr, fieldCount, obj) {
        var structSpec = this._structList[structIndex];
        if (structSpec == null) {
          throw new Error("failed to marshal struct ".concat(structIndex, " (invalid struct index)"));
        }
        var useFieldKeys = fieldCount !== structSpec.fieldSpecs.length;
        try {
          this._memory.enterConstrainedRange(fieldPtr, fieldCount * 16);
          for (var i = 0; i < fieldCount; ++i) {
            var fieldKey = useFieldKeys ? this._memory.readI16(fieldPtr + 14) : i;
            var fieldSpec = structSpec.fieldSpecs[fieldKey];
            if (!fieldSpec) {
              throw new Error("failed to marshal struct ".concat(structIndex, " field ").concat(i, " to clr (field key ").concat(fieldKey, " did not refer to a field)"));
            }
            var value = obj != null ? obj[fieldSpec.fieldName] : null;
            this.marshalToClr(fieldPtr, fieldSpec.paramSpec, value);
            fieldPtr += 16;
          }
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "structToJs",
      value: function structToJs(structIndex, fieldPtr, fieldCount) {
        var structSpec = this._structList[structIndex];
        if (structSpec == null) {
          throw new Error("failed to marshal struct ".concat(structIndex, " (invalid struct index)"));
        }
        var result = {};
        var useFieldKeys = fieldCount !== structSpec.fieldSpecs.length;
        try {
          this._memory.enterConstrainedRange(fieldPtr, fieldCount * 16);
          for (var i = 0; i < fieldCount; ++i) {
            var fieldKey = useFieldKeys ? this._memory.readI16(fieldPtr + 14) : i;
            var fieldSpec = structSpec.fieldSpecs[fieldKey];
            if (!fieldSpec) {
              throw new Error("failed to marshal struct ".concat(structIndex, " field ").concat(i, " from clr (field key ").concat(fieldKey, " did not refer to a field)"));
            }
            result[fieldSpec.fieldName] = this.marshalToJs(fieldPtr, fieldSpec.paramSpec);
            fieldPtr += 16;
          }
          return result;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "paramSpecToJs",
      value: function paramSpecToJs(paramSpecPtr) {
        var type = this._memory.readU8(paramSpecPtr);
        var flags = this._memory.readU8(paramSpecPtr + 1);
        var elementType = this._memory.readU8(paramSpecPtr + 2);
        var elementFlags = this._memory.readU8(paramSpecPtr + 3);
        return {
          type: type,
          nullable: (flags & 1) === 1,
          nullAsUndefined: (flags & 2) === 2,
          elementSpec: elementType !== 0 /* InteropValueType.Void */ ? {
            type: elementType,
            nullable: (elementFlags & 1) === 1,
            nullAsUndefined: (elementFlags & 2) === 2
          } : undefined
        };
      }
    }, {
      key: "functionSpecToJs",
      value: function functionSpecToJs(functionSpecPtr) {
        var result = {
          returnSpec: this.paramSpecToJs(functionSpecPtr),
          paramSpecs: []
        };
        functionSpecPtr += 4;
        for (var i = 0; i < 8; ++i) {
          var paramSpec = this.paramSpecToJs(functionSpecPtr);
          if (paramSpec.type === 0 /* InteropValueType.Void */) {
            break;
          }
          result.paramSpecs.push(paramSpec);
          functionSpecPtr += 4;
        }
        return result;
      }
    }, {
      key: "allocateTransient",
      value: function allocateTransient(sz) {
        if (sz <= 0) {
          return 0;
        }
        var alignedHead = this._transientBufferHead + 7 & ~7;
        var newHead = alignedHead + sz;
        if (newHead > this._transientBufferSz) {
          if (this._transientBufferPtr !== 0) {
            // Grow transient buffer
            var newSz = Interop.npo2(newHead);
            var newPtr = this._malloc(newSz);
            if (newPtr === 0) {
              throw new Error("failed to allocate ".concat(newSz, "b"));
            }
            this._memory.flush();
            this._memory.memcpy(newPtr, this._transientBufferPtr, this._transientBufferHead);
            this._free(this._transientBufferPtr);
            this._transientBufferPtr = newPtr;
            this._transientBufferSz = newSz;
            console.log("grew transient buffer to ".concat(newSz, " to fit allocation of ").concat(sz, " (head=").concat(this._transientBufferHead, ", alignedHead=").concat(alignedHead, ")"));
          } else {
            // Init transient buffer
            this._transientBufferSz = Math.max(Interop.npo2(newHead), 4096);
            this._transientBufferPtr = this._malloc(this._transientBufferSz);
            if (this._transientBufferPtr === 0) {
              throw new Error("failed to allocate ".concat(this._transientBufferSz, "b"));
            }
            this._memory.flush();
            console.log("initialized transient buffer to ".concat(this._transientBufferSz, " to fit allocation of ").concat(sz, " (head=").concat(this._transientBufferHead, ", alignedHead=").concat(alignedHead, ")"));
          }
        }
        this._transientBufferHead = newHead;
        return this._transientBufferPtr + alignedHead;
      }
    }, {
      key: "stringifyValueForDisplay",
      value: function stringifyValueForDisplay(value) {
        if (value === undefined) {
          return 'undefined';
        }
        if (value === null) {
          return 'null';
        }
        if (typeof value === 'string') {
          return "'".concat(value, "'");
        }
        if (typeof value === 'number' || typeof value === 'boolean') {
          return "".concat(value);
        }
        if (Array.isArray(value)) {
          return "array[#".concat(value.length, ", %").concat(this._objects.getObjectHandle(value), "]");
        }
        if (_typeof(value) === 'object') {
          return "object[#".concat(Object.keys(value).length, ", %").concat(this._objects.getObjectHandle(value), "]");
        }
        return _typeof(value);
      }
    }, {
      key: "stringifyImportBindingForDisplay",
      value: function stringifyImportBindingForDisplay(importIndex) {
        var boundImportSymbol = this._boundImportSymbolList[importIndex];
        return "".concat(importIndex, ": ").concat(stringifyParamSpec(boundImportSymbol.functionSpec.returnSpec), " ").concat(boundImportSymbol.fullName, "(").concat(boundImportSymbol.functionSpec.paramSpecs.map(stringifyParamSpec).join(', '), ")");
      }
    }], [{
      key: "npo2",
      value: function npo2(v) {
        v += v === 0 ? 1 : 0;
        --v;
        v |= v >>> 1;
        v |= v >>> 2;
        v |= v >>> 4;
        v |= v >>> 8;
        v |= v >>> 16;
        return v + 1;
      }
    }]);
    return Interop;
  }();

  var WasmMemoryManager = /*#__PURE__*/function () {
    function WasmMemoryManager(memory) {
      _classCallCheck(this, WasmMemoryManager);
      _defineProperty(this, "_memory", void 0);
      _defineProperty(this, "_viewArrayBuffer", void 0);
      _defineProperty(this, "_u8", void 0);
      _defineProperty(this, "_i8", void 0);
      _defineProperty(this, "_u16", void 0);
      _defineProperty(this, "_i16", void 0);
      _defineProperty(this, "_u32", void 0);
      _defineProperty(this, "_i32", void 0);
      _defineProperty(this, "_f32", void 0);
      _defineProperty(this, "_f64", void 0);
      _defineProperty(this, "_dataView", void 0);
      _defineProperty(this, "_rangeMin", void 0);
      _defineProperty(this, "_rangeMax", void 0);
      _defineProperty(this, "_rangeStack", []);
      this._memory = memory;
      this._viewArrayBuffer = memory.buffer;
      this._u8 = new Uint8Array(memory.buffer);
      this._i8 = new Int8Array(memory.buffer);
      this._u16 = new Uint16Array(memory.buffer);
      this._i16 = new Int16Array(memory.buffer);
      this._u32 = new Uint32Array(memory.buffer);
      this._i32 = new Int32Array(memory.buffer);
      this._f32 = new Float32Array(memory.buffer);
      this._f64 = new Float64Array(memory.buffer);
      this._dataView = new DataView(memory.buffer);
    }
    _createClass(WasmMemoryManager, [{
      key: "checkAlignment",
      value: function checkAlignment(ptr, alignment) {
        if (ptr % alignment !== 0) {
          throw new Error("alignment error - expected ".concat(alignment, ", was misaligned by ").concat(ptr % alignment));
        }
      }
    }, {
      key: "checkConstrainedRange",
      value: function checkConstrainedRange(ptr, sz) {
        var _this$_rangeMin, _this$_rangeMax;
        var min = (_this$_rangeMin = this._rangeMin) !== null && _this$_rangeMin !== void 0 ? _this$_rangeMin : 0;
        var max = (_this$_rangeMax = this._rangeMax) !== null && _this$_rangeMax !== void 0 ? _this$_rangeMax : this._memory.buffer.byteLength;
        if (ptr < min || ptr + sz > max) {
          throw new Error("constrained range error - expected within ".concat(min, "->").concat(max, ", got ").concat(ptr, "->").concat(ptr + sz));
        }
      }
    }, {
      key: "checkDetached",
      value: function checkDetached() {
        var _this$_viewArrayBuffe;
        if (this._memory.buffer !== this._viewArrayBuffer) {
          throw new Error("view array buffer has changed");
        }
        if ((_this$_viewArrayBuffe = this._viewArrayBuffer) !== null && _this$_viewArrayBuffe !== void 0 && _this$_viewArrayBuffe.detached) {
          throw new Error("view array buffer is detached");
        }
      }
    }, {
      key: "writeU8",
      value: function writeU8(ptr, value) {
        this._u8[ptr] = value;
      }
    }, {
      key: "writeI8",
      value: function writeI8(ptr, value) {
        this._i8[ptr] = value;
      }
    }, {
      key: "writeU16",
      value: function writeU16(ptr, value) {
        this._u16[ptr >> 1] = value;
      }
    }, {
      key: "writeI16",
      value: function writeI16(ptr, value) {
        this._i16[ptr >> 1] = value;
      }
    }, {
      key: "writeU32",
      value: function writeU32(ptr, value) {
        this._u32[ptr >> 2] = value;
      }
    }, {
      key: "writeI32",
      value: function writeI32(ptr, value) {
        this._i32[ptr >> 2] = value;
      }
    }, {
      key: "writeU64",
      value: function writeU64(ptr, value) {
        this._dataView.setBigUint64(ptr, value, true);
      }
    }, {
      key: "writeI64",
      value: function writeI64(ptr, value) {
        this._dataView.setBigInt64(ptr, value, true);
      }
    }, {
      key: "writeF32",
      value: function writeF32(ptr, value) {
        this._f32[ptr >> 2] = value;
      }
    }, {
      key: "writeF64",
      value: function writeF64(ptr, value) {
        this._f64[ptr >> 3] = value;
      }
    }, {
      key: "writeString",
      value: function writeString(ptr, value, nullTerminated) {
        var first = ptr >> 1;
        for (var i = 0; i < value.length; ++i) {
          this._u16[first + i] = value.charCodeAt(i);
        }
        if (nullTerminated) {
          this._u16[first + value.length] = 0;
        }
      }
    }, {
      key: "readU8",
      value: function readU8(ptr) {
        return this._u8[ptr];
      }
    }, {
      key: "readI8",
      value: function readI8(ptr) {
        return this._i8[ptr];
      }
    }, {
      key: "readU16",
      value: function readU16(ptr) {
        return this._u16[ptr >> 1];
      }
    }, {
      key: "readI16",
      value: function readI16(ptr) {
        return this._i16[ptr >> 1];
      }
    }, {
      key: "readU32",
      value: function readU32(ptr) {
        return this._u32[ptr >> 2];
      }
    }, {
      key: "readI32",
      value: function readI32(ptr) {
        return this._i32[ptr >> 2];
      }
    }, {
      key: "readU64",
      value: function readU64(ptr) {
        return this._dataView.getBigUint64(ptr, true);
      }
    }, {
      key: "readI64",
      value: function readI64(ptr) {
        return this._dataView.getBigInt64(ptr, true);
      }
    }, {
      key: "readF32",
      value: function readF32(ptr) {
        return this._f32[ptr >> 2];
      }
    }, {
      key: "readF64",
      value: function readF64(ptr) {
        return this._f64[ptr >> 3];
      }
    }, {
      key: "readNullTerminatedString",
      value: function readNullTerminatedString(ptr) {
        var result = "";
        var value = this.readU16(ptr);
        while (value !== 0) {
          result += String.fromCharCode(value);
          ptr += 2;
          value = this.readU16(ptr);
        }
        return result;
      }
    }, {
      key: "readString",
      value: function readString(ptr, length) {
        var result = "";
        var first = ptr >> 1;
        var last = first + length;
        for (var i = first; i < last; ++i) {
          result += String.fromCharCode(this._u16[i]);
        }
        return result;
      }
    }, {
      key: "getDataView",
      value: function getDataView(ptr, sz) {
        return new DataView(this._memory.buffer, ptr, sz);
      }
    }, {
      key: "getArrayView",
      value: function getArrayView(ptr, sz) {
        return new Uint8Array(this._memory.buffer, ptr, sz);
      }
    }, {
      key: "memcpy",
      value: function memcpy(dst, src, sz) {
        this._u8.set(this._u8.subarray(src, src + sz), dst);
      }
    }, {
      key: "enterConstrainedRange",
      value: function enterConstrainedRange(ptr, sz) {
        {
          return;
        }
      }
    }, {
      key: "exitConstrainedRange",
      value: function exitConstrainedRange() {
        {
          return;
        }
      }
    }, {
      key: "flush",
      value: function flush() {
        if (this._memory.buffer === this._viewArrayBuffer) {
          return;
        }
        this._viewArrayBuffer = this._memory.buffer;
        this._u8 = new Uint8Array(this._memory.buffer);
        this._i8 = new Int8Array(this._memory.buffer);
        this._u16 = new Uint16Array(this._memory.buffer);
        this._i16 = new Int16Array(this._memory.buffer);
        this._u32 = new Uint32Array(this._memory.buffer);
        this._i32 = new Int32Array(this._memory.buffer);
        this._f32 = new Float32Array(this._memory.buffer);
        this._f64 = new Float64Array(this._memory.buffer);
        this._dataView = new DataView(this._memory.buffer);
      }
    }]);
    return WasmMemoryManager;
  }();

  var BaseBindings = /*#__PURE__*/function () {
    function BaseBindings(logFunc, interop) {
      _classCallCheck(this, BaseBindings);
      _defineProperty(this, "bindingsImport", void 0);
      _defineProperty(this, "imports", {});
      _defineProperty(this, "_interop", void 0);
      _defineProperty(this, "logFunc", void 0);
      _defineProperty(this, "_memory", void 0);
      _defineProperty(this, "_malloc", void 0);
      this.logFunc = logFunc;
      this._interop = interop;
      this.bindingsImport = {};
      this.setupImports();
    }
    _createClass(BaseBindings, [{
      key: "init",
      value: function init(exports, memory) {
        this._memory = memory;
        this._malloc = exports.malloc;
      }
    }, {
      key: "loop",
      value: function loop() {}
    }, {
      key: "setupImports",
      value: function setupImports() {}
    }, {
      key: "log",
      value: function log(text) {
        this.logFunc(text);
      }
    }]);
    return BaseBindings;
  }();

  var NoopBindings = /*#__PURE__*/function (_BaseBindings) {
    _inherits(NoopBindings, _BaseBindings);
    var _super = _createSuper(NoopBindings);
    function NoopBindings() {
      _classCallCheck(this, NoopBindings);
      return _super.apply(this, arguments);
    }
    return _createClass(NoopBindings);
  }(BaseBindings);

  // Note: when compiling for world, this import is replaced with the actual world bindings instead

  var RESOURCE_LIST = ["energy", "score", "score_x", "score_y", "score_z"]; // 5 total
  {
    for (var _i = 0, _RESOURCE_LIST = RESOURCE_LIST; _i < _RESOURCE_LIST.length; _i++) {
      _RESOURCE_LIST[_i];
    }
  }
  var BODYPART_LIST = ['move', 'work', 'carry', 'attack', 'ranged_attack', 'heal', 'tough']; // 7 total
  var BODYPART_TO_ENUM_MAP = {};
  {
    var _i2 = 0;
    for (var _i3 = 0, _BODYPART_LIST = BODYPART_LIST; _i3 < _BODYPART_LIST.length; _i3++) {
      var bodyPart = _BODYPART_LIST[_i3];
      BODYPART_TO_ENUM_MAP[bodyPart] = _i2++;
    }
  }
  var ArenaBindings = /*#__PURE__*/function (_BaseBindings) {
    _inherits(ArenaBindings, _BaseBindings);
    var _super = _createSuper(ArenaBindings);
    function ArenaBindings() {
      _classCallCheck(this, ArenaBindings);
      return _super.apply(this, arguments);
    }
    _createClass(ArenaBindings, [{
      key: "init",
      value: function init(exports, memory) {
        _get(_getPrototypeOf(ArenaBindings.prototype), "init", this).call(this, exports, memory);
      }
    }, {
      key: "loop",
      value: function loop() {
        _get(_getPrototypeOf(ArenaBindings.prototype), "loop", this).call(this);
      }
    }, {
      key: "setupImports",
      value: function setupImports() {
        var _this = this;
        _get(_getPrototypeOf(ArenaBindings.prototype), "setupImports", this).call(this);
        this.bindingsImport.js_renew_object = function () {};
        this.bindingsImport.js_batch_renew_objects = function () {};
        this.bindingsImport.js_fetch_object_room_position = function () {};
        this.bindingsImport.js_batch_fetch_object_room_positions = function () {};
        this.bindingsImport.js_get_object_by_id = function () {};
        this.bindingsImport.js_get_object_id = function () {};
        this.imports['object'] = {
          getConstructorOf: function getConstructorOf(x) {
            return Object.getPrototypeOf(x).constructor;
          },
          interpretDateTime: function interpretDateTime(x) {
            return x.getTime() / 1000;
          }
        };
        this.imports['game/utils'] = _objectSpread2(_objectSpread2({}, utils), {}, {
          getTerrain: function getTerrain(minX, minY, maxX, maxY, outMemoryView) {
            var pos = {
              x: 0,
              y: 0
            };
            var i = 0;
            for (var y = minY; y <= maxY; ++y) {
              pos.y = y;
              for (var x = minX; x <= maxX; ++x) {
                pos.x = x;
                outMemoryView.setInt8(i, utils.getTerrainAt(pos));
                ++i;
              }
            }
          }
        });
        this.imports['game/prototypes'] = prototypes;
        this.imports['game/constants'] = {
          get: function get() {
            return constants;
          }
        };
        this.imports['game/pathFinder'] = _objectSpread2(_objectSpread2({}, pathFinder), {}, {
          searchPath: function searchPath(origin, goalsPtr, goalsCnt, options) {
            var goal;
            try {
              _this._memory.enterConstrainedRange(goalsPtr, goalsCnt * 12);
              if (goalsCnt == 1) {
                var r = _this._memory.readI32(goalsPtr + 8);
                if (r === 0) {
                  goal = {
                    x: _this._memory.readI32(goalsPtr + 0),
                    y: _this._memory.readI32(goalsPtr + 4)
                  };
                } else {
                  goal = {
                    pos: {
                      x: _this._memory.readI32(goalsPtr + 0),
                      y: _this._memory.readI32(goalsPtr + 4)
                    },
                    range: r
                  };
                }
              } else {
                goal = [];
                goal.length = goalsCnt;
                for (var _i4 = 0; _i4 < goalsCnt; ++_i4) {
                  var _r = _this._memory.readI32(goalsPtr + 8);
                  if (_r === 0) {
                    goal[_i4] = {
                      x: _this._memory.readI32(goalsPtr + 0),
                      y: _this._memory.readI32(goalsPtr + 4)
                    };
                  } else {
                    goal[_i4] = {
                      pos: {
                        x: _this._memory.readI32(goalsPtr + 0),
                        y: _this._memory.readI32(goalsPtr + 4)
                      },
                      range: _r
                    };
                  }
                  goalsPtr += 12;
                }
              }
              var originPos = {
                x: origin >> 16,
                y: origin & 0xffff
              };
              return pathFinder.searchPath(originPos, goal, options);
            } finally {
              _this._memory.exitConstrainedRange();
            }
          },
          decodePath: function decodePath(resultObj, outPtr) {
            return _this.copyPath(resultObj.path, outPtr);
          },
          CostMatrix: _objectSpread2(_objectSpread2({}, this.buildWrappedPrototype(pathFinder.CostMatrix)), {}, {
            setRect: function setRect(thisObj, minX, minY, maxX, maxY, memoryView) {
              var i = 0;
              for (var y = minY; y <= maxY; ++y) {
                for (var x = minX; x <= maxX; ++x) {
                  thisObj.set(x, y, memoryView.getInt8(i));
                  ++i;
                }
              }
            }
          }),
          createCostMatrix: function createCostMatrix() {
            return new pathFinder.CostMatrix();
          }
        });
        this.imports['game/visual'] = {
          Visual: this.buildWrappedPrototype(visual.Visual),
          createVisual: function createVisual(layer, persistent) {
            return new visual.Visual(layer, persistent);
          }
        };
        this.imports['game'] = {
          getUtils: function getUtils() {
            return utils;
          },
          getPrototypes: function getPrototypes() {
            return prototypes;
          }
        };
        var wrappedPrototypes = this.buildWrappedPrototypes(prototypes);
        this.imports['game/prototypes/wrapped'] = _objectSpread2(_objectSpread2({}, wrappedPrototypes), {}, {
          GameObject: _objectSpread2(_objectSpread2({}, wrappedPrototypes.GameObject), {}, {
            findPathTo: function findPathTo(thisObj, pos, opts, outPtr) {
              var result = thisObj.findPathTo(pos, opts != null ? opts : undefined);
              if (!result) {
                return 0;
              }
              return _this.copyPath(result, outPtr);
            }
          }),
          Store: {
            getCapacity: function getCapacity(thisObj, resourceType) {
              return thisObj.getCapacity(resourceType);
            },
            getUsedCapacity: function getUsedCapacity(thisObj, resourceType) {
              return thisObj.getUsedCapacity(resourceType);
            },
            getFreeCapacity: function getFreeCapacity(thisObj, resourceType) {
              return thisObj.getFreeCapacity(resourceType);
            }
          },
          Creep: _objectSpread2(_objectSpread2({}, wrappedPrototypes.Creep), {}, {
            getEncodedBody: function getEncodedBody(thisObj, outPtr) {
              return _this.encodeCreepBody(thisObj.body, outPtr);
            }
          })
        });
      }
    }, {
      key: "encodeCreepBody",
      value: function encodeCreepBody(body, outPtr) {
        try {
          this._memory.enterConstrainedRange(outPtr, 50 * 2);
          for (var _i5 = 0; _i5 < body.length; ++_i5) {
            var _body$_i = body[_i5],
              type = _body$_i.type,
              hits = _body$_i.hits;
            // Encode each body part to a 16 bit int as 2 bytes
            // type: 0-8 (4 bits 0-15) b1
            // hits: 0-100 (7 bits 0-127) b0
            var encodedBodyPart = 0;
            encodedBodyPart |= BODYPART_TO_ENUM_MAP[type] << 8;
            encodedBodyPart |= hits;
            this._memory.writeU16(outPtr, encodedBodyPart);
            outPtr += 2;
          }
          return body.length;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "copyPath",
      value: function copyPath(path, outPtr) {
        try {
          this._memory.enterConstrainedRange(outPtr, path.length * 4);
          for (var _i6 = 0; _i6 < path.length; ++_i6) {
            this._memory.writeI16(outPtr, path[_i6].x);
            outPtr += 2;
            this._memory.writeI16(outPtr, path[_i6].y);
            outPtr += 2;
          }
          return path.length;
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "buildWrappedPrototypes",
      value: function buildWrappedPrototypes(prototypes) {
        var wrappedPrototypes = {};
        for (var prototypeName in prototypes) {
          wrappedPrototypes[prototypeName] = this.buildWrappedPrototype(prototypes[prototypeName]);
        }
        return wrappedPrototypes;
      }
    }, {
      key: "buildWrappedPrototype",
      value: function buildWrappedPrototype(constructor) {
        /** @type {Record<string, Function>} */
        var wrappedPrototype = {};
        var prototype = constructor.prototype;
        var keys = Object.getOwnPropertyNames(prototype);
        var _iterator = _createForOfIteratorHelper(keys),
          _step;
        try {
          var _loop = function _loop() {
              var _Object$getOwnPropert;
              var key = _step.value;
              if (key === 'constructor') {
                return 0; // continue
              }
              var value = (_Object$getOwnPropert = Object.getOwnPropertyDescriptor(prototype, key)) === null || _Object$getOwnPropert === void 0 ? void 0 : _Object$getOwnPropert.value;
              if (typeof value !== 'function') {
                return 0; // continue
              }
              wrappedPrototype[key] = function (thisObj) {
                for (var _len = arguments.length, args = new Array(_len > 1 ? _len - 1 : 0), _key = 1; _key < _len; _key++) {
                  args[_key - 1] = arguments[_key];
                }
                return value.call.apply(value, [thisObj].concat(args));
              };
            },
            _ret;
          for (_iterator.s(); !(_step = _iterator.n()).done;) {
            _ret = _loop();
            if (_ret === 0) continue;
          }
        } catch (err) {
          _iterator.e(err);
        } finally {
          _iterator.f();
        }
        return wrappedPrototype;
      }
    }]);
    return ArenaBindings;
  }(BaseBindings);

  var bindingsTable = {
    "arena": ArenaBindings,
    "world": NoopBindings,
    "test": NoopBindings
  };
  function getBindings(env, logFunc, interop) {
    var bindingsCtor = bindingsTable[env];
    if (!bindingsCtor) {
      return undefined;
    }
    return new bindingsCtor(logFunc, interop);
  }

  var utf8Decoder = new TextDecoder();
  var Stdio = /*#__PURE__*/function () {
    function Stdio(outFunc) {
      _classCallCheck(this, Stdio);
      _defineProperty(this, "_outFunc", void 0);
      _defineProperty(this, "buffer", void 0);
      this._outFunc = outFunc;
    }
    _createClass(Stdio, [{
      key: "write",
      value: function write(memory, buf, buf_len) {
        var buffer = memory.getArrayView(buf, buf_len);
        this.addTextToBuffer(utf8Decoder.decode(buffer));
      }
    }, {
      key: "addTextToBuffer",
      value: function addTextToBuffer(text) {
        if (!this.buffer) {
          this.buffer = text;
        } else {
          this.buffer += text;
        }
        var newlineIdx;
        while ((newlineIdx = this.buffer.indexOf('\n')) >= 0) {
          var _this$buffer;
          var line = this.buffer.substring(0, newlineIdx).trim();
          this._outFunc(line);
          this.buffer = (_this$buffer = this.buffer) === null || _this$buffer === void 0 ? void 0 : _this$buffer.substring(newlineIdx + 1);
        }
      }
    }]);
    return Stdio;
  }();
  var JSTYPE_TO_ENUM = {
    undefined: 0,
    string: 1,
    number: 2,
    bigint: 3,
    "boolean": 4,
    object: 5,
    "function": 6,
    symbol: 7
  };
  function decompressWasm(compressedBytes, originalSize) {
    var decompressedBytes = new Uint8Array(originalSize);
    return inflateSync(compressedBytes, {
      out: decompressedBytes
    });
  }
  function decodeWasm(encodedWasm, originalSize, encoding) {
    var bytes;
    if (encoding == 'b64') {
      bytes = new Uint8Array(originalSize);
      base64Decode(encodedWasm, bytes);
    } else {
      bytes = decode(encodedWasm);
    }
    return bytes;
  }
  var EMPTY_ARR = [];
  var Bootloader = /*#__PURE__*/function () {
    function Bootloader(env, profileFn) {
      _classCallCheck(this, Bootloader);
      _defineProperty(this, "_env", void 0);
      _defineProperty(this, "_pendingLogs", []);
      _defineProperty(this, "_deferLogsToTick", void 0);
      _defineProperty(this, "_profileFn", void 0);
      _defineProperty(this, "_stdout", void 0);
      _defineProperty(this, "_stderr", void 0);
      _defineProperty(this, "_interop", void 0);
      _defineProperty(this, "_bindings", void 0);
      _defineProperty(this, "_systemImport", void 0);
      _defineProperty(this, "_wasmModule", void 0);
      _defineProperty(this, "_wasmInstance", void 0);
      _defineProperty(this, "_memory", void 0);
      _defineProperty(this, "_compiled", false);
      _defineProperty(this, "_started", false);
      _defineProperty(this, "_inTick", false);
      _defineProperty(this, "_profilingEnabled", false);
      this._env = env;
      this._deferLogsToTick = env === 'arena';
      this._profileFn = profileFn;
      this._stdout = new Stdio(this.log.bind(this));
      this._stderr = new Stdio(this.log.bind(this));
      this._interop = new Interop(profileFn);
      this.setImports('__object', {
        hasProperty: function hasProperty(obj, key) {
          return key in obj;
        },
        getTypeOfProperty: function getTypeOfProperty(obj, key) {
          return JSTYPE_TO_ENUM[_typeof(obj[key])];
        },
        getKeys: function getKeys(obj) {
          var _ref;
          return (_ref = obj ? Object.keys(obj) : null) !== null && _ref !== void 0 ? _ref : EMPTY_ARR;
        },
        getProperty: function getProperty(obj, key) {
          return obj[key];
        },
        setProperty: function setProperty(obj, key, value) {
          return obj[key] = value;
        },
        deleteProperty: function deleteProperty(obj, key) {
          return delete obj[key];
        },
        create: function create(proto) {
          return Object.create(proto);
        }
      });
      this._bindings = getBindings(env, this.log.bind(this), this._interop);
      if (this._bindings) {
        for (var moduleName in this._bindings.imports) {
          this.setImports(moduleName, this._bindings.imports[moduleName]);
        }
      }
      this._systemImport = _defineProperty(_defineProperty(_defineProperty(_defineProperty({}, "get-time", this.sys_get_time.bind(this)), "get-random", this.sys_get_random.bind(this)), "write-stderr", this.sys_write_stderr.bind(this)), "write-stdout", this.sys_write_stdout.bind(this));
    }
    _createClass(Bootloader, [{
      key: "compiled",
      get: function get() {
        return this._compiled;
      }
    }, {
      key: "started",
      get: function get() {
        return this._started;
      }
    }, {
      key: "profilingEnabled",
      get: function get() {
        return this._profilingEnabled;
      },
      set: function set(value) {
        this._profilingEnabled = value;
      }
    }, {
      key: "exports",
      get: function get() {
        return this._wasmInstance.exports;
      }
    }, {
      key: "sys_get_time",
      value: function sys_get_time(time_ptr) {
        this._memory.flush();
        this._memory.writeU64(time_ptr, BigInt(new Date().getTime()) * 1000000n);
      }
    }, {
      key: "sys_get_random",
      value: function sys_get_random(buf, buf_len) {
        try {
          this._memory.flush();
          this._memory.enterConstrainedRange(buf, buf_len);
          while (buf_len >= 4) {
            this._memory.writeU32(buf, Math.random() * 0xffffffff);
            buf += 4;
            buf_len -= 4;
          }
          while (buf_len > 0) {
            this._memory.writeU8(buf, Math.random() * 0xff);
            ++buf;
            --buf_len;
          }
        } finally {
          this._memory.exitConstrainedRange();
        }
      }
    }, {
      key: "sys_write_stderr",
      value: function sys_write_stderr(buf, buf_len) {
        this._memory.flush();
        this._stderr.write(this._memory, buf, buf_len);
      }
    }, {
      key: "sys_write_stdout",
      value: function sys_write_stdout(buf, buf_len) {
        this._memory.flush();
        this._stdout.write(this._memory, buf, buf_len);
      }
    }, {
      key: "setImports",
      value: function setImports(moduleName, importTable) {
        this._interop.setImports(moduleName, importTable);
      }
    }, {
      key: "log",
      value: function log(text) {
        if (!this._deferLogsToTick || this._inTick) {
          this.dispatchLog(text);
          return;
        }
        this._pendingLogs.push(text);
      }
    }, {
      key: "dispatchLog",
      value: function dispatchLog(text) {
        console.log("DOTNET: ".concat(text));
      }
    }, {
      key: "compile",
      value: function compile(wasmBytes) {
        if (this._compiled) {
          return;
        }
        // Compile wasm module
        if (this._wasmModule) {
          this.log("Reusing wasm module from previous attempt...");
        } else {
          var t0 = this._profileFn();
          this._wasmModule = new WebAssembly.Module(wasmBytes);
          var t1 = this._profileFn();
          this.log("Compiled wasm module in ".concat(t1 - t0, " ms"));
        }
        // Instantiate wasm module
        if (this._wasmInstance) {
          this.log("Reusing wasm instance from previous attempt...");
        } else {
          var _t = this._profileFn();
          this._wasmInstance = new WebAssembly.Instance(this._wasmModule, this.getWasmImports());
          var _t2 = this._profileFn();
          this.log("Instantiated wasm module in ".concat(_t2 - _t, " ms"));
        }
        // Wire things up
        this._memory = new WasmMemoryManager(this._wasmInstance.exports.memory);
        this._interop.memory = this._memory;
        this._interop.malloc = this._wasmInstance.exports.malloc;
        this._interop.free = this._wasmInstance.exports.free;
        this._compiled = true;
      }
    }, {
      key: "start",
      value: function start(customInitExportNames) {
        var _this$_bindings;
        if (!this._wasmInstance || !this._compiled || this._started || !this._memory) {
          return;
        }
        // Run WASM entrypoint
        try {
          var t0 = this._profileFn();
          this._wasmInstance.exports._initialize();
          var t1 = this._profileFn();
          this.log("Started in ".concat(t1 - t0, " ms"));
        } catch (err) {
          if (err instanceof Error) {
            var _err$stack;
            this.log((_err$stack = err.stack) !== null && _err$stack !== void 0 ? _err$stack : "".concat(err));
          } else {
            this.log("".concat(err));
          }
        }
        // Run bindings init
        (_this$_bindings = this._bindings) === null || _this$_bindings === void 0 || _this$_bindings.init(this._wasmInstance.exports, this._memory);
        // Run usercode init
        {
          var _t3 = this._profileFn();
          if (customInitExportNames) {
            var _iterator = _createForOfIteratorHelper(customInitExportNames),
              _step;
            try {
              for (_iterator.s(); !(_step = _iterator.n()).done;) {
                var exportName = _step.value;
                this._wasmInstance.exports[exportName]();
              }
            } catch (err) {
              _iterator.e(err);
            } finally {
              _iterator.f();
            }
          }
          this._wasmInstance.exports['screeps:screepsdotnet/botapi#init']();
          var _t4 = this._profileFn();
          if (this._profilingEnabled) {
            this.log("Init in ".concat(((_t4 - _t3) * 100 | 0) / 100, " ms (").concat(this._interop.buildProfilerString(), ")"));
          }
        }
        this._started = true;
      }
    }, {
      key: "loop",
      value: function loop() {
        var _this$_bindings2;
        if (!this._wasmInstance || !this._started) {
          return;
        }
        // Run bindings loop
        this._interop.loop();
        (_this$_bindings2 = this._bindings) === null || _this$_bindings2 === void 0 || _this$_bindings2.loop();
        // Dispatch log messages
        this._inTick = true;
        this.dispatchPendingLogs();
        // Run usercode loop
        {
          var t0 = this._profileFn();
          this._wasmInstance.exports['screeps:screepsdotnet/botapi#loop']();
          var t1 = this._profileFn();
          if (this._profilingEnabled) {
            this.log("Loop in ".concat(((t1 - t0) * 100 | 0) / 100, " ms (").concat(this._interop.buildProfilerString(), ")"));
          }
        }
      }
    }, {
      key: "getWasmImports",
      value: function getWasmImports() {
        var imports = _defineProperty(_defineProperty({}, 'screeps:screepsdotnet/js-bindings', _objectSpread2({}, this._interop.interopImport)), 'screeps:screepsdotnet/system-bindings', _objectSpread2({}, this._systemImport));
        if (this._env === 'world' || this._env === 'test') {
          var _this$_bindings3;
          imports['screeps:screepsdotnet/world-bindings'] = _objectSpread2({}, (_this$_bindings3 = this._bindings) === null || _this$_bindings3 === void 0 ? void 0 : _this$_bindings3.bindingsImport);
        } else if (this._env === 'arena') {
          var _this$_bindings4;
          imports['screeps:screepsdotnet/arena-bindings'] = _objectSpread2({}, (_this$_bindings4 = this._bindings) === null || _this$_bindings4 === void 0 ? void 0 : _this$_bindings4.bindingsImport);
        }
        return imports;
      }
    }, {
      key: "dispatchPendingLogs",
      value: function dispatchPendingLogs() {
        for (var i = 0; i < this._pendingLogs.length; ++i) {
          this.dispatchLog(this._pendingLogs[i]);
        }
        this._pendingLogs.length = 0;
      }
    }]);
    return Bootloader;
  }();

  exports.Bootloader = Bootloader;
  exports.decodeWasm = decodeWasm;
  exports.decompressWasm = decompressWasm;

  return exports;

})({});

export const Bootloader = bootloader.Bootloader;
export const decodeWasm = bootloader.decodeWasm;
export const decompressWasm = bootloader.decompressWasm;
