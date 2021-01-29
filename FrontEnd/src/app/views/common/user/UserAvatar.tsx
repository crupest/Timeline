import React from "react";

import { useAvatar } from "@/services/user";

import BlobImage from "../BlobImage";

export interface UserAvatarProps
  extends React.ImgHTMLAttributes<HTMLImageElement> {
  username: string;
}

const UserAvatar: React.FC<UserAvatarProps> = ({ username, ...otherProps }) => {
  const avatar = useAvatar(username);

  return <BlobImage blob={avatar} {...otherProps} />;
};

export default UserAvatar;
