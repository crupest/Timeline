import React from "react";
import classnames from "classnames";
import range from "lodash/range";

import "./Skeleton.css";

export interface SkeletonProps {
  lineNumber?: number;
  className?: string;
  style?: React.CSSProperties;
}

const Skeleton: React.FC<SkeletonProps> = (props) => {
  const { lineNumber: lineNumberProps, className, style } = props;
  const lineNumber = lineNumberProps ?? 3;

  return (
    <div className={classnames(className, "cru-skeleton")} style={style}>
      {range(lineNumber).map((i) => (
        <div
          key={i}
          className={classnames(
            "cru-skeleton-line",
            i === lineNumber - 1 && "last"
          )}
        />
      ))}
    </div>
  );
};

export default Skeleton;
