// TODO: Migrate hooks

export {
  useIsSmallScreen,
  useClickOutside,
  useScrollToBottom,
} from "@/utilities/hooks";

import { useMediaQuery } from "react-responsive";
import { breakpoints } from "./breakpoints";

export function useMobile(): boolean {
  return useMediaQuery({ maxWidth: breakpoints.sm });
}
