import React, { useState } from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Container, Form, Row, Col, Button, Modal } from "react-bootstrap";

import { useUser, userService } from "@/services/user";
import OperationDialog from "../common/OperationDialog";

interface ChangePasswordDialogProps {
  open: boolean;
  close: () => void;
}

const ChangePasswordDialog: React.FC<ChangePasswordDialogProps> = (props) => {
  const history = useHistory();

  const [redirect, setRedirect] = useState<boolean>(false);

  return (
    <OperationDialog
      open={props.open}
      title="settings.dialogChangePassword.title"
      themeColor="danger"
      inputPrompt="settings.dialogChangePassword.prompt"
      inputScheme={[
        {
          type: "text",
          label: "settings.dialogChangePassword.inputOldPassword",
          password: true,
        },
        {
          type: "text",
          label: "settings.dialogChangePassword.inputNewPassword",
          password: true,
        },
        {
          type: "text",
          label: "settings.dialogChangePassword.inputRetypeNewPassword",
          password: true,
        },
      ]}
      inputValidator={([oldPassword, newPassword, retypedNewPassword]) => {
        const result: Record<number, string> = {};
        if (oldPassword === "") {
          result[0] = "settings.dialogChangePassword.errorEmptyOldPassword";
        }
        if (newPassword === "") {
          result[1] = "settings.dialogChangePassword.errorEmptyNewPassword";
        }
        if (retypedNewPassword !== newPassword) {
          result[2] = "settings.dialogChangePassword.errorRetypeNotMatch";
        }
        return result;
      }}
      onProcess={async ([oldPassword, newPassword]) => {
        await userService.changePassword(oldPassword, newPassword);
        setRedirect(true);
      }}
      close={() => {
        props.close();
        if (redirect) {
          history.push("/login");
        }
      }}
    />
  );
};

const ConfirmLogoutDialog: React.FC<{
  onClose: () => void;
  onConfirm: () => void;
}> = ({ onClose, onConfirm }) => {
  const { t } = useTranslation();

  return (
    <Modal show centered onHide={onClose}>
      <Modal.Header>
        <Modal.Title className="text-danger">
          {t("settings.dialogConfirmLogout.title")}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{t("settings.dialogConfirmLogout.prompt")}</Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onClose}>
          {t("operationDialog.cancel")}
        </Button>
        <Button variant="danger" onClick={onConfirm}>
          {t("operationDialog.confirm")}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

const SettingsPage: React.FC = (_) => {
  const { i18n, t } = useTranslation();
  const user = useUser();
  const history = useHistory();

  const [dialog, setDialog] = useState<null | "changepassword" | "logout">(
    null
  );

  const language = i18n.language.slice(0, 2);

  return (
    <>
      <Container>
        {user ? (
          <div className="cru-card my-3 py-3">
            <h3 className="px-3 mb-3 text-primary">
              {t("settings.subheaders.account")}
            </h3>
            <div
              className="settings-item clickable first"
              onClick={() => {
                history.push(`/users/${user.username}`);
              }}
            >
              {t("settings.gotoSelf")}
            </div>
            <div
              className="settings-item clickable text-danger"
              onClick={() => setDialog("changepassword")}
            >
              {t("settings.changePassword")}
            </div>
            <div
              className="settings-item clickable text-danger"
              onClick={() => {
                setDialog("logout");
              }}
            >
              {t("settings.logout")}
            </div>
          </div>
        ) : null}
        <div className="cru-card my-3 py-3">
          <h3 className="px-3 mb-3 text-primary">
            {t("settings.subheaders.customization")}
          </h3>
          <Row className="settings-item first mx-0">
            <Col xs="12" sm="auto">
              <div>{t("settings.languagePrimary")}</div>
              <small className="d-block text-secondary">
                {t("settings.languageSecondary")}
              </small>
            </Col>
            <Col xs="auto" className="ml-auto">
              <Form.Control
                as="select"
                value={language}
                onChange={(e) => {
                  void i18n.changeLanguage(e.target.value);
                }}
              >
                <option value="zh">中文</option>
                <option value="en">English</option>
              </Form.Control>
            </Col>
          </Row>
        </div>
      </Container>
      {(() => {
        switch (dialog) {
          case "changepassword":
            return (
              <ChangePasswordDialog
                open
                close={() => {
                  setDialog(null);
                }}
              />
            );
          case "logout":
            return (
              <ConfirmLogoutDialog
                onClose={() => setDialog(null)}
                onConfirm={() => {
                  void userService.logout().then(() => {
                    history.push("/");
                  });
                }}
              />
            );
          default:
            return null;
        }
      })()}
    </>
  );
};

export default SettingsPage;
