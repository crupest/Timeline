import React from "react";
import { useTranslation } from "react-i18next";
import { Button } from "react-bootstrap";

import { pushAlert } from "./services/alert";

if ("serviceWorker" in navigator) {
  let isThisTriggerUpgrade = false;

  const upgradeSuccessLocalStorageKey = "TIMELINE_UPGRADE_SUCCESS";

  if (window.localStorage.getItem(upgradeSuccessLocalStorageKey)) {
    pushAlert({
      message: {
        type: "i18n",
        key: "serviceWorker.upgradeSuccess",
      },
      type: "success",
    });
    window.localStorage.removeItem(upgradeSuccessLocalStorageKey);
  }

  void import("workbox-window").then(({ Workbox, messageSW }) => {
    const wb = new Workbox("/sw.js");
    let registration: ServiceWorkerRegistration | undefined;

    // externalactivated is not usable but I still use its name.
    wb.addEventListener("controlling", () => {
      const upgradeReload = (): void => {
        window.localStorage.setItem(upgradeSuccessLocalStorageKey, "true");
        window.location.reload();
      };

      if (isThisTriggerUpgrade) {
        upgradeReload();
      } else {
        const Message: React.FC = () => {
          const { t } = useTranslation();
          return (
            <>
              {t("serviceWorker.externalActivatedPrompt")}
              <Button
                variant="outline-success"
                size="sm"
                onClick={upgradeReload}
              >
                {t("serviceWorker.reloadNow")}
              </Button>
            </>
          );
        };

        pushAlert({
          message: Message,
          dismissTime: "never",
          type: "warning",
        });
      }
    });

    wb.addEventListener("activated", (event) => {
      if (!event.isUpdate) {
        pushAlert({
          message: {
            type: "i18n",
            key: "serviceWorker.availableOffline",
          },
          type: "success",
        });
      }
    });

    // Add an event listener to detect when the registered
    // service worker has installed but is waiting to activate.
    wb.addEventListener("waiting", (): void => {
      const upgrade = (): void => {
        isThisTriggerUpgrade = true;
        if (registration && registration.waiting) {
          // Send a message to the waiting service worker,
          // instructing it to activate.
          // Note: for this to work, you have to add a message
          // listener in your service worker. See below.
          void messageSW(registration.waiting, { type: "SKIP_WAITING" });
        }
      };

      const UpgradeMessage: React.FC = () => {
        const { t } = useTranslation();
        return (
          <>
            {t("serviceWorker.upgradePrompt")}
            <Button variant="outline-success" size="sm" onClick={upgrade}>
              {t("serviceWorker.upgradeNow")}
            </Button>
          </>
        );
      };

      pushAlert({
        message: UpgradeMessage,
        dismissTime: "never",
        type: "success",
      });
    });

    void wb.register().then((reg) => {
      registration = reg;
    });
  });
}
