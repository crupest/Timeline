import React from "react";
import clsx from "clsx";
import { useTranslation } from "react-i18next";
import { fromEvent } from "rxjs";
import { Dropdown, Button } from "react-bootstrap";

import { timelineVisibilityTooltipTranslationMap } from "@/services/timeline";
import { useAvatar } from "@/services/user";

import BlobImage from "../common/BlobImage";
import { TimelineCardComponentProps } from "../timeline-common/TimelinePageTemplateUI";

export type PersonalTimelineManageItem = "avatar" | "nickname";

export type UserInfoCardProps = TimelineCardComponentProps<
  PersonalTimelineManageItem
>;

const UserInfoCard: React.FC<UserInfoCardProps> = (props) => {
  const { onHeight, onManage } = props;
  const { t } = useTranslation();

  const avatar = useAvatar(props.timeline.owner.username);

  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const containerRef = React.useRef<HTMLDivElement>(null!);

  const notifyHeight = React.useCallback((): void => {
    if (onHeight) {
      onHeight(containerRef.current.getBoundingClientRect().height);
    }
  }, [onHeight]);

  React.useEffect(() => {
    const subscription = fromEvent(window, "resize").subscribe(notifyHeight);
    return () => subscription.unsubscribe();
  });

  return (
    <div
      ref={containerRef}
      className={clsx("rounded border bg-light p-2 clearfix", props.className)}
      onTransitionEnd={notifyHeight}
    >
      <BlobImage
        blob={avatar}
        onLoad={notifyHeight}
        className="avatar large mr-2 rounded-circle float-left"
      />
      <div>
        {props.timeline.owner.nickname}
        <small className="ml-3 text-secondary">
          @{props.timeline.owner.username}
        </small>
      </div>
      <p className="mb-0">{props.timeline.description}</p>
      <small className="mt-1 d-block">
        {t(timelineVisibilityTooltipTranslationMap[props.timeline.visibility])}
      </small>
      <div className="text-right mt-2">
        {onManage != null ? (
          <Dropdown>
            <Dropdown.Toggle variant="outline-primary">
              {t("timeline.manage")}
            </Dropdown.Toggle>
            <Dropdown.Menu>
              <Dropdown.Item onClick={() => onManage("nickname")}>
                {t("timeline.manageItem.nickname")}
              </Dropdown.Item>
              <Dropdown.Item onClick={() => onManage("avatar")}>
                {t("timeline.manageItem.avatar")}
              </Dropdown.Item>
              <Dropdown.Item onClick={() => onManage("property")}>
                {t("timeline.manageItem.property")}
              </Dropdown.Item>
              <Dropdown.Item onClick={props.onMember}>
                {t("timeline.manageItem.member")}
              </Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
        ) : (
          <Button variant="outline-primary" onClick={props.onMember}>
            {t("timeline.memberButton")}
          </Button>
        )}
      </div>
    </div>
  );
};

export default UserInfoCard;
