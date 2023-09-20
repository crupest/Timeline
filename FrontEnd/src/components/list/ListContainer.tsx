import { ComponentPropsWithoutRef, forwardRef, Ref } from "react";
import classNames from "classnames";

import "./ListContainer.css";

function _ListContainer(
  { className, children, ...otherProps }: ComponentPropsWithoutRef<"div">,
  ref: Ref<HTMLDivElement>,
) {
  return (
    <div
      ref={ref}
      className={classNames("cru-list-container", className)}
      {...otherProps}
    >
      {children}
    </div>
  );
}

const ListContainer = forwardRef(_ListContainer);

export default ListContainer;
