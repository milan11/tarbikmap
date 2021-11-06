export const sizeof_uint16 = 2;
export const sizeof_uint32 = 4;

export function concatArrays(...arrays: Uint8Array[]) {
  let totalLength = 0;
  for (let i = 0; i < arrays.length; ++i) {
    totalLength += arrays[i].length;
  }

  var result = new Uint8Array(totalLength);

  let currentLength = 0;
  for (let i = 0; i < arrays.length; ++i) {
    result.set(arrays[i], currentLength);

    currentLength += arrays[i].length;
  }

  return result;
}

export function byteArrayToInt_be(byteArray: Uint8Array, offset: number, length: number) {
  let value = 0;
  for (let i = 0; i < length; ++i) {
    value = value * 256 + byteArray[offset + i];
  }

  return value;
}

export function byteArrayToInt_le(byteArray: Uint8Array, offset: number, length: number) {
  let value = 0;
  for (let i = length - 1; i >= 0; --i) {
    value = value * 256 + byteArray[offset + i];
  }

  return value;
}

export function numToByteArray(number: number, length: number) {
  const result = new Uint8Array(length);

  for (let i = 0; i < length; ++i) {
    result[length - i - 1] = number % 256;
    number /= 256;
  }

  return result;
}
