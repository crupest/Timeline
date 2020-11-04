import React, { useState } from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Form, Container, Row, Col, Button, Modal } from "react-bootstrap";

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
      titleColor="dangerous"
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
        await userService.changePassword(oldPassword, newPassword).toPromise();
        await userService.logout();
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
  toggle: () => void;
  onConfirm: () => void;
}> = ({ toggle, onConfirm }) => {
  const { t } = useTranslation();

  return (
    <Modal show centered onHide={toggle}>
      <Modal.Header>
        <Modal.Title className="text-danger">
          {t("settings.dialogConfirmLogout.title")}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{t("settings.dialogConfirmLogout.prompt")}</Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={toggle}>
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
    <Container fluid>
      {user ? (
        <>
          <Row className="border-bottom p-3 cursor-pointer">
            <Col xs="12">
              <h5
                onClick={() => {
                  history.push(`/users/${user.username}`);
                }}
              >
                {t("settings.gotoSelf")}
              </h5>
            </Col>
          </Row>
          <Row className="border-bottom p-3 cursor-pointer">
            <Col xs="12">
              <h5
                className="text-danger"
                onClick={() => setDialog("changepassword")}
              >
                {t("settings.changePassword")}
              </h5>
            </Col>
          </Row>
          <Row className="border-bottom p-3 cursor-pointer">
            <Col xs="12">
              <h5
                className="text-danger"
                onClick={() => {
                  setDialog("logout");
                }}
              >
                {t("settings.logout")}
              </h5>
            </Col>
          </Row>
        </>
      ) : null}
      <Row className="align-items-center border-bottom p-3">
        <Col xs="12" sm="auto">
          <h5>{t("settings.languagePrimary")}</h5>
          <p>{t("settings.languageSecondary")}</p>
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
                toggle={() => setDialog(null)}
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
    </Container>
  );
};

export default SettingsPage;
