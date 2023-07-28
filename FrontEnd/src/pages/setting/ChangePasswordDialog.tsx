import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { userService } from "@/services/user";

import OperationDialog, {
  InputErrorDict,
} from "@/views/common/dialog/OperationDialog";

interface ChangePasswordDialogProps {
  open: boolean;
  close: () => void;
}

export function ChangePasswordDialog(props: ChangePasswordDialogProps) {
  const { open, close } = props;

  const navigate = useNavigate();

  const [redirect, setRedirect] = useState<boolean>(false);

  return (
    <OperationDialog
      open={open}
      close={close}
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
        validator: ({ oldPassword, newPassword, retypedNewPassword }) => {
          const result: InputErrorDict = {};
          if (oldPassword === "") {
            result["oldPassword"] =
              "settings.dialogChangePassword.errorEmptyOldPassword";
          }
          if (newPassword === "") {
            result["newPassword"] =
              "settings.dialogChangePassword.errorEmptyNewPassword";
          }
          if (retypedNewPassword !== newPassword) {
            result["retypedNewPassword"] =
              "settings.dialogChangePassword.errorRetypeNotMatch";
          }
          return result;
        },
      }}
      onProcess={async ({ oldPassword, newPassword }) => {
        await userService.changePassword(
          oldPassword as string,
          newPassword as string,
        );
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
