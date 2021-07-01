import React from "react";
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
      <div className="row justify-content-center">
        <div className="col col-12 col-md-6">
          <div className="row">
            <div className="col col-12 my-2">
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
            </div>
            <div className="col col-12 my-2">
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
            </div>
          </div>
        </div>
        <div className="col-12 col-md-6 my-2">
          <TimelineBoard
            title={t("home.relatedTimeline")}
            load={() =>
              getHttpTimelineClient().listTimeline({ relate: user.username })
            }
          />
        </div>
      </div>
    </>
  );
};

export default CenterBoards;
