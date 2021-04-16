import React from "react";
import { Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpTimelineClient } from "@/http/timeline";

import TimelineBoard from "./TimelineBoard";

const BoardWithoutUser: React.FC = () => {
  const { t } = useTranslation();

  return (
    <Row className="my-3 justify-content-center">
      <Col xs="12" md="6">
        <TimelineBoard
          title={t("home.highlightTimeline")}
          load={() => getHttpHighlightClient().list()}
        />
      </Col>
      <Col xs="12" md="6" className="my-3 my-md-0">
        <TimelineBoard
          title={t("home.publicTimeline")}
          load={() =>
            getHttpTimelineClient().listTimeline({ visibility: "Public" })
          }
        />
      </Col>
    </Row>
  );
};

export default BoardWithoutUser;
