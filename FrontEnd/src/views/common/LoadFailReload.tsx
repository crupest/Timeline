import React from "react";
import { Trans } from "react-i18next";

export interface LoadFailReloadProps {
  className?: string;
  style?: React.CSSProperties;
  onReload: () => void;
}

const LoadFailReload: React.FC<LoadFailReloadProps> = ({
  onReload,
  className,
  style,
}) => {
  return (
    <Trans
      i18nKey="loadFailReload"
      parent="div"
      className={className}
      style={style}
    >
      0
      <a
        href="#"
        onClick={(e) => {
          onReload();
          e.preventDefault();
        }}
      >
        1
      </a>
      2
    </Trans>
  );
};

export default LoadFailReload;
