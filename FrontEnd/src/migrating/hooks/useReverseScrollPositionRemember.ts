// Not used now!!! But preserved for future use.

import { useEffect } from "react";

let on = false;

let rememberedReversePosition = getReverseScrollPosition();

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

  const old = document.documentElement.style.scrollBehavior;
  document.documentElement.style.scrollBehavior = "auto";

  const newPosition =
    document.documentElement.scrollHeight -
    window.innerHeight -
    reversePosition;

  window.scrollTo(0, newPosition);

  document.documentElement.style.scrollBehavior = old;
}

const scrollListener = (): void => {
  rememberedReversePosition = getReverseScrollPosition();
};

const resizeObserver = new ResizeObserver(() => {
  scrollToReverseScrollPosition(rememberedReversePosition);
});

export default function useReverseScrollPositionRemember(): void {
  useEffect(() => {
    if (on) return;
    on = true;
    window.addEventListener("scroll", scrollListener);
    resizeObserver.observe(document.documentElement);

    return () => {
      resizeObserver.disconnect();
      window.removeEventListener("scroll", scrollListener);
      on = false;
    };
  }, []);
}
