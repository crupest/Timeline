import React from "react";
import { Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { UserWithToken } from "@/services/user";
import { TimelineInfo } from "@/services/timeline";
import { getHttpTimelineClient } from "@/http/timeline";

import TimelineBoard from "./TimelineBoard";
import OfflineBoard from "./OfflineBoard";

const BoardWithUser: React.FC<{ user: UserWithToken }> = ({ user }) => {
  const { t } = useTranslation();

  const [ownTimelines, setOwnTimelines] = React.useState<
    TimelineInfo[] | "offline" | "loading"
  >("loading");
  const [joinTimelines, setJoinTimelines] = React.useState<
    TimelineInfo[] | "offline" | "loading"
  >("loading");

  React.useEffect(() => {
    let subscribe = true;
    if (ownTimelines === "loading") {
      void getHttpTimelineClient()
        .listTimeline({ relate: user.username, relateType: "own" })
        .then(
          (timelines) => {
            if (subscribe) {
              setOwnTimelines(timelines);
            }
          },
          () => {
            setOwnTimelines("offline");
          }
        );
    }
    return () => {
      subscribe = false;
    };
  }, [user, ownTimelines]);

  React.useEffect(() => {
    let subscribe = true;
    if (joinTimelines === "loading") {
      void getHttpTimelineClient()
        .listTimeline({ relate: user.username, relateType: "join" })
        .then(
          (timelines) => {
            if (subscribe) {
              setJoinTimelines(timelines);
            }
          },
          () => {
            setJoinTimelines("offline");
          }
        );
    }
    return () => {
      subscribe = false;
    };
  }, [user, joinTimelines]);

  return (
    <Row className="my-3 justify-content-center">
      {ownTimelines === "offline" && joinTimelines === "offline" ? (
        <Col sm="8" lg="6">
          <OfflineBoard
            onReload={() => {
              setOwnTimelines("loading");
              setJoinTimelines("loading");
            }}
          />
        </Col>
      ) : (
        <>
          <Col sm="6" lg="5" className="mb-3 mb-sm-0">
            <TimelineBoard
              title={t("home.ownTimeline")}
              timelines={ownTimelines}
              onReload={() => {
                setOwnTimelines("loading");
              }}
            />
          </Col>
          <Col sm="6" lg="5">
            <TimelineBoard
              title={t("home.joinTimeline")}
              timelines={joinTimelines}
              onReload={() => {
                setJoinTimelines("loading");
              }}
            />
          </Col>
        </>
      )}
    </Row>
  );
};

export default BoardWithUser;
