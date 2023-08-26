import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

import { HttpBadRequestError } from "~src/http/common";
import { getHttpTokenClient } from "~src/http/token";
import { userService, useUser } from "~src/services/user";

import { LoadingButton } from "~src/components/button";
import { useInputs, InputGroup } from "~src/components/input/InputGroup";

import "./index.css";

export default function RegisterPage() {
  const navigate = useNavigate();

  const { t } = useTranslation();

  const user = useUser();

  const { hasErrorAndDirty, confirm, setAllDisabled, inputGroupProps } =
    useInputs({
      init: {
        scheme: {
          inputs: [
            {
              key: "username",
              type: "text",
              label: "register.username",
            },
            {
              key: "password",
              type: "text",
              label: "register.password",
              password: true,
            },
            {
              key: "confirmPassword",
              type: "text",
              label: "register.confirmPassword",
              password: true,
            },
            {
              key: "registerCode",

              type: "text",
              label: "register.registerCode",
            },
          ],
          validator: (
            { username, password, confirmPassword, registerCode },
            errors,
          ) => {
            if (username === "") {
              errors["username"] = "register.error.usernameEmpty";
            }
            if (password === "") {
              errors["password"] = "register.error.passwordEmpty";
            }
            if (confirmPassword !== password) {
              errors["confirmPassword"] = "register.error.confirmPasswordWrong";
            }
            if (registerCode === "") {
              errors["registerCode"] = "register.error.registerCodeEmpty";
            }
          },
        },
        dataInit: {},
      },
    });

  const [process, setProcess] = useState<boolean>(false);
  const [resultError, setResultError] = useState<string | null>(null);

  useEffect(() => {
    if (user != null) {
      navigate("/");
    }
  }, [navigate, user]);

  return (
    <div className="container register-page">
      <InputGroup {...inputGroupProps} />
      {resultError && <div className="cru-color-danger">{t(resultError)}</div>}
      <LoadingButton
        text="register.register"
        loading={process}
        disabled={hasErrorAndDirty}
        onClick={() => {
          const confirmResult = confirm();
          if (confirmResult.type === "ok") {
            const { username, password, registerCode } = confirmResult.values;
            setProcess(true);
            setAllDisabled(true);
            void getHttpTokenClient()
              .register({
                username: username as string,
                password: password as string,
                registerCode: registerCode as string,
              })
              .then(
                () => {
                  void userService
                    .login(
                      {
                        username: username as string,
                        password: password as string,
                      },
                      true,
                    )
                    .then(() => {
                      navigate("/");
                    });
                },
                (error) => {
                  if (error instanceof HttpBadRequestError) {
                    setResultError("register.error.registerCodeInvalid");
                  } else {
                    setResultError("error.network");
                  }
                  setProcess(false);
                  setAllDisabled(false);
                },
              );
          }
        }}
      />
    </div>
  );
}
