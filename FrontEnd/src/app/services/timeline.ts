import { TimelineVisibility } from "@/http/timeline";
import XRegExp from "xregexp";
import { Observable } from "rxjs";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";

import { UiLogicError } from "@/common";

import { token$ } from "@/http/common";

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

function createTimelineHubConnection(token: string | null): HubConnection {
  return new HubConnectionBuilder()
    .withUrl("/api/hub/timeline", {
      accessTokenFactory: token == null ? undefined : () => token,
    })
    .withAutomaticReconnect()
    .build();
}

let timelineHubConnection: HubConnection | null = null;

token$.subscribe((token) => {
  if (timelineHubConnection != null) {
    void timelineHubConnection.stop();
  }
  timelineHubConnection = createTimelineHubConnection(token);
  void timelineHubConnection.start();
});

export function getTimelinePostUpdate(
  timelineName: string
): Observable<string> {
  return new Observable((subscriber) => {
    if (timelineHubConnection == null)
      throw new UiLogicError("Connection is null.");

    const connection = timelineHubConnection;

    const handler = (tn: string): void => {
      if (timelineName === tn) {
        subscriber.next(tn);
      }
    };
    connection.on("OnTimelinePostChanged", handler);
    void connection.invoke("SubscribeTimelinePostChange", timelineName);

    return () => {
      void connection.invoke("UnsubscribeTimelinePostChange", timelineName);
      connection.off("OnTimelinePostChanged", handler);
    };
  });
}
