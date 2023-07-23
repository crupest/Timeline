function bytesToBase64(bytes: Uint8Array): string {
  const binString = Array.from(bytes, (x) => String.fromCodePoint(x)).join("");
  return btoa(binString);
}

export default function base64(
  data: Blob | Uint8Array | string,
): Promise<string> {
  if (typeof data === "string") {
    // From https://developer.mozilla.org/en-US/docs/Glossary/Base64#the_unicode_problem
    const binString = new TextEncoder().encode(data);
    return Promise.resolve(bytesToBase64(binString));
  }

  if (data instanceof Uint8Array) {
    return Promise.resolve(bytesToBase64(data));
  }

  return new Promise<string>((resolve) => {
    const reader = new FileReader();
    reader.onload = function () {
      resolve((reader.result as string).replace(/^data:.*;base64,/, ""));
    };
    reader.readAsDataURL(data);
  });
}
