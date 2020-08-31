import React from "react";
import clsx from "clsx";
import { Link } from "react-router-dom";
import { Spinner } from "reactstrap";
import { Trans } from "react-i18next";

import { TimelineInfo } from "@/services/timeline";
import TimelineLogo from "../common/TimelineLogo";
import UserTimelineLogo from "../common/UserTimelineLogo";

export interface TimelineBoardProps {
  title?: string;
  timelines: TimelineInfo[] | "offline" | "loading";
  onReload: () => void;
  className?: string;
}

const TimelineBoard: React.FC<TimelineBoardProps> = (props) => {
  const { title, timelines, className } = props;

  return (
    <div className={clsx("timeline-board", className)}>
      {title != null && <h3 className="text-center">{title}</h3>}
      {(() => {
        if (timelines === "loading") {
          return (
            <div className="d-flex flex-grow-1 justify-content-center align-items-center">
              <Spinner color="primary" />
            </div>
          );
        } else if (timelines === "offline") {
          return (
            <div className="d-flex flex-grow-1 justify-content-center align-items-center">
              <Trans i18nKey="loadFailReload" parent="div">
                0
                <a
                  href="#"
                  onClick={(e) => {
                    props.onReload();
                    e.preventDefault();
                  }}
                >
                  1
                </a>
                2
              </Trans>
            </div>
          );
        } else {
          return timelines.map((timeline) => {
            const { name } = timeline;
            const isPersonal = name.startsWith("@");
            const url = isPersonal
              ? `/users/${timeline.owner.username}`
              : `/timelines/${name}`;
            return (
              <div key={name} className="timeline-board-item">
                {isPersonal ? (
                  <UserTimelineLogo className="icon" />
                ) : (
                  <TimelineLogo className="icon" />
                )}
                <Link to={url}>{name}</Link>
              </div>
            );
          });
        }
      })()}
    </div>
  );
};

export default TimelineBoard;
