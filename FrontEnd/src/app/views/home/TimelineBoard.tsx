import React from "react";
import clsx from "clsx";
import { Link } from "react-router-dom";
import { Trans, useTranslation } from "react-i18next";
import { Spinner } from "react-bootstrap";

import { HttpTimelineInfo } from "@/http/timeline";

import TimelineLogo from "../common/TimelineLogo";
import UserTimelineLogo from "../common/UserTimelineLogo";

interface TimelineBoardItemProps {
  timeline: HttpTimelineInfo;
  // In height.
  offset?: number;
  // In px.
  arbitraryOffset?: number;
  // If not null, will disable navigation on click.
  actions?: {
    onDelete: () => void;
    onMove: {
      start: (e: React.PointerEvent) => void;
      moving: (e: React.PointerEvent) => void;
      end: (e: React.PointerEvent) => void;
    };
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
      {isPersonal ? (
        <UserTimelineLogo className="icon" />
      ) : (
        <TimelineLogo className="icon" />
      )}
      <span className="title">{title}</span>
      <small className="ml-2 text-secondary">{name}</small>
      <span className="flex-grow-1"></span>
      {actions != null ? (
        <div className="right">
          <i
            className="bi-trash icon-button text-danger px-2"
            onClick={actions.onDelete}
          />
          <i
            className="bi-grip-vertical icon-button text-gray px-2 touch-action-none"
            onPointerDown={(e) => {
              e.currentTarget.setPointerCapture(e.pointerId);
              actions.onMove.start(e);
            }}
            onPointerUp={(e) => {
              actions.onMove.end(e);
              try {
                e.currentTarget.releasePointerCapture(e.pointerId);
              } catch (_) {
                void null;
              }
            }}
            onPointerMove={actions.onMove.moving}
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
    transition: offset != null ? "translate 0.5s" : undefined,
    zIndex: arbitraryOffset != null ? 1 : undefined,
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

interface TimelineBoardItemContainerProps {
  timelines: HttpTimelineInfo[];
  editHandler?: {
    // offset may exceed index range plusing index.
    onMove: (timeline: string, index: number, offset: number) => void;
    onDelete: (timeline: string) => void;
  };
}

const TimelineBoardItemContainer: React.FC<TimelineBoardItemContainerProps> = ({
  timelines,
  editHandler,
}) => {
  const [moveState, setMoveState] = React.useState<null | {
    index: number;
    offset: number;
    startPointY: number;
  }>(null);

  return (
    <>
      {timelines.map((timeline, index) => {
        const height = 48;

        let offset: number | undefined = undefined;
        let arbitraryOffset: number | undefined = undefined;
        if (moveState != null) {
          if (index === moveState.index) {
            arbitraryOffset = moveState.offset;
          } else {
            if (moveState.offset >= 0) {
              const offsetCount = Math.round(moveState.offset / height);
              if (
                index > moveState.index &&
                index <= moveState.index + offsetCount
              ) {
                offset = -1;
              } else {
                offset = 0;
              }
            } else {
              const offsetCount = Math.round(-moveState.offset / height);
              if (
                index < moveState.index &&
                index >= moveState.index - offsetCount
              ) {
                offset = 1;
              } else {
                offset = 0;
              }
            }
          }
        }

        return (
          <TimelineBoardItem
            key={timeline.name}
            timeline={timeline}
            offset={offset}
            arbitraryOffset={arbitraryOffset}
            actions={
              editHandler != null
                ? {
                    onDelete: () => {
                      editHandler.onDelete(timeline.name);
                    },
                    onMove: {
                      start: (e) => {
                        if (moveState != null) return;
                        setMoveState({
                          index,
                          offset: 0,
                          startPointY: e.clientY,
                        });
                      },
                      moving: (e) => {
                        if (moveState == null) return;
                        setMoveState({
                          index,
                          offset: e.clientY - moveState.startPointY,
                          startPointY: moveState.startPointY,
                        });
                      },
                      end: () => {
                        if (moveState != null) {
                          const offsetCount = Math.round(
                            moveState.offset / height
                          );
                          editHandler.onMove(
                            timeline.name,
                            moveState.index,
                            offsetCount
                          );
                        }
                        setMoveState(null);
                      },
                    },
                  }
                : undefined
            }
          />
        );
      })}
    </>
  );
};

interface TimelineBoardUIProps {
  title?: string;
  timelines: HttpTimelineInfo[] | "offline" | "loading";
  onReload: () => void;
  className?: string;
  editHandler?: {
    onMove: (timeline: string, index: number, offset: number) => void;
    onDelete: (timeline: string) => void;
  };
}

const TimelineBoardUI: React.FC<TimelineBoardUIProps> = (props) => {
  const { title, timelines, className, editHandler } = props;

  const { t } = useTranslation();

  const editable = editHandler != null;

  const [editing, setEditing] = React.useState<boolean>(false);

  return (
    <div className={clsx("timeline-board", className)}>
      <div className="timeline-board-header">
        {title != null && <h3>{title}</h3>}
        {editable &&
          (editing ? (
            <div
              className="flat-button text-primary"
              onClick={() => {
                setEditing(false);
              }}
            >
              {t("done")}
            </div>
          ) : (
            <div
              className="flat-button text-primary"
              onClick={() => {
                setEditing(true);
              }}
            >
              {t("edit")}
            </div>
          ))}
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
          return (
            <TimelineBoardItemContainer
              timelines={timelines}
              editHandler={
                editHandler && editing
                  ? {
                      onDelete: editHandler.onDelete,
                      onMove: (timeline, index, offset) => {
                        if (index + offset >= timelines.length) {
                          offset = timelines.length - index - 1;
                        } else if (index + offset < 0) {
                          offset = -index;
                        }
                        editHandler.onMove(timeline, index, offset);
                      },
                    }
                  : undefined
              }
            />
          );
        }
      })()}
    </div>
  );
};

export interface TimelineBoardProps {
  title?: string;
  className?: string;
  load: () => Promise<HttpTimelineInfo[]>;
  editHandler?: {
    onMove: (timeline: string, index: number, offset: number) => Promise<void>;
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
    HttpTimelineInfo[] | "offline" | "loading"
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
      editHandler={
        typeof timelines === "object" && editHandler != null
          ? {
              onMove: (timeline, index, offset) => {
                const newTimelines = timelines.slice();
                const [t] = newTimelines.splice(index, 1);
                newTimelines.splice(index + offset, 0, t);
                setTimelines(newTimelines);
                editHandler.onMove(timeline, index, offset).then(null, () => {
                  setTimelines(timelines);
                });
              },
              onDelete: (timeline) => {
                const newTimelines = timelines.slice();
                newTimelines.splice(
                  timelines.findIndex((t) => t.name === timeline),
                  1
                );
                setTimelines(newTimelines);
                editHandler.onDelete(timeline).then(null, () => {
                  setTimelines(timelines);
                });
              },
            }
          : undefined
      }
    />
  );
};

export default TimelineBoard;
