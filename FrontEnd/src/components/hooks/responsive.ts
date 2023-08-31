import { useMediaQuery } from "react-responsive";

import { breakpoints } from "../breakpoints";

export function useMobile(onChange?: (mobile: boolean) => void): boolean {
  return useMediaQuery({ maxWidth: breakpoints.sm }, undefined, onChange);
}
