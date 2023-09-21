import { useEffect } from "react";

import { useC, I18nText } from "../common";

export default function useWindowLeave(
  allow: boolean,
  message: I18nText = "timeline.confirmLeave",
) {
  const c = useC();

  useEffect(() => {
    if (!allow) {
      window.onbeforeunload = () => {
        return c(message);
      };

      return () => {
        window.onbeforeunload = null;
      };
    }
  }, [c, allow, message]);
}
