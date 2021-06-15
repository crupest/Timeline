import { convertI18nText, I18nText } from "@/common";
import React from "react";
import { Modal, Button } from "react-bootstrap";
import { useTranslation } from "react-i18next";

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
        <Button variant="secondary" onClick={onClose}>
          {t("operationDialog.cancel")}
        </Button>
        <Button
          variant="danger"
          onClick={() => {
            onConfirm();
            onClose();
          }}
        >
          {t("operationDialog.confirm")}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default ConfirmDialog;
