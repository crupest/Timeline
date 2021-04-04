import React from "react";

let on = false;

export default function useReverseScrollPositionRemember(): void {
  React.useEffect(() => {
    if (on) return;
    on = true;

    let scrollPosition =
      document.documentElement.scrollHeight -
      document.documentElement.scrollTop;

    const scrollListener = (): void => {
      scrollPosition = document.documentElement.scrollHeight - window.scrollY;
    };

    window.addEventListener("scroll", scrollListener);

    const resizeObserver = new ResizeObserver(() => {
      document.documentElement.scrollTop =
        document.documentElement.scrollHeight - scrollPosition;
    });

    resizeObserver.observe(document.documentElement);

    return () => {
      window.removeEventListener("scroll", scrollListener);
      resizeObserver.disconnect();
      on = false;
    };
  }, []);
}
