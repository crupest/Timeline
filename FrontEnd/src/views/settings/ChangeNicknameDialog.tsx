import { getHttpUserClient } from "@/http/user";
import { useUser } from "@/services/user";
import React from "react";

import OperationDialog from "../common/dailog/OperationDialog";

export interface ChangeNicknameDialogProps {
  open: boolean;
  close: () => void;
}

const ChangeNicknameDialog: React.FC<ChangeNicknameDialogProps> = (props) => {
  const user = useUser();

  if (user == null) return null;

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
      onClose={props.close}
    />
  );
};

export default ChangeNicknameDialog;
