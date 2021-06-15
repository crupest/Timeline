import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Container, ListGroup, Modal, Row, Col, Button } from "react-bootstrap";

import { convertI18nText, I18nText } from "@/common";

import { HttpUser } from "@/http/user";
import { getHttpSearchClient } from "@/http/search";

import SearchInput from "../common/SearchInput";
import UserAvatar from "../common/user/UserAvatar";
import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

const TimelineMemberItem: React.FC<{
  user: HttpUser;
  add?: boolean;
  onAction?: (username: string) => void;
}> = ({ user, add, onAction }) => {
  const { t } = useTranslation();

  return (
    <ListGroup.Item className="container">
      <Row>
        <Col xs="auto">
          <UserAvatar username={user.username} className="avatar small" />
        </Col>
        <Col>
          <Row>{user.nickname}</Row>
          <Row>
            <small>{"@" + user.username}</small>
          </Row>
        </Col>
        {onAction ? (
          <Col xs="auto">
            <Button
              variant={add ? "success" : "danger"}
              onClick={() => {
                onAction(user.username);
              }}
            >
              {t(`timeline.member.${add ? "add" : "remove"}`)}
            </Button>
          </Col>
        ) : null}
      </Row>
    </ListGroup.Item>
  );
};

const TimelineMemberUserSearch: React.FC<{
  timeline: HttpTimelineInfo;
  onChange: () => void;
}> = ({ timeline, onChange }) => {
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
    <>
      <SearchInput
        className="mt-3"
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
                      (m) => m.username === user.username
                    ) === -1 && timeline.owner.username !== user.username
                );
                setUserSearchState({ type: "users", data: users });
              },
              (e) => {
                setUserSearchState({
                  type: "error",
                  data: { type: "custom", value: String(e) },
                });
              }
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
              <ListGroup className="mt-2">
                {users.map((user) => (
                  <TimelineMemberItem
                    key={user.username}
                    user={user}
                    add
                    onAction={() => {
                      void getHttpTimelineClient()
                        .memberPut(timeline.name, user.username)
                        .then(() => {
                          setUserSearchText("");
                          setUserSearchState({ type: "init" });
                          onChange();
                        });
                    }}
                  />
                ))}
              </ListGroup>
            );
          }
        } else if (userSearchState.type === "error") {
          return (
            <div className="text-danger">
              {convertI18nText(userSearchState.data, t)}
            </div>
          );
        }
      })()}
    </>
  );
};

export interface TimelineMemberProps {
  timeline: HttpTimelineInfo;
  onChange: () => void;
}

const TimelineMember: React.FC<TimelineMemberProps> = (props) => {
  const { timeline, onChange } = props;
  const members = [timeline.owner, ...timeline.members];

  return (
    <Container className="px-4 py-3">
      <ListGroup>
        {members.map((member, index) => (
          <TimelineMemberItem
            key={member.username}
            user={member}
            onAction={
              timeline.manageable && index !== 0
                ? () => {
                    void getHttpTimelineClient()
                      .memberDelete(timeline.name, member.username)
                      .then(onChange);
                  }
                : undefined
            }
          />
        ))}
      </ListGroup>
      {timeline.manageable ? (
        <TimelineMemberUserSearch timeline={timeline} onChange={onChange} />
      ) : null}
    </Container>
  );
};

export default TimelineMember;

export interface TimelineMemberDialogProps extends TimelineMemberProps {
  open: boolean;
  onClose: () => void;
}

export const TimelineMemberDialog: React.FC<TimelineMemberDialogProps> = (
  props
) => {
  return (
    <Modal show centered onHide={props.onClose}>
      <TimelineMember {...props} />
    </Modal>
  );
};
