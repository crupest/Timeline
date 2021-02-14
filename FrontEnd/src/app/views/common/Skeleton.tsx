import React from "react";
import clsx from "clsx";
import { range } from "lodash";

export interface SkeletonProps {
  lineNumber?: number;
  className?: string;
  style?: React.CSSProperties;
}

const Skeleton: React.FC<SkeletonProps> = (props) => {
  const { lineNumber: lineNumberProps, className, style } = props;
  const lineNumber = lineNumberProps ?? 3;

  return (
    <div className={clsx(className, "cru-skeleton")} style={style}>
      {range(lineNumber).map((i) => (
        <div key={i} className="cru-skeleton-line" />
      ))}
    </div>
  );
};

export default Skeleton;
