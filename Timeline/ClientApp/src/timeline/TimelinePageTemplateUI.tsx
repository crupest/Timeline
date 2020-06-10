import React from 'react';
import { Spinner } from 'reactstrap';
import { useTranslation } from 'react-i18next';
import { Subject, fromEvent } from 'rxjs';

import { getAlertHost } from '../common/alert-service';

import Timeline, {
  TimelinePostInfoEx,
  TimelineDeleteCallback,
} from './Timeline';
import AppBar from '../common/AppBar';
import TimelinePostEdit, { TimelinePostSendCallback } from './TimelinePostEdit';
import CollapseButton from '../common/CollapseButton';

export interface TimelineCardComponentProps<TTimeline, TManageItems> {
  timeline: TTimeline;
  onManage?: (item: TManageItems | 'property') => void;
  onMember: () => void;
  className?: string;
  onHeight?: (height: number) => void;
}

export interface TimelinePageTemplateUIProps<
  TTimeline extends { name: string },
  TManageItems
> {
  avatarKey?: string | number;
  timeline?: TTimeline;
  posts?: TimelinePostInfoEx[] | 'forbid';
  CardComponent: React.ComponentType<
    TimelineCardComponentProps<TTimeline, TManageItems>
  >;
  onMember: () => void;
  onManage?: (item: TManageItems | 'property') => void;
  onPost?: TimelinePostSendCallback;
  onDelete: TimelineDeleteCallback;
  error?: string;
}

export default function TimelinePageTemplateUI<
  TTimeline extends { name: string },
  TEditItems
>(
  props: TimelinePageTemplateUIProps<TTimeline, TEditItems>
): React.ReactElement | null {
  const { timeline } = props;

  const { t } = useTranslation();

  const bottomSpaceRef = React.useRef<HTMLDivElement | null>(null);

  const onPostEditHeightChange = React.useCallback((height: number): void => {
    const { current: bottomSpaceDiv } = bottomSpaceRef;
    if (bottomSpaceDiv != null) {
      bottomSpaceDiv.style.height = `${height}px`;
    }
    if (height === 0) {
      const alertHost = getAlertHost();
      if (alertHost != null) {
        alertHost.style.removeProperty('margin-bottom');
      }
    } else {
      const alertHost = getAlertHost();
      if (alertHost != null) {
        alertHost.style.marginBottom = `${height}px`;
      }
    }
  }, []);

  const resizeSubject = React.useMemo(() => new Subject(), []);
  const triggerResizeEvent = React.useCallback(() => {
    resizeSubject.next(null);
  }, [resizeSubject]);

  React.useEffect(() => {
    let scrollToBottom = true;
    const disableScrollToBottom = (): void => {
      scrollToBottom = false;
    };

    const subscriptions = [
      fromEvent(window, 'wheel').subscribe(disableScrollToBottom),
      fromEvent(window, 'pointerdown').subscribe(disableScrollToBottom),
      fromEvent(window, 'keydown').subscribe(disableScrollToBottom),
      resizeSubject.subscribe(() => {
        if (scrollToBottom) {
          window.scrollTo(0, document.body.scrollHeight);
        }
      }),
    ];

    return () => {
      subscriptions.forEach((s) => s.unsubscribe());
    };
  }, [resizeSubject, timeline, props.posts]);

  const [cardHeight, setCardHeight] = React.useState<number>(0);

  const onCardHeightChange = React.useCallback((height: number) => {
    setCardHeight(height);
  }, []);

  const genCardCollapseLocalStorageKey = (timelineName: string): string =>
    `timeline.${timelineName}.cardCollapse`;

  const cardCollapseLocalStorageKey =
    timeline != null ? genCardCollapseLocalStorageKey(timeline.name) : null;

  const [infoCardCollapse, setInfoCardCollapse] = React.useState<boolean>(true);
  React.useEffect(() => {
    if (cardCollapseLocalStorageKey != null) {
      const savedCollapse =
        window.localStorage.getItem(cardCollapseLocalStorageKey) === 'true';
      setInfoCardCollapse(savedCollapse);
    }
  }, [cardCollapseLocalStorageKey]);

  let body: React.ReactElement;

  if (props.error != null) {
    body = <p className="text-danger">{t(props.error)}</p>;
  } else {
    if (timeline != null) {
      let timelineBody: React.ReactElement;
      if (props.posts != null) {
        if (props.posts === 'forbid') {
          timelineBody = (
            <p className="text-danger">{t('timeline.messageCantSee')}</p>
          );
        } else {
          timelineBody = (
            <Timeline
              posts={props.posts}
              onDelete={props.onDelete}
              onResize={triggerResizeEvent}
            />
          );
          if (props.onPost != null) {
            timelineBody = (
              <>
                {timelineBody}
                <div ref={bottomSpaceRef} className="flex-fix-length" />
                <TimelinePostEdit
                  onPost={props.onPost}
                  onHeightChange={onPostEditHeightChange}
                  timelineName={timeline.name}
                />
              </>
            );
          }
        }
      } else {
        timelineBody = <Spinner />;
      }
      const { CardComponent } = props;

      body = (
        <>
          <div
            className="fixed-top mt-appbar info-card-container"
            data-collapse={infoCardCollapse ? 'true' : 'false'}
          >
            <CollapseButton
              collapse={infoCardCollapse}
              onClick={() => {
                const newState = !infoCardCollapse;
                setInfoCardCollapse(newState);
                window.localStorage.setItem(
                  genCardCollapseLocalStorageKey(timeline.name),
                  newState.toString()
                );
              }}
              className="float-right m-1 info-card-collapse-button"
            />
            <CardComponent
              timeline={timeline}
              onManage={props.onManage}
              onMember={props.onMember}
              onHeight={onCardHeightChange}
              className="info-card-content"
            />
          </div>
          {timelineBody}
        </>
      );
    } else {
      body = <Spinner />;
    }
  }

  return (
    <>
      <AppBar />
      <div
        style={{ marginTop: 56 + cardHeight }}
        className="timeline-page-container"
      >
        {body}
      </div>
    </>
  );
}
