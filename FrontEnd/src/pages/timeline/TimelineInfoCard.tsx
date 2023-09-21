import { useState } from "react";
import { HubConnectionState } from "@microsoft/signalr";

import { useUser } from "~src/services/user";

import { HttpTimelineInfo } from "~src/http/timeline";
import { getHttpBookmarkClient } from "~src/http/bookmark";

import { pushAlert } from "~src/components/alert";
import { useMobile } from "~src/components/hooks";
import { IconButton } from "~src/components/button";
import { Dialog, FullPageDialog, useDialog } from "~src/components/dialog";
import UserAvatar from "~src/components/user/UserAvatar";
import PopupMenu from "~src/components/menu/PopupMenu";
import Card from "~src/components/Card";

import TimelineDeleteDialog from "./TimelineDeleteDialog";
import ConnectionStatusBadge from "./ConnectionStatusBadge";
import TimelineMember from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";

import "./TimelineInfoCard.css";

function CollapseButton({
  collapse,
  onClick,
  className,
}: {
  collapse: boolean;
  onClick: () => void;
  className?: string;
}) {
  return (
    <IconButton
      color="primary"
      icon={collapse ? "info-circle" : "x-circle"}
      onClick={onClick}
      className={className}
    />
  );
}

interface TimelineInfoCardProps {
  timeline: HttpTimelineInfo;
  connectionStatus: HubConnectionState;
  onReload: () => void;
}

function TimelineInfoContent({
  timeline,
  onReload,
}: Omit<TimelineInfoCardProps, "connectionStatus">) {
  const user = useUser();

  const { createDialogSwitch, dialogPropsMap } = useDialog([
    "member",
    "property",
    "delete",
  ]);

  return (
    <div>
      <h3 className="timeline-card-title">
        {timeline.title}
        <small className="timeline-card-title-name">{timeline.nameV2}</small>
      </h3>
      <div className="timeline-card-user">
        <UserAvatar
          username={timeline.owner.username}
          className="timeline-card-user-avatar"
        />
        <span className="timeline-card-user-nickname">
          {timeline.owner.nickname}
        </span>
        <small className="timeline-card-user-username">
          @{timeline.owner.username}
        </small>
      </div>
      <p className="timeline-card-description">{timeline.description}</p>
      <div className="timeline-card-buttons">
        {user && (
          <IconButton
            icon={timeline.isBookmark ? "bookmark-fill" : "bookmark"}
            color="primary"
            className="timeline-card-button"
            onClick={() => {
              getHttpBookmarkClient()
                [timeline.isBookmark ? "delete" : "post"](
                  user.username,
                  timeline.owner.username,
                  timeline.nameV2,
                )
                .then(onReload, () => {
                  pushAlert({
                    message: timeline.isBookmark
                      ? "timeline.removeBookmarkFail"
                      : "timeline.addBookmarkFail",
                    color: "danger",
                  });
                });
            }}
          />
        )}
        <IconButton
          icon="people"
          color="primary"
          className="timeline-card-button"
          onClick={createDialogSwitch("member")}
        />
        {timeline.manageable && (
          <PopupMenu
            items={[
              {
                type: "button",
                text: "timeline.manageItem.property",
                onClick: createDialogSwitch("property"),
              },
              { type: "divider" },
              {
                type: "button",
                onClick: createDialogSwitch("delete"),
                color: "danger",
                text: "timeline.manageItem.delete",
              },
            ]}
            containerClassName="d-inline"
          >
            <IconButton
              color="primary"
              className="timeline-card-button"
              icon="three-dots-vertical"
            />
          </PopupMenu>
        )}
      </div>

      <Dialog {...dialogPropsMap["member"]}>
        <TimelineMember timeline={timeline} onChange={onReload} />
      </Dialog>

      <TimelinePropertyChangeDialog
        timeline={timeline}
        onChange={onReload}
        {...dialogPropsMap["property"]}
      />
      <TimelineDeleteDialog timeline={timeline} {...dialogPropsMap["delete"]} />
    </div>
  );
}

export default function TimelineInfoCard(props: TimelineInfoCardProps) {
  const { timeline, connectionStatus, onReload } = props;

  const [collapse, setCollapse] = useState(true);

  const isMobile = useMobile((mobile) => {
    if (!mobile) {
      switchDialog(null);
    } else {
      setCollapse(true);
    }
  });

  const { switchDialog, dialogPropsMap } = useDialog(["full-page"], {
    onClose: {
      "full-page": () => {
        setCollapse(true);
      },
    },
  });

  return (
    <Card
      color="secondary"
      className={`timeline-card timeline-card-${
        collapse ? "collapse" : "expand"
      }`}
    >
      <div className="timeline-card-top-right-area">
        <ConnectionStatusBadge status={connectionStatus} />
        <CollapseButton
          collapse={collapse}
          onClick={() => {
            const open = collapse;
            setCollapse(!open);
            if (isMobile && open) {
              switchDialog("full-page");
            }
          }}
        />
      </div>
      {!collapse && !isMobile && (
        <TimelineInfoContent timeline={timeline} onReload={onReload} />
      )}
      <FullPageDialog {...dialogPropsMap["full-page"]}>
        <TimelineInfoContent timeline={timeline} onReload={onReload} />
      </FullPageDialog>
    </Card>
  );
}
