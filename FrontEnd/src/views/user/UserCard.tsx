import React from "react";

import TimelinePageCardTemplate from "../timeline-common/TimelinePageCardTemplate";
import { TimelinePageCardProps } from "../timeline-common/TimelinePageTemplate";
import UserAvatar from "../common/user/UserAvatar";

const UserCard: React.FC<TimelinePageCardProps> = (props) => {
  const { timeline } = props;

  const [dialog, setDialog] = React.useState<"member" | "property" | null>(
    null
  );

  return (
    <>
      <TimelinePageCardTemplate
        infoArea={
          <>
            <h3 className="cru-color-primary d-inline-block">
              {timeline.title}
              <small className="ms-3 cru-color-secondary">
                {timeline.name}
              </small>
            </h3>
            <div>
              <UserAvatar
                username={timeline.owner.username}
                className="cru-avatar small cru-round me-3"
              />
              {timeline.owner.nickname}
            </div>
          </>
        }
        manageItems={
          timeline.manageable
            ? [
                {
                  type: "button",
                  text: "timeline.manageItem.property",
                  onClick: () => setDialog("property"),
                },
              ]
            : undefined
        }
        dialog={dialog}
        setDialog={setDialog}
        {...props}
      />
    </>
  );
};

export default UserCard;
