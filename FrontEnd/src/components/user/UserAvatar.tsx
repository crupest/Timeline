import { Ref, ComponentPropsWithoutRef } from "react";

import { getHttpUserClient } from "~src/http/user";

export interface UserAvatarProps extends ComponentPropsWithoutRef<"img"> {
  username: string;
  imgRef?: Ref<HTMLImageElement> | null;
}

export default function UserAvatar({
  username,
  imgRef,
  ...otherProps
}: UserAvatarProps) {
  return (
    <img
      ref={imgRef}
      src={getHttpUserClient().generateAvatarUrl(username)}
      {...otherProps}
    />
  );
}
