import { useMediaQuery } from "react-responsive";

export function useIsSmallScreen(): boolean {
  return useMediaQuery({ maxWidth: 576 });
}
