import React from 'react';
import clsx from 'clsx';
import { Container } from 'reactstrap';

import { TimelinePostInfo } from '../data/timeline';
import { useUser } from '../data/user';
import { useAvatarVersion } from '../user/api';

import TimelineItem from './TimelineItem';

export interface TimelinePostInfoEx extends TimelinePostInfo {
  deletable: boolean;
}

export type TimelineDeleteCallback = (index: number, id: number) => void;

export interface TimelineProps {
  className?: string;
  posts: TimelinePostInfoEx[];
  onDelete: TimelineDeleteCallback;
  onResize?: () => void;
}

const Timeline: React.FC<TimelineProps> = (props) => {
  const user = useUser();
  const avatarVersion = useAvatarVersion();

  const { posts, onDelete, onResize } = props;

  const [indexShowDeleteButton, setIndexShowDeleteButton] = React.useState<
    number
  >(-1);

  const onItemClick = React.useCallback(() => {
    setIndexShowDeleteButton(-1);
  }, []);

  const onToggleDelete = React.useMemo(() => {
    return posts.map((post, i) => {
      return post.deletable
        ? () => {
            setIndexShowDeleteButton((oldIndexShowDeleteButton) => {
              return oldIndexShowDeleteButton !== i ? i : -1;
            });
          }
        : undefined;
    });
  }, [posts]);

  const onItemDelete = React.useMemo(() => {
    return posts.map((post, i) => {
      return () => {
        onDelete(i, post.id);
      };
    });
  }, [posts, onDelete]);

  return (
    <Container
      fluid
      className={clsx('d-flex flex-column position-relative', props.className)}
    >
      <div className="timeline-enter-animation-mask" />
      {(() => {
        const length = posts.length;
        return posts.map((post, i) => {
          const av: number | undefined =
            user != null && user.username === post.author.username
              ? avatarVersion
              : undefined;

          const toggleMore = onToggleDelete[i];

          return (
            <TimelineItem
              post={post}
              key={post.id}
              current={length - 1 === i}
              more={
                toggleMore
                  ? {
                      isOpen: indexShowDeleteButton === i,
                      toggle: toggleMore,
                      onDelete: onItemDelete[i],
                    }
                  : undefined
              }
              onClick={onItemClick}
              avatarVersion={av}
              onResize={onResize}
            />
          );
        });
      })()}
    </Container>
  );
};

export default Timeline;
