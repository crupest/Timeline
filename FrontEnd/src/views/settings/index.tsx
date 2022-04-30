import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import classnames from "classnames";

import { convertI18nText, I18nText } from "@/common";
import { useUser, userService } from "@/services/user";
import { getHttpUserClient } from "@/http/user";
import { TimelineVisibility } from "@/http/timeline";

import ConfirmDialog from "../common/dailog/ConfirmDialog";
import Card from "../common/Card";
import Spinner from "../common/Spinner";
import ChangePasswordDialog from "./ChangePasswordDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";
import ChangeNicknameDialog from "./ChangeNicknameDialog";

import "./index.css";

interface SettingSectionProps {
  title: I18nText;
  children: React.ReactNode;
}

const SettingSection: React.FC<SettingSectionProps> = ({ title, children }) => {
  const { t } = useTranslation();

  return (
    <Card className="my-3 py-3">
      <h3 className="px-3 mb-3 cru-color-primary">
        {convertI18nText(title, t)}
      </h3>
      {children}
    </Card>
  );
};

interface ButtonSettingItemProps {
  title: I18nText;
  subtext?: I18nText;
  onClick: () => void;
  first?: boolean;
  danger?: boolean;
}

const ButtonSettingItem: React.FC<ButtonSettingItemProps> = ({
  title,
  subtext,
  onClick,
  first,
  danger,
}) => {
  const { t } = useTranslation();

  return (
    <div
      className={classnames(
        "settings-item clickable",
        first && "first",
        danger && "cru-color-danger"
      )}
      onClick={onClick}
    >
      {convertI18nText(title, t)}
      {subtext && (
        <small className="d-block cru-color-secondary">
          {convertI18nText(subtext, t)}
        </small>
      )}
    </div>
  );
};

interface SelectSettingItemProps {
  title: I18nText;
  subtext?: I18nText;
  options: {
    value: string;
    label: I18nText;
  }[];
  value?: string;
  onSelect: (value: string) => void;
  first?: boolean;
}

const SelectSettingsItem: React.FC<SelectSettingItemProps> = ({
  title,
  subtext,
  options,
  value,
  onSelect,
  first,
}) => {
  const { t } = useTranslation();

  return (
    <div className={classnames("row settings-item mx-0", first && "first")}>
      <div className="px-0 col col-12 col-sm-auto">
        <div>{convertI18nText(title, t)}</div>
        <small className="d-block cru-color-secondary">
          {convertI18nText(subtext, t)}
        </small>
      </div>
      <div className="col col-12 col-sm-auto">
        {value == null ? (
          <Spinner />
        ) : (
          <select
            value={value}
            onChange={(e) => {
              onSelect(e.target.value);
            }}
          >
            {options.map(({ value, label }) => (
              <option key={value} value={value}>
                {convertI18nText(label, t)}
              </option>
            ))}
          </select>
        )}
      </div>
    </div>
  );
};

const SettingsPage: React.FC = (_) => {
  const { i18n } = useTranslation();
  const user = useUser();
  const navigate = useNavigate();

  const [dialog, setDialog] = useState<
    null | "changepassword" | "changeavatar" | "changenickname" | "logout"
  >(null);

  const [bookmarkVisibility, setBookmarkVisibility] =
    useState<TimelineVisibility>();

  React.useEffect(() => {
    if (user != null) {
      void getHttpUserClient()
        .getBookmarkVisibility(user.username)
        .then(({ visibility }) => {
          setBookmarkVisibility(visibility);
        });
    } else {
      setBookmarkVisibility(undefined);
    }
  }, [user]);

  const language = i18n.language.slice(0, 2);

  return (
    <>
      <div className="container">
        {user ? (
          <SettingSection title="settings.subheaders.account">
            <ButtonSettingItem
              title="settings.changeAvatar"
              onClick={() => setDialog("changeavatar")}
              first
            />
            <ButtonSettingItem
              title="settings.changeNickname"
              onClick={() => setDialog("changenickname")}
            />
            <SelectSettingsItem
              title="settings.changeBookmarkVisibility"
              options={[
                {
                  value: "Private",
                  label: "visibility.private",
                },
                {
                  value: "Register",
                  label: "visibility.register",
                },
                {
                  value: "Public",
                  label: "visibility.public",
                },
              ]}
              value={bookmarkVisibility}
              onSelect={(value) => {
                void getHttpUserClient()
                  .putBookmarkVisibility(user.username, {
                    visibility: value as TimelineVisibility,
                  })
                  .then(() => {
                    setBookmarkVisibility(value as TimelineVisibility);
                  });
              }}
            />
            <ButtonSettingItem
              title="settings.changePassword"
              onClick={() => setDialog("changepassword")}
              danger
            />
            <ButtonSettingItem
              title="settings.logout"
              onClick={() => {
                setDialog("logout");
              }}
              danger
            />
          </SettingSection>
        ) : null}
        <SettingSection title="settings.subheaders.customization">
          <SelectSettingsItem
            title="settings.languagePrimary"
            subtext="settings.languageSecondary"
            options={[
              {
                value: "zh",
                label: {
                  type: "custom",
                  value: "中文",
                },
              },
              {
                value: "en",
                label: {
                  type: "custom",
                  value: "English",
                },
              },
            ]}
            value={language}
            onSelect={(value) => {
              void i18n.changeLanguage(value);
            }}
            first
          />
        </SettingSection>
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
