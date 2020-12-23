import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { Dropdown, Button } from "react-bootstrap";
import Svg from "react-inlinesvg";
import starIcon from "bootstrap-icons/icons/star.svg";
import bookmarkIcon from "bootstrap-icons/icons/bookmark.svg";

import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";

import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";
import SyncStatusBadge from "../timeline-common/SyncStatusBadge";
import CollapseButton from "../timeline-common/CollapseButton";

export interface TimelineCardTemplateProps
  extends Omit<TimelineCardComponentProps<"">, "onManage" | "onMember"> {
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
  onBookmark,
  onHighlight,
  toggleCollapse,
  syncStatus,
  className,
}: TimelineCardTemplateProps): React.ReactElement | null {
  const { t } = useTranslation();

  return (
    <div className={clsx("cru-card p-2 clearfix", className)}>
      <div className="float-right d-flex align-items-center">
        <SyncStatusBadge status={syncStatus} className="mr-2" />
        <CollapseButton collapse={collapse} onClick={toggleCollapse} />
      </div>
      <div style={{ display: collapse ? "none" : "block" }}>
        {infoArea}
        <p className="mb-0">{timeline.description}</p>
        <small className="mt-1 d-block">
          {t(timelineVisibilityTooltipTranslationMap[timeline.visibility])}
        </small>
        <div className="text-right mt-2">
          {onHighlight != null ? (
            <Svg
              src={starIcon}
              className="icon-button text-yellow mr-3"
              onClick={onHighlight}
            />
          ) : null}
          {onBookmark != null ? (
            <Svg
              src={bookmarkIcon}
              className="icon-button text-yellow mr-3"
              onClick={onBookmark}
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
