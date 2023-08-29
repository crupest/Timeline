import {
  useState,
  useEffect,
  ReactNode,
  ComponentPropsWithoutRef,
} from "react";
import { useTranslation } from "react-i18next"; // For change language.
import { useNavigate } from "react-router-dom";
import classNames from "classnames";

import { useUser, userService } from "~src/services/user";
import { getHttpUserClient } from "~src/http/user";
import { pushAlert } from "~src/services/alert";

import { useC, Text } from "~src/common";

import {
  useDialog,
  DialogProvider,
  ConfirmDialog,
} from "~src/components/dialog";
import Card from "~src/components/Card";
import Spinner from "~src/components/Spinner";
import Page from "~src/components/Page";

import ChangePasswordDialog from "./ChangePasswordDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";
import ChangeNicknameDialog from "./ChangeNicknameDialog";

import "./index.css";

interface SettingSectionProps
  extends Omit<ComponentPropsWithoutRef<typeof Card>, "title"> {
  title: Text;
  children?: ReactNode;
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
      <div className="setting-section-item-area">{children}</div>
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
          className="select-setting-item-select"
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

function RegisterCodeSettingItem() {
  const user = useUser();

  // undefined: loading
  const [registerCode, setRegisterCode] = useState<undefined | null | string>();

  const { controller, createDialogSwitch } = useDialog({
    confirm: (
      <ConfirmDialog
        title="settings.renewRegisterCode"
        body="settings.renewRegisterCodeDesc"
        onConfirm={() => {
          if (user == null) throw new Error();
          void getHttpUserClient()
            .renewRegisterCode(user.username)
            .then(() => {
              setRegisterCode(undefined);
            });
        }}
      />
    ),
  });

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
      className="register-code-setting-item"
      onClick={createDialogSwitch("confirm")}
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
                type: "create",
                message: "settings.myRegisterCodeCopied",
              });
            });
            event.stopPropagation();
          }}
        >
          {registerCode}
        </code>
      )}
      <DialogProvider controller={controller} />
    </SettingItemContainer>
  );
}

function LanguageChangeSettingItem() {
  const { i18n } = useTranslation();

  const language = i18n.language.slice(0, 2);

  return (
    <SelectSettingsItem
      title="settings.languagePrimary"
      description="settings.languageSecondary"
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
    />
  );
}

export default function SettingPage() {
  const user = useUser();
  const navigate = useNavigate();

  const { controller, createDialogSwitch } = useDialog({
    "change-nickname": <ChangeNicknameDialog />,
    "change-avatar": <ChangeAvatarDialog />,
    "change-password": <ChangePasswordDialog />,
    logout: (
      <ConfirmDialog
        title="settings.dialogConfirmLogout.title"
        body="settings.dialogConfirmLogout.prompt"
        onConfirm={() => {
          void userService.logout().then(() => {
            navigate("/");
          });
        }}
      />
    ),
  });

  return (
    <Page noTopPadding>
      {user ? (
        <SettingSection title="settings.subheader.account">
          <RegisterCodeSettingItem />
          <ButtonSettingItem
            title="settings.changeAvatar"
            onClick={createDialogSwitch("change-avatar")}
          />
          <ButtonSettingItem
            title="settings.changeNickname"
            onClick={createDialogSwitch("change-nickname")}
          />
          <ButtonSettingItem
            title="settings.changePassword"
            onClick={createDialogSwitch("change-password")}
            danger
          />
          <ButtonSettingItem
            title="settings.logout"
            onClick={createDialogSwitch("logout")}
            danger
          />
        </SettingSection>
      ) : null}
      <SettingSection title="settings.subheader.customization">
        <LanguageChangeSettingItem />
      </SettingSection>
      <DialogProvider controller={controller} />
    </Page>
  );
}
