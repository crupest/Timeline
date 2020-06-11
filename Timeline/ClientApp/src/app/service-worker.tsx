import React from 'react';
import { WorkboxLifecycleEvent } from 'workbox-window/utils/WorkboxEvent';
import { Button } from 'reactstrap';
import { useTranslation } from 'react-i18next';

import { pushAlert } from './common/alert-service';

if ('serviceWorker' in navigator) {
  void import('workbox-window').then(({ Workbox, messageSW }) => {
    const wb = new Workbox('/sw.js');
    let registration: ServiceWorkerRegistration | undefined;

    const showFirstPrompt = (event: WorkboxLifecycleEvent): void => {
      if (!event.isUpdate) {
        pushAlert({
          message: {
            type: 'i18n',
            key: 'serviceWorker.availableOffline',
          },
          type: 'success',
        });
      }
    };

    wb.addEventListener('activated', showFirstPrompt);
    wb.addEventListener('externalactivated', showFirstPrompt);

    const showSkipWaitingPrompt = (): void => {
      const upgrade = (): void => {
        // Assuming the user accepted the update, set up a listener
        // that will reload the page as soon as the previously waiting
        // service worker has taken control.
        wb.addEventListener('controlling', () => {
          window.location.reload();
        });

        if (registration && registration.waiting) {
          // Send a message to the waiting service worker,
          // instructing it to activate.
          // Note: for this to work, you have to add a message
          // listener in your service worker. See below.
          void messageSW(registration.waiting, { type: 'SKIP_WAITING' });
        }
      };

      const UpgradeMessage: React.FC = () => {
        const { t } = useTranslation();
        return (
          <>
            {t('serviceWorker.upgradeTitle')}
            <Button color="success" size="sm" onClick={upgrade} outline>
              {t('serviceWorker.upgradeNow')}
            </Button>
          </>
        );
      };

      pushAlert({
        message: UpgradeMessage,
        dismissTime: 'never',
        type: 'success',
      });
    };

    // Add an event listener to detect when the registered
    // service worker has installed but is waiting to activate.
    wb.addEventListener('waiting', showSkipWaitingPrompt);
    wb.addEventListener('externalwaiting', showSkipWaitingPrompt);

    void wb.register().then((reg) => {
      registration = reg;
    });
  });
}
