import React from "react";
import { fromEvent } from "rxjs";
import { filter, throttleTime } from "rxjs/operators";

function useScrollToTop(
  handler: () => void,
  enable = true,
  option = {
    maxOffset: 50,
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
        filter(() => {
          return window.scrollY <= option.maxOffset;
        }),
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

export default useScrollToTop;
