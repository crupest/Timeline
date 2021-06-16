export default function refreshAnimation(): void {
  document.body.querySelectorAll("*").forEach((e) => {
    if (e instanceof HTMLElement) {
      const an = getComputedStyle(e).animationName;
      if (an !== "none") {
        e.style.animationName = "none";
        setTimeout(() => {
          e.style.animationName = an;
        });
      }
    }
  });
}
