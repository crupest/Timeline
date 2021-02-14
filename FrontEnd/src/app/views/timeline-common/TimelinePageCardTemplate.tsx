import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";

import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpBookmarkClient } from "@/http/bookmark";

import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import { TimelinePageCardProps } from "./TimelinePageTemplate";

import CollapseButton from "./CollapseButton";
import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";

export interface TimelineCardTemplateProps extends TimelinePageCardProps {
  infoArea: React.ReactElement;
  manageArea:
    | { type: "member" }
    | {
        type: "manage";
        items: (
          | {
              type: "button";
              text: string;
              color?: string;
              onClick: () => void;
            }
          | { type: "divider" }
        )[];
      };
  dialog: string | "property" | "member" | null;
  setDialog: (dialog: "property" | "member" | null) => void;
}

const TimelinePageCardTemplate: React.FC<TimelineCardTemplateProps> = ({
  timeline,
  collapse,
  toggleCollapse,
  infoArea,
  manageArea,
  onReload,
  className,
  dialog,
  setDialog,
}) => {
  const { t } = useTranslation();

  const [overrideIsHighlight, setOverrideIsHighlight] = React.useState<
    boolean | null
  >(null);
  const [overrideIsBookmark, setOverrideIsBookmark] = React.useState<
    boolean | null
  >(null);

  const isHighlight = overrideIsHighlight ?? timeline.isHighlight;
  const isBookmark = overrideIsBookmark ?? timeline.isBookmark;

  React.useEffect(() => {
    setOverrideIsHighlight(null);
    setOverrideIsBookmark(null);
  }, [timeline]);

  const user = useUser();

  return (
    <>
      <div className={clsx("cru-card p-2 clearfix", className)}>
        <div className="float-right d-flex align-items-center">
          <CollapseButton collapse={collapse} onClick={toggleCollapse} />
        </div>
        <div style={{ display: collapse ? "none" : "block" }}>
          {infoArea}
          <p className="mb-0">{timeline.description}</p>
          <small className="mt-1 d-block">
            {t(timelineVisibilityTooltipTranslationMap[timeline.visibility])}
          </small>
          <div className="text-right mt-2">
            <i
              className={clsx(
                isHighlight ? "bi-star-fill" : "bi-star",
                "icon-button text-yellow mr-3"
              )}
              onClick={
                user?.hasHighlightTimelineAdministrationPermission
                  ? () => {
                      getHttpHighlightClient()
                        [isHighlight ? "delete" : "put"](timeline.name)
                        .then(
                          () => setOverrideIsHighlight(!isHighlight),
                          () => {
                            pushAlert({
                              message: timeline.isHighlight
                                ? "timeline.removeHighlightFail"
                                : "timeline.addHighlightFail",
                              type: "danger",
                            });
                          }
                        );
                    }
                  : undefined
              }
            />
            {user != null ? (
              <i
                className={clsx(
                  isBookmark ? "bi-bookmark-fill" : "bi-bookmark",
                  "icon-button text-yellow mr-3"
                )}
                onClick={() => {
                  getHttpBookmarkClient()
                    [isBookmark ? "delete" : "put"](timeline.name)
                    .then(
                      () => setOverrideIsBookmark(!isBookmark),
                      () => {
                        pushAlert({
                          message: timeline.isBookmark
                            ? "timeline.removeBookmarkFail"
                            : "timeline.addBookmarkFail",
                          type: "danger",
                        });
                      }
                    );
                }}
              />
            ) : null}
            {manageArea.type === "manage" ? (
              <Dropdown className="d-inline-block">
                <Dropdown.Toggle variant="outline-primary">
                  {t("timeline.manage")}
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  {manageArea.items.map((item, index) => {
                    if (item.type === "divider") {
                      return <Dropdown.Divider key={index} />;
                    } else {
                      return (
                        <Dropdown.Item
                          key={index}
                          onClick={item.onClick}
                          className={
                            item.color != null
                              ? "text-" + item.color
                              : undefined
                          }
                        >
                          {t(item.text)}
                        </Dropdown.Item>
                      );
                    }
                  })}
                </Dropdown.Menu>
              </Dropdown>
            ) : (
              <Button
                variant="outline-primary"
                onClick={() => setDialog("member")}
              >
                {t("timeline.memberButton")}
              </Button>
            )}
          </div>
        </div>
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
