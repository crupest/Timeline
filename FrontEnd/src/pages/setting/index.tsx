import { useState, ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import classNames from "classnames";

import { useC, I18nText } from "@/common";
import { useUser, userService } from "@/services/user";
import { getHttpUserClient } from "@/http/user";
import { TimelineVisibility } from "@/http/timeline";

import ConfirmDialog from "../common/dialog/ConfirmDialog";
import Card from "../common/Card";
import Spinner from "../common/Spinner";
import ChangePasswordDialog from "./ChangePasswordDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";
import ChangeNicknameDialog from "./ChangeNicknameDialog";

import "./index.css";
import { pushAlert } from "@/services/alert";

interface SettingSectionProps {
  title: I18nText;
  children: ReactNode;
}

function SettingSection({ title, children }: SettingSectionProps) {
  const c = useC();

  return (
    <Card>
      <h2 className="">{c(title)}</h2>
      {children}
    </Card>
  );
}

interface SettingItemContainerWithoutChildrenProps {
  title: I18nText;
  subtext?: I18nText;
  first?: boolean;
  danger?: boolean;
  style?: React.CSSProperties;
  className?: string;
  onClick?: () => void;
}

interface SettingItemContainerProps
  extends SettingItemContainerWithoutChildrenProps {
  children?: React.ReactNode;
}

function SettingItemContainer({
  title,
  subtext,
  first,
  danger,
  children,
  style,
  className,
  onClick,
}: SettingItemContainerProps): JSX.Element {
  const { t } = useTranslation();

  return (
    <div
      style={style}
      className={classNames(
        "row settings-item mx-0",
        first && "first",
        onClick && "clickable",
        className,
      )}
      onClick={onClick}
    >
      <div className="px-0 col col-auto">
        <div className={classNames(danger && "cru-color-danger")}>
          {convertI18nText(title, t)}
        </div>
        <small className="d-block cru-color-secondary">
          {convertI18nText(subtext, t)}
        </small>
      </div>
      <div className="col col-auto">{children}</div>
    </div>
  );
}

type ButtonSettingItemProps = SettingItemContainerWithoutChildrenProps;

const ButtonSettingItem: React.FC<ButtonSettingItemProps> = ({ ...props }) => {
  return <SettingItemContainer {...props} />;
};

interface SelectSettingItemProps
  extends SettingItemContainerWithoutChildrenProps {
  options: {
    value: string;
    label: I18nText;
  }[];
  value?: string;
  onSelect: (value: string) => void;
}

const SelectSettingsItem: React.FC<SelectSettingItemProps> = ({
  options,
  value,
  onSelect,
  ...props
}) => {
  const { t } = useTranslation();

  return (
    <SettingItemContainer {...props}>
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
    </SettingItemContainer>
  );
};

const SettingsPage: React.FC = () => {
  const { i18n } = useTranslation();
  const user = useUser();
  const navigate = useNavigate();

  const [dialog, setDialog] = useState<
    | null
    | "changepassword"
    | "changeavatar"
    | "changenickname"
    | "logout"
    | "renewregistercode"
  >(null);

  const [registerCode, setRegisterCode] = useState<undefined | null | string>(
    undefined,
  );

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

  React.useEffect(() => {
    setRegisterCode(undefined);
  }, [user]);

  React.useEffect(() => {
    if (user != null && registerCode === undefined) {
      void getHttpUserClient()
        .getRegisterCode(user.username)
        .then((code) => {
          setRegisterCode(code.registerCode ?? null);
        });
    }
  }, [user, registerCode]);

  const language = i18n.language.slice(0, 2);

  return (
    <>
      <div className="container">
        {user ? (
          <SettingSection title="settings.subheaders.account">
            <SettingItemContainer
              title="settings.myRegisterCode"
              subtext="settings.myRegisterCodeDesc"
              onClick={() => setDialog("renewregistercode")}
            >
              {registerCode === undefined ? (
                <Spinner />
              ) : registerCode === null ? (
                <span>Noop</span>
              ) : (
                <code
                  className="register-code"
                  onClick={(event) => {
                    void navigator.clipboard
                      .writeText(registerCode)
                      .then(() => {
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
};

export default SettingsPage;
