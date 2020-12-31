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
      <Col xs="12" md="6">
        <TimelineBoard
          title={t("home.highlightTimeline")}
          load={() => getHttpHighlightClient().list()}
          editHandler={
            user.hasHighlightTimelineAdministrationPermission
              ? {
                  onDelete: () => {
                    // TODO: Implement this.
                    return Promise.resolve();
                  },
                }
              : undefined
          }
        />
      </Col>
      <Col xs="12" md="6" className="my-3 my-md-0">
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
