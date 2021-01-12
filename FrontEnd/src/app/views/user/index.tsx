import React, { useState } from "react";
import { useParams } from "react-router";

import { userInfoService } from "@/services/user";

import TimelinePageTemplate from "../timeline-common/TimelinePageTemplate";

import UserPageUI from "./UserPageUI";
import { PersonalTimelineManageItem } from "./UserInfoCard";
import ChangeNicknameDialog from "./ChangeNicknameDialog";
import ChangeAvatarDialog from "./ChangeAvatarDialog";

const UserPage: React.FC = (_) => {
  const { username } = useParams<{ username: string }>();

  const [dialog, setDialog] = useState<null | PersonalTimelineManageItem>(null);

  let dialogElement: React.ReactElement | undefined;

  const closeDialog = (): void => setDialog(null);

  if (dialog === "nickname") {
    dialogElement = (
      <ChangeNicknameDialog
        open
        close={closeDialog}
        onProcess={(newNickname) =>
          userInfoService.setNickname(username, newNickname)
        }
      />
    );
  } else if (dialog === "avatar") {
    dialogElement = (
      <ChangeAvatarDialog
        open
        close={closeDialog}
        process={(file) => userInfoService.setAvatar(username, file)}
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
      />
      {dialogElement}
    </>
  );
};

export default UserPage;
