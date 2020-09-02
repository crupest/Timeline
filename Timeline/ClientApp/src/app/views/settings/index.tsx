import React, { useState } from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Form, Container, Row, Col, Button, Modal } from "react-bootstrap";

import { useUser, userService } from "@/services/user";
import AppBar from "../common/AppBar";
import OperationDialog, {
  OperationInputErrorInfo,
} from "../common/OperationDialog";

interface ChangePasswordDialogProps {
  open: boolean;
  close: () => void;
}

const ChangePasswordDialog: React.FC<ChangePasswordDialogProps> = (props) => {
  const history = useHistory();
  const { t } = useTranslation();

  const [redirect, setRedirect] = useState<boolean>(false);

  return (
    <OperationDialog
      open={props.open}
      title={t("settings.dialogChangePassword.title")}
      titleColor="dangerous"
      inputPrompt={t("settings.dialogChangePassword.prompt")}
      inputScheme={[
        {
          type: "text",
          label: t("settings.dialogChangePassword.inputOldPassword"),
          password: true,
          validator: (v) =>
            v === ""
              ? "settings.dialogChangePassword.errorEmptyOldPassword"
              : null,
        },
        {
          type: "text",
          label: t("settings.dialogChangePassword.inputNewPassword"),
          password: true,
          validator: (v, values) => {
            const error: OperationInputErrorInfo = {};
            error[1] =
              v === ""
                ? "settings.dialogChangePassword.errorEmptyNewPassword"
                : null;
            if (v === values[2]) {
              error[2] = null;
            } else {
              if (values[2] !== "") {
                error[2] = "settings.dialogChangePassword.errorRetypeNotMatch";
              }
            }
            return error;
          },
        },
        {
          type: "text",
          label: t("settings.dialogChangePassword.inputRetypeNewPassword"),
          password: true,
          validator: (v, values) =>
            v !== values[1]
              ? "settings.dialogChangePassword.errorRetypeNotMatch"
              : null,
        },
      ]}
      onProcess={async ([oldPassword, newPassword]) => {
        await userService
          .changePassword(oldPassword as string, newPassword as string)
          .toPromise();
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
    <>
      <AppBar />
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
    </>
  );
};

export default SettingsPage;
