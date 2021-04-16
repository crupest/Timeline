import React from "react";
import { Row, Col } from "react-bootstrap";
import { useTranslation } from "react-i18next";

import { pushAlert } from "@/services/alert";
import { useUserLoggedIn } from "@/services/user";

import { getHttpTimelineClient } from "@/http/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";
import { getHttpHighlightClient } from "@/http/highlight";

import TimelineBoard from "./TimelineBoard";

const CenterBoards: React.FC = () => {
  const { t } = useTranslation();

  const user = useUserLoggedIn();

  return (
    <>
      <Row className="justify-content-center">
        <Col xs="12" md="6">
          <Row>
            <Col xs="12" className="my-2">
              <TimelineBoard
                title={t("home.bookmarkTimeline")}
                load={() => getHttpBookmarkClient().list()}
                editHandler={{
                  onDelete: (timeline) => {
                    return getHttpBookmarkClient()
                      .delete(timeline)
                      .catch((e) => {
                        pushAlert({
                          message: "home.message.deleteBookmarkFail",
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
                          message: "home.message.moveBookmarkFail",
                          type: "danger",
                        });
                        throw e;
                      });
                  },
                }}
              />
            </Col>
            <Col xs="12" className="my-2">
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
                                message: "home.message.deleteHighlightFail",
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
                                message: "home.message.moveHighlightFail",
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
          </Row>
        </Col>
        <Col xs="12" md="6" className="my-2">
          <TimelineBoard
            title={t("home.relatedTimeline")}
            load={() =>
              getHttpTimelineClient().listTimeline({ relate: user.username })
            }
          />
        </Col>
      </Row>
    </>
  );
};

export default CenterBoards;
