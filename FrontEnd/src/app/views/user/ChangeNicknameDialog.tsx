import React from "react";

import OperationDialog from "../common/OperationDialog";

export interface ChangeNicknameDialogProps {
  open: boolean;
  close: () => void;
  onProcess: (newNickname: string) => Promise<void>;
}

const ChangeNicknameDialog: React.FC<ChangeNicknameDialogProps> = (props) => {
  return (
    <OperationDialog
      open={props.open}
      title="userPage.dialogChangeNickname.title"
      inputScheme={[
        { type: "text", label: "userPage.dialogChangeNickname.inputLabel" },
      ]}
      onProcess={([newNickname]) => {
        return props.onProcess(newNickname);
      }}
      close={props.close}
    />
  );
};

export default ChangeNicknameDialog;
