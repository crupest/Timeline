import { useRef, useEffect } from "react";
import { fromEvent, filter, throttleTime } from "rxjs";

function useScrollToBottom(
  handler: () => void,
  enable = true,
  option = {
    maxOffset: 5,
    throttle: 1000,
  },
): void {
  const handlerRef = useRef<(() => void) | null>(null);

  useEffect(() => {
    handlerRef.current = handler;

    return () => {
      handlerRef.current = null;
    };
  }, [handler]);

  useEffect(() => {
    const subscription = fromEvent(window, "scroll")
      .pipe(
        filter(
          () =>
            window.scrollY >=
            document.body.scrollHeight - window.innerHeight - option.maxOffset,
        ),
        throttleTime(option.throttle),
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
