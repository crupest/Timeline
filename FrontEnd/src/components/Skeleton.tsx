import { ComponentPropsWithoutRef } from "react";
import classNames from "classnames";

import { range } from "~src/utilities";

import "./Skeleton.css";

interface SkeletonProps extends ComponentPropsWithoutRef<"div"> {
  lineNumber?: number;
}

export default function Skeleton(props: SkeletonProps) {
  const { lineNumber, className, ...otherProps } = props;

  return (
    <div className={classNames(className, "cru-skeleton")} {...otherProps}>
      {range(lineNumber ?? 3).map((i) => (
        <div key={i} className="cru-skeleton-line" />
      ))}
    </div>
  );
}
