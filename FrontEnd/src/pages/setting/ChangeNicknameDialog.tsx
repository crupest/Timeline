import { getHttpUserClient } from "~src/http/user";
import { useUserLoggedIn } from "~src/services/user";

import { OperationDialog } from "~src/components/dialog";

export default function ChangeNicknameDialog() {
  const user = useUserLoggedIn();

  return (
    <OperationDialog
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
