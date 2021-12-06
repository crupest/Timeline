import React from "react";
import { useNavigate } from "react-router-dom";

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
  const navigate = useNavigate();

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
        className="mx-2 my-3 home-search"
        value={navText}
        onChange={setNavText}
        onButtonClick={() => {
          navigate(`search?q=${navText}`);
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
