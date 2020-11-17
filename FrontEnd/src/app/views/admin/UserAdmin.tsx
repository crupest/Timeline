import React, { useState, useEffect } from "react";
import {
  ListGroup,
  Row,
  Col,
  Dropdown,
  Spinner,
  Button,
} from "react-bootstrap";

import OperationDialog from "../common/OperationDialog";
import AdminSubPage from "./AdminSubPage";

import { User, AuthUser } from "@/services/user";
import { getHttpUserClient, HttpUser } from "@/http/user";

const kModify = "modify";
const kDelete = "delete";

type TModify = typeof kModify;
type TDelete = typeof kDelete;

type ContextMenuItem = TModify | TDelete;

interface UserCardProps {
  on: { [key in ContextMenuItem]: () => void };
  user: User;
}

const UserItem: React.FC<UserCardProps> = ({ user, on }) => {
  return (
    <ListGroup.Item>
      <div>
        <span className="text-primary">@{user.username + " "}</span>
        <small className="ml-2 text-secondary">{user.nickname}</small>
      </div>
      <div>
        {user.permissions.map((permission) => {
          return (
            <small key={permission} className="text-danger">
              {permission + " "}
            </small>
          );
        })}
      </div>
      <Dropdown className="text-right">
        <Dropdown.Toggle variant="outline-primary">Manage</Dropdown.Toggle>
        <Dropdown.Menu>
          <Dropdown.Item onClick={on["modify"]}>Modify</Dropdown.Item>
          <Dropdown.Item className="text-danger" onClick={on["delete"]}>
            Delete
          </Dropdown.Item>
        </Dropdown.Menu>
      </Dropdown>
    </ListGroup.Item>
  );
};

interface DialogProps<TData = undefined, TReturn = undefined> {
  open: boolean;
  close: () => void;
  token: string;
  data: TData;
  onSuccess: (data: TReturn) => void;
}

const CreateUserDialog: React.FC<DialogProps<undefined, HttpUser>> = ({
  open,
  close,
  token,
  onSuccess,
}) => {
  return (
    <OperationDialog
      title="Create"
      titleColor="create"
      inputPrompt="You are creating a new user."
      inputScheme={
        [
          { type: "text", label: "Username" },
          { type: "text", label: "Password" },
        ] as const
      }
      onProcess={([username, password]) =>
        getHttpUserClient().createUser(
          {
            username,
            password,
          },
          token
        )
      }
      close={close}
      open={open}
      onSuccessAndClose={onSuccess}
    />
  );
};

const UsernameLabel: React.FC = (props) => {
  return <span style={{ color: "blue" }}>{props.children}</span>;
};

const UserDeleteDialog: React.FC<DialogProps<
  { username: string },
  unknown
>> = ({ open, close, token, data: { username }, onSuccess }) => {
  return (
    <OperationDialog
      open={open}
      close={close}
      title="Dangerous"
      titleColor="dangerous"
      inputPrompt={() => (
        <>
          You are deleting user <UsernameLabel>{username}</UsernameLabel> !
        </>
      )}
      onProcess={() => getHttpUserClient().delete(username, token)}
      onSuccessAndClose={onSuccess}
    />
  );
};

const UserModifyDialog: React.FC<DialogProps<
  {
    oldUser: HttpUser;
  },
  HttpUser
>> = ({ open, close, token, data: { oldUser }, onSuccess }) => {
  return (
    <OperationDialog
      open={open}
      close={close}
      title="Caution"
      titleColor="dangerous"
      inputPrompt={() => (
        <>
          You are change the password of user
          <UsernameLabel>{oldUser.username}</UsernameLabel> !
        </>
      )}
      inputScheme={
        [
          { type: "text", label: "New Username", initValue: oldUser.username },
          { type: "text", label: "New Password" },
          { type: "text", label: "New Nickname", initValue: oldUser.nickname },
        ] as const
      }
      onProcess={([username, password, nickname]) =>
        getHttpUserClient().patch(
          oldUser.username,
          {
            username: username !== oldUser.username ? username : undefined,
            password: password !== "" ? password : undefined,
            nickname: nickname !== oldUser.nickname ? nickname : undefined,
          },
          token
        )
      }
      onSuccessAndClose={onSuccess}
    />
  );
};

interface UserAdminProps {
  user: AuthUser;
}

const UserAdmin: React.FC<UserAdminProps> = (props) => {
  type DialogInfo =
    | null
    | {
        type: "create";
      }
    | {
        type: TModify;
        user: HttpUser;
      }
    | { type: TDelete; username: string };

  const [users, setUsers] = useState<User[] | null>(null);
  const [dialog, setDialog] = useState<DialogInfo>(null);
  const [usersVersion, setUsersVersion] = useState<number>(0);
  const updateUsers = (): void => {
    setUsersVersion(usersVersion + 1);
  };

  const token = props.user.token;

  useEffect(() => {
    let subscribe = true;
    void getHttpUserClient()
      .list()
      .then((us) => {
        if (subscribe) {
          setUsers(us);
        }
      });
    return () => {
      subscribe = false;
    };
  }, [usersVersion]);

  let dialogNode: React.ReactNode;
  if (dialog) {
    switch (dialog.type) {
      case "create":
        dialogNode = (
          <CreateUserDialog
            open
            close={() => setDialog(null)}
            token={token}
            data={undefined}
            onSuccess={updateUsers}
          />
        );
        break;
      case "delete":
        dialogNode = (
          <UserDeleteDialog
            open
            close={() => setDialog(null)}
            token={token}
            data={{ username: dialog.username }}
            onSuccess={updateUsers}
          />
        );
        break;
      case kModify:
        dialogNode = (
          <UserModifyDialog
            open
            close={() => setDialog(null)}
            token={token}
            data={{ oldUser: dialog.user }}
            onSuccess={updateUsers}
          />
        );
        break;
    }
  }

  if (users) {
    const userComponents = users.map((user) => {
      return (
        <UserItem
          key={user.username}
          user={user}
          on={{
            modify: () => {
              setDialog({
                type: "modify",
                user,
              });
            },
            delete: () => {
              setDialog({
                type: "delete",
                username: user.username,
              });
            },
          }}
        />
      );
    });

    return (
      <AdminSubPage>
        <Row className="justify-content-end my-2">
          <Col xs="auto">
            <Button
              variant="outline-success"
              onClick={() =>
                setDialog({
                  type: "create",
                })
              }
            >
              Create
            </Button>
          </Col>
        </Row>
        {userComponents}
        {dialogNode}
      </AdminSubPage>
    );
  } else {
    return <Spinner animation="border" />;
  }
};

export default UserAdmin;
