import React from "react";
import { useParams } from "react-router";

import TimelinePageTemplate from "../timeline-common/TimelinePageTemplate";

import TimelinePageUI from "./TimelinePageUI";
import { OrdinaryTimelineManageItem } from "./TimelineInfoCard";
import TimelineDeleteDialog from "./TimelineDeleteDialog";

const TimelinePage: React.FC = () => {
  const { name } = useParams<{ name: string }>();

  const [dialog, setDialog] = React.useState<OrdinaryTimelineManageItem | null>(
    null
  );
  const [reloadKey, setReloadKey] = React.useState<number>(0);

  let dialogElement: React.ReactElement | undefined;
  if (dialog === "delete") {
    dialogElement = (
      <TimelineDeleteDialog open close={() => setDialog(null)} name={name} />
    );
  }

  return (
    <>
      <TimelinePageTemplate
        name={name}
        UiComponent={TimelinePageUI}
        onManage={(item) => setDialog(item)}
        notFoundI18nKey="timeline.timelineNotExist"
        reloadKey={reloadKey}
        onReload={() => setReloadKey(reloadKey + 1)}
      />
      {dialogElement}
    </>
  );
};

export default TimelinePage;
