import { getHttpUserClient } from "~src/http/user";
import { useUserLoggedIn } from "~src/services/user";

import { OperationDialog } from "~src/components/dialog";

export default function ChangeNicknameDialog({
  open,
  onClose,
}: {
  open: boolean;
  onClose: () => void;
}) {
  const user = useUserLoggedIn();

  return (
    <OperationDialog
      open={open}
      onClose={onClose}
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
    />
  );
}
