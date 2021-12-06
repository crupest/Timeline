import React from "react";
import { useParams } from "react-router-dom";

import TimelinePageTemplate from "../timeline-common/TimelinePageTemplate";
import UserCard from "./UserCard";

import { UiLogicError } from "@/common";

import "./index.css";

const UserPage: React.FC = () => {
  const { username } = useParams();

  if (username == null) {
    throw new UiLogicError("No route param 'username'.");
  }

  const [reloadKey, setReloadKey] = React.useState<number>(0);

  let dialogElement: React.ReactElement | undefined;

  return (
    <>
      <TimelinePageTemplate
        timelineName={`@${username}`}
        notFoundI18nKey="timeline.userNotExist"
        reloadKey={reloadKey}
        onReload={() => setReloadKey(reloadKey + 1)}
        CardComponent={UserCard}
      />
      {dialogElement}
    </>
  );
};

export default UserPage;
