import { getHttpUserClient } from "~src/http/user";
import { useUserLoggedIn } from "~src/services/user";

import OperationDialog from "~src/components/dialog/OperationDialog";

export interface ChangeNicknameDialogProps {
  open: boolean;
  onClose: () => void;
}

export default function ChangeNicknameDialog(props: ChangeNicknameDialogProps) {
  const { open, onClose } = props;

  const user = useUserLoggedIn();

  return (
    <OperationDialog
      open={open}
      title="settings.dialogChangeNickname.title"
      inputs={[
        {
          key: "newNickname",
          type: "text",
          label: "settings.dialogChangeNickname.inputLabel",
        },
      ]}
      onProcess={({ newNickname }) => {
        return getHttpUserClient().patch(user.username, {
          nickname: newNickname,
        });
      }}
      onClose={onClose}
    />
  );
}
