import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";

import { useUser, userService } from "@/services/user";

import AppBar from "../common/AppBar";
import LoadingButton from "../common/button/LoadingButton";

import "./index.css";

const LoginPage: React.FC = (_) => {
  const { t } = useTranslation();
  const history = useHistory();
  const [username, setUsername] = React.useState<string>("");
  const [usernameDirty, setUsernameDirty] = React.useState<boolean>(false);
  const [password, setPassword] = React.useState<string>("");
  const [passwordDirty, setPasswordDirty] = React.useState<boolean>(false);
  const [rememberMe, setRememberMe] = React.useState<boolean>(true);
  const [process, setProcess] = React.useState<boolean>(false);
  const [error, setError] = React.useState<string | null>(null);

  const user = useUser();

  React.useEffect(() => {
    if (user != null) {
      const id = setTimeout(() => history.push("/"), 3000);
      return () => {
        clearTimeout(id);
      };
    }
  }, [history, user]);

  if (user != null) {
    return (
      <>
        <AppBar />
        <p>{t("login.alreadyLogin")}</p>
      </>
    );
  }

  const submit = (): void => {
    if (username === "" || password === "") {
      setUsernameDirty(true);
      setPasswordDirty(true);
      return;
    }

    setProcess(true);
    userService
      .login(
        {
          username: username,
          password: password,
        },
        rememberMe
      )
      .then(
        () => {
          if (history.length === 0) {
            history.push("/");
          } else {
            history.goBack();
          }
        },
        (e: Error) => {
          setProcess(false);
          setError(e.message);
        }
      );
  };

  const onEnterPressInPassword: React.KeyboardEventHandler = (e) => {
    if (e.key === "Enter") {
      submit();
    }
  };

  return (
    <div className="login-container container-fluid mt-2">
      <h1 className="text-center">{t("welcome")}</h1>
      <div>
        <label htmlFor="username">{t("user.username")}</label>
        <input
          id="username"
          disabled={process}
          onChange={(e) => {
            setUsername(e.target.value);
            setUsernameDirty(true);
          }}
          value={username}
        />
        {usernameDirty && username === "" && (
          <div>{t("login.emptyUsername")}</div>
        )}
        <label htmlFor="password">{t("user.password")}</label>
        <input
          id="password"
          type="password"
          disabled={process}
          onChange={(e) => {
            setPassword(e.target.value);
            setPasswordDirty(true);
          }}
          value={password}
          onKeyDown={onEnterPressInPassword}
        />
        {passwordDirty && password === "" && (
          <div>{t("login.emptyPassword")}</div>
        )}
      </div>
      <div>
        <input
          id="remember-me"
          type="checkbox"
          checked={rememberMe}
          onChange={(e) => {
            setRememberMe(e.currentTarget.checked);
          }}
        />
        <label>{t("user.rememberMe")}</label>
      </div>
      {error ? <p className="text-danger">{t(error)}</p> : null}
      <div className="cru-text-end">
        <LoadingButton
          loading={process}
          onClick={(e) => {
            submit();
            e.preventDefault();
          }}
          disabled={username === "" || password === "" ? true : undefined}
        >
          {t("user.login")}
        </LoadingButton>
      </div>
    </div>
  );
};

export default LoginPage;
