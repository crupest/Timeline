import React from "react";
import InputPanel, { InputPanelError } from "../common/input/InputPanel";

const RegisterPage: React.FC = () => {
  const [username, setUsername] = React.useState<string>("");
  const [password, setPassword] = React.useState<string>("");
  const [confirmPassword, setConfirmPassword] = React.useState<string>("");
  const [registerCode, setRegisterCode] = React.useState<string>("");

  const [dirty, setDirty] = React.useState<boolean[]>(new Array(4).fill(false));

  const [error, setError] = React.useState<InputPanelError>();

  const validate = (): InputPanelError => {
    const e: InputPanelError = {};
    if (dirty[0] && username.length === 0) {
      e[0] = "register.error.usernameEmpty";
    }
    if (dirty[1] && password.length === 0) {
      e[1] = "register.error.passwordEmpty";
    }
    if (dirty[2] && confirmPassword !== password) {
      e[2] = "register.error.confirmPasswordWrong";
    }
    if (dirty[3] && registerCode.length === 0) {
      e[3] = "register.error.registerCodeEmpty";
    }
    return e;
  };

  return (
    <div>
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

          setError(validate());
        }}
        error={error}
      />
    </div>
  );
};

export default RegisterPage;
