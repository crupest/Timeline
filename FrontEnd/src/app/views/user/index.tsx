import React, { useState } from "react";
import { useParams } from "react-router";

import { getHttpUserClient } from "@/http/user";

import TimelinePageTemplate from "../timeline-common/TimelinePageTemplate";
import UserPageUI from "./UserPageUI";
import { PersonalTimelineManageItem } from "./UserInfoCard";
import ChangeNicknameDialog from "./ChangeNicknameDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";

const UserPage: React.FC = (_) => {
  const { username } = useParams<{ username: string }>();

  const [dialog, setDialog] = useState<null | PersonalTimelineManageItem>(null);

  const [reloadKey, setReloadKey] = React.useState<number>(0);

  let dialogElement: React.ReactElement | undefined;

  const closeDialog = (): void => setDialog(null);

  if (dialog === "nickname") {
    dialogElement = (
      <ChangeNicknameDialog
        open
        close={closeDialog}
        onProcess={async (newNickname) => {
          await getHttpUserClient().patch(username, { nickname: newNickname });
          setReloadKey(reloadKey + 1);
        }}
      />
    );
  } else if (dialog === "avatar") {
    dialogElement = (
      <ChangeAvatarDialog
        open
        close={closeDialog}
        process={async (file) => {
          await getHttpUserClient().putAvatar(username, file);
          setReloadKey(reloadKey + 1);
        }}
      />
    );
  }

  return (
    <>
      <TimelinePageTemplate
        name={`@${username}`}
        UiComponent={UserPageUI}
        onManage={(item) => setDialog(item)}
        notFoundI18nKey="timeline.userNotExist"
        reloadKey={reloadKey}
        onReload={() => setReloadKey(reloadKey + 1)}
      />
      {dialogElement}
    </>
  );
};

export default UserPage;
