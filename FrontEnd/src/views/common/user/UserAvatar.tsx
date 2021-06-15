import React from "react";

import { getHttpUserClient } from "http/user";

export interface UserAvatarProps
  extends React.ImgHTMLAttributes<HTMLImageElement> {
  username: string;
}

const UserAvatar: React.FC<UserAvatarProps> = ({ username, ...otherProps }) => {
  return (
    <img
      src={getHttpUserClient().generateAvatarUrl(username)}
      {...otherProps}
    />
  );
};

export default UserAvatar;
