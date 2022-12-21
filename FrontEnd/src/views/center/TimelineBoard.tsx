import React from "react";
import classnames from "classnames";
import { Link } from "react-router-dom";

import { TimelineBookmark } from "@/http/bookmark";

import TimelineLogo from "../common/TimelineLogo";
import LoadFailReload from "../common/LoadFailReload";
import FlatButton from "../common/button/FlatButton";
import Card from "../common/Card";
import Spinner from "../common/Spinner";
import IconButton from "../common/button/IconButton";

interface TimelineBoardItemProps {
  timeline: TimelineBookmark;
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
  const content = (
    <>
      <TimelineLogo className="icon" />
      <span className="title">
        {timeline.timelineOwner}/{timeline.timelineName}
      </span>
      <span className="flex-grow-1"></span>
      {actions != null ? (
        <div className="right">
          <IconButton
            icon="trash"
            color="danger"
            className="px-2"
            onClick={actions.onDelete}
          />
          <IconButton
            icon="grip-vertical"
            className="px-2 touch-action-none"
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
    transform:
      arbitraryOffset != null
        ? `translate(0,${arbitraryOffset}px)`
        : offset != null
        ? `translate(0,${offset * 100}%)`
        : undefined,
    transition: offset != null ? "transform 0.5s" : undefined,
    zIndex: arbitraryOffset != null ? 1 : undefined,
  };

  return actions == null ? (
    <Link
      to={`${timeline.timelineOwner}/${timeline.timelineName}`}
      className="timeline-board-item"
    >
      {content}
    </Link>
  ) : (
    <div style={offsetStyle} className="timeline-board-item">
      {content}
    </div>
  );
};

interface TimelineBoardItemContainerProps {
  timelines: TimelineBookmark[];
  editHandler?: {
    // offset may exceed index range plusing index.
    onMove: (
      owner: string,
      timeline: string,
      index: number,
      offset: number
    ) => void;
    onDelete: (owner: string, timeline: string) => void;
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
            key={timeline.timelineOwner + "/" + timeline.timelineName}
            timeline={timeline}
            offset={offset}
            arbitraryOffset={arbitraryOffset}
            actions={
              editHandler != null
                ? {
                    onDelete: () => {
                      editHandler.onDelete(
                        timeline.timelineOwner,
                        timeline.timelineName
                      );
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
                            timeline.timelineOwner,
                            timeline.timelineName,
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
  title?: string | null;
  state: "offline" | "loading" | "loaded";
  timelines: TimelineBookmark[];
  onReload: () => void;
  className?: string;
  editHandler?: {
    onMove: (
      owner: string,
      timeline: string,
      index: number,
      offset: number
    ) => void;
    onDelete: (owner: string, timeline: string) => void;
  };
}

const TimelineBoardUI: React.FC<TimelineBoardUIProps> = (props) => {
  const { title, state, timelines, className, editHandler } = props;

  const editable = editHandler != null;

  const [editing, setEditing] = React.useState<boolean>(false);

  return (
    <Card className={classnames("timeline-board", className)}>
      <div className="timeline-board-header">
        {title != null && <h3>{title}</h3>}
        {editable &&
          (editing ? (
            <FlatButton
              text="done"
              onClick={() => {
                setEditing(false);
              }}
            />
          ) : (
            <FlatButton
              text="edit"
              onClick={() => {
                setEditing(true);
              }}
            />
          ))}
      </div>
      {(() => {
        if (state === "loading") {
          return (
            <div className="d-flex flex-grow-1 justify-content-center align-items-center">
              <Spinner />
            </div>
          );
        } else if (state === "offline") {
          return (
            <div className="d-flex flex-grow-1 justify-content-center align-items-center">
              <LoadFailReload onReload={props.onReload} />
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
                      onMove: (owner, timeline, index, offset) => {
                        if (index + offset >= timelines.length) {
                          offset = timelines.length - index - 1;
                        } else if (index + offset < 0) {
                          offset = -index;
                        }
                        editHandler.onMove(owner, timeline, index, offset);
                      },
                    }
                  : undefined
              }
            />
          );
        }
      })()}
    </Card>
  );
};

export interface TimelineBoardProps {
  title?: string | null;
  className?: string;
  load: () => Promise<TimelineBookmark[]>;
  editHandler?: {
    onMove: (
      owner: string,
      timeline: string,
      index: number,
      offset: number
    ) => Promise<void>;
    onDelete: (owner: string, timeline: string) => Promise<void>;
  };
}

const TimelineBoard: React.FC<TimelineBoardProps> = ({
  className,
  title,
  load,
  editHandler,
}) => {
  const [state, setState] = React.useState<"offline" | "loading" | "loaded">(
    "loading"
  );
  const [timelines, setTimelines] = React.useState<TimelineBookmark[]>([]);

  React.useEffect(() => {
    let subscribe = true;
    if (state === "loading") {
      void load().then(
        (timelines) => {
          if (subscribe) {
            setState("loaded");
            setTimelines(timelines);
          }
        },
        () => {
          setState("offline");
        }
      );
    }
    return () => {
      subscribe = false;
    };
  }, [load, state]);

  return (
    <TimelineBoardUI
      title={title}
      className={className}
      state={state}
      timelines={timelines}
      onReload={() => {
        setState("loaded");
      }}
      editHandler={
        typeof timelines === "object" && editHandler != null
          ? {
              onMove: (owner, timeline, index, offset) => {
                const newTimelines = timelines.slice();
                const [t] = newTimelines.splice(index, 1);
                newTimelines.splice(index + offset, 0, t);
                setTimelines(newTimelines);
                editHandler
                  .onMove(owner, timeline, index, offset)
                  .then(null, () => {
                    setTimelines(timelines);
                  });
              },
              onDelete: (owner, timeline) => {
                const newTimelines = timelines.slice();
                newTimelines.splice(
                  timelines.findIndex(
                    (t) =>
                      t.timelineOwner === owner && t.timelineName === timeline
                  ),
                  1
                );
                setTimelines(newTimelines);
                editHandler.onDelete(owner, timeline).then(null, () => {
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
