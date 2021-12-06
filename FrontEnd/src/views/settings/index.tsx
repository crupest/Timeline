import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";

import { useUser, userService } from "@/services/user";

import ChangePasswordDialog from "./ChangePasswordDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";
import ChangeNicknameDialog from "./ChangeNicknameDialog";
import ConfirmDialog from "../common/dailog/ConfirmDialog";
import Card from "../common/Card";

import "./index.css";

const SettingsPage: React.FC = (_) => {
  const { i18n, t } = useTranslation();
  const user = useUser();
  const navigate = useNavigate();

  const [dialog, setDialog] = useState<
    null | "changepassword" | "changeavatar" | "changenickname" | "logout"
  >(null);

  const language = i18n.language.slice(0, 2);

  return (
    <>
      <div className="container">
        {user ? (
          <Card className="my-3 py-3">
            <h3 className="px-3 mb-3 cru-color-primary">
              {t("settings.subheaders.account")}
            </h3>
            <div
              className="settings-item clickable first"
              onClick={() => setDialog("changeavatar")}
            >
              {t("settings.changeAvatar")}
            </div>
            <div
              className="settings-item clickable"
              onClick={() => setDialog("changenickname")}
            >
              {t("settings.changeNickname")}
            </div>
            <div
              className="settings-item clickable cru-color-danger"
              onClick={() => setDialog("changepassword")}
            >
              {t("settings.changePassword")}
            </div>
            <div
              className="settings-item clickable cru-color-danger"
              onClick={() => {
                setDialog("logout");
              }}
            >
              {t("settings.logout")}
            </div>
          </Card>
        ) : null}
        <Card className="my-3 py-3">
          <h3 className="px-3 mb-3 cru-color-primary">
            {t("settings.subheaders.customization")}
          </h3>
          <div className="row settings-item first mx-0">
            <div className="col col-12 col-sm-auto">
              <div>{t("settings.languagePrimary")}</div>
              <small className="d-block cru-color-secondary">
                {t("settings.languageSecondary")}
              </small>
            </div>
            <div className="col col-12 col-sm-auto">
              <select
                value={language}
                onChange={(e) => {
                  void i18n.changeLanguage(e.target.value);
                }}
              >
                <option value="zh">中文</option>
                <option value="en">English</option>
              </select>
            </div>
          </div>
        </Card>
      </div>
      <ChangePasswordDialog
        open={dialog === "changepassword"}
        close={() => setDialog(null)}
      />
      <ConfirmDialog
        title="settings.dialogConfirmLogout.title"
        body="settings.dialogConfirmLogout.prompt"
        onClose={() => setDialog(null)}
        open={dialog === "logout"}
        onConfirm={() => {
          void userService.logout().then(() => {
            navigate("/");
          });
        }}
      />
      <ChangeAvatarDialog
        open={dialog === "changeavatar"}
        close={() => setDialog(null)}
      />
      <ChangeNicknameDialog
        open={dialog === "changenickname"}
        close={() => setDialog(null)}
      />
    </>
  );
};

export default SettingsPage;
