import React from "react";
import { fromEvent } from "rxjs";
import { filter, throttleTime } from "rxjs/operators";

function useScrollToBottom(
  handler: () => void,
  enable = true,
  option = {
    maxOffset: 5,
    throttle: 1000,
  }
): void {
  const handlerRef = React.useRef<(() => void) | null>(null);

  React.useEffect(() => {
    handlerRef.current = handler;

    return () => {
      handlerRef.current = null;
    };
  }, [handler]);

  React.useEffect(() => {
    const subscription = fromEvent(window, "scroll")
      .pipe(
        filter(
          () =>
            window.scrollY >=
            document.body.scrollHeight - window.innerHeight - option.maxOffset
        ),
        throttleTime(option.throttle)
      )
      .subscribe(() => {
        if (enable) {
          handlerRef.current?.();
        }
      });

    return () => {
      subscription.unsubscribe();
    };
  }, [enable, option.maxOffset, option.throttle]);
}

export default useScrollToBottom;