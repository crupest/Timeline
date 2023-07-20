import {
  useState,
  useEffect,
  ReactNode,
  ComponentPropsWithoutRef,
} from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import classNames from "classnames";

import { useC, Text } from "@/common";
import { useUser, userService } from "@/services/user";
import { getHttpUserClient } from "@/http/user";
import { TimelineVisibility } from "@/http/timeline";

import ConfirmDialog from "@/views/common/dialog/ConfirmDialog";
import Card from "@/views/common/Card";
import Spinner from "@/views/common/Spinner";
import ChangePasswordDialog from "./ChangePasswordDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";
import ChangeNicknameDialog from "./ChangeNicknameDialog";

import "./index.css";
import { pushAlert } from "@/services/alert";

interface SettingSectionProps
  extends Omit<ComponentPropsWithoutRef<"div">, "title"> {
  title: Text;
  children: ReactNode;
}

function SettingSection({
  title,
  className,
  children,
  ...otherProps
}: SettingSectionProps) {
  const c = useC();

  return (
    <Card className={classNames(className, "setting-section")} {...otherProps}>
      <h2 className="setting-section-title">{c(title)}</h2>
      {children}
    </Card>
  );
}

interface SettingItemContainerProps
  extends Omit<ComponentPropsWithoutRef<"div">, "title"> {
  title: Text;
  description?: Text;
  danger?: boolean;
  extraClassName?: string;
}

function SettingItemContainer({
  title,
  description,
  danger,
  extraClassName,
  className,
  children,
  ...otherProps
}: SettingItemContainerProps) {
  const c = useC();

  return (
    <div
      className={classNames(
        className,
        "setting-item-container",
        danger && "danger",
        extraClassName,
      )}
      {...otherProps}
    >
      <div className="setting-item-label-area">
        <div className="setting-item-label-title">{c(title)}</div>
        <small className="setting-item-label-sub">{c(description)}</small>
      </div>
      <div className="setting-item-value-area">{children}</div>
    </div>
  );
}

type ButtonSettingItemProps = Omit<SettingItemContainerProps, "extraClassName">;

function ButtonSettingItem(props: ButtonSettingItemProps) {
  return (
    <SettingItemContainer extraClassName="setting-type-button" {...props} />
  );
}

interface SelectSettingItemProps
  extends Omit<SettingItemContainerProps, "onSelect" | "extraClassName"> {
  options: {
    value: string;
    label: Text;
  }[];
  value?: string | null;
  onSelect: (value: string) => void;
}

function SelectSettingsItem({
  options,
  value,
  onSelect,
  ...extraProps
}: SelectSettingItemProps) {
  const c = useC();

  return (
    <SettingItemContainer extraClassName="setting-type-select" {...extraProps}>
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
              {c(label)}
            </option>
          ))}
        </select>
      )}
    </SettingItemContainer>
  );
}

function RegisterCodeSettingItem({
  openRenewDialog,
}: {
  openRenewDialog: () => void;
}) {
  const user = useUser();

  // undefined: loading
  const [registerCode, setRegisterCode] = useState<undefined | null | string>();

  useEffect(() => {
    setRegisterCode(undefined);
  }, [user]);

  useEffect(() => {
    if (user != null && registerCode === undefined) {
      void getHttpUserClient()
        .getRegisterCode(user.username)
        .then((code) => {
          setRegisterCode(code.registerCode ?? null);
        });
    }
  }, [user, registerCode]);

  return (
    <SettingItemContainer
      title="settings.myRegisterCode"
      description="settings.myRegisterCodeDesc"
      onClick={openRenewDialog}
    >
      {registerCode === undefined ? (
        <Spinner />
      ) : registerCode === null ? (
        <span>Noop</span>
      ) : (
        <code
          className="register-code"
          onClick={(event) => {
            void navigator.clipboard.writeText(registerCode).then(() => {
              pushAlert({
                type: "success",
                message: "settings.myRegisterCodeCopied",
              });
            });
            event.stopPropagation();
          }}
        >
          {registerCode}
        </code>
      )}
    </SettingItemContainer>
  );
}

export default function SettingsPage() {
  const c = useC();
  const { i18n } = useTranslation();
  const user = useUser();
  const navigate = useNavigate();

  type DialogName =
    | "change-password"
    | "change-avatar"
    | "change-nickname"
    | "logout"
    | "renew-register-code";

  const [dialog, setDialog] = useState<null | DialogName>(null);

  function dialogOpener(name: DialogName): () => void {
    return () => setDialog(name);
  }

  const language = i18n.language.slice(0, 2);

  return (
    <>
      <div className="container">
        {user ? (
          <SettingSection title="settings.subheader.account">
            <RegisterCodeSettingItem
              openRenewDialog={dialogOpener("renew-register-code")}
            />
            <ButtonSettingItem
              title="settings.changeAvatar"
              onClick={dialogOpener("change-avatar")}
            />
            <ButtonSettingItem
              title="settings.changeNickname"
              onClick={dialogOpener("change-nickname")}
            />
            <ButtonSettingItem
              title="settings.changePassword"
              onClick={dialogOpener("change-password")}
              danger
            />
            <ButtonSettingItem
              title="settings.logout"
              onClick={dialogOpener("logout")}
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
      <ConfirmDialog
        title="settings.renewRegisterCode"
        body="settings.renewRegisterCodeDesc"
        onClose={() => setDialog(null)}
        open={dialog === "renewregistercode"}
        onConfirm={() => {
          if (user == null) throw new UiLogicError();
          void getHttpUserClient()
            .renewRegisterCode(user.username)
            .then(() => {
              setRegisterCode(undefined);
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
}
