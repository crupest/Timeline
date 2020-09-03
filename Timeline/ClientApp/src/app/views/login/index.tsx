import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Form } from "react-bootstrap";

import { useUser, userService } from "@/services/user";

import AppBar from "../common/AppBar";
import LoadingButton from "../common/LoadingButton";

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
        <p className="mt-appbar">{t("login.alreadyLogin")}</p>
      </>
    );
  }

  function onSubmit(event: React.SyntheticEvent): void {
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
    event.preventDefault();
  }

  return (
    <div className="container login-container mt-appbar">
      <h1>{t("welcome")}</h1>
      <Form>
        <Form.Group>
          <Form.Label htmlFor="username">{t("user.username")}</Form.Label>
          <Form.Control
            id="username"
            disabled={process}
            onChange={(e) => {
              setUsername(e.target.value);
              setUsernameDirty(true);
            }}
            value={username}
            isInvalid={usernameDirty && username === ""}
          />
          {usernameDirty && username === "" && (
            <Form.Control.Feedback type="invalid">
              {t("login.emptyUsername")}
            </Form.Control.Feedback>
          )}
        </Form.Group>
        <Form.Group>
          <Form.Label htmlFor="password">{t("user.password")}</Form.Label>
          <Form.Control
            id="password"
            type="password"
            disabled={process}
            onChange={(e) => {
              setPassword(e.target.value);
              setPasswordDirty(true);
            }}
            value={password}
            isInvalid={passwordDirty && password === ""}
          />
          {passwordDirty && password === "" && (
            <Form.Control.Feedback type="invalid">
              {t("login.emptyPassword")}
            </Form.Control.Feedback>
          )}
        </Form.Group>
        <Form.Group>
          <Form.Check<"input">
            id="remember-me"
            type="checkbox"
            checked={rememberMe}
            onChange={(e) => {
              setRememberMe(e.target.checked);
            }}
            label={t("user.rememberMe")}
          />
        </Form.Group>
        {error ? <p className="text-danger">{t(error)}</p> : null}
        <div className="text-right">
          <LoadingButton
            loading={process}
            variant="primary"
            onClick={onSubmit}
            disabled={username === "" || password === "" ? true : undefined}
          >
            {t("user.login")}
          </LoadingButton>
        </div>
      </Form>
    </div>
  );
};

export default LoginPage;
