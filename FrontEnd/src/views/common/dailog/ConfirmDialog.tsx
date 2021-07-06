import { convertI18nText, I18nText } from "@/common";
import React from "react";
import { useTranslation } from "react-i18next";

import Button from "../button/Button";
import Dialog from "./Dialog";

const ConfirmDialog: React.FC<{
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: I18nText;
  body: I18nText;
}> = ({ open, onClose, onConfirm, title, body }) => {
  const { t } = useTranslation();

  return (
    <Dialog onClose={onClose} open={open}>
      <h3 className="cru-color-danger">{convertI18nText(title, t)}</h3>
      <hr />
      <p>{convertI18nText(body, t)}</p>
      <hr />
      <div className="cru-dialog-bottom-area">
        <Button
          text="operationDialog.cancel"
          color="secondary"
          outline
          onClick={onClose}
        />
        <Button
          text="operationDialog.confirm"
          color="danger"
          onClick={() => {
            onConfirm();
            onClose();
          }}
        />
      </div>
    </Dialog>
  );
};

export default ConfirmDialog;
