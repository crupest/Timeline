import React from "react";
import { Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { AuthUser } from "@/services/user";
import { TimelineInfo } from "@/services/timeline";
import { getHttpTimelineClient } from "@/http/timeline";

import TimelineBoard from "./TimelineBoard";
import OfflineBoard from "./OfflineBoard";

const BoardWithUser: React.FC<{ user: AuthUser }> = ({ user }) => {
  const { t } = useTranslation();

  const [relatedTimelines, setRelatedTimelines] = React.useState<
    TimelineInfo[] | "offline" | "loading"
  >("loading");

  React.useEffect(() => {
    let subscribe = true;
    if (relatedTimelines === "loading") {
      void getHttpTimelineClient()
        .listTimeline({ relate: user.username })
        .then(
          (timelines) => {
            if (subscribe) {
              setRelatedTimelines(timelines);
            }
          },
          () => {
            setRelatedTimelines("offline");
          }
        );
    }
    return () => {
      subscribe = false;
    };
  }, [user, relatedTimelines]);

  return (
    <Row className="my-3 justify-content-center">
      {relatedTimelines === "offline" ? (
        <Col sm="8" lg="6">
          <OfflineBoard
            onReload={() => {
              setRelatedTimelines("loading");
            }}
          />
        </Col>
      ) : (
        <Col sm="6" lg="5">
          <TimelineBoard
            title={t("home.relatedTimeline")}
            timelines={relatedTimelines}
            onReload={() => {
              setRelatedTimelines("loading");
            }}
          />
        </Col>
      )}
    </Row>
  );
};

export default BoardWithUser;
