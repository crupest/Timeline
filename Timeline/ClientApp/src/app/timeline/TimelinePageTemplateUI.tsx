import React from 'react';
import { Spinner } from 'reactstrap';
import { useTranslation } from 'react-i18next';
import { fromEvent } from 'rxjs';
import Svg from 'react-inlinesvg';

import arrowsAngleContractIcon from 'bootstrap-icons/icons/arrows-angle-contract.svg';
import arrowsAngleExpandIcon from 'bootstrap-icons/icons/arrows-angle-expand.svg';

import { getAlertHost } from '../common/alert-service';

import Timeline, {
  TimelinePostInfoEx,
  TimelineDeleteCallback,
} from './Timeline';
import AppBar from '../common/AppBar';
import TimelinePostEdit, { TimelinePostSendCallback } from './TimelinePostEdit';
import { useEventEmiiter } from '../common';

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

  const timelineRef = React.useRef<HTMLDivElement | null>(null);

  const [getResizeEvent, triggerResizeEvent] = useEventEmiiter();

  React.useEffect(() => {
    const { current: timelineElement } = timelineRef;
    if (timelineElement != null) {
      let loadingScrollToBottom = true;
      let pinBottom = false;

      const isAtBottom = (): boolean =>
        window.innerHeight + window.scrollY + 10 >= document.body.scrollHeight;

      const disableLoadingScrollToBottom = (): void => {
        loadingScrollToBottom = false;
        if (isAtBottom()) pinBottom = true;
      };

      const checkAndScrollToBottom = (): void => {
        if (loadingScrollToBottom || pinBottom) {
          window.scrollTo(0, document.body.scrollHeight);
        }
      };

      const subscriptions = [
        fromEvent(timelineElement, 'wheel').subscribe(
          disableLoadingScrollToBottom
        ),
        fromEvent(timelineElement, 'pointerdown').subscribe(
          disableLoadingScrollToBottom
        ),
        fromEvent(timelineElement, 'keydown').subscribe(
          disableLoadingScrollToBottom
        ),
        fromEvent(window, 'scroll').subscribe(() => {
          if (loadingScrollToBottom) return;

          if (isAtBottom()) {
            pinBottom = true;
          } else {
            pinBottom = false;
          }
        }),
        fromEvent(window, 'resize').subscribe(checkAndScrollToBottom),
        getResizeEvent().subscribe(checkAndScrollToBottom),
      ];

      return () => {
        subscriptions.forEach((s) => s.unsubscribe());
      };
    }
  }, [getResizeEvent, triggerResizeEvent, timeline, props.posts]);

  const [cardHeight, setCardHeight] = React.useState<number>(0);

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
              containerRef={timelineRef}
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
        timelineBody = (
          <div className="full-viewport-center-child">
            <Spinner color="primary" type="grow" />
          </div>
        );
      }
      const { CardComponent } = props;

      body = (
        <>
          <div
            className="fixed-top mt-appbar info-card-container"
            data-collapse={infoCardCollapse ? 'true' : 'false'}
          >
            <Svg
              src={
                infoCardCollapse
                  ? arrowsAngleExpandIcon
                  : arrowsAngleContractIcon
              }
              collapse={infoCardCollapse}
              onClick={() => {
                const newState = !infoCardCollapse;
                setInfoCardCollapse(newState);
                window.localStorage.setItem(
                  genCardCollapseLocalStorageKey(timeline.name),
                  newState.toString()
                );
              }}
              className="float-right m-1 info-card-collapse-button text-primary icon-button"
            />
            <CardComponent
              timeline={timeline}
              onManage={props.onManage}
              onMember={props.onMember}
              onHeight={setCardHeight}
              className="info-card-content"
            />
          </div>
          {timelineBody}
        </>
      );
    } else {
      body = (
        <div className="full-viewport-center-child">
          <Spinner color="primary" type="grow" />
        </div>
      );
    }
  }

  return (
    <>
      <AppBar />
      <div>
        <div
          style={{ height: 56 + cardHeight }}
          className="timeline-page-top-space flex-fix-length"
        />
        {body}
      </div>
    </>
  );
}
