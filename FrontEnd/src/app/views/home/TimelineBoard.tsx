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
  // In height.
  offset?: number;
  // In px.
  arbitraryOffset?: number;
  // If not null, will disable navigation on click.
  actions?: {
    onDelete: () => void;
    onMoveStart: (e: React.PointerEvent) => void;
    onMoving: (e: React.PointerEvent) => void;
    onMoveEnd: (e: React.PointerEvent) => void;
  };
}

const TimelineBoardItem: React.FC<TimelineBoardItemProps> = ({
  timeline,
  arbitraryOffset,
  offset,
  actions,
}) => {
  const { name, title } = timeline;
  const isPersonal = name.startsWith("@");
  const url = isPersonal
    ? `/users/${timeline.owner.username}`
    : `/timelines/${name}`;

  const content = (
    <>
      <div>
        {isPersonal ? (
          <UserTimelineLogo className="icon" />
        ) : (
          <TimelineLogo className="icon" />
        )}
        {title}
        <small className="ml-2 text-secondary">{name}</small>
      </div>
      {actions != null ? (
        <div>
          <i className="bi-trash icon-button text-danger px-2" />
          <i
            className="bi-grip-vertical icon-button text-gray px-2"
            onPointerDown={actions.onMoveStart}
            onPointerUp={actions.onMoveEnd}
            onPointerMove={actions.onMoving}
          />
        </div>
      ) : null}
    </>
  );

  const offsetStyle: React.CSSProperties = {
    translate:
      arbitraryOffset != null
        ? `0 ${arbitraryOffset}px`
        : offset != null
        ? `0 ${offset * 100}%`
        : undefined,
    transition:
      arbitraryOffset == null && offset != null ? "translate 0.5s" : undefined,
  };

  return actions == null ? (
    <Link to={url} className="timeline-board-item">
      {content}
    </Link>
  ) : (
    <div style={offsetStyle} className="timeline-board-item">
      {content}
    </div>
  );
};

export interface TimelineBoardUIProps {
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

  const [moveState, setMoveState] = React.useState<null | {
    index: number;
    offset: number;
  }>(null);

  return (
    <div className={clsx("timeline-board", className)}>
      <div className="timeline-board-header">
        {title != null && <h3>{title}</h3>}
        {
          editable &&
            (editing ? (
              <div
                className="flat-button text-primary"
                onClick={() => {
                  setEditing(false);
                }}
              >
                Done
              </div>
            ) : (
              <div
                className="flat-button text-primary"
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
              <TimelineBoardItem
                key={timeline.name}
                timeline={timeline}
                actions={
                  editHandler != null && editing
                    ? {
                        onDelete: () => {
                          //TODO: Implement this.
                        },
                        onMoveStart: () => {
                          //TODO: Implement this.
                        },
                        onMoving: () => {
                          //TODO: Implement this.
                        },
                        onMoveEnd: () => {
                          //TODO: Implement this.
                        },
                      }
                    : undefined
                }
              />
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
  editHandler?: {
    onDelete: (timeline: string) => Promise<void>;
  };
}

const TimelineBoard: React.FC<TimelineBoardProps> = ({
  className,
  title,
  load,
  editHandler,
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
      editHandler={editHandler}
    />
  );
};

export default TimelineBoard;
