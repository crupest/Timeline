import { useState } from "react";
import { HubConnectionState } from "@microsoft/signalr";

import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";

import { HttpTimelineInfo } from "@/http/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";

import { useMobile } from "@/views/common/common";
import { Dialog, useDialog } from "@/views/common/dialog";
import UserAvatar from "@/views/common/user/UserAvatar";
import PopupMenu from "@/views/common/menu/PopupMenu";
import FullPageDialog from "@/views/common/dialog/FullPageDialog";
import Card from "@/views/common/Card";
import TimelineDeleteDialog from "./TimelineDeleteDialog";
import ConnectionStatusBadge from "./ConnectionStatusBadge";
import CollapseButton from "./CollapseButton";
import TimelineMember from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import IconButton from "@/views/common/button/IconButton";

import "./TimelineCard.css";

interface TimelinePageCardProps {
  timeline: HttpTimelineInfo;
  connectionStatus: HubConnectionState;
  onReload: () => void;
}

export default function TimelineCard(props: TimelinePageCardProps) {
  const { timeline, connectionStatus, onReload } = props;

  const user = useUser();

  const [collapse, setCollapse] = useState(true);
  const toggleCollapse = (): void => {
    setCollapse((o) => !o);
  };

  const isMobile = useMobile();

  const { createDialogSwitch, dialogPropsMap } = useDialog([
    "member",
    "property",
    "delete",
  ]);

  const content = (
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
                    type: "danger",
                  });
                });
            }}
          />
        )}
        <IconButton
          icon="people"
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
    </div>
  );

  return (
    <Card
      color="secondary"
      className={`timeline-card timeline-card-${
        collapse ? "collapse" : "expand"
      }`}
    >
      <div className="timeline-card-top-right-area">
        <ConnectionStatusBadge status={connectionStatus} />
        <CollapseButton collapse={collapse} onClick={toggleCollapse} />
      </div>
      {isMobile ? (
        <FullPageDialog
          onBack={toggleCollapse}
          show={!collapse}
          contentContainerClassName="p-2"
        >
          {content}
        </FullPageDialog>
      ) : (
        <div style={{ display: collapse ? "none" : "block" }}>{content}</div>
      )}
      <Dialog {...dialogPropsMap["member"]}>
        <TimelineMember timeline={timeline} onChange={onReload} />
      </Dialog>
      <TimelinePropertyChangeDialog
        timeline={timeline}
        onChange={onReload}
        {...dialogPropsMap["property"]}
      />
      <TimelineDeleteDialog timeline={timeline} {...dialogPropsMap["delete"]} />
    </Card>
  );
}
