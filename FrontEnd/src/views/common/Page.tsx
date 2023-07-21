import { ComponentPropsWithoutRef, Ref } from "react";
import classNames from "classnames";

interface PageProps extends ComponentPropsWithoutRef<"div"> {
  noTopPadding?: boolean;
  pageRef?: Ref<HTMLDivElement>;
}

export default function Page({ noTopPadding, pageRef, className, children }: PageProps) {
  return (
    <div ref={pageRef} className={classNames(className, "cru-page", noTopPadding && "cru-page-no-top-padding")}>
      {children}
    </div>
  );
}
