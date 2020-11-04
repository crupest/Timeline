import React from "react";
import { Row, Col } from "react-bootstrap";

import { TimelineInfo } from "@/services/timeline";
import { getHttpTimelineClient } from "@/http/timeline";

import TimelineBoard from "./TimelineBoard";
import OfflineBoard from "./OfflineBoard";

const BoardWithoutUser: React.FC = () => {
  const [publicTimelines, setPublicTimelines] = React.useState<
    TimelineInfo[] | "offline" | "loading"
  >("loading");

  React.useEffect(() => {
    let subscribe = true;
    if (publicTimelines === "loading") {
      void getHttpTimelineClient()
        .listTimeline({ visibility: "Public" })
        .then(
          (timelines) => {
            if (subscribe) {
              setPublicTimelines(timelines);
            }
          },
          () => {
            setPublicTimelines("offline");
          }
        );
    }
    return () => {
      subscribe = false;
    };
  }, [publicTimelines]);

  return (
    <Row className="my-3 justify-content-center">
      {publicTimelines === "offline" ? (
        <Col sm="8" lg="6">
          <OfflineBoard
            onReload={() => {
              setPublicTimelines("loading");
            }}
          />
        </Col>
      ) : (
        <Col sm="8" lg="6">
          <TimelineBoard
            timelines={publicTimelines}
            onReload={() => {
              setPublicTimelines("loading");
            }}
          />
        </Col>
      )}
    </Row>
  );
};

export default BoardWithoutUser;
