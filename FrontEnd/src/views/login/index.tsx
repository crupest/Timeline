import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Container, Form } from "react-bootstrap";

import { useUser, userService } from "@/services/user";

import AppBar from "../common/AppBar";
import LoadingButton from "../common/LoadingButton";

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
    <Container fluid className="login-container mt-2">
      <h1 className="text-center">{t("welcome")}</h1>
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
            onKeyDown={onEnterPressInPassword}
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
              setRememberMe(e.currentTarget.checked);
            }}
            label={t("user.rememberMe")}
          />
        </Form.Group>
        {error ? <p className="text-danger">{t(error)}</p> : null}
        <div className="text-end">
          <LoadingButton
            loading={process}
            variant="primary"
            onClick={(e) => {
              submit();
              e.preventDefault();
            }}
            disabled={username === "" || password === "" ? true : undefined}
          >
            {t("user.login")}
          </LoadingButton>
        </div>
      </Form>
    </Container>
  );
};

export default LoginPage;
