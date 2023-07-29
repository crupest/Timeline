import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Trans } from "react-i18next";

import { useUser, userService } from "@/services/user";

import { useC } from "@/views/common/common";
import LoadingButton from "@/views/common/button/LoadingButton";
import {
  InputErrorDict,
  InputGroup,
  useInputs,
} from "@/views/common/input/InputGroup";
import Page from "@/views/common/Page";

import "./index.css";

export default function LoginPage() {
  const c = useC();

  const user = useUser();

  const navigate = useNavigate();

  const [process, setProcess] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const { hasErrorAndDirty, confirm, setAllDisabled, inputGroupProps } =
    useInputs({
      init: {
        scheme: {
          inputs: [
            {
              key: "username",
              type: "text",
              label: "user.username",
            },
            {
              key: "password",
              type: "text",
              label: "user.password",
              password: true,
            },
            {
              key: "rememberMe",
              type: "bool",
              label: "user.rememberMe",
            },
          ],
          validator: ({ username, password }) => {
            const result: InputErrorDict = {};
            if (username === "") {
              result["username"] = "login.emptyUsername";
            }
            if (password === "") {
              result["password"] = "login.emptyPassword";
            }
            return result;
          },
        },
        dataInit: {},
      },
    });

  useEffect(() => {
    if (user != null) {
      const id = setTimeout(() => navigate("/"), 3000);
      return () => {
        clearTimeout(id);
      };
    }
  }, [navigate, user]);

  if (user != null) {
    return <p>{c("login.alreadyLogin")}</p>;
  }

  const submit = (): void => {
    const confirmResult = confirm();
    if (confirmResult.type === "ok") {
      const { username, password, rememberMe } = confirmResult.values;
      setAllDisabled(true);
      setProcess(true);
      userService
        .login(
          {
            username: username as string,
            password: password as string,
          },
          rememberMe as boolean,
        )
        .then(
          () => {
            if (history.length === 0) {
              navigate("/");
            } else {
              navigate(-1);
            }
          },
          (e: Error) => {
            setProcess(false);
            setAllDisabled(false);
            setError(e.message);
          },
        );
    }
  };

  return (
    <Page className="login-page">
      <div className="login-page-container">
        <div className="login-page-welcome">{c("welcome")}</div>
        <InputGroup {...inputGroupProps} />
        {error ? <p className="login-page-error">{c(error)}</p> : null}
        <div className="login-page-button-row">
          <LoadingButton
            loading={process}
            onClick={(e) => {
              submit();
              e.preventDefault();
            }}
            disabled={hasErrorAndDirty}
          >
            {c("user.login")}
          </LoadingButton>
        </div>
        <Trans i18nKey="login.noAccount">
          0<Link to="/register">1</Link>2
        </Trans>
      </div>
    </Page>
  );
}
