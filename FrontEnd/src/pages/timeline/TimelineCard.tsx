import { useState } from "react";
import classnames from "classnames";
import { HubConnectionState } from "@microsoft/signalr";

import { useIsSmallScreen } from "@/utilities/hooks";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";
import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";
import { HttpTimelineInfo } from "@/http/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";

import { useC } from "@/views/common/common";
import { useDialog } from "@/views/common/dialog";
import UserAvatar from "@/views/common/user/UserAvatar";
import PopupMenu from "@/views/common/menu/PopupMenu";
import FullPageDialog from "@/views/common/dialog/FullPageDialog";
import Card from "@/views/common/Card";
import TimelineDeleteDialog from "./TimelineDeleteDialog";
import ConnectionStatusBadge from "./ConnectionStatusBadge";
import CollapseButton from "./CollapseButton";
import { TimelineMemberDialog } from "./TimelineMember";
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

  const c = useC();

  const [collapse, setCollapse] = useState(true);
  const toggleCollapse = (): void => {
    setCollapse((o) => !o);
  };

  const isSmallScreen = useIsSmallScreen();

  const { createDialogSwitch, dialog, dialogPropsMap, switchDialog } =
    useDialog(["member", "property", "delete"]);

  const content = (
    <div className="cru-primary">
      <h3 className="timeline-card-title">
        {timeline.title}
        <small className="timeline-card-title-name">{timeline.nameV2}</small>
      </h3>
      <div>
        <UserAvatar
          username={timeline.owner.username}
          className="cru-avatar small cru-round me-3"
        />
        {timeline.owner.nickname}
        <small className="ms-3 cru-color-secondary">
          @{timeline.owner.username}
        </small>
      </div>
      <p className="mb-0">{timeline.description}</p>
      <small className="mt-1 d-block">
        {c(timelineVisibilityTooltipTranslationMap[timeline.visibility])}
      </small>
      <div className="mt-2 cru-text-end">
        {user != null ? (
          <IconButton
            icon={timeline.isBookmark ? "bookmark-fill" : "bookmark"}
            className="me-3"
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
        ) : null}
        <IconButton
          icon="people"
          className="me-3"
          onClick={createDialogSwitch("member")}
        />
        {timeline.manageable ? (
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
            <IconButton icon="three-dots-vertical" />
          </PopupMenu>
        ) : null}
      </div>
    </div>
  );

  return (
    <>
      <Card className="timeline-card">
        <div
          className={classnames(
            "cru-float-right d-flex align-items-center",
            !collapse && "ms-3",
          )}
        >
          <ConnectionStatusBadge status={connectionStatus} className="me-2" />
          <CollapseButton collapse={collapse} onClick={toggleCollapse} />
        </div>
        {isSmallScreen ? (
          <FullPageDialog
            onBack={toggleCollapse}
            show={!collapse}
            contentContainerClassName="p-2"
          >
            {content}
          </FullPageDialog>
        ) : (
          <div style={{ display: collapse ? "none" : "inline" }}>{content}</div>
        )}
      </Card>
      <TimelineMemberDialog
        timeline={timeline}
        onChange={onReload}
        {...dialogPropsMap["member"]}
      />
      <TimelinePropertyChangeDialog
        timeline={timeline}
        onChange={onReload}
        {...dialogPropsMap["property"]}
      />
      <TimelineDeleteDialog timeline={timeline} {...dialogPropsMap["delete"]} />
    </>
  );
}
