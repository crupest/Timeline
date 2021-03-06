import React from "react";

import { TimelinePageCardProps } from "../timeline-common/TimelinePageTemplate";
import TimelinePageCardTemplate from "../timeline-common/TimelinePageCardTemplate";

import UserAvatar from "../common/user/UserAvatar";
import TimelineDeleteDialog from "./TimelineDeleteDialog";

const TimelineCard: React.FC<TimelinePageCardProps> = (props) => {
  const { timeline } = props;

  const [dialog, setDialog] = React.useState<
    "member" | "property" | "delete" | null
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
              <small className="ml-3 text-secondary">
                @{timeline.owner.username}
              </small>
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
                { type: "divider" },
                {
                  type: "button",
                  onClick: () => setDialog("delete"),
                  color: "danger",
                  text: "timeline.manageItem.delete",
                },
              ]
            : undefined
        }
        dialog={dialog}
        setDialog={setDialog}
        {...props}
      />
      {(() => {
        if (dialog === "delete") {
          return (
            <TimelineDeleteDialog
              timeline={timeline}
              open
              close={() => setDialog(null)}
            />
          );
        }
      })()}
    </>
  );
};

export default TimelineCard;
