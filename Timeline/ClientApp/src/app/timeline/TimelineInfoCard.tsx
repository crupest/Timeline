import React from 'react';
import clsx from 'clsx';
import {
  Dropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
  Button,
} from 'reactstrap';
import { useTranslation } from 'react-i18next';
import { fromEvent } from 'rxjs';

import { useAvatarUrl } from '../data/user';
import { timelineVisibilityTooltipTranslationMap } from '../data/timeline';

import { TimelineCardComponentProps } from './TimelinePageTemplateUI';

export type OrdinaryTimelineManageItem = 'delete';

export type TimelineInfoCardProps = TimelineCardComponentProps<
  OrdinaryTimelineManageItem
>;

const TimelineInfoCard: React.FC<TimelineInfoCardProps> = (props) => {
  const { onHeight, onManage } = props;

  const { t } = useTranslation();

  const avatarUrl = useAvatarUrl(props.timeline.owner.username);

  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const containerRef = React.useRef<HTMLDivElement>(null!);

  const notifyHeight = React.useCallback((): void => {
    if (onHeight) {
      onHeight(containerRef.current.getBoundingClientRect().height);
    }
  }, [onHeight]);

  React.useEffect(() => {
    const subscription = fromEvent(window, 'resize').subscribe(notifyHeight);
    return () => subscription.unsubscribe();
  });

  const [manageDropdownOpen, setManageDropdownOpen] = React.useState<boolean>(
    false
  );
  const toggleManageDropdown = React.useCallback(
    (): void => setManageDropdownOpen((old) => !old),
    []
  );

  return (
    <div
      ref={containerRef}
      className={clsx('rounded border p-2 bg-light', props.className)}
      onTransitionEnd={notifyHeight}
    >
      <h3 className="text-primary mx-3 d-inline-block align-middle">
        {props.timeline.name}
      </h3>
      <div className="d-inline-block align-middle">
        <img
          src={avatarUrl}
          onLoad={notifyHeight}
          className="avatar small rounded-circle"
        />
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
          <Dropdown isOpen={manageDropdownOpen} toggle={toggleManageDropdown}>
            <DropdownToggle outline color="primary">
              {t('timeline.manage')}
            </DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={() => onManage('property')}>
                {t('timeline.manageItem.property')}
              </DropdownItem>
              <DropdownItem onClick={props.onMember}>
                {t('timeline.manageItem.member')}
              </DropdownItem>
              <DropdownItem divider />
              <DropdownItem
                className="text-danger"
                onClick={() => onManage('delete')}
              >
                {t('timeline.manageItem.delete')}
              </DropdownItem>
            </DropdownMenu>
          </Dropdown>
        ) : (
          <Button color="primary" outline onClick={props.onMember}>
            {t('timeline.memberButton')}
          </Button>
        )}
      </div>
    </div>
  );
};

export default TimelineInfoCard;
