import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";

import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpBookmarkClient } from "@/http/bookmark";

import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import { useIsSmallScreen } from "@/utilities/mediaQuery";

import { TimelinePageCardProps } from "./TimelinePageTemplate";

import CollapseButton from "./CollapseButton";
import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import { MenuItems, PopupMenu } from "../common/Menu";
import FullPage from "../common/FullPage";

export interface TimelineCardTemplateProps extends TimelinePageCardProps {
  infoArea: React.ReactElement;
  manageItems?: MenuItems;
  dialog: string | "property" | "member" | null;
  setDialog: (dialog: "property" | "member" | null) => void;
}

const TimelinePageCardTemplate: React.FC<TimelineCardTemplateProps> = ({
  timeline,
  collapse,
  toggleCollapse,
  infoArea,
  manageItems,
  onReload,
  className,
  dialog,
  setDialog,
}) => {
  const { t } = useTranslation();

  const isSmallScreen = useIsSmallScreen();

  const user = useUser();

  const content = (
    <>
      {infoArea}
      <p className="mb-0">{timeline.description}</p>
      <small className="mt-1 d-block">
        {t(timelineVisibilityTooltipTranslationMap[timeline.visibility])}
      </small>
      <div className="text-right mt-2">
        <i
          className={clsx(
            timeline.isHighlight ? "bi-star-fill" : "bi-star",
            "icon-button text-yellow mr-3"
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
            className={clsx(
              timeline.isBookmark ? "bi-bookmark-fill" : "bi-bookmark",
              "icon-button text-yellow mr-3"
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
          className={"icon-button bi-people text-primary mr-3"}
          onClick={() => setDialog("member")}
        />
        {manageItems != null ? (
          <PopupMenu items={manageItems}>
            <i className="icon-button bi-three-dots-vertical text-primary" />
          </PopupMenu>
        ) : null}
      </div>
    </>
  );

  return (
    <>
      <div className={clsx("cru-card p-2 clearfix", className)}>
        <div className="float-right d-flex align-items-center">
          <CollapseButton collapse={collapse} onClick={toggleCollapse} />
        </div>
        {isSmallScreen ? (
          <FullPage
            onBack={toggleCollapse}
            show={!collapse}
            contentContainerClassName="p-2"
          >
            {content}
          </FullPage>
        ) : (
          <div style={{ display: collapse ? "none" : "block" }}>{content}</div>
        )}
      </div>
      {(() => {
        if (dialog === "member") {
          return (
            <TimelineMemberDialog
              timeline={timeline}
              onClose={() => setDialog(null)}
              open
              onChange={onReload}
            />
          );
        } else if (dialog === "property") {
          return (
            <TimelinePropertyChangeDialog
              timeline={timeline}
              close={() => setDialog(null)}
              open
              onChange={onReload}
            />
          );
        }
      })()}
    </>
  );
};

export default TimelinePageCardTemplate;
