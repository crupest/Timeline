import React from "react";
import { useTranslation } from "react-i18next";
import classnames from "classnames";
import { HubConnectionState } from "@microsoft/signalr";

import { useIsSmallScreen } from "@/utilities/mediaQuery";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";
import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";
import { HttpTimelineInfo } from "@/http/timeline";
import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpBookmarkClient } from "@/http/bookmark";

import UserAvatar from "../common/user/UserAvatar";
import PopupMenu from "../common/menu/PopupMenu";
import FullPageDialog from "../common/dailog/FullPageDialog";
import Card from "../common/Card";
import TimelineDeleteDialog from "./TimelineDeleteDialog";
import ConnectionStatusBadge from "./ConnectionStatusBadge";
import CollapseButton from "./CollapseButton";
import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";

export interface TimelinePageCardProps {
  timeline: HttpTimelineInfo;
  collapse: boolean;
  toggleCollapse: () => void;
  connectionStatus: HubConnectionState;
  className?: string;
  onReload: () => void;
}

const TimelineCard: React.FC<TimelinePageCardProps> = (props) => {
  const {
    timeline,
    collapse,
    toggleCollapse,
    connectionStatus,
    onReload,
    className,
  } = props;

  const { t } = useTranslation();

  const [dialog, setDialog] = React.useState<
    "member" | "property" | "delete" | null
  >(null);

  const isSmallScreen = useIsSmallScreen();

  const user = useUser();

  const content = (
    <>
      <h3 className="cru-color-primary d-inline-block align-middle">
        {timeline.title}
        <small className="ms-3 cru-color-secondary">{timeline.name}</small>
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
        {t(timelineVisibilityTooltipTranslationMap[timeline.visibility])}
      </small>
      <div className="mt-2 cru-text-end">
        <i
          className={classnames(
            timeline.isHighlight ? "bi-star-fill" : "bi-star",
            "icon-button cru-color-primary me-3"
          )}
          onClick={
            user?.hasHighlightTimelineAdministrationPermission
              ? () => {
                  getHttpHighlightClient()
                    [timeline.isHighlight ? "delete" : "put"](timeline.name)
                    .then(onReload, () => {
                      pushAlert({
                        message: timeline.isHighlight
                          ? "timeline.removeHighlightFail"
                          : "timeline.addHighlightFail",
                        type: "danger",
                      });
                    });
                }
              : undefined
          }
        />
        {user != null ? (
          <i
            className={classnames(
              timeline.isBookmark ? "bi-bookmark-fill" : "bi-bookmark",
              "icon-button cru-color-primary me-3"
            )}
            onClick={() => {
              getHttpBookmarkClient()
                [timeline.isBookmark ? "delete" : "put"](timeline.name)
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
        <i
          className={"icon-button bi-people cru-color-primary me-3"}
          onClick={() => setDialog("member")}
        />
        {timeline.manageable ? (
          <PopupMenu
            items={[
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
            ]}
            containerClassName="d-inline"
          >
            <i className="icon-button bi-three-dots-vertical cru-color-primary" />
          </PopupMenu>
        ) : null}
      </div>
    </>
  );

  return (
    <>
      <Card className={classnames("p-2 cru-clearfix", className)}>
        <div
          className={classnames(
            "cru-float-right d-flex align-items-center",
            !collapse && "ms-3"
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
        onClose={() => setDialog(null)}
        open={dialog === "member"}
        onChange={onReload}
      />
      <TimelinePropertyChangeDialog
        timeline={timeline}
        close={() => setDialog(null)}
        open={dialog === "property"}
        onChange={onReload}
      />
      <TimelineDeleteDialog
        timeline={timeline}
        open={dialog === "delete"}
        close={() => setDialog(null)}
      />
    </>
  );
};

export default TimelineCard;
