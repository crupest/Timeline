import React, { useState, useEffect } from "react";
import axios from "axios";
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
import { getHttpUserClient } from "@/http/user";

const apiBaseUrl = "/api";

interface CreateUserInfo {
  username: string;
  password: string;
}

async function createUser(user: CreateUserInfo, token: string): Promise<User> {
  const res = await axios.post<User>(
    `${apiBaseUrl}/userop/createuser?token=${token}`,
    user
  );
  return res.data;
}

function deleteUser(username: string, token: string): Promise<void> {
  return axios.delete(`${apiBaseUrl}/users/${username}?token=${token}`);
}

function changeUsername(
  oldUsername: string,
  newUsername: string,
  token: string
): Promise<void> {
  return axios.patch(`${apiBaseUrl}/users/${oldUsername}?token=${token}`, {
    username: newUsername,
  });
}

function changePassword(
  username: string,
  newPassword: string,
  token: string
): Promise<void> {
  return axios.patch(`${apiBaseUrl}/users/${username}?token=${token}`, {
    password: newPassword,
  });
}

const kChangeUsername = "changeusername";
const kChangePassword = "changepassword";
const kChangePermission = "changepermission";
const kDelete = "delete";

type TChangeUsername = typeof kChangeUsername;
type TChangePassword = typeof kChangePassword;
type TChangePermission = typeof kChangePermission;
type TDelete = typeof kDelete;

type ContextMenuItem =
  | TChangeUsername
  | TChangePassword
  | TChangePermission
  | TDelete;

interface UserCardProps {
  onContextMenu: (item: ContextMenuItem) => void;
  user: User;
}

const UserItem: React.FC<UserCardProps> = (props) => {
  const user = props.user;

  const createClickCallback = (item: ContextMenuItem): (() => void) => {
    return () => {
      props.onContextMenu(item);
    };
  };

  return (
    <ListGroup.Item className="container">
      <Row className="align-items-center">
        <Col>
          <p className="mb-0 text-primary">{user.username}</p>
          <small
            className={user.permissions ? "text-danger" : "text-secondary"}
          >
            {user.permissions ? "administrator" : "user"}
          </small>
        </Col>
        <Col className="col-auto">
          <Dropdown>
            <Dropdown.Toggle variant="warning" className="text-light">
              Manage
            </Dropdown.Toggle>
            <Dropdown.Menu>
              <Dropdown.Item onClick={createClickCallback(kChangeUsername)}>
                Change Username
              </Dropdown.Item>
              <Dropdown.Item onClick={createClickCallback(kChangePassword)}>
                Change Password
              </Dropdown.Item>
              <Dropdown.Item onClick={createClickCallback(kChangePermission)}>
                Change Permission
              </Dropdown.Item>
              <Dropdown.Item
                className="text-danger"
                onClick={createClickCallback(kDelete)}
              >
                Delete
              </Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
        </Col>
      </Row>
    </ListGroup.Item>
  );
};

interface DialogProps {
  open: boolean;
  close: () => void;
}

interface CreateUserDialogProps extends DialogProps {
  process: (user: CreateUserInfo) => Promise<void>;
}

const CreateUserDialog: React.FC<CreateUserDialogProps> = (props) => {
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
        props.process({
          username: username,
          password: password,
        })
      }
      close={props.close}
      open={props.open}
    />
  );
};

const UsernameLabel: React.FC = (props) => {
  return <span style={{ color: "blue" }}>{props.children}</span>;
};

interface UserDeleteDialogProps extends DialogProps {
  username: string;
  process: () => Promise<void>;
}

const UserDeleteDialog: React.FC<UserDeleteDialogProps> = (props) => {
  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="Dangerous"
      titleColor="dangerous"
      inputPrompt={() => (
        <>
          {"You are deleting user "}
          <UsernameLabel>{props.username}</UsernameLabel>
          {" !"}
        </>
      )}
      onProcess={props.process}
    />
  );
};

interface UserModifyDialogProps<T> extends DialogProps {
  username: string;
  process: (value: T) => Promise<void>;
}

const UserChangeUsernameDialog: React.FC<UserModifyDialogProps<string>> = (
  props
) => {
  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="Caution"
      titleColor="dangerous"
      inputPrompt={() => (
        <>
          {"You are change the username of user "}
          <UsernameLabel>{props.username}</UsernameLabel>
          {" !"}
        </>
      )}
      inputScheme={[{ type: "text", label: "New Username" }]}
      onProcess={([newUsername]) => {
        return props.process(newUsername);
      }}
    />
  );
};

const UserChangePasswordDialog: React.FC<UserModifyDialogProps<string>> = (
  props
) => {
  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="Caution"
      titleColor="dangerous"
      inputPrompt={() => (
        <>
          {"You are change the password of user "}
          <UsernameLabel>{props.username}</UsernameLabel>
          {" !"}
        </>
      )}
      inputScheme={[{ type: "text", label: "New Password" }]}
      onProcess={([newPassword]) => {
        return props.process(newPassword);
      }}
    />
  );
};

interface UserChangePermissionDialogProps extends DialogProps {
  username: string;
  newPermission: boolean;
  process: () => Promise<void>;
}

const UserChangePermissionDialog: React.FC<UserChangePermissionDialogProps> = (
  props
) => {
  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="Caution"
      titleColor="dangerous"
      inputPrompt={() => (
        <>
          {"You are change user "}
          <UsernameLabel>{props.username}</UsernameLabel>
          {" to "}
          <span style={{ color: "orange" }}>
            {props.newPermission ? "administrator" : "normal user"}
          </span>
          {" !"}
        </>
      )}
      onProcess={props.process}
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
    | { type: TDelete; username: string }
    | {
        type: TChangeUsername;
        username: string;
      }
    | {
        type: TChangePassword;
        username: string;
      }
    | {
        type: TChangePermission;
        username: string;
        newPermission: boolean;
      };

  const [users, setUsers] = useState<User[] | null>(null);
  const [dialog, setDialog] = useState<DialogInfo>(null);

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
  }, []);

  let dialogNode: React.ReactNode;
  if (dialog)
    switch (dialog.type) {
      case "create":
        dialogNode = (
          <CreateUserDialog
            open
            close={() => setDialog(null)}
            process={async (user) => {
              const u = await createUser(user, token);
              setUsers((oldUsers) => [...(oldUsers ?? []), u]);
            }}
          />
        );
        break;
      case "delete":
        dialogNode = (
          <UserDeleteDialog
            open
            close={() => setDialog(null)}
            username={dialog.username}
            process={async () => {
              await deleteUser(dialog.username, token);
              setUsers((oldUsers) =>
                (oldUsers ?? []).filter((u) => u.username !== dialog.username)
              );
            }}
          />
        );
        break;
      case kChangeUsername:
        dialogNode = (
          <UserChangeUsernameDialog
            open
            close={() => setDialog(null)}
            username={dialog.username}
            process={async (newUsername) => {
              await changeUsername(dialog.username, newUsername, token);
              setUsers((oldUsers) => {
                const users = (oldUsers ?? []).slice();
                const findedUser = users.find(
                  (u) => u.username === dialog.username
                );
                if (findedUser) findedUser.username = newUsername;
                return users;
              });
            }}
          />
        );
        break;
      case kChangePassword:
        dialogNode = (
          <UserChangePasswordDialog
            open
            close={() => setDialog(null)}
            username={dialog.username}
            process={async (newPassword) => {
              await changePassword(dialog.username, newPassword, token);
            }}
          />
        );
        break;
      case kChangePermission: {
        break;
      }
    }

  if (users) {
    const userComponents = users.map((user) => {
      return (
        <UserItem
          key={user.username}
          user={user}
          onContextMenu={(item) => {}}
        />
      );
    });

    return (
      <AdminSubPage>
        <Button
          variant="success"
          onClick={() =>
            setDialog({
              type: "create",
            })
          }
          className="align-self-end"
        >
          Create User
        </Button>
        {userComponents}
        {dialogNode}
      </AdminSubPage>
    );
  } else {
    return <Spinner animation="border" />;
  }
};

export default UserAdmin;
