import React from "react";

let on = false;
let recordDisabled = false;

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

function scrollToRecordPosition(): void {
  if (document.documentElement.scrollHeight <= window.innerHeight) return;
  document.documentElement.scrollTop =
    document.documentElement.scrollHeight - window.innerHeight - scrollPosition;
}

const scrollListener = (): void => {
  if (recordDisabled) return;
  scrollPosition = getScrollPosition();
};

const resizeObserver = new ResizeObserver(() => {
  scrollToRecordPosition();
});

export function setRecordDisabled(disabled: boolean): void {
  recordDisabled = disabled;
  if (!disabled) scrollToRecordPosition();
}

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
