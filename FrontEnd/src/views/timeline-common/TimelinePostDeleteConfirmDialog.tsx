import React from "react";
import { useTranslation } from "react-i18next";

import Button from "../common/button/Button";

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
        <Button
          text="operationDialog.cancel"
          color="secondary"
          onClick={onClose}
        />
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
