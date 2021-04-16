import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Container, Button, Row, Col } from "react-bootstrap";

import { useUser } from "@/services/user";
import SearchInput from "../common/SearchInput";

import TimelineListView from "./TimelineListView";
import TimelineCreateDialog from "../home/TimelineCreateDialog";
import { HttpTimelineInfo } from "@/http/timeline";
import { getHttpHighlightClient } from "@/http/highlight";

const highlightTimelineMessageMap = {
  loading: "home.loadingHighlightTimelines",
  done: "home.loadedHighlightTimelines",
  error: "home.errorHighlightTimelines",
} as const;

const HomeV2: React.FC = () => {
  const history = useHistory();

  const { t } = useTranslation();

  const user = useUser();

  const [navText, setNavText] = React.useState<string>("");

  const [dialog, setDialog] = React.useState<"create" | null>(null);

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
      <Container fluid className="px-0">
        <Row className="mx-0 my-3 px-2 justify-content-end">
          <Col xs="12" sm="auto">
            <SearchInput
              value={navText}
              onChange={setNavText}
              onButtonClick={() => {
                history.push(`search?q=${navText}`);
              }}
              additionalButton={
                user != null && (
                  <Button
                    variant="outline-success"
                    onClick={() => {
                      setDialog("create");
                    }}
                  >
                    {t("home.createButton")}
                  </Button>
                )
              }
            />
          </Col>
        </Row>
        <TimelineListView
          headerText={highlightTimelineMessageMap[highlightTimelineState]}
          timelines={highlightTimelines}
        />
      </Container>
      {dialog === "create" && (
        <TimelineCreateDialog open close={() => setDialog(null)} />
      )}
    </>
  );
};

export default HomeV2;
