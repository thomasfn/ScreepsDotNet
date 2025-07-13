var bootloader = (function (exports) {
  'use strict';

  function _iterableToArrayLimit(arr, i) {
    var _i = null == arr ? null : "undefined" != typeof Symbol && arr[Symbol.iterator] || arr["@@iterator"];
    if (null != _i) {
      var _s,
        _e,
        _x,
        _r,
        _arr = [],
        _n = !0,
        _d = !1;
      try {
        if (_x = (_i = _i.call(arr)).next, 0 === i) {
          if (Object(_i) !== _i) return;
          _n = !1;
        } else for (; !(_n = (_s = _x.call(_i)).done) && (_arr.push(_s.value), _arr.length !== i); _n = !0);
      } catch (err) {
        _d = !0, _e = err;
      } finally {
        try {
          if (!_n && null != _i.return && (_r = _i.return(), Object(_r) !== _r)) return;
        } finally {
          if (_d) throw _e;
        }
      }
      return _arr;
    }
  }
  function ownKeys(object, enumerableOnly) {
    var keys = Object.keys(object);
    if (Object.getOwnPropertySymbols) {
      var symbols = Object.getOwnPropertySymbols(object);
      enumerableOnly && (symbols = symbols.filter(function (sym) {
        return Object.getOwnPropertyDescriptor(object, sym).enumerable;
      })), keys.push.apply(keys, symbols);
    }
    return keys;
  }
  function _objectSpread2(target) {
    for (var i = 1; i < arguments.length; i++) {
      var source = null != arguments[i] ? arguments[i] : {};
      i % 2 ? ownKeys(Object(source), !0).forEach(function (key) {
        _defineProperty(target, key, source[key]);
      }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)) : ownKeys(Object(source)).forEach(function (key) {
        Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));
      });
    }
    return target;
  }
  function _typeof(obj) {
    "@babel/helpers - typeof";

    return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (obj) {
      return typeof obj;
    } : function (obj) {
      return obj && "function" == typeof Symbol && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj;
    }, _typeof(obj);
  }
  function _classCallCheck(instance, Constructor) {
    if (!(instance instanceof Constructor)) {
      throw new TypeError("Cannot call a class as a function");
    }
  }
  function _defineProperties(target, props) {
    for (var i = 0; i < props.length; i++) {
      var descriptor = props[i];
      descriptor.enumerable = descriptor.enumerable || false;
      descriptor.configurable = true;
      if ("value" in descriptor) descriptor.writable = true;
      Object.defineProperty(target, _toPropertyKey(descriptor.key), descriptor);
    }
  }
  function _createClass(Constructor, protoProps, staticProps) {
    if (protoProps) _defineProperties(Constructor.prototype, protoProps);
    if (staticProps) _defineProperties(Constructor, staticProps);
    Object.defineProperty(Constructor, "prototype", {
      writable: false
    });
    return Constructor;
  }
  function _defineProperty(obj, key, value) {
    key = _toPropertyKey(key);
    if (key in obj) {
      Object.defineProperty(obj, key, {
        value: value,
        enumerable: true,
        configurable: true,
        writable: true
      });
    } else {
      obj[key] = value;
    }
    return obj;
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
  function _getPrototypeOf(o) {
    _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf.bind() : function _getPrototypeOf(o) {
      return o.__proto__ || Object.getPrototypeOf(o);
    };
    return _getPrototypeOf(o);
  }
  function _setPrototypeOf(o, p) {
    _setPrototypeOf = Object.setPrototypeOf ? Object.setPrototypeOf.bind() : function _setPrototypeOf(o, p) {
      o.__proto__ = p;
      return o;
    };
    return _setPrototypeOf(o, p);
  }
  function _isNativeReflectConstruct() {
    if (typeof Reflect === "undefined" || !Reflect.construct) return false;
    if (Reflect.construct.sham) return false;
    if (typeof Proxy === "function") return true;
    try {
      Boolean.prototype.valueOf.call(Reflect.construct(Boolean, [], function () {}));
      return true;
    } catch (e) {
      return false;
    }
  }
  function _assertThisInitialized(self) {
    if (self === void 0) {
      throw new ReferenceError("this hasn't been initialised - super() hasn't been called");
    }
    return self;
  }
  function _possibleConstructorReturn(self, call) {
    if (call && (typeof call === "object" || typeof call === "function")) {
      return call;
    } else if (call !== void 0) {
      throw new TypeError("Derived constructors may only return object or undefined");
    }
    return _assertThisInitialized(self);
  }
  function _createSuper(Derived) {
    var hasNativeReflectConstruct = _isNativeReflectConstruct();
    return function _createSuperInternal() {
      var Super = _getPrototypeOf(Derived),
        result;
      if (hasNativeReflectConstruct) {
        var NewTarget = _getPrototypeOf(this).constructor;
        result = Reflect.construct(Super, arguments, NewTarget);
      } else {
        result = Super.apply(this, arguments);
      }
      return _possibleConstructorReturn(this, result);
    };
  }
  function _superPropBase(object, property) {
    while (!Object.prototype.hasOwnProperty.call(object, property)) {
      object = _getPrototypeOf(object);
      if (object === null) break;
    }
    return object;
  }
  function _get() {
    if (typeof Reflect !== "undefined" && Reflect.get) {
      _get = Reflect.get.bind();
    } else {
      _get = function _get(target, property, receiver) {
        var base = _superPropBase(target, property);
        if (!base) return;
        var desc = Object.getOwnPropertyDescriptor(base, property);
        if (desc.get) {
          return desc.get.call(arguments.length < 3 ? target : receiver);
        }
        return desc.value;
      };
    }
    return _get.apply(this, arguments);
  }
  function _slicedToArray(arr, i) {
    return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest();
  }
  function _arrayWithHoles(arr) {
    if (Array.isArray(arr)) return arr;
  }
  function _unsupportedIterableToArray(o, minLen) {
    if (!o) return;
    if (typeof o === "string") return _arrayLikeToArray(o, minLen);
    var n = Object.prototype.toString.call(o).slice(8, -1);
    if (n === "Object" && o.constructor) n = o.constructor.name;
    if (n === "Map" || n === "Set") return Array.from(o);
    if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen);
  }
  function _arrayLikeToArray(arr, len) {
    if (len == null || len > arr.length) len = arr.length;
    for (var i = 0, arr2 = new Array(len); i < len; i++) arr2[i] = arr[i];
    return arr2;
  }
  function _nonIterableRest() {
    throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method.");
  }
  function _createForOfIteratorHelper(o, allowArrayLike) {
    var it = typeof Symbol !== "undefined" && o[Symbol.iterator] || o["@@iterator"];
    if (!it) {
      if (Array.isArray(o) || (it = _unsupportedIterableToArray(o)) || allowArrayLike && o && typeof o.length === "number") {
        if (it) o = it;
        var i = 0;
        var F = function () {};
        return {
          s: F,
          n: function () {
            if (i >= o.length) return {
              done: true
            };
            return {
              done: false,
              value: o[i++]
            };
          },
          e: function (e) {
            throw e;
          },
          f: F
        };
      }
      throw new TypeError("Invalid attempt to iterate non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method.");
    }
    var normalCompletion = true,
      didErr = false,
      err;
    return {
      s: function () {
        it = it.call(o);
      },
      n: function () {
        var step = it.next();
        normalCompletion = step.done;
        return step;
      },
      e: function (e) {
        didErr = true;
        err = e;
      },
      f: function () {
        try {
          if (!normalCompletion && it.return != null) it.return();
        } finally {
          if (didErr) throw err;
        }
      }
    };
  }
  function _toPrimitive(input, hint) {
    if (typeof input !== "object" || input === null) return input;
    var prim = input[Symbol.toPrimitive];
    if (prim !== undefined) {
      var res = prim.call(input, hint || "default");
      if (typeof res !== "object") return res;
      throw new TypeError("@@toPrimitive must return a primitive value.");
    }
    return (hint === "string" ? String : Number)(input);
  }
  function _toPropertyKey(arg) {
    var key = _toPrimitive(arg, "string");
    return typeof key === "symbol" ? key : String(key);
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
  for (var i$1 = 0; i$1 < 32768; ++i$1) {
    // reverse table algorithm from SO
    var x = (i$1 & 0xAAAA) >> 1 | (i$1 & 0x5555) << 1;
    x = (x & 0xCCCC) >> 2 | (x & 0x3333) << 2;
    x = (x & 0xF0F0) >> 4 | (x & 0x0F0F) << 4;
    rev[i$1] = ((x & 0xFF00) >> 8 | (x & 0x00FF) << 8) >> 1;
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
  for (var i$1 = 0; i$1 < 144; ++i$1) flt[i$1] = 8;
  for (var i$1 = 144; i$1 < 256; ++i$1) flt[i$1] = 9;
  for (var i$1 = 256; i$1 < 280; ++i$1) flt[i$1] = 7;
  for (var i$1 = 280; i$1 < 288; ++i$1) flt[i$1] = 8;
  // fixed distance tree
  var fdt = new u8(32);
  for (var i$1 = 0; i$1 < 32; ++i$1) fdt[i$1] = 5;
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

  var INTEROP_VALUE_TYPE_NAMES = ['void', 'bool', 'byte', 'sbyte', 'ushort', 'short', 'uint', 'int', 'ulong', 'long', 'float', 'double', 'void*', 'char*', 'JSObject', '[]', 'Name'];
  function stringifyParamSpec(paramSpec) {
    if (paramSpec.type === 15 /* InteropValueType.Arr */ && paramSpec.elementSpec) {
      return "".concat(stringifyParamSpec(paramSpec.elementSpec), "[]");
    }
    return "".concat(INTEROP_VALUE_TYPE_NAMES[paramSpec.type]).concat(paramSpec.nullable ? '?' : '');
  }
  var EXCEPTION_PARAM_SPEC = {
    type: 13 /* InteropValueType.Str */,
    nullable: false,
    nullAsUndefined: false
  };
  var CLR_TRACKING_ID = Symbol('clr-tracking-id');
  function hasId(obj) {
    // TODO: Are we going to have an issue here with pojo's with ids? e.g. an object from Memory which is just { id: 'xyz' }
    return 'id' in obj;
  }
  var Interop = /*#__PURE__*/function () {
    function Interop(profileFn) {
      _classCallCheck(this, Interop);
      _defineProperty(this, "interopImport", void 0);
      _defineProperty(this, "_profileFn", void 0);
      _defineProperty(this, "_imports", {});
      _defineProperty(this, "_boundImportList", []);
      _defineProperty(this, "_boundRawImportList", []);
      _defineProperty(this, "_boundImportSymbolList", []);
      _defineProperty(this, "_objectTrackingList", {});
      _defineProperty(this, "_objectTrackingListById", {});
      _defineProperty(this, "_nonExtensibleObjectTrackingMap", new WeakMap());
      _defineProperty(this, "_nameList", []);
      _defineProperty(this, "_nameTable", {});
      _defineProperty(this, "_memoryManager", void 0);
      _defineProperty(this, "_malloc", void 0);
      _defineProperty(this, "_nextClrTrackingId", 0);
      _defineProperty(this, "_numBoundImportInvokes", 0);
      _defineProperty(this, "_numImportBinds", 0);
      _defineProperty(this, "_numBeginTrackingObjects", 0);
      _defineProperty(this, "_numReleaseTrackingObjects", 0);
      _defineProperty(this, "_numTotalTrackingObjects", 0);
      _defineProperty(this, "_timeInInterop", 0);
      _defineProperty(this, "_timeInJsUserCode", 0);
      this._profileFn = profileFn;
      this.interopImport = {};
      this.interopImport['bind-import'] = this.js_bind_import.bind(this);
      this.interopImport['invoke-import'] = this.js_invoke_import.bind(this);
      this.interopImport['release-object-reference'] = this.js_release_object_reference.bind(this);
      this.interopImport['set-name'] = this.js_set_name.bind(this);
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
      key: "memoryManager",
      get: function get() {
        return this._memoryManager;
      },
      set: function set(value) {
        this._memoryManager = value;
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
      key: "setImports",
      value: function setImports(moduleName, importTable) {
        this._imports[moduleName] = importTable;
      }
    }, {
      key: "loop",
      value: function loop() {
        this._numBoundImportInvokes = 0;
        this._numImportBinds = 0;
        this._numBeginTrackingObjects = 0;
        this._numReleaseTrackingObjects = 0;
        this._timeInInterop = 0;
        this._timeInJsUserCode = 0;
      }
    }, {
      key: "buildProfilerString",
      value: function buildProfilerString() {
        var phrases = ["".concat((this._timeInInterop * 100 | 0) / 100, " ms in interop"), "".concat((this._timeInJsUserCode * 100 | 0) / 100, " ms in screeps api"), "".concat(this._numBoundImportInvokes, " js interop calls"), "".concat(this._numTotalTrackingObjects, " +").concat(this._numBeginTrackingObjects, " -").concat(this._numReleaseTrackingObjects, " tracked js objects")];
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
        if (!this._memoryManager) {
          return -1;
        }
        var memoryView = this._memoryManager.view;
        var moduleName = this.stringToJs(memoryView, moduleNamePtr);
        var importTable = this._imports[moduleName];
        if (!importTable) {
          throw new Error("unknown import module '".concat(moduleName, "'"));
        }
        var importName = this.stringToJs(memoryView, importNamePtr);
        var importFunction = this.resolveImport(moduleName, importTable, importName);
        this._boundRawImportList.push(importFunction);
        var functionSpec = this.functionSpecToJs(memoryView, functionSpecPtr);
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
      value: function js_release_object_reference(clrTrackingId) {
        var obj = this._objectTrackingList[clrTrackingId];
        if (obj == null) {
          return;
        }
        delete this._objectTrackingList[clrTrackingId];
        if (hasId(obj) && this._objectTrackingListById[obj.id] === obj) {
          delete this._objectTrackingListById[obj.id];
        }
        this.clearClrTrackingId(obj);
        ++this._numReleaseTrackingObjects;
        --this._numTotalTrackingObjects;
      }
    }, {
      key: "js_set_name",
      value: function js_set_name(nameIndex, valuePtr) {
        if (!this._memoryManager) {
          return;
        }
        var value = this.stringToJs(this._memoryManager.view, valuePtr);
        this._nameList[nameIndex] = value;
        this._nameTable[value] = nameIndex;
      }
    }, {
      key: "js_invoke_i_i",
      value: function js_invoke_i_i(importIndex, p0) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
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
        return boundImportFunction(this._objectTrackingList[p0]);
      }
    }, {
      key: "js_invoke_i_oi",
      value: function js_invoke_i_oi(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(this._objectTrackingList[p0], p1);
      }
    }, {
      key: "js_invoke_i_on",
      value: function js_invoke_i_on(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(this._objectTrackingList[p0], this._nameList[p1]);
      }
    }, {
      key: "js_invoke_i_oii",
      value: function js_invoke_i_oii(importIndex, p0, p1, p2) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(this._objectTrackingList[p0], p1, p2);
      }
    }, {
      key: "js_invoke_i_oo",
      value: function js_invoke_i_oo(importIndex, p0, p1) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(this._objectTrackingList[p0], this._objectTrackingList[p1]);
      }
    }, {
      key: "js_invoke_i_ooi",
      value: function js_invoke_i_ooi(importIndex, p0, p1, p2) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(this._objectTrackingList[p0], this._objectTrackingList[p1], p2);
      }
    }, {
      key: "js_invoke_i_ooii",
      value: function js_invoke_i_ooii(importIndex, p0, p1, p2, p3) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(this._objectTrackingList[p0], this._objectTrackingList[p1], p2, p3);
      }
    }, {
      key: "js_invoke_d_v",
      value: function js_invoke_d_v(importIndex) {
        var boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
          throw new Error("attempt to invoke invalid import index ".concat(importIndex));
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction();
      }
    }, {
      key: "createImportBinding",
      value: function createImportBinding(importFunction, functionSpec, importIndex) {
        var _this = this;
        return function (paramsBufferPtr) {
          if (!_this._memoryManager) {
            return 0;
          }
          var memoryView = _this._memoryManager.view;
          var t0 = _this._profileFn();
          // TODO: Cache args array to eliminate allocation here
          var args = [];
          args.length = functionSpec.paramSpecs.length;
          var returnValPtr = paramsBufferPtr;
          var exceptionValPtr = paramsBufferPtr + 16;
          var argsPtr = exceptionValPtr + 16;
          try {
            for (var i = 0; i < functionSpec.paramSpecs.length; ++i) {
              args[i] = _this.marshalToJs(memoryView, argsPtr, functionSpec.paramSpecs[i]);
              argsPtr += 16;
            }
          } catch (err) {
            throw new Error("".concat(_this.stringifyImportBindingForDisplay(importIndex), ": ").concat(err.message));
          }
          var t1 = _this._profileFn();
          _this._timeInInterop += t1 - t0;
          var returnVal;
          try {
            returnVal = importFunction.apply(void 0, args);
            _this.marshalToClr(memoryView, returnValPtr, functionSpec.returnSpec, returnVal);
            //console.log(`${importIndex}:${this._boundImportSymbolList[importIndex]}(${args.map(this.stringifyValueForDisplay.bind(this)).join(', ')}) -> ${this.stringifyValueForDisplay(returnVal)}`);
            return 1;
          } catch (err) {
            _this.marshalToClr(memoryView, exceptionValPtr, EXCEPTION_PARAM_SPEC, "".concat(err));
            return 0;
          } finally {
            var t2 = _this._profileFn();
            _this._timeInJsUserCode += t2 - t1;
          }
        };
      }
    }, {
      key: "marshalToJs",
      value: function marshalToJs(memoryView, valuePtr, paramSpec) {
        var _paramSpec$elementSpe, _INTEROP_VALUE_TYPE_N2;
        var valueType = memoryView.u8[valuePtr + 12];
        if (valueType === 0 /* InteropValueType.Void */ && paramSpec.nullable) {
          return paramSpec.nullAsUndefined ? undefined : null;
        }
        if (paramSpec.type === 15 /* InteropValueType.Arr */ && ((_paramSpec$elementSpe = paramSpec.elementSpec) === null || _paramSpec$elementSpe === void 0 ? void 0 : _paramSpec$elementSpe.type) === 13 /* InteropValueType.Str */ && valueType === 15 /* InteropValueType.Arr */) {
          return this.stringArrayToJs(memoryView, memoryView.i32[valuePtr >> 2], memoryView.i32[valuePtr + 8 >> 2], paramSpec.elementSpec);
        }
        if (paramSpec.type === 7 /* InteropValueType.I32 */ && valueType === 12 /* InteropValueType.Ptr */) {
          return memoryView.i32[valuePtr >> 2];
        }
        if (valueType !== paramSpec.type) {
          var _INTEROP_VALUE_TYPE_N;
          throw new Error("failed to marshal ".concat(stringifyParamSpec(paramSpec), " from '").concat((_INTEROP_VALUE_TYPE_N = INTEROP_VALUE_TYPE_NAMES[valueType]) !== null && _INTEROP_VALUE_TYPE_N !== void 0 ? _INTEROP_VALUE_TYPE_N : 'unknown', "'"));
        }
        switch (paramSpec.type) {
          case 0 /* InteropValueType.Void */:
            return undefined;
          case 1 /* InteropValueType.U1 */:
            return memoryView.u8[valuePtr] !== 0;
          case 2 /* InteropValueType.U8 */:
            return memoryView.u8[valuePtr];
          case 3 /* InteropValueType.I8 */:
            return memoryView.i8[valuePtr];
          case 4 /* InteropValueType.U16 */:
            return memoryView.u16[valuePtr >> 1];
          case 5 /* InteropValueType.I16 */:
            return memoryView.i16[valuePtr >> 1];
          case 6 /* InteropValueType.U32 */:
            return memoryView.u32[valuePtr >> 2];
          case 7 /* InteropValueType.I32 */:
            return memoryView.i32[valuePtr >> 2];
          case 8 /* InteropValueType.U64 */:
            return memoryView.u32[valuePtr >> 2] << 32 | memoryView.u32[valuePtr + 4 >> 2];
          case 9 /* InteropValueType.I64 */:
            return memoryView.i32[valuePtr >> 2] << 32 | memoryView.i32[valuePtr + 4 >> 2];
          case 10 /* InteropValueType.F32 */:
            return memoryView.f32[valuePtr >> 2];
          case 11 /* InteropValueType.F64 */:
            return memoryView.f64[valuePtr >> 3];
          case 12 /* InteropValueType.Ptr */:
            return new DataView(memoryView.u8.buffer, memoryView.i32[valuePtr >> 2], memoryView.i32[valuePtr + 8 >> 2]);
          case 13 /* InteropValueType.Str */:
            return this.stringToJs(memoryView, memoryView.i32[valuePtr >> 2]);
          case 14 /* InteropValueType.Obj */:
            return this._objectTrackingList[memoryView.i32[valuePtr + 4 >> 2]];
          case 15 /* InteropValueType.Arr */:
            if (paramSpec.elementSpec == null) {
              throw new Error("malformed param spec (array with no element spec)");
            }
            return this.arrayToJs(memoryView, memoryView.i32[valuePtr >> 2], memoryView.i32[valuePtr + 8 >> 2], paramSpec.elementSpec);
          case 16 /* InteropValueType.Nme */:
            return this._nameList[memoryView.i32[valuePtr >> 2]];
          default:
            throw new Error("failed to marshal ".concat(stringifyParamSpec(paramSpec), " from '").concat((_INTEROP_VALUE_TYPE_N2 = INTEROP_VALUE_TYPE_NAMES[valueType]) !== null && _INTEROP_VALUE_TYPE_N2 !== void 0 ? _INTEROP_VALUE_TYPE_N2 : 'unknown', "'"));
        }
      }
    }, {
      key: "marshalToClr",
      value: function marshalToClr(memoryView, valuePtr, paramSpec, value) {
        if (value == null) {
          if (paramSpec.nullable || paramSpec.type === 0 /* InteropValueType.Void */) {
            memoryView.u8[valuePtr + 12] = 0 /* InteropValueType.Void */;
            return;
          }
          throw new Error("failed to marshal null as '".concat(stringifyParamSpec(paramSpec), "'"));
        }
        switch (paramSpec.type) {
          case 0 /* InteropValueType.Void */:
            memoryView.u8[valuePtr + 12] = 0 /* InteropValueType.Void */;
            break;
          case 1 /* InteropValueType.U1 */:
            memoryView.u8[valuePtr] = value ? 1 : 0;
            memoryView.u8[valuePtr + 12] = 1 /* InteropValueType.U1 */;
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
              this.marshalNumericToClr(memoryView, valuePtr, paramSpec, value);
              break;
            }
            if (value instanceof BigInt) {
              throw new Error("failed to marshal BigInt as '".concat(stringifyParamSpec(paramSpec), "' (not yet implemented)"));
            }
            throw new Error("failed to marshal non-numeric as '".concat(stringifyParamSpec(paramSpec), "'"));
          // case InteropValueType.Ptr: return;
          case 13 /* InteropValueType.Str */:
            memoryView.i32[valuePtr >> 2] = this.stringToClr(memoryView, typeof value === 'string' ? value : "".concat(value));
            memoryView.u8[valuePtr + 12] = 13 /* InteropValueType.Str */;
            break;
          case 14 /* InteropValueType.Obj */:
            if (_typeof(value) !== 'object' && typeof value !== 'function') {
              throw new Error("failed to marshal ".concat(_typeof(value), " as '").concat(stringifyParamSpec(paramSpec), "' (not an object)"));
            }
            memoryView.i32[valuePtr + 4 >> 2] = this.getOrAssignClrTrackingId(value);
            memoryView.u8[valuePtr + 12] = 14 /* InteropValueType.Obj */;
            break;
          case 15 /* InteropValueType.Arr */:
            if (paramSpec.elementSpec == null) {
              throw new Error("malformed param spec (array with no element spec)");
            }
            if (!Array.isArray(value)) {
              //value = [value];
              // TODO: We could have a param spec flag that wraps single values in arrays in case we need to support apis that sometimes returns an array and sometimes a single value
              throw new Error("failed to marshal ".concat(_typeof(value), " as '").concat(stringifyParamSpec(paramSpec), "' (not an array)"));
            }
            if (paramSpec.elementSpec.type === 13 /* InteropValueType.Str */) {
              memoryView.i32[valuePtr >> 2] = this.stringArrayToClr(memoryView, value, paramSpec.elementSpec);
            } else {
              memoryView.i32[valuePtr >> 2] = this.arrayToClr(memoryView, value, paramSpec.elementSpec);
            }
            memoryView.i32[valuePtr + 8 >> 2] = value.length;
            memoryView.u8[valuePtr + 12] = 15 /* InteropValueType.Arr */;
            break;
          case 16 /* InteropValueType.Nme */:
            var valueAsStr = typeof value === 'string' ? value : "".concat(value);
            var nameIndex = this._nameTable[valueAsStr];
            if (nameIndex == null) {
              memoryView.i32[valuePtr >> 2] = this.stringToClr(memoryView, valueAsStr);
              memoryView.u8[valuePtr + 12] = 13 /* InteropValueType.Str */;
            } else {
              memoryView.i32[valuePtr >> 2] = nameIndex;
              memoryView.u8[valuePtr + 12] = 16 /* InteropValueType.Nme */;
            }

            break;
          default:
            throw new Error("failed to marshal '".concat(_typeof(value), "' as '").concat(stringifyParamSpec(paramSpec), "' (not yet implemented)"));
        }
      }
    }, {
      key: "marshalNumericToClr",
      value: function marshalNumericToClr(memoryView, valuePtr, paramSpec, value) {
        switch (paramSpec.type) {
          case 2 /* InteropValueType.U8 */:
            memoryView.u8[valuePtr] = value;
            break;
          case 3 /* InteropValueType.I8 */:
            memoryView.i8[valuePtr] = value;
            break;
          case 4 /* InteropValueType.U16 */:
            memoryView.u16[valuePtr >> 1] = value;
            break;
          case 5 /* InteropValueType.I16 */:
            memoryView.i16[valuePtr >> 1] = value;
            break;
          case 6 /* InteropValueType.U32 */:
            memoryView.u32[valuePtr >> 2] = value;
            break;
          case 7 /* InteropValueType.I32 */:
            memoryView.i32[valuePtr >> 2] = value;
            break;
          // case InteropValueType.U64: break;
          // case InteropValueType.I64: break;
          case 10 /* InteropValueType.F32 */:
            memoryView.f32[valuePtr >> 2] = value;
            break;
          case 11 /* InteropValueType.F64 */:
            memoryView.f64[valuePtr >> 3] = value;
            break;
          default:
            throw new Error("failed to marshal numeric as '".concat(stringifyParamSpec(paramSpec), "' (not yet implemented)"));
        }
        memoryView.u8[valuePtr + 12] = paramSpec.type;
      }
    }, {
      key: "stringToJs",
      value: function stringToJs(memoryView, stringPtr) {
        var code;
        var result = '';
        do {
          code = memoryView.u16[stringPtr >> 1];
          if (code !== 0) {
            result += String.fromCharCode(code);
          }
          stringPtr += 2;
        } while (code !== 0);
        return result;
      }
    }, {
      key: "stringToClr",
      value: function stringToClr(memoryView, str) {
        var strPtr = this._malloc((str.length + 1) * 2);
        var charPtr = strPtr;
        for (var i = 0; i < str.length; ++i) {
          memoryView.u16[charPtr >> 1] = str.charCodeAt(i);
          charPtr += 2;
        }
        memoryView.u16[charPtr >> 1] = 0;
        return strPtr;
      }
    }, {
      key: "arrayToJs",
      value: function arrayToJs(memoryView, arrayPtr, arrayLen, elementSpec) {
        var result = [];
        result.length = arrayLen;
        for (var i = 0; i < arrayLen; ++i) {
          result[i] = this.marshalToJs(memoryView, arrayPtr, elementSpec);
          arrayPtr += 16;
        }
        return result;
      }
    }, {
      key: "arrayToClr",
      value: function arrayToClr(memoryView, value, elementSpec) {
        var arrPtr = this._malloc(value.length * 16);
        var elPtr = arrPtr;
        for (var i = 0; i < value.length; ++i) {
          this.marshalToClr(memoryView, elPtr, elementSpec, value[i]);
          elPtr += 16;
        }
        return arrPtr;
      }
    }, {
      key: "stringArrayToJs",
      value: function stringArrayToJs(memoryView, arrayPtr, arrayLen, elementSpec) {
        var result = [];
        result.length = arrayLen;
        for (var i = 0; i < arrayLen; ++i) {
          var code = void 0;
          if (elementSpec.nullable) {
            code = memoryView.u16[arrayPtr >> 1];
            arrayPtr += 2;
            if (code === 0) {
              result[i] = elementSpec.nullAsUndefined ? undefined : null;
              arrayPtr += 2;
              break;
            }
          }
          var element = '';
          do {
            code = memoryView.u16[arrayPtr >> 1];
            if (code !== 0) {
              element += String.fromCharCode(code);
            }
            arrayPtr += 2;
          } while (code !== 0);
          result[i] = element;
        }
        return result;
      }
    }, {
      key: "stringArrayToClr",
      value: function stringArrayToClr(memoryView, value, elementSpec) {
        var bufferSize = 0;
        var _iterator2 = _createForOfIteratorHelper(value),
          _step2;
        try {
          for (_iterator2.s(); !(_step2 = _iterator2.n()).done;) {
            var element = _step2.value;
            if (elementSpec.nullable) {
              ++bufferSize;
              if (element == null) {
                ++bufferSize;
                continue;
              }
            }
            var str = typeof element === 'string' ? element : "".concat(element);
            bufferSize += str.length + 1;
          }
        } catch (err) {
          _iterator2.e(err);
        } finally {
          _iterator2.f();
        }
        var strPtr = this._malloc(bufferSize * 2);
        var charPtr = strPtr;
        var _iterator3 = _createForOfIteratorHelper(value),
          _step3;
        try {
          for (_iterator3.s(); !(_step3 = _iterator3.n()).done;) {
            var _element = _step3.value;
            if (elementSpec.nullable) {
              memoryView.u16[charPtr >> 1] = _element != null ? 1 : 0;
              charPtr += 2;
            }
            var _str = typeof _element === 'string' ? _element : "".concat(_element);
            for (var i = 0; i < _str.length; ++i) {
              memoryView.u16[charPtr >> 1] = _str.charCodeAt(i);
              charPtr += 2;
            }
            memoryView.u16[charPtr >> 1] = 0;
            charPtr += 2;
          }
        } catch (err) {
          _iterator3.e(err);
        } finally {
          _iterator3.f();
        }
        return strPtr;
      }
    }, {
      key: "paramSpecToJs",
      value: function paramSpecToJs(memoryView, paramSpecPtr) {
        var type = memoryView.u8[paramSpecPtr];
        var flags = memoryView.u8[paramSpecPtr + 1];
        var elementType = memoryView.u8[paramSpecPtr + 2];
        var elementFlags = memoryView.u8[paramSpecPtr + 3];
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
      value: function functionSpecToJs(memoryView, functionSpecPtr) {
        var result = {
          returnSpec: this.paramSpecToJs(memoryView, functionSpecPtr),
          paramSpecs: []
        };
        functionSpecPtr += 4;
        for (var i = 0; i < 8; ++i) {
          var paramSpec = this.paramSpecToJs(memoryView, functionSpecPtr);
          if (paramSpec.type === 0 /* InteropValueType.Void */) {
            break;
          }
          result.paramSpecs.push(paramSpec);
          functionSpecPtr += 4;
        }
        return result;
      }
    }, {
      key: "getClrTrackingId",
      value: function getClrTrackingId(obj) {
        var _obj$CLR_TRACKING_ID;
        return (_obj$CLR_TRACKING_ID = obj[CLR_TRACKING_ID]) !== null && _obj$CLR_TRACKING_ID !== void 0 ? _obj$CLR_TRACKING_ID : this._nonExtensibleObjectTrackingMap.get(obj);
      }
    }, {
      key: "assignClrTrackingId",
      value: function assignClrTrackingId(obj, newClrTrackingId) {
        if (newClrTrackingId == null) {
          newClrTrackingId = this._nextClrTrackingId++;
          ++this._numBeginTrackingObjects;
          ++this._numTotalTrackingObjects;
        }
        if (Object.isExtensible(obj)) {
          obj[CLR_TRACKING_ID] = newClrTrackingId;
        } else {
          this._nonExtensibleObjectTrackingMap.set(obj, newClrTrackingId);
        }
        this._objectTrackingList[newClrTrackingId] = obj;
        if (hasId(obj)) {
          this._objectTrackingListById[obj.id] = obj;
        }
        return newClrTrackingId;
      }
    }, {
      key: "clearClrTrackingId",
      value: function clearClrTrackingId(obj) {
        if (Object.isExtensible(obj)) {
          obj[CLR_TRACKING_ID] = undefined;
        } else {
          this._nonExtensibleObjectTrackingMap["delete"](obj);
        }
      }
    }, {
      key: "replaceClrTrackedObject",
      value: function replaceClrTrackedObject(p0, newObj) {
        var clrTrackingId = typeof p0 === 'number' ? p0 : this.getClrTrackingId(p0);
        if (clrTrackingId == null) {
          return;
        }
        var oldObj = typeof p0 === 'number' ? this._objectTrackingList[clrTrackingId] : p0;
        if (oldObj != null) {
          this.clearClrTrackingId(oldObj);
        }
        return this.assignClrTrackingId(newObj, clrTrackingId);
      }
    }, {
      key: "getClrTrackedObject",
      value: function getClrTrackedObject(clrTrackingId) {
        return this._objectTrackingList[clrTrackingId];
      }
    }, {
      key: "getOrAssignClrTrackingId",
      value: function getOrAssignClrTrackingId(obj) {
        var _clrTrackingId;
        var clrTrackingId = this.getClrTrackingId(obj);
        if (clrTrackingId == null) {
          // It doesn't - if it has an id, see if we're already tracking a stale version of the game object
          if (hasId(obj)) {
            var previousVersion = this._objectTrackingListById[obj.id];
            if (previousVersion != null && previousVersion !== obj) {
              // Replace the previous version with this one and reuse the tracking id
              clrTrackingId = this.replaceClrTrackedObject(previousVersion, obj);
            }
          }
        }
        return (_clrTrackingId = clrTrackingId) !== null && _clrTrackingId !== void 0 ? _clrTrackingId : this.assignClrTrackingId(obj);
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
          return "array[#".concat(value.length, ", %").concat(this.getClrTrackingId(value), "]");
        }
        if (_typeof(value) === 'object') {
          return "object[#".concat(Object.keys(value).length, ", %").concat(this.getClrTrackingId(value), "]");
        }
        return _typeof(value);
      }
    }, {
      key: "stringifyImportBindingForDisplay",
      value: function stringifyImportBindingForDisplay(importIndex) {
        var boundImportSymbol = this._boundImportSymbolList[importIndex];
        return "".concat(importIndex, ": ").concat(stringifyParamSpec(boundImportSymbol.functionSpec.returnSpec), " ").concat(boundImportSymbol.fullName, "(").concat(boundImportSymbol.functionSpec.paramSpecs.map(stringifyParamSpec).join(', '), ")");
      }
    }, {
      key: "visitClrTrackedObjects",
      value: function visitClrTrackedObjects(visitor) {
        for (var i = 0; i < this._nextClrTrackingId; ++i) {
          var obj = this._objectTrackingList[i];
          if (obj == null) {
            continue;
          }
          visitor(obj);
        }
      }
    }]);
    return Interop;
  }();

  function createWasmMemoryView(memory) {
    return {
      u8: new Uint8Array(memory.buffer),
      i8: new Int8Array(memory.buffer),
      u16: new Uint16Array(memory.buffer),
      i16: new Int16Array(memory.buffer),
      u32: new Uint32Array(memory.buffer),
      i32: new Int32Array(memory.buffer),
      f32: new Float32Array(memory.buffer),
      f64: new Float64Array(memory.buffer),
      dataView: new DataView(memory.buffer)
    };
  }
  var WasmMemoryManager = /*#__PURE__*/function () {
    function WasmMemoryManager(memory) {
      _classCallCheck(this, WasmMemoryManager);
      _defineProperty(this, "_memory", void 0);
      _defineProperty(this, "_view", void 0);
      _defineProperty(this, "_viewArrayBuffer", void 0);
      this._memory = memory;
    }
    _createClass(WasmMemoryManager, [{
      key: "view",
      get: function get() {
        if (this._view == null || this._viewArrayBuffer !== this._memory.buffer) {
          this._view = this.createNewView();
          this._viewArrayBuffer = this._memory.buffer;
        }
        return this._view;
      }
    }, {
      key: "createNewView",
      value: function createNewView() {
        return createWasmMemoryView(this._memory);
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
      _defineProperty(this, "_memoryManager", void 0);
      _defineProperty(this, "_malloc", void 0);
      this.logFunc = logFunc;
      this._interop = interop;
      this.bindingsImport = {};
      this.setupImports();
    }
    _createClass(BaseBindings, [{
      key: "init",
      value: function init(exports, memoryManager) {
        this._memoryManager = memoryManager;
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

  var RESOURCE_LIST$1 = ["energy", "score", "score_x", "score_y", "score_z"]; // 5 total
  {
    for (var _i$1 = 0, _RESOURCE_LIST$1 = RESOURCE_LIST$1; _i$1 < _RESOURCE_LIST$1.length; _i$1++) {
      _RESOURCE_LIST$1[_i$1];
    }
  }
  var BODYPART_LIST$1 = ['move', 'work', 'carry', 'attack', 'ranged_attack', 'heal', 'tough']; // 7 total
  var BODYPART_TO_ENUM_MAP$1 = {};
  {
    var _i2$1 = 0;
    for (var _i3$1 = 0, _BODYPART_LIST$1 = BODYPART_LIST$1; _i3$1 < _BODYPART_LIST$1.length; _i3$1++) {
      var bodyPart$1 = _BODYPART_LIST$1[_i3$1];
      BODYPART_TO_ENUM_MAP$1[bodyPart$1] = _i2$1++;
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
      value: function init(exports, memoryManager) {
        _get(_getPrototypeOf(ArenaBindings.prototype), "init", this).call(this, exports, memoryManager);
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
            var i32 = _this._memoryManager.view.i32;
            var goal;
            var goalsPtrI32 = goalsPtr >> 2;
            if (goalsCnt == 1) {
              var r = i32[goalsPtrI32 + 2];
              if (r === 0) {
                goal = {
                  x: i32[goalsPtrI32 + 0],
                  y: i32[goalsPtrI32 + 1]
                };
              } else {
                goal = {
                  pos: {
                    x: i32[goalsPtrI32 + 0],
                    y: i32[goalsPtrI32 + 1]
                  },
                  range: r
                };
              }
            } else {
              goal = [];
              goal.length = goalsCnt;
              for (var _i4 = 0; _i4 < goalsCnt; ++_i4) {
                var _r = i32[goalsPtrI32 + 2];
                if (_r === 0) {
                  goal[_i4] = {
                    x: i32[goalsPtrI32 + 0],
                    y: i32[goalsPtrI32 + 1]
                  };
                } else {
                  goal[_i4] = {
                    pos: {
                      x: i32[goalsPtrI32 + 0],
                      y: i32[goalsPtrI32 + 1]
                    },
                    range: _r
                  };
                }
                goalsPtrI32 += 3;
              }
            }
            var originPos = {
              x: origin >> 16,
              y: origin & 0xffff
            };
            return pathFinder.searchPath(originPos, goal, options);
          },
          decodePath: function decodePath(resultObj, outPtr) {
            return _this.copyPath(_this._memoryManager.view, resultObj.path, outPtr);
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
              return _this.copyPath(_this._memoryManager.view, result, outPtr);
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
              return _this.encodeCreepBody(_this._memoryManager.view, thisObj.body, outPtr);
            }
          })
        });
      }
    }, {
      key: "encodeCreepBody",
      value: function encodeCreepBody(memoryView, body, outPtr) {
        var i16 = memoryView.i16;
        var ptrI16 = outPtr >> 1;
        for (var _i5 = 0; _i5 < body.length; ++_i5) {
          var _body$_i = body[_i5],
            type = _body$_i.type,
            hits = _body$_i.hits;
          // Encode each body part to a 16 bit int as 2 bytes
          // type: 0-8 (4 bits 0-15) b1
          // hits: 0-100 (7 bits 0-127) b0
          var encodedBodyPart = 0;
          encodedBodyPart |= BODYPART_TO_ENUM_MAP$1[type] << 8;
          encodedBodyPart |= hits;
          i16[ptrI16] = encodedBodyPart;
          ++ptrI16;
        }
        return body.length;
      }
    }, {
      key: "copyPath",
      value: function copyPath(memoryView, path, outPtr) {
        var i32 = memoryView.i32;
        var ptrI32 = outPtr >> 2;
        for (var _i6 = 0; _i6 < path.length; ++_i6) {
          i32[ptrI32 + 0] = path[_i6].x;
          i32[ptrI32 + 1] = path[_i6].y;
          ptrI32 += 2;
        }
        return path.length;
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

  var CPU_HALT_WHEN_NO_CHECKIN_FOR = 10;
  var RESOURCE_LIST = ["energy", "power", "H", "O", "U", "L", "K", "Z", "X", "G", "silicon", "metal", "biomass", "mist", "OH", "ZK", "UL", "UH", "UO", "KH", "KO", "LH", "LO", "ZH", "ZO", "GH", "GO", "UH2O", "UHO2", "KH2O", "KHO2", "LH2O", "LHO2", "ZH2O", "ZHO2", "GH2O", "GHO2", "XUH2O", "XUHO2", "XKH2O", "XKHO2", "XLH2O", "XLHO2", "XZH2O", "XZHO2", "XGH2O", "XGHO2", "ops", "utrium_bar", "lemergium_bar", "zynthium_bar", "keanium_bar", "ghodium_melt", "oxidant", "reductant", "purifier", "battery", "composite", "crystal", "liquid", "wire", "switch", "transistor", "microchip", "circuit", "device", "cell", "phlegm", "tissue", "muscle", "organoid", "organism", "alloy", "tube", "fixtures", "frame", "hydraulics", "machine", "condensate", "concentrate", "extract", "spirit", "emanation", "essence", "season", "score"]; // 85 total
  var RESOURCE_TO_ENUM_MAP = {};
  {
    var i = 0;
    for (var _i = 0, _RESOURCE_LIST = RESOURCE_LIST; _i < _RESOURCE_LIST.length; _i++) {
      var resourceName = _RESOURCE_LIST[_i];
      RESOURCE_TO_ENUM_MAP[resourceName] = i++;
    }
  }
  var BODYPART_LIST = ['move', 'work', 'carry', 'attack', 'ranged_attack', 'tough', 'heal', 'claim']; // 8 total
  var BODYPART_TO_ENUM_MAP = {};
  {
    var _i2 = 0;
    for (var _i3 = 0, _BODYPART_LIST = BODYPART_LIST; _i3 < _BODYPART_LIST.length; _i3++) {
      var bodyPart = _BODYPART_LIST[_i3];
      BODYPART_TO_ENUM_MAP[bodyPart] = _i2++;
    }
  }
  var TEMP_ROOM_COORD_A = [0, 0];
  var TEMP_ROOM_COORD_B = [0, 0];
  var WorldBindings = /*#__PURE__*/function (_BaseBindings) {
    _inherits(WorldBindings, _BaseBindings);
    var _super = _createSuper(WorldBindings);
    function WorldBindings() {
      var _this;
      _classCallCheck(this, WorldBindings);
      for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
        args[_key] = arguments[_key];
      }
      _this = _super.call.apply(_super, [this].concat(args));
      _defineProperty(_assertThisInitialized(_this), "_lastCheckIn", 0);
      _defineProperty(_assertThisInitialized(_this), "_memoryCache", void 0);
      _defineProperty(_assertThisInitialized(_this), "_invoke_room_callback", void 0);
      _defineProperty(_assertThisInitialized(_this), "_invoke_cost_callback", void 0);
      _defineProperty(_assertThisInitialized(_this), "_invoke_route_callback", void 0);
      return _this;
    }
    _createClass(WorldBindings, [{
      key: "init",
      value: function init(exports, memoryManager) {
        _get(_getPrototypeOf(WorldBindings.prototype), "init", this).call(this, exports, memoryManager);
        this._invoke_room_callback = exports['screeps:screepsdotnet/botapi#invoke-room-callback'];
        this._invoke_cost_callback = exports['screeps:screepsdotnet/botapi#invoke-cost-callback'];
        this._invoke_route_callback = exports['screeps:screepsdotnet/botapi#invoke-route-callback'];
        this._memoryCache = Memory;
        this._memoryCache = RawMemory._parsed;
        this._lastCheckIn = Game.time;
      }
    }, {
      key: "loop",
      value: function loop() {
        _get(_getPrototypeOf(WorldBindings.prototype), "loop", this).call(this);
        // Memhack
        var _global = global;
        delete _global.Memory;
        _global.Memory = this._memoryCache;
        RawMemory._parsed = this._memoryCache;
        // Checkin
        var ticksSinceLestCheckIn = Game.time - this._lastCheckIn;
        if (ticksSinceLestCheckIn >= CPU_HALT_WHEN_NO_CHECKIN_FOR) {
          Game.cpu.halt && Game.cpu.halt();
        } else if (ticksSinceLestCheckIn >= CPU_HALT_WHEN_NO_CHECKIN_FOR - 1) {
          this.log("no checkin for ".concat(ticksSinceLestCheckIn, " ticks, halting cpu next tick..."));
        }
      }
    }, {
      key: "setupImports",
      value: function setupImports() {
        var _global$ScoreCollecto,
          _global$ScoreContaine,
          _this2 = this;
        _get(_getPrototypeOf(WorldBindings.prototype), "setupImports", this).call(this);
        this.bindingsImport['renew-object'] = this.js_renew_object.bind(this);
        this.bindingsImport['batch-renew-objects'] = this.js_batch_renew_objects.bind(this);
        this.bindingsImport['fetch-object-room-position'] = this.js_fetch_object_room_position.bind(this);
        this.bindingsImport['batch-fetch-object-room-positions'] = this.js_batch_fetch_object_room_positions.bind(this);
        this.bindingsImport['get-object-by-id'] = this.js_get_object_by_id.bind(this);
        this.bindingsImport['get-object-id'] = this.js_get_object_id.bind(this);
        var _global = global;
        var gameConstructors = {
          StructureContainer: StructureContainer,
          StructureController: StructureController,
          StructureExtension: StructureExtension,
          StructureExtractor: StructureExtractor,
          StructureFactory: StructureFactory,
          StructureInvaderCore: StructureInvaderCore,
          StructureKeeperLair: StructureKeeperLair,
          StructureLab: StructureLab,
          StructureLink: StructureLink,
          StructureNuker: StructureNuker,
          StructureObserver: StructureObserver,
          StructurePowerBank: StructurePowerBank,
          StructurePowerSpawn: StructurePowerSpawn,
          StructurePortal: StructurePortal,
          StructureRampart: StructureRampart,
          StructureRoad: StructureRoad,
          StructureSpawn: StructureSpawn,
          StructureStorage: StructureStorage,
          StructureTerminal: StructureTerminal,
          StructureTower: StructureTower,
          StructureWall: StructureWall,
          OwnedStructure: OwnedStructure,
          Structure: Structure,
          Source: Source,
          Mineral: Mineral,
          Deposit: Deposit,
          Creep: Creep,
          PowerCreep: PowerCreep,
          Flag: Flag,
          Resource: Resource,
          ConstructionSite: ConstructionSite,
          Tombstone: Tombstone,
          Ruin: Ruin,
          RoomObject: RoomObject,
          Room: Room,
          RoomVisual: RoomVisual,
          Nuke: Nuke,
          ScoreCollector: (_global$ScoreCollecto = _global.ScoreCollector) !== null && _global$ScoreCollecto !== void 0 ? _global$ScoreCollecto : function () {},
          ScoreContainer: (_global$ScoreContaine = _global.ScoreContainer) !== null && _global$ScoreContaine !== void 0 ? _global$ScoreContaine : function () {}
        };
        this.imports['object'] = {
          getConstructorOf: function getConstructorOf(x) {
            return Object.getPrototypeOf(x).constructor;
          },
          interpretDateTime: function interpretDateTime(x) {
            return x.getTime() / 1000;
          }
        };
        this.imports['game'] = {
          checkIn: function checkIn() {
            return _this2._lastCheckIn = Game.time;
          },
          getGameObj: function getGameObj() {
            return Game;
          },
          getMemoryObj: function getMemoryObj() {
            return _this2._memoryCache;
          },
          getConstantsObj: function getConstantsObj() {
            return global;
          },
          getRawMemoryObj: function getRawMemoryObj() {
            return RawMemory;
          },
          getPrototypes: function getPrototypes() {
            return gameConstructors;
          },
          createRoomPosition: function createRoomPosition(encodedInt) {
            var roomPosition = Object.create(RoomPosition.prototype);
            roomPosition.__packedPos = encodedInt;
            return roomPosition;
          },
          createCostMatrix: function createCostMatrix() {
            return new PathFinder.CostMatrix();
          },
          createRoomVisual: function createRoomVisual(roomName) {
            return new RoomVisual(roomName);
          },
          game: {
            getObjectById: function getObjectById(id) {
              return Game.getObjectById(id);
            },
            notify: function notify(message, groupInterval) {
              return Game.notify(message, groupInterval);
            }
          },
          interShardMemory: {
            getLocal: function getLocal() {
              return InterShardMemory.getLocal();
            },
            setLocal: function setLocal(value) {
              return InterShardMemory.setLocal(value);
            },
            getRemote: function getRemote(shard) {
              return InterShardMemory.getRemote(shard);
            }
          },
          map: {
            describeExits: function describeExits(roomName) {
              return Game.map.describeExits(roomName);
            },
            findExit: function findExit(fromRoom, toRoom, opts) {
              return Game.map.findExit(fromRoom, toRoom, opts);
            },
            findRoute: function findRoute(fromRoom, toRoom, opts) {
              return Game.map.findRoute(fromRoom, toRoom, opts);
            },
            getRoomLinearDistance: function getRoomLinearDistance(roomName1, roomName2, continuous) {
              return Game.map.getRoomLinearDistance(roomName1, roomName2, continuous);
            },
            getRoomTerrain: function getRoomTerrain(roomName) {
              return Game.map.getRoomTerrain(roomName);
            },
            getWorldSize: function getWorldSize() {
              return Game.map.getWorldSize();
            },
            getRoomStatus: function getRoomStatus(roomName) {
              return Game.map.getRoomStatus(roomName);
            },
            getRouteCallbackObject: function getRouteCallbackObject() {
              return _this2.routeCallback.bind(_this2);
            }
          },
          market: {
            calcTransactionCost: function calcTransactionCost(amount, roomName1, roomName2) {
              return Game.market.calcTransactionCost(amount, roomName1, roomName2);
            },
            cancelOrder: function cancelOrder(orderId) {
              return Game.market.cancelOrder(orderId);
            },
            changeOrderPrice: function changeOrderPrice(orderId, newPrice) {
              return Game.market.changeOrderPrice(orderId, newPrice);
            },
            createOrder: function createOrder(params) {
              return Game.market.createOrder(params);
            },
            deal: function deal(orderId, amount, targetRoomName) {
              return Game.market.deal(orderId, amount, targetRoomName);
            },
            extendOrder: function extendOrder(orderId, addAmount) {
              return Game.market.extendOrder(orderId, addAmount);
            },
            getAllOrders: function getAllOrders(filter) {
              return Game.market.getAllOrders(filter);
            },
            getHistory: function getHistory(resource) {
              var result = Game.market.getHistory(resource);
              return Array.isArray(result) ? result : [];
            },
            getOrderById: function getOrderById(id) {
              return Game.market.getOrderById(id);
            }
          },
          cpu: {
            getHeapStatistics: function getHeapStatistics() {
              return Game.cpu.getHeapStatistics && Game.cpu.getHeapStatistics();
            },
            getUsed: function getUsed() {
              return Game.cpu.getUsed();
            },
            halt: function halt() {
              return Game.cpu.halt && Game.cpu.halt();
            },
            setShardLimits: function setShardLimits(limits) {
              return Game.cpu.setShardLimits(limits);
            },
            unlock: function unlock() {
              return Game.cpu.unlock();
            },
            generatePixel: function generatePixel() {
              return Game.cpu.generatePixel();
            }
          },
          rawMemory: {
            get: function get() {
              return RawMemory.get();
            },
            set: function set(value) {
              return RawMemory.set(value);
            },
            setActiveSegments: function setActiveSegments(ids) {
              return RawMemory.setActiveSegments(ids);
            },
            setActiveForeignSegment: function setActiveForeignSegment(username, id) {
              return RawMemory.setActiveForeignSegment(username, id);
            },
            setDefaultPublicSegment: function setDefaultPublicSegment(id) {
              return RawMemory.setDefaultPublicSegment(id);
            },
            setPublicSegments: function setPublicSegments(ids) {
              return RawMemory.setPublicSegments(ids);
            }
          },
          visual: {
            line: function line(pos1, pos2, style) {
              return Game.map.visual.line(pos1, pos2, style);
            },
            circle: function circle(pos, style) {
              return Game.map.visual.circle(pos, style);
            },
            rect: function rect(topLeftPos, width, height, style) {
              return Game.map.visual.rect(topLeftPos, width, height, style);
            },
            poly: function poly(points, style) {
              return Game.map.visual.poly(points, style);
            },
            text: function text(_text, pos, style) {
              return Game.map.visual.text(_text, pos, style);
            },
            clear: function clear() {
              return Game.map.visual.clear();
            },
            getSize: function getSize() {
              return Game.map.visual.getSize();
            },
            "export": function _export() {
              return Game.map.visual["export"]();
            },
            "import": function _import(data) {
              return Game.map.visual["import"](data);
            }
          },
          powerCreep: {
            create: PowerCreep.create
          }
        };
        var wrappedPrototypes = this.buildWrappedPrototypes(gameConstructors);
        this.imports['game/prototypes/wrapped'] = _objectSpread2(_objectSpread2({}, wrappedPrototypes), {}, {
          Spawning: this.buildWrappedPrototype(StructureSpawn.Spawning),
          RoomObject: _objectSpread2(_objectSpread2({}, wrappedPrototypes.RoomObject), {}, {
            getStoreCapacity: function getStoreCapacity(thisObj, resourceType) {
              return thisObj.store.getCapacity(resourceType);
            },
            getStoreUsedCapacity: function getStoreUsedCapacity(thisObj, resourceType) {
              return thisObj.store.getUsedCapacity(resourceType);
            },
            getStoreFreeCapacity: function getStoreFreeCapacity(thisObj, resourceType) {
              return thisObj.store.getFreeCapacity(resourceType);
            },
            getStoreContainedResources: function getStoreContainedResources(thisObj) {
              return Object.keys(thisObj.store);
            },
            indexStore: function indexStore(thisObj, resourceType) {
              return thisObj.store[resourceType];
            }
          }),
          CostMatrix: _objectSpread2(_objectSpread2({}, this.buildWrappedPrototype(PathFinder.CostMatrix)), {}, {
            setRect: function setRect(thisObj, minX, minY, maxX, maxY, dataView) {
              var i = 0;
              for (var y = minY; y <= maxY; ++y) {
                for (var x = minX; x <= maxX; ++x) {
                  thisObj.set(x, y, dataView.getUint8(i));
                  ++i;
                }
              }
            }
          }),
          Room: _objectSpread2(_objectSpread2({}, wrappedPrototypes.Room), {}, {
            createFlag: function createFlag(thisObj, x, y, name, color, secondaryColor) {
              var result = thisObj.createFlag(x, y, name, color, secondaryColor);
              if (typeof result === 'string') {
                return {
                  name: result,
                  code: 0
                };
              } else {
                return {
                  code: result
                };
              }
            },
            findFast: function findFast(thisObj, type, outRoomObjectArrayPtr, maxObjectCount) {
              return _this2.encodeRoomObjectArray(_this2._memoryManager.view, thisObj.find(type), undefined, outRoomObjectArrayPtr, maxObjectCount);
            },
            lookAtFast: function lookAtFast(thisObj, x, y, outRoomObjectArrayPtr, maxObjectCount) {
              return _this2.encodeRoomObjectArray(_this2._memoryManager.view, thisObj.lookAt(x, y), undefined, outRoomObjectArrayPtr, maxObjectCount);
            },
            lookAtAreaFast: function lookAtAreaFast(thisObj, top, left, bottom, right, outRoomObjectArrayPtr, maxObjectCount) {
              return _this2.encodeRoomObjectArray(_this2._memoryManager.view, thisObj.lookAtArea(top, left, bottom, right, true), undefined, outRoomObjectArrayPtr, maxObjectCount);
            },
            lookForAtFast: function lookForAtFast(thisObj, type, x, y, outRoomObjectArrayPtr, maxObjectCount) {
              return _this2.encodeRoomObjectArray(_this2._memoryManager.view, thisObj.lookForAt(type, x, y), undefined, outRoomObjectArrayPtr, maxObjectCount);
            },
            lookForAtAreaFast: function lookForAtAreaFast(thisObj, type, top, left, bottom, right, outRoomObjectArrayPtr, maxObjectCount) {
              return _this2.encodeRoomObjectArray(_this2._memoryManager.view, thisObj.lookForAtArea(type, top, left, bottom, right, true), type, outRoomObjectArrayPtr, maxObjectCount);
            }
          }),
          Creep: _objectSpread2(_objectSpread2({}, wrappedPrototypes.Creep), {}, {
            getEncodedBody: function getEncodedBody(thisObj, outPtr) {
              return _this2.encodeCreepBody(_this2._memoryManager.view, thisObj.body, outPtr);
            }
          }),
          PathFinder: _objectSpread2(_objectSpread2({}, PathFinder), {}, {
            search: function search(origin, goal, opts) {
              return PathFinder.search(origin, goal, opts);
            },
            getRoomCallbackObject: function getRoomCallbackObject() {
              return _this2.roomCallback.bind(_this2);
            },
            getCostCallbackObject: function getCostCallbackObject() {
              return _this2.costCallback.bind(_this2);
            }
          }),
          RoomTerrain: {
            get: function get(thisObj, x, y) {
              return thisObj.get(x, y);
            },
            getRawBuffer: function getRawBuffer(thisObj, dataView) {
              return thisObj.getRawBuffer(new Uint8Array(dataView.buffer, dataView.byteOffset, dataView.byteLength));
            }
          }
        });
      }
    }, {
      key: "js_renew_object",
      value: function js_renew_object(jsHandle) {
        var oldObject = this._interop.getClrTrackedObject(jsHandle);
        if (oldObject == null) {
          return 1;
        } // clr tracked object not found (clr object disposed?)
        if (oldObject instanceof Room) {
          var newRoom = Game.rooms[oldObject.name];
          if (newRoom == null) {
            return 3;
          } // no longer exists (lost visibilty)
          this._interop.replaceClrTrackedObject(jsHandle, newRoom);
          return 0; // success
        }

        if (oldObject instanceof Flag) {
          var newFlag = Game.flags[oldObject.name];
          if (newFlag == null) {
            return 3;
          } // no longer exists (removed or lost visibilty)
          this._interop.replaceClrTrackedObject(jsHandle, newFlag);
          return 0; // success
        }

        var id = oldObject.id;
        if (id == null) {
          return 2;
        } // unrenewable (not a room object, e.g. unrelated js object)
        var newRoomObject = Game.getObjectById(id);
        if (newRoomObject == null) {
          return 3;
        } // no longer exists (destroyed or lost visibilty)
        this._interop.replaceClrTrackedObject(jsHandle, newRoomObject);
        return 0;
      }
    }, {
      key: "js_batch_renew_objects",
      value: function js_batch_renew_objects(jsHandleListPtr, count) {
        var i32 = this._memoryManager.view.i32;
        var baseIdx = jsHandleListPtr >> 2;
        var numSuccess = 0;
        for (var _i4 = 0; _i4 < count; ++_i4) {
          if (this.js_renew_object(i32[baseIdx + _i4]) === 0) {
            ++numSuccess;
          } else {
            i32[baseIdx + _i4] = -1;
          }
        }
        return numSuccess;
      }
    }, {
      key: "js_fetch_object_room_position",
      value: function js_fetch_object_room_position(jsHandle) {
        var roomObject = this._interop.getClrTrackedObject(jsHandle);
        var pos = roomObject.pos;
        if (pos == null) {
          return 0;
        }
        return pos.__packedPos;
      }
    }, {
      key: "js_batch_fetch_object_room_positions",
      value: function js_batch_fetch_object_room_positions(jsHandleListPtr, count, outRoomPosListPtr) {
        var i32 = this._memoryManager.view.i32;
        var baseJsHandleIdx = jsHandleListPtr >> 2;
        var baseOutRoomPostListIdx = outRoomPosListPtr >> 2;
        for (var _i5 = 0; _i5 < count; ++_i5) {
          i32[baseOutRoomPostListIdx + _i5] = this.js_fetch_object_room_position(i32[baseJsHandleIdx + _i5]);
        }
      }
    }, {
      key: "js_get_object_by_id",
      value: function js_get_object_by_id(objectIdPtr) {
        var _this$_interop$getClr;
        var u8 = this._memoryManager.view.u8;
        var id = '';
        for (var _i6 = 0; _i6 < 24; ++_i6) {
          var code = u8[objectIdPtr + _i6];
          if (code === 0) {
            break;
          }
          id += String.fromCharCode(code);
        }
        var obj = Game.getObjectById(id);
        if (obj == null) {
          //this.log(`js_get_object_by_id: failed to retrieve '${id}'`);
          return -1;
        }
        return (_this$_interop$getClr = this._interop.getClrTrackingId(obj)) !== null && _this$_interop$getClr !== void 0 ? _this$_interop$getClr : this._interop.assignClrTrackingId(obj);
      }
    }, {
      key: "js_get_object_id",
      value: function js_get_object_id(jsHandle, outRawObjectIdPtr) {
        var obj = this._interop.getClrTrackedObject(jsHandle);
        if (obj == null) {
          return 0;
        }
        var id = obj.id;
        if (typeof id !== 'string') {
          return 0;
        }
        this.copyRawObjectId(this._memoryManager.view, id, outRawObjectIdPtr);
        return id.length;
      }
    }, {
      key: "copyRawObjectId",
      value: function copyRawObjectId(memoryView, id, outPtr) {
        var u8 = memoryView.u8,
          i32 = memoryView.i32;
        if (id) {
          var l = id.length;
          for (var j = 0; j < l; ++j) {
            u8[outPtr + j] = id.charCodeAt(j);
          }
          for (var _j = l; _j < 24; ++_j) {
            u8[outPtr + _j] = 0;
          }
        } else {
          for (var _j2 = 0; _j2 < 6; ++_j2) {
            i32[outPtr + _j2] = 0;
          }
        }
      }
    }, {
      key: "encodeRoomObjectArray",
      value: function encodeRoomObjectArray(memoryView, arr, key, outRoomObjectArrayPtr, maxObjectCount) {
        var i32 = memoryView.i32;
        var numEncoded = 0;
        var nextRoomObjectArrayPtrI32 = outRoomObjectArrayPtr >> 2;
        for (var _i7 = 0; _i7 < Math.min(maxObjectCount, arr.length); ++_i7) {
          // Lookup object
          var obj = arr[_i7];
          if (key) {
            obj = obj[key];
          }
          if (!(obj instanceof RoomObject) && obj.type) {
            obj = obj[obj.type];
          }
          if (!(obj instanceof RoomObject)) {
            continue;
          }
          // Copy metadata
          i32[nextRoomObjectArrayPtrI32++] = Object.getPrototypeOf(obj).constructor.__dotnet_typeId || 0;
          i32[nextRoomObjectArrayPtrI32++] = this._interop.getOrAssignClrTrackingId(obj);
          ++numEncoded;
        }
        return numEncoded;
      }
    }, {
      key: "encodeCreepBody",
      value: function encodeCreepBody(memoryView, body, outPtr) {
        var i32 = memoryView.i32;
        for (var _i8 = 0; _i8 < body.length; ++_i8) {
          var _body$_i = body[_i8],
            type = _body$_i.type,
            hits = _body$_i.hits,
            boost = _body$_i.boost;
          // Encode each body part to a 32 bit int as 4 bytes
          // unused: b3
          // type: 0-8 (4 bits 0-15) b2
          // hits: 0-100 (7 bits 0-127) b1
          // boost: null or 0-85 (7 bits 0-127, 127 means null) b0
          var encodedBodyPart = 0;
          encodedBodyPart |= BODYPART_TO_ENUM_MAP[type] << 16;
          encodedBodyPart |= hits << 8;
          encodedBodyPart |= boost == null ? 127 : RESOURCE_TO_ENUM_MAP[boost];
          i32[outPtr >> 2] = encodedBodyPart;
          outPtr += 4;
        }
        return body.length;
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
                for (var _len2 = arguments.length, args = new Array(_len2 > 1 ? _len2 - 1 : 0), _key2 = 1; _key2 < _len2; _key2++) {
                  args[_key2 - 1] = arguments[_key2];
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
    }, {
      key: "parseRoomName",
      value: function parseRoomName(roomName, outCoord) {
        var xx = parseInt(roomName.substr(1), 10);
        var verticalPos = 2;
        if (xx >= 100) {
          verticalPos = 4;
        } else if (xx >= 10) {
          verticalPos = 3;
        }
        var yy = parseInt(roomName.substr(verticalPos + 1), 10);
        var horizontalDir = roomName.charAt(0);
        var verticalDir = roomName.charAt(verticalPos);
        if (horizontalDir === 'W' || horizontalDir === 'w') {
          xx = -xx - 1;
        }
        if (verticalDir === 'N' || verticalDir === 'n') {
          yy = -yy - 1;
        }
        if (outCoord) {
          outCoord[0] = xx;
          outCoord[1] = yy;
        } else {
          outCoord = [xx, yy];
        }
        return outCoord;
      }
    }, {
      key: "roomCallback",
      value: function roomCallback(roomName) {
        var roomCoord = this.parseRoomName(roomName, TEMP_ROOM_COORD_A);
        var result = this._invoke_room_callback(roomCoord[0], roomCoord[1]);
        if (result < 0) {
          return false;
        }
        if (result === 0) {
          return undefined;
        }
        return this._interop.getClrTrackedObject(result);
      }
    }, {
      key: "costCallback",
      value: function costCallback(roomName, costMatrix) {
        var roomCoord = this.parseRoomName(roomName, TEMP_ROOM_COORD_A);
        var result = this._invoke_cost_callback(roomCoord[0], roomCoord[1], this._interop.getOrAssignClrTrackingId(costMatrix));
        if (result === 0) {
          return undefined;
        }
        return this._interop.getClrTrackedObject(result);
      }
    }, {
      key: "routeCallback",
      value: function routeCallback(roomName, fromRoomName) {
        var roomCoord = this.parseRoomName(roomName, TEMP_ROOM_COORD_A);
        var fromRoomCoord = this.parseRoomName(fromRoomName, TEMP_ROOM_COORD_B);
        return this._invoke_route_callback(roomCoord[0], roomCoord[1], fromRoomCoord[0], fromRoomCoord[1]);
      }
    }, {
      key: "accountClrTrackedObjects",
      value: function accountClrTrackedObjects() {
        var counts = {};
        var totalCount = 0;
        this._interop.visitClrTrackedObjects(function (x) {
          var _counts$name;
          var name;
          if (x instanceof Creep) {
            name = 'Creep';
          } else if (x instanceof Structure) {
            name = 'Structure';
          } else if (x instanceof RoomObject) {
            name = 'RoomObject';
          } else if (x instanceof Room) {
            name = 'Room';
          } else if (x instanceof RoomPosition) {
            name = 'RoomPosition';
          } else if (x instanceof global.Store) {
            name = 'Store';
          } else if (x instanceof StructureSpawn.Spawning) {
            name = 'StructureSpawn.Spawning';
          } else if (x == global) {
            name = 'global';
          } else if (x == Game) {
            name = 'Game';
          } else if (x == Game.creeps) {
            name = 'Game.creeps';
          } else if (x == Game.structures) {
            name = 'Game.structures';
          } else if (x == Game.spawns) {
            name = 'Game.spawns';
          } else if (x == Game.rooms) {
            name = 'Game.rooms';
          } else if (x == Game.cpu) {
            name = 'Game.cpu';
          } else if (x == Game.map) {
            name = 'Game.map';
          } else if (Array.isArray(x)) {
            name = 'Array';
          } else {
            if (_typeof(x) === 'object' && x != null) {
              var keys = Object.keys(x);
              name = "unknown(".concat(keys.join(','), ")");
            } else {
              name = 'unknown';
            }
          }
          counts[name] = ((_counts$name = counts[name]) !== null && _counts$name !== void 0 ? _counts$name : 0) + 1;
          ++totalCount;
        });
        var lines = ["total: ".concat(totalCount)];
        for (var name in counts) {
          lines.push("".concat(name, ": ").concat(counts[name]));
        }
        this.log(lines.join('<br />'));
      }
    }]);
    return WorldBindings;
  }(BaseBindings);

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
      value: function write(view, buf, buf_len) {
        var buffer = view.u8.slice(buf, buf + buf_len);
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
      _defineProperty(this, "_memoryManager", void 0);
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
      switch (env) {
        case 'world':
          this._bindings = new WorldBindings(this.log.bind(this), this._interop);
          break;
        case 'arena':
          this._bindings = new ArenaBindings(this.log.bind(this), this._interop);
          break;
      }
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
        var dataView = this._memoryManager.view.dataView;
        dataView.setBigUint64(time_ptr, BigInt(new Date().getTime()) * 1000000n, true);
      }
    }, {
      key: "sys_get_random",
      value: function sys_get_random(buf, buf_len) {
        var _this$_memoryManager$ = this._memoryManager.view,
          u32 = _this$_memoryManager$.u32,
          u8 = _this$_memoryManager$.u8;
        while (buf_len >= 4) {
          u32[buf >> 2] = Math.random() * (1 << 32);
          buf += 4;
          buf_len -= 4;
        }
        while (buf_len > 0) {
          u8[buf] = Math.random() * (1 << 8);
          ++buf;
          --buf_len;
        }
      }
    }, {
      key: "sys_write_stderr",
      value: function sys_write_stderr(buf, buf_len) {
        if (!this._memoryManager) {
          return;
        }
        this._stderr.write(this._memoryManager.view, buf, buf_len);
      }
    }, {
      key: "sys_write_stdout",
      value: function sys_write_stdout(buf, buf_len) {
        if (!this._memoryManager) {
          return;
        }
        this._stdout.write(this._memoryManager.view, buf, buf_len);
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
        this._memoryManager = new WasmMemoryManager(this._wasmInstance.exports.memory);
        this._interop.memoryManager = this._memoryManager;
        this._interop.malloc = this._wasmInstance.exports.malloc;
        this._compiled = true;
      }
    }, {
      key: "start",
      value: function start(customInitExportNames) {
        var _this$_bindings;
        if (!this._wasmInstance || !this._compiled || this._started || !this._memoryManager) {
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
        (_this$_bindings = this._bindings) === null || _this$_bindings === void 0 || _this$_bindings.init(this._wasmInstance.exports, this._memoryManager);
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

module.exports = bootloader;
