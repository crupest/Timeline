import { Base64 } from "js-base64";

export function base64(blob: Blob | string): Promise<string> {
  if (typeof blob === "string") {
    return Promise.resolve(Base64.encode(blob));
  }

  return new Promise<string>((resolve) => {
    const reader = new FileReader();
    reader.onload = function () {
      resolve((reader.result as string).replace(/^data:.*;base64,/, ""));
    };
    reader.readAsDataURL(blob);
  });
}
