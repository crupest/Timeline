import React from "react";

let on = false;

let reverseScrollPosition = getReverseScrollPosition();
let reverseScrollToPosition: number | null = null;
let lastScrollPosition = window.scrollY;

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

  reverseScrollToPosition = newPosition;

  window.scrollTo(0, newPosition);

  document.documentElement.style.scrollBehavior = old;
}

const scrollListener = (): void => {
  if (
    reverseScrollToPosition != null &&
    Math.abs(window.scrollY - reverseScrollToPosition) > 50
  ) {
    console.log(
      `Reverse scroll position coerce. Required: ${reverseScrollToPosition}. Actual: ${window.scrollY}.`
    );
    scrollToReverseScrollPosition(reverseScrollPosition);
    return;
  }
  if (
    reverseScrollToPosition == null &&
    Math.abs(window.scrollY - lastScrollPosition) > 1000
  ) {
    console.log(
      `Scroll jump detected. New: ${window.scrollY}. Old: ${lastScrollPosition}.`
    );
    scrollToReverseScrollPosition(reverseScrollPosition);
    return;
  }

  reverseScrollToPosition = null;
  lastScrollPosition = window.scrollY;
  reverseScrollPosition = getReverseScrollPosition();
};

const resizeObserver = new ResizeObserver(() => {
  scrollToReverseScrollPosition(reverseScrollPosition);
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
