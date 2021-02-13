import React from "react";

import { UiLogicError } from "@/common";

import { HttpNetworkError, HttpNotFoundError } from "@/http/common";
import { getHttpTimelineClient, HttpTimelineInfo } from "@/http/timeline";

import { TimelineMemberDialog } from "./TimelineMember";
import TimelinePropertyChangeDialog from "./TimelinePropertyChangeDialog";
import { TimelinePageTemplateUIProps } from "./TimelinePageTemplateUI";

export interface TimelinePageTemplateProps<TManageItem> {
  name: string;
  onManage: (item: TManageItem) => void;
  UiComponent: React.ComponentType<
    Omit<TimelinePageTemplateUIProps<TManageItem>, "CardComponent">
  >;
  notFoundI18nKey: string;
  reloadKey: number;
  onReload: () => void;
}

export default function TimelinePageTemplate<TManageItem>(
  props: TimelinePageTemplateProps<TManageItem>
): React.ReactElement | null {
  const { name, reloadKey, onReload } = props;

  const [dialog, setDialog] = React.useState<null | "property" | "member">(
    null
  );

  const [timeline, setTimeline] = React.useState<
    HttpTimelineInfo | "loading" | "offline" | "notexist" | "error"
  >("loading");

  React.useEffect(() => {
    setTimeline("loading");

    let subscribe = true;
    void getHttpTimelineClient()
      .getTimeline(name)
      .then(
        (data) => {
          if (subscribe) {
            setTimeline(data);
          }
        },
        (error) => {
          if (subscribe) {
            if (error instanceof HttpNetworkError) {
              setTimeline("offline");
            } else if (error instanceof HttpNotFoundError) {
              setTimeline("notexist");
            } else {
              console.error(error);
              setTimeline("error");
            }
          }
        }
      );
    return () => {
      subscribe = false;
    };
  }, [name, reloadKey]);

  let dialogElement: React.ReactElement | undefined;
  const closeDialog = (): void => setDialog(null);

  if (dialog === "property") {
    if (typeof timeline !== "object") {
      throw new UiLogicError(
        "Timeline is null but attempt to open change property dialog."
      );
    }

    dialogElement = (
      <TimelinePropertyChangeDialog
        open
        close={closeDialog}
        timeline={timeline}
        onChange={onReload}
      />
    );
  } else if (dialog === "member") {
    if (typeof timeline !== "object") {
      throw new UiLogicError(
        "Timeline is null but attempt to open change property dialog."
      );
    }

    dialogElement = (
      <TimelineMemberDialog
        open
        onClose={closeDialog}
        timeline={timeline}
        onChange={onReload}
      />
    );
  }

  const { UiComponent } = props;

  return (
    <>
      <UiComponent
        timeline={
          typeof timeline === "object"
            ? {
                ...timeline,
                operations: {
                  onManage: timeline.manageable
                    ? (item) => {
                        if (item === "property") {
                          setDialog(item);
                        } else {
                          props.onManage(item);
                        }
                      }
                    : undefined,
                  onMember: () => setDialog("member"),
                },
              }
            : timeline
        }
        notExistMessageI18nKey={props.notFoundI18nKey}
      />
      {dialogElement}
    </>
  );
}
