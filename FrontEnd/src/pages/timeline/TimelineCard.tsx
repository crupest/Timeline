import { useState } from "react";
import { HubConnectionState } from "@microsoft/signalr";

import { useUser } from "~src/services/user";

import { HttpTimelineInfo } from "~src/http/timeline";
import { getHttpBookmarkClient } from "~src/http/bookmark";

import { pushAlert } from "~src/components/alert";
import { useMobile } from "~src/components/hooks";
import { Dialog, DialogProvider, useDialog } from "~src/components/dialog";
import UserAvatar from "~src/components/user/UserAvatar";
import PopupMenu from "~src/components/menu/PopupMenu";
import FullPageDialog from "~src/components/dialog/FullPageDialog";
import Card from "~src/components/Card";
import TimelineDeleteDialog from "./TimelineDeleteDialog";
import ConnectionStatusBadge from "./ConnectionStatusBadge";
import CollapseButton from "./CollapseButton";
import TimelineMember from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import IconButton from "~src/components/button/IconButton";

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

  const { controller, createDialogSwitch } = useDialog({
    member: (
      <Dialog>
        <TimelineMember timeline={timeline} onChange={onReload} />
      </Dialog>
    ),
    property: (
      <TimelinePropertyChangeDialog timeline={timeline} onChange={onReload} />
    ),
    delete: <TimelineDeleteDialog timeline={timeline} />,
  });

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
      <DialogProvider controller={controller} />
    </Card>
  );
}
