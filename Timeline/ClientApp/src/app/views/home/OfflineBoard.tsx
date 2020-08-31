import React from "react";
import { Link } from "react-router-dom";
import { Trans } from "react-i18next";

import { getAllCachedTimelineNames } from "@/services/timeline";
import UserTimelineLogo from "../common/UserTimelineLogo";
import TimelineLogo from "../common/TimelineLogo";

export interface OfflineBoardProps {
  onReload: () => void;
}

const OfflineBoard: React.FC<OfflineBoardProps> = ({ onReload }) => {
  const [timelines, setTimelines] = React.useState<string[]>([]);

  React.useEffect(() => {
    let subscribe = true;
    void getAllCachedTimelineNames().then((t) => {
      if (subscribe) setTimelines(t);
    });
    return () => {
      subscribe = false;
    };
  });

  return (
    <>
      <Trans i18nKey="home.offlinePrompt">
        0
        <a
          href="#"
          onClick={(e) => {
            onReload();
            e.preventDefault();
          }}
        >
          1
        </a>
        2
      </Trans>
      {timelines.map((timeline) => {
        const isPersonal = timeline.startsWith("@");
        const url = isPersonal
          ? `/users/${timeline.slice(1)}`
          : `/timelines/${timeline}`;
        return (
          <div key={timeline} className="timeline-board-item">
            {isPersonal ? (
              <UserTimelineLogo className="icon" />
            ) : (
              <TimelineLogo className="icon" />
            )}
            <Link to={url}>{timeline}</Link>
          </div>
        );
      })}
    </>
  );
};

export default OfflineBoard;
