import React from "react";
import { Modal, Button } from "react-bootstrap";
import { useTranslation } from "react-i18next";

const TimelinePostDeleteConfirmDialog: React.FC<{
  onClose: () => void;
  onConfirm: () => void;
}> = ({ onClose, onConfirm }) => {
  const { t } = useTranslation();

  return (
    <Modal onHide={onClose} show centered>
      <Modal.Header>
        <Modal.Title className="text-danger">
          {t("timeline.post.deleteDialog.title")}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{t("timeline.post.deleteDialog.prompt")}</Modal.Body>
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

export default TimelinePostDeleteConfirmDialog;
