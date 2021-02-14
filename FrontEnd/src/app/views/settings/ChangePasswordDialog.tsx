import React, { useState } from "react";
import { useHistory } from "react-router";

import { userService } from "@/services/user";

import OperationDialog from "../common/OperationDialog";

export interface ChangePasswordDialogProps {
  open: boolean;
  close: () => void;
}

const ChangePasswordDialog: React.FC<ChangePasswordDialogProps> = (props) => {
  const history = useHistory();

  const [redirect, setRedirect] = useState<boolean>(false);

  return (
    <OperationDialog
      open={props.open}
      title="settings.dialogChangePassword.title"
      themeColor="danger"
      inputPrompt="settings.dialogChangePassword.prompt"
      inputScheme={[
        {
          type: "text",
          label: "settings.dialogChangePassword.inputOldPassword",
          password: true,
        },
        {
          type: "text",
          label: "settings.dialogChangePassword.inputNewPassword",
          password: true,
        },
        {
          type: "text",
          label: "settings.dialogChangePassword.inputRetypeNewPassword",
          password: true,
        },
      ]}
      inputValidator={([oldPassword, newPassword, retypedNewPassword]) => {
        const result: Record<number, string> = {};
        if (oldPassword === "") {
          result[0] = "settings.dialogChangePassword.errorEmptyOldPassword";
        }
        if (newPassword === "") {
          result[1] = "settings.dialogChangePassword.errorEmptyNewPassword";
        }
        if (retypedNewPassword !== newPassword) {
          result[2] = "settings.dialogChangePassword.errorRetypeNotMatch";
        }
        return result;
      }}
      onProcess={async ([oldPassword, newPassword]) => {
        await userService.changePassword(oldPassword, newPassword);
        setRedirect(true);
      }}
      close={() => {
        props.close();
        if (redirect) {
          history.push("/login");
        }
      }}
    />
  );
};

export default ChangePasswordDialog;
