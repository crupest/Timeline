import { ComponentPropsWithoutRef, forwardRef, Ref } from "react";
import classNames from "classnames";

import "./ListItemContainer.css";

function _ListItemContainer(
  { className, children, ...otherProps }: ComponentPropsWithoutRef<"div">,
  ref: Ref<HTMLDivElement>,
) {
  return (
    <div
      ref={ref}
      className={classNames("cru-list-item-container", className)}
      {...otherProps}
    >
      {children}
    </div>
  );
}

const ListItemContainer = forwardRef(_ListItemContainer);

export default ListItemContainer;
