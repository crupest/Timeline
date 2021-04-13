import React from "react";

let on = false;

export default function useReverseScrollPositionRemember(): void {
  React.useEffect(() => {
    if (on) return;
    on = true;

    function getScrollPosition(): number {
      if (document.documentElement.scrollHeight <= window.innerHeight) {
        return 0;
      } else {
        return (
          document.documentElement.scrollHeight -
          document.documentElement.scrollTop -
          window.innerHeight
        );
      }
    }

    let scrollPosition = getScrollPosition();
    const scrollListener = (): void => {
      scrollPosition = getScrollPosition();
    };

    window.addEventListener("scroll", scrollListener);

    const resizeObserver = new ResizeObserver(() => {
      if (document.documentElement.scrollHeight <= window.innerHeight) return;
      document.documentElement.scrollTop =
        document.documentElement.scrollHeight -
        window.innerHeight -
        scrollPosition;
    });

    resizeObserver.observe(document.documentElement);

    return () => {
      window.removeEventListener("scroll", scrollListener);
      resizeObserver.disconnect();
      on = false;
    };
  }, []);
}
