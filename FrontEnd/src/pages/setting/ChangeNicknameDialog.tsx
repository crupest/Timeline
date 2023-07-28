import { getHttpUserClient } from "@/http/user";
import { useUserLoggedIn } from "@/services/user";

import OperationDialog from "@/views/common/dialog/OperationDialog";

export interface ChangeNicknameDialogProps {
  open: boolean;
  close: () => void;
}

export default function ChangeNicknameDialog(props: ChangeNicknameDialogProps) {
  const { open, close } = props;

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
          nickname: newNickname as string,
        });
      }}
      close={close}
    />
  );
}
