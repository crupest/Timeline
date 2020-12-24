import React from "react";
import clsx from "clsx";
import { Link } from "react-router-dom";
import { Trans } from "react-i18next";
import { Spinner } from "react-bootstrap";

import { TimelineInfo } from "@/services/timeline";
import TimelineLogo from "../common/TimelineLogo";
import UserTimelineLogo from "../common/UserTimelineLogo";
import { HttpTimelineInfo } from "@/http/timeline";

interface TimelineBoardItemProps {
  timeline: HttpTimelineInfo;
  // If not null, will disable navigation on click.
  actions?: {
    onDelete: () => void;
    onMove: (e: React.MouseEvent) => void;
  };
}

const TimelineBoardItem: React.FC<TimelineBoardItemProps> = ({
  timeline,
  actions,
}) => {
  const { name, title } = timeline;
  const isPersonal = name.startsWith("@");
  const url = isPersonal
    ? `/users/${timeline.owner.username}`
    : `/timelines/${name}`;

  const content = (
    <>
      {isPersonal ? (
        <UserTimelineLogo className="icon" />
      ) : (
        <TimelineLogo className="icon" />
      )}
      {title}
      <small className="ml-2 text-secondary">{name}</small>
    </>
  );

  return actions == null ? (
    <Link to={url} className="timeline-board-item">
      {content}
    </Link>
  ) : (
    <div className="timeline-board-item">{content}</div>
  );
};

interface TimelineBoardUIProps {
  title?: string;
  timelines: TimelineInfo[] | "offline" | "loading";
  onReload: () => void;
  className?: string;
  editHandler?: {
    onDelete: (timeline: string) => Promise<void>;
  };
}

const TimelineBoardUI: React.FC<TimelineBoardUIProps> = (props) => {
  const { title, timelines, className, editHandler } = props;

  const editable = editHandler != null;

  const [editing, setEditing] = React.useState<boolean>(false);

  return (
    <div className={clsx("timeline-board", className)}>
      <div>
        {title != null && <h3 className="text-center">{title}</h3>}
        {
          editable &&
            (editing ? (
              <div
                onClick={() => {
                  setEditing(false);
                }}
              >
                Done
              </div>
            ) : (
              <div
                onClick={() => {
                  setEditing(true);
                }}
              >
                Edit
              </div>
            )) // TODO: i18n
        }
      </div>
      {(() => {
        if (timelines === "loading") {
          return (
            <div className="d-flex flex-grow-1 justify-content-center align-items-center">
              <Spinner variant="primary" animation="border" />
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
            return (
              <TimelineBoardItem key={timeline.name} timeline={timeline} />
            );
          });
        }
      })()}
    </div>
  );
};

export interface TimelineBoardProps {
  title?: string;
  className?: string;
  load: () => Promise<TimelineInfo[]>;
}

const TimelineBoard: React.FC<TimelineBoardProps> = ({
  className,
  title,
  load,
}) => {
  const [timelines, setTimelines] = React.useState<
    TimelineInfo[] | "offline" | "loading"
  >("loading");

  React.useEffect(() => {
    let subscribe = true;
    if (timelines === "loading") {
      void load().then(
        (timelines) => {
          if (subscribe) {
            setTimelines(timelines);
          }
        },
        () => {
          setTimelines("offline");
        }
      );
    }
    return () => {
      subscribe = false;
    };
  }, [load, timelines]);

  return (
    <TimelineBoardUI
      title={title}
      className={className}
      timelines={timelines}
      onReload={() => {
        setTimelines("loading");
      }}
    />
  );
};

export default TimelineBoard;
