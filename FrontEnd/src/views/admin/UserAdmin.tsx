import React, { useState, useEffect } from "react";
import classnames from "classnames";

import OperationDialog, {
  OperationDialogBoolInput,
} from "../common/dailog/OperationDialog";

import { AuthUser } from "@/services/user";
import { getHttpUserClient, HttpUser, kUserPermissionList } from "@/http/user";
import { Trans, useTranslation } from "react-i18next";
import Button from "../common/button/Button";
import Spinner from "../common/Spinner";
import FlatButton from "../common/button/FlatButton";

const CreateUserDialog: React.FC<{
  open: boolean;
  close: () => void;
  onSuccess: (user: HttpUser) => void;
}> = ({ open, close, onSuccess }) => {
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
      onClose={close}
      open={open}
      onSuccessAndClose={onSuccess}
    />
  );
};

const UsernameLabel: React.FC = (props) => {
  return <span style={{ color: "blue" }}>{props.children}</span>;
};

const UserDeleteDialog: React.FC<{
  open: boolean;
  close: () => void;
  user: HttpUser;
  onSuccess: () => void;
}> = ({ open, close, user, onSuccess }) => {
  return (
    <OperationDialog
      open={open}
      onClose={close}
      title="admin:user.dialog.delete.title"
      themeColor="danger"
      inputPrompt={() => (
        <Trans i18nKey="admin:user.dialog.delete.prompt">
          0<UsernameLabel>{user.username}</UsernameLabel>2
        </Trans>
      )}
      onProcess={() => getHttpUserClient().delete(user.username)}
      onSuccessAndClose={onSuccess}
    />
  );
};

const UserModifyDialog: React.FC<{
  open: boolean;
  close: () => void;
  user: HttpUser;
  onSuccess: () => void;
}> = ({ open, close, user, onSuccess }) => {
  return (
    <OperationDialog
      open={open}
      onClose={close}
      title="admin:user.dialog.modify.title"
      themeColor="danger"
      inputPrompt={() => (
        <Trans i18nKey="admin:user.dialog.modify.prompt">
          0<UsernameLabel>{user.username}</UsernameLabel>2
        </Trans>
      )}
      inputScheme={
        [
          {
            type: "text",
            label: "admin:user.username",
            initValue: user.username,
          },
          { type: "text", label: "admin:user.password" },
          {
            type: "text",
            label: "admin:user.nickname",
            initValue: user.nickname,
          },
        ] as const
      }
      onProcess={([username, password, nickname]) =>
        getHttpUserClient().patch(user.username, {
          username: username !== user.username ? username : undefined,
          password: password !== "" ? password : undefined,
          nickname: nickname !== user.nickname ? nickname : undefined,
        })
      }
      onSuccessAndClose={onSuccess}
    />
  );
};

const UserPermissionModifyDialog: React.FC<{
  open: boolean;
  close: () => void;
  user: HttpUser;
  onSuccess: () => void;
}> = ({ open, close, user, onSuccess }) => {
  const oldPermissionBoolList: boolean[] = kUserPermissionList.map(
    (permission) => user.permissions.includes(permission)
  );

  return (
    <OperationDialog
      open={open}
      onClose={close}
      title="admin:user.dialog.modifyPermissions.title"
      themeColor="danger"
      inputPrompt={() => (
        <Trans i18nKey="admin:user.dialog.modifyPermissions.prompt">
          0<UsernameLabel>{user.username}</UsernameLabel>2
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
            await getHttpUserClient().putUserPermission(
              user.username,
              permission
            );
          } else {
            await getHttpUserClient().deleteUserPermission(
              user.username,
              permission
            );
          }
        }
        return newPermissionBoolList;
      }}
      onSuccessAndClose={onSuccess}
    />
  );
};

interface UserItemProps {
  user: HttpUser;
  onChange: () => void;
}

const UserItem: React.FC<UserItemProps> = ({ user, onChange }) => {
  const { t } = useTranslation();

  const [dialog, setDialog] = useState<
    "delete" | "modify" | "permission" | null
  >(null);

  const [editMaskVisible, setEditMaskVisible] = React.useState<boolean>(false);

  return (
    <>
      <div className="admin-user-item">
        <i
          className="bi-pencil-square cru-float-right icon-button cru-color-primary-enhance"
          onClick={() => setEditMaskVisible(true)}
        />
        <h5 className="cru-color-primary">{user.username}</h5>
        <small className="d-block cru-color-secondary">
          {t("admin:user.nickname")}
          {user.nickname}
        </small>
        <small className="d-block cru-color-secondary">
          {t("admin:user.uniqueId")}
          {user.uniqueId}
        </small>
        <small className="d-block cru-color-secondary">
          {t("admin:user.permissions")}
          {user.permissions.map((permission) => {
            return (
              <span key={permission} className="cru-color-danger">
                {permission}{" "}
              </span>
            );
          })}
        </small>
        <div
          className={classnames("edit-mask", !editMaskVisible && "d-none")}
          onClick={() => setEditMaskVisible(false)}
        >
          <FlatButton
            text="admin:user.modify"
            onClick={() => setDialog("modify")}
          />
          <FlatButton
            text="admin:user.modifyPermissions"
            onClick={() => setDialog("permission")}
          />
          <FlatButton
            text="admin:user.delete"
            color="danger"
            onClick={() => setDialog("delete")}
          />
        </div>
      </div>
      <UserDeleteDialog
        open={dialog === "delete"}
        close={() => setDialog(null)}
        user={user}
        onSuccess={onChange}
      />
      <UserModifyDialog
        open={dialog === "modify"}
        close={() => setDialog(null)}
        user={user}
        onSuccess={onChange}
      />
      <UserPermissionModifyDialog
        open={dialog === "permission"}
        close={() => setDialog(null)}
        user={user}
        onSuccess={onChange}
      />
    </>
  );
};

interface UserAdminProps {
  user: AuthUser;
}

const UserAdmin: React.FC<UserAdminProps> = () => {
  const [users, setUsers] = useState<HttpUser[] | null>(null);
  const [dialog, setDialog] = useState<"create" | null>(null);
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

  if (users) {
    const userComponents = users.map((user) => {
      return (
        <UserItem key={user.username} user={user} onChange={updateUsers} />
      );
    });

    return (
      <>
        <div className="row justify-content-end my-2">
          <div className="col col-auto">
            <Button
              text="admin:create"
              color="success"
              onClick={() => setDialog("create")}
            />
          </div>
        </div>
        {userComponents}
        <CreateUserDialog
          open={dialog === "create"}
          close={() => setDialog(null)}
          onSuccess={updateUsers}
        />
      </>
    );
  } else {
    return <Spinner />;
  }
};

export default UserAdmin;
