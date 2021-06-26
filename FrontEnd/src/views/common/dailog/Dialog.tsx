import React from "react";

export interface DialogProps {
  onClose: () => void;
  open?: boolean;
  children?: React.ReactNode;
}

export default function Dialog(props: DialogProps): React.ReactElement | null {
  const { open, onClose, children } = props;

  return <div>{children}</div>;
}
