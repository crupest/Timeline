import React from "react";
import { Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { AuthUser } from "@/services/user";
import { pushAlert } from "@/services/alert";

import { getHttpHighlightClient } from "@/http/highlight";
import { getHttpTimelineClient } from "@/http/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";

import TimelineBoard from "./TimelineBoard";

const BoardWithUser: React.FC<{ user: AuthUser }> = ({ user }) => {
  const { t } = useTranslation();

  return (
    <>
      <Row className="my-3 justify-content-center">
        <Col xs="12" md="6">
          <TimelineBoard
            title={t("home.bookmarkTimeline")}
            load={() => getHttpBookmarkClient().list()}
            editHandler={{
              onDelete: (timeline) => {
                return getHttpBookmarkClient()
                  .delete(timeline)
                  .catch((e) => {
                    pushAlert({
                      message: {
                        type: "i18n",
                        key: "home.message.deleteBookmarkFail",
                      },
                      type: "danger",
                    });
                    throw e;
                  });
              },
              onMove: (timeline, index, offset) => {
                return getHttpBookmarkClient()
                  .move(
                    { timeline, newPosition: index + offset + 1 } // +1 because backend contract: index starts at 1
                  )
                  .catch((e) => {
                    pushAlert({
                      message: {
                        type: "i18n",
                        key: "home.message.moveBookmarkFail",
                      },
                      type: "danger",
                    });
                    throw e;
                  });
              },
            }}
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
      <Row className="my-3 justify-content-center">
        <Col xs="12" md="6">
          <TimelineBoard
            title={t("home.highlightTimeline")}
            load={() => getHttpHighlightClient().list()}
            editHandler={
              user.hasHighlightTimelineAdministrationPermission
                ? {
                    onDelete: (timeline) => {
                      return getHttpHighlightClient()
                        .delete(timeline)
                        .catch((e) => {
                          pushAlert({
                            message: {
                              type: "i18n",
                              key: "home.message.deleteHighlightFail",
                            },
                            type: "danger",
                          });
                          throw e;
                        });
                    },
                    onMove: (timeline, index, offset) => {
                      return getHttpHighlightClient()
                        .move(
                          { timeline, newPosition: index + offset + 1 } // +1 because backend contract: index starts at 1
                        )
                        .catch((e) => {
                          pushAlert({
                            message: {
                              type: "i18n",
                              key: "home.message.moveHighlightFail",
                            },
                            type: "danger",
                          });
                          throw e;
                        });
                    },
                  }
                : undefined
            }
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
    </>
  );
};

export default BoardWithUser;
