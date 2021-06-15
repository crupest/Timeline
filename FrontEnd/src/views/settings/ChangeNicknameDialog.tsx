import { getHttpUserClient } from "@/http/user";
import { useUserLoggedIn } from "@/services/user";
import React from "react";

import OperationDialog from "../common/OperationDialog";

export interface ChangeNicknameDialogProps {
  open: boolean;
  close: () => void;
}

const ChangeNicknameDialog: React.FC<ChangeNicknameDialogProps> = (props) => {
  const user = useUserLoggedIn();

  return (
    <OperationDialog
      open={props.open}
      title="settings.dialogChangeNickname.title"
      inputScheme={[
        { type: "text", label: "settings.dialogChangeNickname.inputLabel" },
      ]}
      onProcess={([newNickname]) => {
        return getHttpUserClient().patch(user.username, {
          nickname: newNickname,
        });
      }}
      close={props.close}
    />
  );
};

export default ChangeNicknameDialog;
