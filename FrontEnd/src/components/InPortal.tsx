import { ReactNode } from "react";
import ReactDOM from "react-dom";

const optionalPortalElement = window.document.getElementById("portal");
if (optionalPortalElement == null) {
  throw Error("No portal element found.");
}

const portalElement = optionalPortalElement;

export default function InPortal({ children }: { children: ReactNode }) {
  return ReactDOM.createPortal(<>{children}</>, portalElement);
}

