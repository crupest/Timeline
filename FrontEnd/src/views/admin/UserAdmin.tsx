import React, { useState, useEffect } from "react";
import classnames from "classnames";
import { ListGroup, Row, Col, Spinner, Button } from "react-bootstrap";

import OperationDialog, {
  OperationDialogBoolInput,
} from "../common/OperationDialog";

import { AuthUser } from "@/services/user";
import {
  getHttpUserClient,
  HttpUser,
  kUserPermissionList,
  UserPermission,
} from "@/http/user";
import { Trans, useTranslation } from "react-i18next";

interface DialogProps<TData = undefined, TReturn = undefined> {
  open: boolean;
  close: () => void;
  data: TData;
  onSuccess: (data: TReturn) => void;
}

const CreateUserDialog: React.FC<DialogProps<undefined, HttpUser>> = ({
  open,
  close,
  onSuccess,
}) => {
  return (
    <OperationDialog
      title="admin:user.dialog.create.title"
      themeColor="success"
      inputPrompt="admin:user.dialog.create.prompt"
      inputScheme={
        [
          { type: "text", label: "admin:user.username" },
          { type: "text", label: "admin:user.password" },
        ] as const
      }
      onProcess={([username, password]) =>
        getHttpUserClient().post({
          username,
          password,
        })
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

const UserDeleteDialog: React.FC<DialogProps<{ username: string }, unknown>> =
  ({ open, close, data: { username }, onSuccess }) => {
    return (
      <OperationDialog
        open={open}
        close={close}
        title="admin:user.dialog.delete.title"
        themeColor="danger"
        inputPrompt={() => (
          <Trans i18nKey="admin:user.dialog.delete.prompt">
            0<UsernameLabel>{username}</UsernameLabel>2
          </Trans>
        )}
        onProcess={() => getHttpUserClient().delete(username)}
        onSuccessAndClose={onSuccess}
      />
    );
  };

const UserModifyDialog: React.FC<
  DialogProps<
    {
      oldUser: HttpUser;
    },
    HttpUser
  >
> = ({ open, close, data: { oldUser }, onSuccess }) => {
  return (
    <OperationDialog
      open={open}
      close={close}
      title="admin:user.dialog.modify.title"
      themeColor="danger"
      inputPrompt={() => (
        <Trans i18nKey="admin:user.dialog.modify.prompt">
          0<UsernameLabel>{oldUser.username}</UsernameLabel>2
        </Trans>
      )}
      inputScheme={
        [
          {
            type: "text",
            label: "admin:user.username",
            initValue: oldUser.username,
          },
          { type: "text", label: "admin:user.password" },
          {
            type: "text",
            label: "admin:user.nickname",
            initValue: oldUser.nickname,
          },
        ] as const
      }
      onProcess={([username, password, nickname]) =>
        getHttpUserClient().patch(oldUser.username, {
          username: username !== oldUser.username ? username : undefined,
          password: password !== "" ? password : undefined,
          nickname: nickname !== oldUser.nickname ? nickname : undefined,
        })
      }
      onSuccessAndClose={onSuccess}
    />
  );
};

const UserPermissionModifyDialog: React.FC<
  DialogProps<
    {
      username: string;
      permissions: UserPermission[];
    },
    UserPermission[]
  >
> = ({ open, close, data: { username, permissions }, onSuccess }) => {
  const oldPermissionBoolList: boolean[] = kUserPermissionList.map(
    (permission) => permissions.includes(permission)
  );

  return (
    <OperationDialog
      open={open}
      close={close}
      title="admin:user.dialog.modifyPermissions.title"
      themeColor="danger"
      inputPrompt={() => (
        <Trans i18nKey="admin:user.dialog.modifyPermissions.prompt">
          0<UsernameLabel>{username}</UsernameLabel>2
        </Trans>
      )}
      inputScheme={kUserPermissionList.map<OperationDialogBoolInput>(
        (permission, index) => ({
          type: "bool",
          label: permission,
          initValue: oldPermissionBoolList[index],
        })
      )}
      onProcess={async (newPermissionBoolList): Promise<boolean[]> => {
        for (let index = 0; index < kUserPermissionList.length; index++) {
          const oldValue = oldPermissionBoolList[index];
          const newValue = newPermissionBoolList[index];
          const permission = kUserPermissionList[index];
          if (oldValue === newValue) continue;
          if (newValue) {
            await getHttpUserClient().putUserPermission(username, permission);
          } else {
            await getHttpUserClient().deleteUserPermission(
              username,
              permission
            );
          }
        }
        return newPermissionBoolList;
      }}
      onSuccessAndClose={(newPermissionBoolList: boolean[]) => {
        const permissions: UserPermission[] = [];
        for (let index = 0; index < kUserPermissionList.length; index++) {
          if (newPermissionBoolList[index]) {
            permissions.push(kUserPermissionList[index]);
          }
        }
        onSuccess(permissions);
      }}
    />
  );
};

const kModify = "modify";
const kModifyPermission = "permission";
const kDelete = "delete";

type TModify = typeof kModify;
type TModifyPermission = typeof kModifyPermission;
type TDelete = typeof kDelete;

type ContextMenuItem = TModify | TModifyPermission | TDelete;

interface UserItemProps {
  on: { [key in ContextMenuItem]: () => void };
  user: HttpUser;
}

const UserItem: React.FC<UserItemProps> = ({ user, on }) => {
  const { t } = useTranslation();

  const [editMaskVisible, setEditMaskVisible] = React.useState<boolean>(false);

  return (
    <ListGroup.Item className="admin-user-item">
      <i
        className="bi-pencil-square float-end icon-button text-warning"
        onClick={() => setEditMaskVisible(true)}
      />
      <h4 className="text-primary">{user.username}</h4>
      <div className="text-secondary">
        {t("admin:user.nickname")}
        {user.nickname}
      </div>
      <div className="text-secondary">
        {t("admin:user.uniqueId")}
        {user.uniqueId}
      </div>
      <div className="text-secondary">
        {t("admin:user.permissions")}
        {user.permissions.map((permission) => {
          return (
            <span key={permission} className="text-danger">
              {permission}{" "}
            </span>
          );
        })}
      </div>
      <div
        className={classnames("edit-mask", !editMaskVisible && "d-none")}
        onClick={() => setEditMaskVisible(false)}
      >
        <button className="text-button primary" onClick={on[kModify]}>
          {t("admin:user.modify")}
        </button>
        <button className="text-button primary" onClick={on[kModifyPermission]}>
          {t("admin:user.modifyPermissions")}
        </button>
        <button className="text-button danger" onClick={on[kDelete]}>
          {t("admin:user.delete")}
        </button>
      </div>
    </ListGroup.Item>
  );
};

interface UserAdminProps {
  user: AuthUser;
}

const UserAdmin: React.FC<UserAdminProps> = () => {
  const { t } = useTranslation();

  type DialogInfo =
    | null
    | {
        type: "create";
      }
    | {
        type: TModify;
        user: HttpUser;
      }
    | {
        type: TModifyPermission;
        username: string;
        permissions: UserPermission[];
      }
    | { type: TDelete; username: string };

  const [users, setUsers] = useState<HttpUser[] | null>(null);
  const [dialog, setDialog] = useState<DialogInfo>(null);
  const [usersVersion, setUsersVersion] = useState<number>(0);
  const updateUsers = (): void => {
    setUsersVersion(usersVersion + 1);
  };

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
            data={undefined}
            onSuccess={updateUsers}
          />
        );
        break;
      case kDelete:
        dialogNode = (
          <UserDeleteDialog
            open
            close={() => setDialog(null)}
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
            data={{ oldUser: dialog.user }}
            onSuccess={updateUsers}
          />
        );
        break;
      case kModifyPermission:
        dialogNode = (
          <UserPermissionModifyDialog
            open
            close={() => setDialog(null)}
            data={{
              username: dialog.username,
              permissions: dialog.permissions,
            }}
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
            permission: () => {
              setDialog({
                type: kModifyPermission,
                username: user.username,
                permissions: user.permissions,
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
      <>
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
              {t("admin:create")}
            </Button>
          </Col>
        </Row>
        {userComponents}
        {dialogNode}
      </>
    );
  } else {
    return <Spinner animation="border" />;
  }
};

export default UserAdmin;
