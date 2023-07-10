import { useRef, useEffect } from "react";

export default function useClickOutside(
  element: HTMLElement | null | undefined,
  onClickOutside: () => void,
  nextTick?: boolean
): void {
  const onClickOutsideRef = useRef<() => void>(onClickOutside);

  useEffect(() => {
    onClickOutsideRef.current = onClickOutside;
  }, [onClickOutside]);

  useEffect(() => {
    if (element != null) {
      const handler = (event: MouseEvent): void => {
        let e: HTMLElement | null = event.target as HTMLElement;
        while (e) {
          if (e == element) {
            return;
          }
          e = e.parentElement;
        }
        onClickOutsideRef.current();
      };
      if (nextTick) {
        setTimeout(() => {
          document.addEventListener("click", handler);
        });
      } else {
        document.addEventListener("click", handler);
      }
      return () => {
        document.removeEventListener("click", handler);
      };
    }
  }, [element, nextTick]);
}
