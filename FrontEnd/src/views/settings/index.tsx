import React, { useState } from "react";
import { useHistory } from "react-router";
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
  const history = useHistory();

  const [dialog, setDialog] = useState<
    null | "changepassword" | "changeavatar" | "changenickname" | "logout"
  >(null);

  const language = i18n.language.slice(0, 2);

  return (
    <>
      <div className="container">
        {user ? (
          <Card className="my-3 py-3">
            <h3 className="px-3 mb-3 text-primary">
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
          </Card>
        ) : null}
        <Card className="my-3 py-3">
          <h3 className="px-3 mb-3 text-primary">
            {t("settings.subheaders.customization")}
          </h3>
          <div className="row settings-item first mx-0">
            <div className="col col-12 col-sm-auto">
              <div>{t("settings.languagePrimary")}</div>
              <small className="d-block text-secondary">
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
      {(() => {
        switch (dialog) {
          case "changepassword":
            return <ChangePasswordDialog open close={() => setDialog(null)} />;
          case "logout":
            return (
              <ConfirmDialog
                title="settings.dialogConfirmLogout.title"
                body="settings.dialogConfirmLogout.prompt"
                onClose={() => setDialog(null)}
                onConfirm={() => {
                  void userService.logout().then(() => {
                    history.push("/");
                  });
                }}
              />
            );
          case "changeavatar":
            return <ChangeAvatarDialog open close={() => setDialog(null)} />;
          case "changenickname":
            return <ChangeNicknameDialog open close={() => setDialog(null)} />;
          default:
            return null;
        }
      })()}
    </>
  );
};

export default SettingsPage;
