import React from "react";

import TimelinePageCardTemplate, {
  TimelineCardTemplateProps,
} from "../timeline-common/TimelinePageCardTemplate";
import { TimelinePageCardProps } from "../timeline-common/TimelinePageTemplate";
import UserAvatar from "../common/user/UserAvatar";
import ChangeNicknameDialog from "./ChangeNicknameDialog";
import { getHttpUserClient } from "@/http/user";
import ChangeAvatarDialog from "./ChangeAvatarDialog";

const UserCard: React.FC<TimelinePageCardProps> = (props) => {
  const { timeline, onReload } = props;

  const [dialog, setDialog] = React.useState<
    "member" | "property" | "avatar" | "nickname" | null
  >(null);

  return (
    <>
      <TimelinePageCardTemplate
        infoArea={
          <>
            <h3 className="text-primary d-inline-block align-middle">
              {timeline.title}
              <small className="ml-3 text-secondary">{timeline.name}</small>
            </h3>
            <div className="align-middle">
              <UserAvatar
                username={timeline.owner.username}
                className="avatar small rounded-circle mr-3"
              />
              {timeline.owner.nickname}
            </div>
          </>
        }
        manageArea={((): TimelineCardTemplateProps["manageArea"] => {
          if (!timeline.manageable) {
            return { type: "member" };
          } else {
            return {
              type: "manage",
              items: [
                {
                  type: "button",
                  text: "timeline.manageItem.nickname",
                  onClick: () => setDialog("nickname"),
                },
                {
                  type: "button",
                  text: "timeline.manageItem.avatar",
                  onClick: () => setDialog("avatar"),
                },
                {
                  type: "button",
                  text: "timeline.manageItem.property",
                  onClick: () => setDialog("property"),
                },
                {
                  type: "button",
                  text: "timeline.manageItem.member",
                  onClick: () => setDialog("member"),
                },
              ],
            };
          }
        })()}
        dialog={dialog}
        setDialog={setDialog}
        {...props}
      />
      {(() => {
        // TODO: Move this two to settings.
        if (dialog === "nickname") {
          return (
            <ChangeNicknameDialog
              open
              close={() => setDialog(null)}
              onProcess={async (newNickname) => {
                await getHttpUserClient().patch(timeline.owner.username, {
                  nickname: newNickname,
                });
                onReload();
              }}
            />
          );
        } else if (dialog === "avatar") {
          return (
            <ChangeAvatarDialog
              open
              close={() => setDialog(null)}
              process={async (file) => {
                await getHttpUserClient().putAvatar(
                  timeline.owner.username,
                  file
                );
                onReload();
              }}
            />
          );
        }
      })()}
    </>
  );
};

export default UserCard;
