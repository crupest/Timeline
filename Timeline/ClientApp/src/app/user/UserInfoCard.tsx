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

import { timelineVisibilityTooltipTranslationMap } from '../data/timeline';
import { useAvatar } from '../data/user';

import { TimelineCardComponentProps } from '../timeline/TimelinePageTemplateUI';
import BlobImage from '../common/BlobImage';

export type PersonalTimelineManageItem = 'avatar' | 'nickname';

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
      className={clsx('rounded border bg-light p-2', props.className)}
      onTransitionEnd={notifyHeight}
    >
      <BlobImage
        blob={avatar}
        onLoad={notifyHeight}
        className="avatar large mr-2 mb-2 rounded-circle float-left"
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
          <Dropdown isOpen={manageDropdownOpen} toggle={toggleManageDropdown}>
            <DropdownToggle outline color="primary">
              {t('timeline.manage')}
            </DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={() => onManage('nickname')}>
                {t('timeline.manageItem.nickname')}
              </DropdownItem>
              <DropdownItem onClick={() => onManage('avatar')}>
                {t('timeline.manageItem.avatar')}
              </DropdownItem>
              <DropdownItem onClick={() => onManage('property')}>
                {t('timeline.manageItem.property')}
              </DropdownItem>
              <DropdownItem onClick={props.onMember}>
                {t('timeline.manageItem.member')}
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

export default UserInfoCard;
