import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { userService } from "~src/services/user";

import { OperationDialog } from "~src/components/dialog";

export function ChangePasswordDialog() {
  const navigate = useNavigate();

  const [redirect, setRedirect] = useState<boolean>(false);

  return (
    <OperationDialog
      title="settings.dialogChangePassword.title"
      color="danger"
      inputPrompt="settings.dialogChangePassword.prompt"
      inputs={{
        inputs: [
          {
            key: "oldPassword",
            type: "text",
            label: "settings.dialogChangePassword.inputOldPassword",
            password: true,
          },
          {
            key: "newPassword",
            type: "text",
            label: "settings.dialogChangePassword.inputNewPassword",
            password: true,
          },
          {
            key: "retypedNewPassword",
            type: "text",
            label: "settings.dialogChangePassword.inputRetypeNewPassword",
            password: true,
          },
        ],
        validator: (
          { oldPassword, newPassword, retypedNewPassword },
          errors,
        ) => {
          if (oldPassword === "") {
            errors["oldPassword"] =
              "settings.dialogChangePassword.errorEmptyOldPassword";
          }
          if (newPassword === "") {
            errors["newPassword"] =
              "settings.dialogChangePassword.errorEmptyNewPassword";
          }
          if (retypedNewPassword !== newPassword) {
            errors["retypedNewPassword"] =
              "settings.dialogChangePassword.errorRetypeNotMatch";
          }
        },
      }}
      onProcess={async ({ oldPassword, newPassword }) => {
        await userService.changePassword(oldPassword, newPassword);
        setRedirect(true);
      }}
      onSuccessAndClose={() => {
        if (redirect) {
          navigate("/login");
        }
      }}
    />
  );
}

export default ChangePasswordDialog;
