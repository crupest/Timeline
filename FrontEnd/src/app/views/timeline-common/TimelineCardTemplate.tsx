import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";

import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpBookmarkClient } from "@/http/bookmark";

import { useUser } from "@/services/user";
import { pushAlert } from "@/services/alert";
import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import CollapseButton from "../timeline-common/CollapseButton";

export interface TimelineCardTemplateProps
  extends Omit<TimelineCardComponentProps<"">, "operations"> {
  infoArea: React.ReactElement;
  manageArea:
    | { type: "member"; onMember: () => void }
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
}

function TimelineCardTemplate({
  timeline,
  collapse,
  infoArea,
  manageArea,
  toggleCollapse,
  className,
}: TimelineCardTemplateProps): React.ReactElement | null {
  const { t } = useTranslation();

  const user = useUser();

  return (
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
              timeline.isHighlight ? "bi-star-fill" : "bi-star",
              "icon-button text-yellow mr-3"
            )}
            onClick={
              user != null && user.hasHighlightTimelineAdministrationPermission
                ? () => {
                    getHttpHighlightClient()
                      [timeline.isHighlight ? "delete" : "put"](timeline.name)
                      .catch(() => {
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
                  .catch(() => {
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
                          item.color != null ? "text-" + item.color : undefined
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
            <Button variant="outline-primary" onClick={manageArea.onMember}>
              {t("timeline.memberButton")}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

export default TimelineCardTemplate;
