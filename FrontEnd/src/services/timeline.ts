import XRegExp from "xregexp";
import {
  Observable,
  BehaviorSubject,
  switchMap,
  filter,
  first,
  distinctUntilChanged,
} from "rxjs";
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from "@microsoft/signalr";

import { TimelineVisibility } from "~src/http/timeline";
import { token$ } from "~src/http/common";

// cSpell:ignore onreconnected onreconnecting

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

type ConnectionState =
  | "Connecting"
  | "Reconnecting"
  | "Disconnected"
  | "Connected";

type Connection = {
  connection: HubConnection;
  state$: Observable<ConnectionState>;
};

function createConnection$(token: string | null): Observable<Connection> {
  return new Observable<Connection>((subscriber) => {
    const connectionStateSubject = new BehaviorSubject<ConnectionState>(
      "Connecting",
    );

    const connection = new HubConnectionBuilder()
      .withUrl("/api/hub/timeline", {
        accessTokenFactory: token == null ? undefined : () => token,
      })
      .withAutomaticReconnect()
      .build();

    connection.onclose = () => {
      connectionStateSubject.next("Disconnected");
    };

    connection.onreconnecting = () => {
      connectionStateSubject.next("Reconnecting");
    };

    connection.onreconnected = () => {
      connectionStateSubject.next("Connected");
    };

    let requestStopped = false;

    void connection.start().then(
      () => {
        connectionStateSubject.next("Connected");
      },
      (e) => {
        if (!requestStopped) {
          throw e;
        }
      },
    );

    subscriber.next({
      connection,
      state$: connectionStateSubject.asObservable(),
    });

    return () => {
      void connection.stop();
      requestStopped = true;
    };
  });
}

const connectionSubject = new BehaviorSubject<Connection | null>(null);

token$
  .pipe(distinctUntilChanged(), switchMap(createConnection$))
  .subscribe(connectionSubject);

const connection$ = connectionSubject
  .asObservable()
  .pipe(filter((c): c is Connection => c != null));

function createTimelinePostUpdateCount$(
  connection: Connection,
  owner: string,
  timeline: string,
): Observable<number> {
  const [o, t] = [owner, timeline];
  return new Observable<number>((subscriber) => {
    const hubConnection = connection.connection;
    let count = 0;

    const handler = (owner: string, timeline: string): void => {
      if (owner === o && timeline === t) {
        subscriber.next(count++);
      }
    };

    let hubOn = false;

    const subscription = connection.state$
      .pipe(first((state) => state === "Connected"))
      .subscribe(() => {
        hubConnection.on("OnTimelinePostChangedV2", handler);
        void hubConnection.invoke(
          "SubscribeTimelinePostChangeV2",
          owner,
          timeline,
        );
        hubOn = true;
      });

    return () => {
      if (hubOn) {
        void hubConnection.invoke(
          "UnsubscribeTimelinePostChangeV2",
          owner,
          timeline,
        );
        hubConnection.off("OnTimelinePostChangedV2", handler);
      }

      subscription.unsubscribe();
    };
  });
}

type OldUpdateInfo = { update: boolean; state: HubConnectionState };

function createTimelinePostOldUpdateInfo$(
  connection: Connection,
  owner: string,
  timeline: string,
): Observable<OldUpdateInfo> {
  return new Observable<OldUpdateInfo>((subscriber) => {
    let savedState: ConnectionState = "Connecting";

    const postUpdateSubscription = createTimelinePostUpdateCount$(
      connection,
      owner,
      timeline,
    ).subscribe(() => {
      subscriber.next({
        update: true,
        state: savedState as HubConnectionState,
      });
    });

    const stateSubscription = connection.state$.subscribe((state) => {
      savedState = state;
      subscriber.next({ update: false, state: state as HubConnectionState });
    });

    return () => {
      stateSubscription.unsubscribe();
      postUpdateSubscription.unsubscribe();
    };
  });
}

export function getTimelinePostUpdate$(
  owner: string,
  timeline: string,
): Observable<OldUpdateInfo> {
  return connection$.pipe(
    switchMap((connection) =>
      createTimelinePostOldUpdateInfo$(connection, owner, timeline),
    ),
  );
}
