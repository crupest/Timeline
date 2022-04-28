import React from "react";
import InputPanel, { InputPanelError } from "../common/input/InputPanel";

const RegisterPage: React.FC = () => {
  const [username, setUsername] = React.useState<string>("");
  const [password, setPassword] = React.useState<string>("");
  const [confirmPassword, setConfirmPassword] = React.useState<string>("");
  const [registerCode, setRegisterCode] = React.useState<string>("");

  const [error, setError] = React.useState<InputPanelError>();

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
        onChange={(values) => {
          setUsername(values[0]);
          setPassword(values[1]);
          setConfirmPassword(values[2]);
          setRegisterCode(values[3]);
        }}
        error={error}
      />
    </div>
  );
};

export default RegisterPage;
