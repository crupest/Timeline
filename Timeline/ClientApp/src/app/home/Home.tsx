import React from 'react';
import { useHistory } from 'react-router';
import { Row, Container, Button, Col } from 'reactstrap';
import { useTranslation } from 'react-i18next';

import { useUser } from '../data/user';
import { TimelineInfo } from '../data/timeline';
import { getHttpTimelineClient } from '../http/timeline';

import AppBar from '../common/AppBar';
import SearchInput from '../common/SearchInput';
import TimelineBoardAreaWithoutUser from './TimelineBoardAreaWithoutUser';
import TimelineBoardAreaWithUser from './TimelineBoardAreaWithUser';
import TimelineCreateDialog from './TimelineCreateDialog';

const Home: React.FC = (_) => {
  const history = useHistory();

  const { t } = useTranslation();

  const user = useUser();

  const [navText, setNavText] = React.useState<string>('');

  const [publicTimelines, setPublicTimelines] = React.useState<
    TimelineInfo[] | undefined
  >(undefined);
  const [ownTimelines, setOwnTimelines] = React.useState<
    TimelineInfo[] | undefined
  >(undefined);
  const [joinTimelines, setJoinTimelines] = React.useState<
    TimelineInfo[] | undefined
  >(undefined);

  React.useEffect(() => {
    let subscribe = true;
    if (user == null) {
      setOwnTimelines(undefined);
      setJoinTimelines(undefined);
      void getHttpTimelineClient()
        .listTimeline({ visibility: 'Public' })
        .then((timelines) => {
          if (subscribe) {
            setPublicTimelines(timelines);
          }
        });
    } else {
      setPublicTimelines(undefined);
      void getHttpTimelineClient()
        .listTimeline({ relate: user.username, relateType: 'own' })
        .then((timelines) => {
          if (subscribe) {
            setOwnTimelines(timelines);
          }
        });
      void getHttpTimelineClient()
        .listTimeline({ relate: user.username, relateType: 'join' })
        .then((timelines) => {
          if (subscribe) {
            setJoinTimelines(timelines);
          }
        });
    }
    return () => {
      subscribe = false;
    };
  }, [user]);

  const [dialog, setDialog] = React.useState<'create' | null>(null);

  const goto = React.useCallback((): void => {
    if (navText === '') {
      history.push('users/crupest');
    } else if (navText.startsWith('@')) {
      history.push(`users/${navText.slice(1)}`);
    } else {
      history.push(`timelines/${navText}`);
    }
  }, [navText, history]);

  const openCreateDialog = React.useCallback(() => {
    setDialog('create');
  }, []);

  const closeDialog = React.useCallback(() => {
    setDialog(null);
  }, []);

  return (
    <>
      <AppBar />
      <Container fluid style={{ marginTop: '56px' }}>
        <Row>
          <Col>
            <SearchInput
              className="justify-content-center"
              value={navText}
              onChange={setNavText}
              onButtonClick={goto}
              buttonText={t('home.go')}
              placeholder="@crupest"
              additionalButton={
                user != null && (
                  <Button color="success" outline onClick={openCreateDialog}>
                    {t('home.createButton')}
                  </Button>
                )
              }
            />
          </Col>
        </Row>
        {(() => {
          if (user == null) {
            return (
              <TimelineBoardAreaWithoutUser publicTimelines={publicTimelines} />
            );
          } else {
            return (
              <TimelineBoardAreaWithUser
                ownTimelines={ownTimelines}
                joinTimelines={joinTimelines}
              />
            );
          }
        })()}
      </Container>
      <footer className="text-right">
        <a
          className="mx-3 text-muted"
          href="http://beian.miit.gov.cn/"
          target="_blank"
          rel="noopener noreferrer"
        >
          <small>鄂ICP备18030913号-1</small>
        </a>
        <a
          className="mx-3 text-muted"
          href="http://www.beian.gov.cn/"
          target="_blank"
          rel="noopener noreferrer"
        >
          <small className="white-space-no-wrap">公安备案 42112102000124</small>
        </a>
      </footer>
      {dialog === 'create' && <TimelineCreateDialog open close={closeDialog} />}
    </>
  );
};

export default Home;
