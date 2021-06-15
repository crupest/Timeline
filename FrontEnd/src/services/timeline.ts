import { TimelineVisibility } from "http/timeline";
import XRegExp from "xregexp";
import { Observable } from "rxjs";
import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";

import { getHttpToken } from "http/common";

const timelineNameReg = XRegExp("^[-_\\p{L}]*$", "u");

export function validateTimelineName(name: string): boolean {
  return timelineNameReg.test(name);
}

export const timelineVisibilityTooltipTranslationMap: Record<
  TimelineVisibility,
  string
> = {
  Public: "timeline.visibilityTooltip.public",
  Register: "timeline.visibilityTooltip.register",
  Private: "timeline.visibilityTooltip.private",
};

export function getTimelinePostUpdate$(
  timelineName: string
): Observable<{ update: boolean; state: HubConnectionState }> {
  return new Observable((subscriber) => {
    subscriber.next({
      update: false,
      state: HubConnectionState.Connecting,
    });

    const token = getHttpToken();
    const connection = new HubConnectionBuilder()
      .withUrl("/api/hub/timeline", {
        accessTokenFactory: token == null ? undefined : () => token,
      })
      .withAutomaticReconnect()
      .build();

    const handler = (tn: string): void => {
      if (timelineName === tn) {
        subscriber.next({ update: true, state: connection.state });
      }
    };

    connection.onclose(() => {
      subscriber.next({
        update: false,
        state: HubConnectionState.Disconnected,
      });
    });

    connection.onreconnecting(() => {
      subscriber.next({
        update: false,
        state: HubConnectionState.Reconnecting,
      });
    });

    connection.onreconnected(() => {
      subscriber.next({
        update: false,
        state: HubConnectionState.Connected,
      });
    });

    connection.on("OnTimelinePostChanged", handler);

    void connection.start().then(() => {
      subscriber.next({ update: false, state: HubConnectionState.Connected });

      return connection.invoke("SubscribeTimelinePostChange", timelineName);
    });

    return () => {
      connection.off("OnTimelinePostChanged", handler);

      if (connection.state === HubConnectionState.Connected) {
        void connection
          .invoke("UnsubscribeTimelinePostChange", timelineName)
          .then(() => connection.stop());
      }
    };
  });
}
