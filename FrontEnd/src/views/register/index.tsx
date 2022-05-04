import React from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

import { HttpBadRequestError } from "@/http/common";
import { getHttpTokenClient } from "@/http/token";
import { userService, useUser } from "@/services/user";

import { LoadingButton } from "../common/button";
import InputPanel, {
  hasError,
  InputPanelError,
} from "../common/input/InputPanel";

import "./index.css";

const validate = (values: string[], dirties: boolean[]): InputPanelError => {
  const e: InputPanelError = {};
  if (dirties[0] && values[0].length === 0) {
    e[0] = "register.error.usernameEmpty";
  }
  if (dirties[1] && values[1].length === 0) {
    e[1] = "register.error.passwordEmpty";
  }
  if (dirties[2] && values[2] !== values[1]) {
    e[2] = "register.error.confirmPasswordWrong";
  }
  if (dirties[3] && values[3].length === 0) {
    e[3] = "register.error.registerCodeEmpty";
  }
  return e;
};

const RegisterPage: React.FC = () => {
  const navigate = useNavigate();

  const { t } = useTranslation();

  const [username, setUsername] = React.useState<string>("");
  const [password, setPassword] = React.useState<string>("");
  const [confirmPassword, setConfirmPassword] = React.useState<string>("");
  const [registerCode, setRegisterCode] = React.useState<string>("");

  const [dirty, setDirty] = React.useState<boolean[]>(new Array(4).fill(false));

  const [process, setProcess] = React.useState<boolean>(false);

  const [inputError, setInputError] = React.useState<InputPanelError>();
  const [resultError, setResultError] = React.useState<string | null>(null);

  const user = useUser();

  React.useEffect(() => {
    if (user != null) {
      navigate("/");
    }
  });

  return (
    <div className="container register-page">
      <InputPanel
        scheme={[
          {
            type: "text",
            label: "register.username",
          },
          {
            type: "text",
            label: "register.password",
            password: true,
          },
          {
            type: "text",
            label: "register.confirmPassword",
            password: true,
          },
          { type: "text", label: "register.registerCode" },
        ]}
        values={[username, password, confirmPassword, registerCode]}
        onChange={(values, index) => {
          setUsername(values[0]);
          setPassword(values[1]);
          setConfirmPassword(values[2]);
          setRegisterCode(values[3]);
          const newDirty = dirty.slice();
          newDirty[index] = true;
          setDirty(newDirty);

          setInputError(validate(values, newDirty));
        }}
        error={inputError}
        disable={process}
      />
      {resultError && <div className="cru-color-danger">{t(resultError)}</div>}
      <LoadingButton
        text="register.register"
        loading={process}
        disabled={hasError(inputError)}
        onClick={() => {
          const newDirty = dirty.slice().fill(true);
          setDirty(newDirty);
          const e = validate(
            [username, password, confirmPassword, registerCode],
            newDirty
          );
          if (hasError(e)) {
            setInputError(e);
          } else {
            setProcess(true);
            void getHttpTokenClient()
              .register({
                username,
                password,
                registerCode,
              })
              .then(
                () => {
                  void userService
                    .login({ username, password }, true)
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
                }
              );
          }
        }}
      />
    </div>
  );
};

export default RegisterPage;
