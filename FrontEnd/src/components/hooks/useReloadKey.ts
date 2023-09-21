import { useState } from "react";

export default function useReloadKey(): [
  key: number | string,
  reload: () => void,
] {
  const [key, setKey] = useState(0);
  return [key, () => setKey((k) => k + 1)];
}
