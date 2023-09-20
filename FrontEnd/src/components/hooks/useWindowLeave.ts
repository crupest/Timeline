import { useEffect } from "react";

import { useC, Text } from "../common";

export default function useWindowLeave(
  allow: boolean,
  message: Text = "timeline.confirmLeave",
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
