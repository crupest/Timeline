import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Container, ListGroup, Modal, Row, Col, Button } from "react-bootstrap";

import { User, useAvatar } from "@/services/user";
import { TimelineInfo, timelineService } from "@/services/timeline";
import { getHttpUserClient, HttpUserNotExistError } from "@/http/user";

import SearchInput from "../common/SearchInput";
import BlobImage from "../common/BlobImage";

const TimelineMemberItem: React.FC<{
  user: User;
  owner: boolean;
  onRemove?: (username: string) => void;
}> = ({ user, owner, onRemove }) => {
  const { t } = useTranslation();

  const avatar = useAvatar(user.username);

  return (
    <ListGroup.Item className="container">
      <Row>
        <Col xs="auto">
          <BlobImage blob={avatar} className="avatar small" />
        </Col>
        <Col>
          <Row>{user.nickname}</Row>
          <Row>
            <small>{"@" + user.username}</small>
          </Row>
        </Col>
        {(() => {
          if (owner) {
            return null;
          }
          if (onRemove == null) {
            return null;
          }
          return (
            <Button
              className="align-self-center"
              variant="danger"
              onClick={() => {
                onRemove(user.username);
              }}
            >
              {t("timeline.member.remove")}
            </Button>
          );
        })()}
      </Row>
    </ListGroup.Item>
  );
};

export interface TimelineMemberProps {
  timeline: TimelineInfo;
  editable: boolean;
}

const TimelineMember: React.FC<TimelineMemberProps> = (props) => {
  const { t } = useTranslation();

  const [userSearchText, setUserSearchText] = useState<string>("");
  const [userSearchState, setUserSearchState] = useState<
    | {
        type: "user";
        data: User;
      }
    | { type: "error"; data: string }
    | { type: "loading" }
    | { type: "init" }
  >({ type: "init" });

  const userSearchAvatar = useAvatar(
    userSearchState.type === "user" ? userSearchState.data.username : undefined
  );

  const { timeline } = props;

  const members = [timeline.owner, ...timeline.members];

  return (
    <Container className="px-4 py-3">
      <ListGroup>
        {members.map((member, index) => (
          <TimelineMemberItem
            key={member.username}
            user={member}
            owner={index === 0}
            onRemove={
              props.editable
                ? () => {
                    void timelineService.removeMember(
                      timeline.name,
                      member.username
                    );
                  }
                : undefined
            }
          />
        ))}
      </ListGroup>
      {(() => {
        if (props.editable) {
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
                  getHttpUserClient()
                    .get(userSearchText)
                    .catch((e) => {
                      if (e instanceof HttpUserNotExistError) {
                        return null;
                      } else {
                        throw e;
                      }
                    })
                    .then(
                      (u) => {
                        if (u == null) {
                          setUserSearchState({
                            type: "error",
                            data: "timeline.userNotExist",
                          });
                        } else {
                          setUserSearchState({ type: "user", data: u });
                        }
                      },
                      (e) => {
                        setUserSearchState({
                          type: "error",
                          data: `${e as string}`,
                        });
                      }
                    );
                }}
              />
              {(() => {
                if (userSearchState.type === "user") {
                  const u = userSearchState.data;
                  const addable =
                    members.findIndex((m) => m.username === u.username) === -1;
                  return (
                    <>
                      {!addable ? (
                        <p>{t("timeline.member.alreadyMember")}</p>
                      ) : null}
                      <Container className="mb-3">
                        <Row>
                          <Col className="col-auto">
                            <BlobImage
                              blob={userSearchAvatar}
                              className="avatar small"
                            />
                          </Col>
                          <Col>
                            <Row>{u.nickname}</Row>
                            <Row>
                              <small>{"@" + u.username}</small>
                            </Row>
                          </Col>
                          <Button
                            variant="primary"
                            className="align-self-center"
                            disabled={!addable}
                            onClick={() => {
                              void timelineService
                                .addMember(timeline.name, u.username)
                                .then(() => {
                                  setUserSearchText("");
                                  setUserSearchState({ type: "init" });
                                });
                            }}
                          >
                            {t("timeline.member.add")}
                          </Button>
                        </Row>
                      </Container>
                    </>
                  );
                } else if (userSearchState.type === "error") {
                  return (
                    <p className="text-danger">{t(userSearchState.data)}</p>
                  );
                }
              })()}
            </>
          );
        } else {
          return null;
        }
      })()}
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
