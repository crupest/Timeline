import React from "react";

const RegisterPage: React.FC = () => {
  const [username, setUsername] = React.useState<string>("");
  const [password, setPassword] = React.useState<string>("");
  const [confirmPassword, setConfirmPassword] = React.useState<string>("");
  const [registerCode, setRegisterCode] = React.useState<string>("");

  return (
    <div>
      <div>
        <label>Username</label>
        <input
          type="text"
          value={username}
          onChange={(e) => {
            setUsername(e.target.value);
          }}
        />
      </div>
      <div>
        <label>Password</label>
        <input
          type="password"
          value={password}
          onChange={(e) => {
            setPassword(e.target.value);
          }}
        />
      </div>
      <div>
        <label>Confirm Password</label>
        <input
          type="password"
          value={confirmPassword}
          onChange={(e) => {
            setConfirmPassword(e.target.value);
          }}
        />
      </div>
      <div>
        <label>Register Code</label>
        <input
          type="text"
          value={registerCode}
          onChange={(e) => {
            setRegisterCode(e.target.value);
          }}
        />
      </div>
    </div>
  );
};

export default RegisterPage;
