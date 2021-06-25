import { convertI18nText, I18nText } from "@/common";
import React from "react";
import { useTranslation } from "react-i18next";

import Button from "./button/Button";

const ConfirmDialog: React.FC<{
  onClose: () => void;
  onConfirm: () => void;
  title: I18nText;
  body: I18nText;
}> = ({ onClose, onConfirm, title, body }) => {
  const { t } = useTranslation();

  return (
    <Modal onHide={onClose} show centered>
      <Modal.Header>
        <Modal.Title className="text-danger">
          {convertI18nText(title, t)}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{convertI18nText(body, t)}</Modal.Body>
      <Modal.Footer>
        <Button
          text="operationDialog.cancel"
          color="secondary"
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
      </Modal.Footer>
    </Modal>
  );
};

export default ConfirmDialog;
