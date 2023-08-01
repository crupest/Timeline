import * as React from "react";
import { useTranslation } from "react-i18next";

import { highlightTimelineUsername } from "@/common";

import { pushAlert } from "@/services/alert";
import { useUserLoggedIn } from "@/services/user";

import { getHttpTimelineClient } from "@/http/timeline";
import { getHttpBookmarkClient } from "@/http/bookmark";

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
                load={() =>
                  getHttpBookmarkClient()
                    .list(user.username)
                    .then((p) => p.items)
                }
                editHandler={{
                  onDelete: (owner, timeline) => {
                    return getHttpBookmarkClient()
                      .delete(user.username, owner, timeline)
                      .catch((e) => {
                        pushAlert({
                          message: "home.message.deleteBookmarkFail",
                          type: "danger",
                        });
                        throw e;
                      });
                  },
                  onMove: (owner, timeline, index, offset) => {
                    return getHttpBookmarkClient()
                      .move(
                        user.username,
                        owner,
                        timeline,
                        index + offset + 1 // +1 because backend contract: index starts at 1
                      )
                      .catch((e) => {
                        pushAlert({
                          message: "home.message.moveBookmarkFail",
                          type: "danger",
                        });
                        throw e;
                      })
                      .then();
                  },
                }}
              />
            </div>
            <div className="col col-12 my-2">
              <TimelineBoard
                title={t("home.highlightTimeline")}
                load={() =>
                  getHttpBookmarkClient()
                    .list(highlightTimelineUsername)
                    .then((p) => p.items)
                }
                editHandler={
                  user.username === highlightTimelineUsername
                    ? {
                        onDelete: (owner, timeline) => {
                          return getHttpBookmarkClient()
                            .delete(highlightTimelineUsername, owner, timeline)
                            .catch((e) => {
                              pushAlert({
                                message: "home.message.deleteHighlightFail",
                                type: "danger",
                              });
                              throw e;
                            });
                        },
                        onMove: (owner, timeline, index, offset) => {
                          return getHttpBookmarkClient()
                            .move(
                              highlightTimelineUsername,
                              owner,
                              timeline,
                              index + offset + 1 // +1 because backend contract: index starts at 1
                            )
                            .catch((e) => {
                              pushAlert({
                                message: "home.message.moveBookmarkFail",
                                type: "danger",
                              });
                              throw e;
                            })
                            .then();
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
              getHttpTimelineClient()
                .listTimeline({ relate: user.username })
                .then((l) =>
                  l.map((t, index) => ({
                    timelineOwner: t.owner.username,
                    timelineName: t.nameV2,
                    position: index + 1,
                  }))
                )
            }
          />
        </div>
      </div>
    </>
  );
};

export default CenterBoards;
