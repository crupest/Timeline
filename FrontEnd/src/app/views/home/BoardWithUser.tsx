import React from "react";
import { Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { AuthUser } from "@/services/user";
import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpTimelineClient } from "@/http/timeline";

import TimelineBoard from "./TimelineBoard";

const BoardWithUser: React.FC<{ user: AuthUser }> = ({ user }) => {
  const { t } = useTranslation();

  return (
    <Row className="my-3 justify-content-center">
      <Col sm="6" lg="5">
        <TimelineBoard
          title={t("home.highlightTimeline")}
          load={() => getHttpHighlightClient().list()}
        />
      </Col>
      <Col sm="6" lg="5">
        <TimelineBoard
          title={t("home.relatedTimeline")}
          load={() =>
            getHttpTimelineClient().listTimeline({ relate: user.username })
          }
        />
      </Col>
    </Row>
  );
};

export default BoardWithUser;
