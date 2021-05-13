import React from "react";

let on = false;

export function getReverseScrollPosition(): number {
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

export function scrollToReverseScrollPosition(reversePosition: number): void {
  if (document.documentElement.scrollHeight <= window.innerHeight) return;

  window.scrollTo(
    0,
    document.documentElement.scrollHeight - window.innerHeight - reversePosition
  );
}

let scrollPosition = getReverseScrollPosition();

const scrollListener = (): void => {
  scrollPosition = getReverseScrollPosition();
};

const resizeObserver = new ResizeObserver(() => {
  scrollToReverseScrollPosition(scrollPosition);
});

export default function useReverseScrollPositionRemember(): void {
  React.useEffect(() => {
    if (on) return;
    on = true;
    window.addEventListener("scroll", scrollListener);
    resizeObserver.observe(document.documentElement);

    return () => {
      window.removeEventListener("scroll", scrollListener);
      resizeObserver.disconnect();
      on = false;
    };
  }, []);
}
