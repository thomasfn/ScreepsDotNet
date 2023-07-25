import { TextEncoder, TextDecoder } from 'fastestsmallesttextencoderdecoder-encodeinto';

global.TextDecoder = global.TextEncoder || TextEncoder;
global.TextEncoder = global.TextDecoder || TextDecoder;
