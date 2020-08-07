import React from 'react';
import { useHistory } from 'react-router';
import { Row, Container, Button, Col } from 'reactstrap';
import { useTranslation } from 'react-i18next';

import { useUser } from '../data/user';

import AppBar from '../common/AppBar';
import SearchInput from '../common/SearchInput';
import BoardWithoutUser from './BoardWithoutUser';
import BoardWithUser from './BoardWithUser';
import TimelineCreateDialog from './TimelineCreateDialog';

const Home: React.FC = () => {
  const history = useHistory();

  const { t } = useTranslation();

  const user = useUser();

  const [navText, setNavText] = React.useState<string>('');

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
                  <Button
                    color="success"
                    outline
                    onClick={() => {
                      setDialog('create');
                    }}
                  >
                    {t('home.createButton')}
                  </Button>
                )
              }
            />
          </Col>
        </Row>
        {(() => {
          if (user == null) {
            return <BoardWithoutUser />;
          } else {
            return <BoardWithUser user={user} />;
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
      {dialog === 'create' && (
        <TimelineCreateDialog
          open
          close={() => {
            setDialog(null);
          }}
        />
      )}
    </>
  );
};

export default Home;
