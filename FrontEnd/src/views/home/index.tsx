import React from "react";
import { useHistory } from "react-router";

import { HttpTimelineInfo } from "@/http/timeline";
import { getHttpHighlightClient } from "@/http/highlight";

import SearchInput from "../common/SearchInput";
import TimelineListView from "./TimelineListView";
import WebsiteIntroduction from "./WebsiteIntroduction";

import "./index.css";

const highlightTimelineMessageMap = {
  loading: "home.loadingHighlightTimelines",
  done: "home.loadedHighlightTimelines",
  error: "home.errorHighlightTimelines",
} as const;

const HomeV2: React.FC = () => {
  const history = useHistory();

  const [navText, setNavText] = React.useState<string>("");

  const [highlightTimelineState, setHighlightTimelineState] = React.useState<
    "loading" | "done" | "error"
  >("loading");
  const [highlightTimelines, setHighlightTimelines] = React.useState<
    HttpTimelineInfo[] | undefined
  >();

  React.useEffect(() => {
    if (highlightTimelineState === "loading") {
      let subscribe = true;
      void getHttpHighlightClient()
        .list()
        .then(
          (data) => {
            if (subscribe) {
              setHighlightTimelineState("done");
              setHighlightTimelines(data);
            }
          },
          () => {
            if (subscribe) {
              setHighlightTimelineState("error");
              setHighlightTimelines(undefined);
            }
          }
        );
      return () => {
        subscribe = false;
      };
    }
  }, [highlightTimelineState]);

  return (
    <>
      <SearchInput
        className="mx-2 my-3 float-sm-end"
        value={navText}
        onChange={setNavText}
        onButtonClick={() => {
          history.push(`search?q=${navText}`);
        }}
        alwaysOneline
      />
      <WebsiteIntroduction className="m-2" />
      <TimelineListView
        headerText={highlightTimelineMessageMap[highlightTimelineState]}
        timelines={highlightTimelines}
      />
    </>
  );
};

export default HomeV2;
