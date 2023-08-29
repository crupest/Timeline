import { useState } from "react";
import { useTranslation } from "react-i18next";

import { convertI18nText, I18nText } from "~src/common";

import { HttpUser } from "~src/http/user";
import { getHttpSearchClient } from "~src/http/search";
import { getHttpTimelineClient, HttpTimelineInfo } from "~src/http/timeline";

import SearchInput from "~src/components/SearchInput";
import UserAvatar from "~src/components/user/UserAvatar";
import { IconButton } from "~src/components/button";
import { ListContainer, ListItemContainer } from "~src/components/list";

import "./TimelineMember.css";

function TimelineMemberItem({
  user,
  add,
  onAction,
}: {
  user: HttpUser;
  add?: boolean;
  onAction?: (username: string) => void;
}) {
  return (
    <ListItemContainer className="timeline-member-item">
      <UserAvatar username={user.username} className="timeline-member-avatar" />
      <div className="timeline-member-info">
        <div className="timeline-member-nickname">{user.nickname}</div>
        <small className="timeline-member-username">
          {"@" + user.username}
        </small>
      </div>
      {onAction ? (
        <div className="timeline-member-action">
          <IconButton
            icon={add ? "plus-lg" : "trash"}
            color={add ? "create" : "danger"}
            onClick={() => {
              onAction(user.username);
            }}
          />
        </div>
      ) : null}
    </ListItemContainer>
  );
}

function TimelineMemberUserSearch({
  timeline,
  onChange,
}: {
  timeline: HttpTimelineInfo;
  onChange: () => void;
}) {
  const { t } = useTranslation();

  const [userSearchText, setUserSearchText] = useState<string>("");
  const [userSearchState, setUserSearchState] = useState<
    | {
        type: "users";
        data: HttpUser[];
      }
    | { type: "error"; data: I18nText }
    | { type: "loading" }
    | { type: "init" }
  >({ type: "init" });

  return (
    <div className="timeline-member-user-search">
      <SearchInput
        className=""
        value={userSearchText}
        onChange={(v) => {
          setUserSearchText(v);
        }}
        loading={userSearchState.type === "loading"}
        onButtonClick={() => {
          if (userSearchText === "") {
            setUserSearchState({
              type: "error",
              data: "login.emptyUsername",
            });
            return;
          }
          setUserSearchState({ type: "loading" });
          getHttpSearchClient()
            .searchUsers(userSearchText)
            .then(
              (users) => {
                users = users.filter(
                  (user) =>
                    timeline.members.findIndex(
                      (m) => m.username === user.username,
                    ) === -1 && timeline.owner.username !== user.username,
                );
                setUserSearchState({ type: "users", data: users });
              },
              (e) => {
                setUserSearchState({
                  type: "error",
                  data: { type: "custom", value: String(e) },
                });
              },
            );
        }}
      />
      {(() => {
        if (userSearchState.type === "users") {
          const users = userSearchState.data;
          if (users.length === 0) {
            return <div>{t("timeline.member.noUserAvailableToAdd")}</div>;
          } else {
            return (
              <div className="">
                {users.map((user) => (
                  <TimelineMemberItem
                    key={user.username}
                    user={user}
                    add
                    onAction={() => {
                      void getHttpTimelineClient()
                        .memberPut(
                          timeline.owner.username,
                          timeline.nameV2,
                          user.username,
                        )
                        .then(() => {
                          setUserSearchText("");
                          setUserSearchState({ type: "init" });
                          onChange();
                        });
                    }}
                  />
                ))}
              </div>
            );
          }
        } else if (userSearchState.type === "error") {
          return (
            <div className="cru-color-danger">
              {convertI18nText(userSearchState.data, t)}
            </div>
          );
        }
      })()}
    </div>
  );
}

interface TimelineMemberProps {
  timeline: HttpTimelineInfo;
  onChange: () => void;
}

export default function TimelineMember(props: TimelineMemberProps) {
  const { timeline, onChange } = props;
  const members = [timeline.owner, ...timeline.members];

  return (
    <div className="container px-4 py-3">
      <ListContainer>
        {members.map((member, index) => (
          <TimelineMemberItem
            key={member.username}
            user={member}
            onAction={
              timeline.manageable && index !== 0
                ? () => {
                    void getHttpTimelineClient()
                      .memberDelete(
                        timeline.owner.username,
                        timeline.nameV2,
                        member.username,
                      )
                      .then(onChange);
                  }
                : undefined
            }
          />
        ))}
      </ListContainer>
      {timeline.manageable ? (
        <TimelineMemberUserSearch timeline={timeline} onChange={onChange} />
      ) : null}
    </div>
  );
}
