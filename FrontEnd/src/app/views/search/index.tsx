import React from "react";
import { useTranslation } from "react-i18next";
import { Container, Row } from "react-bootstrap";
import { useHistory, useLocation } from "react-router";
import { Link } from "react-router-dom";

import { HttpNetworkError } from "@/http/common";
import { getHttpSearchClient } from "@/http/search";
import { HttpTimelineInfo } from "@/http/timeline";

import SearchInput from "../common/SearchInput";
import UserAvatar from "../common/user/UserAvatar";

const TimelineSearchResultItemView: React.FC<{
  timeline: HttpTimelineInfo;
}> = ({ timeline }) => {
  const link = timeline.name.startsWith("@")
    ? `users/${timeline.owner.username}`
    : `timelines/${timeline.name}`;

  return (
    <div className="timeline-search-result-item my-2 p-3">
      <h4>
        <Link to={link} className="mb-2 text-primary">
          {timeline.title}
          <small className="ms-3 text-secondary">{timeline.name}</small>
        </Link>
      </h4>
      <div>
        <UserAvatar
          username={timeline.owner.username}
          className="timeline-search-result-item-avatar me-2"
        />
        {timeline.owner.nickname}
        <small className="ms-3 text-secondary">
          @{timeline.owner.username}
        </small>
      </div>
    </div>
  );
};

const SearchPage: React.FC = () => {
  const { t } = useTranslation();

  const history = useHistory();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryParam = searchParams.get("q");

  const [searchText, setSearchText] = React.useState<string>("");
  const [state, setState] =
    React.useState<
      HttpTimelineInfo[] | "init" | "loading" | "network-error" | "error"
    >("init");

  const [forceResearchKey, setForceResearchKey] = React.useState<number>(0);

  React.useEffect(() => {
    setState("init");
    if (queryParam != null && queryParam.length > 0) {
      setSearchText(queryParam);
      setState("loading");
      void getHttpSearchClient()
        .searchTimelines(queryParam)
        .then(
          (ts) => {
            setState(ts);
          },
          (e) => {
            if (e instanceof HttpNetworkError) {
              setState("network-error");
            } else {
              setState("error");
            }
          }
        );
    }
  }, [queryParam, forceResearchKey]);

  return (
    <Container className="my-3">
      <Row className="justify-content-center">
        <SearchInput
          className="col-12 col-sm-9 col-md-6"
          value={searchText}
          onChange={setSearchText}
          loading={state === "loading"}
          onButtonClick={() => {
            if (queryParam === searchText) {
              setForceResearchKey((old) => old + 1);
            } else {
              history.push(`/search?q=${searchText}`);
            }
          }}
        />
      </Row>
      {(() => {
        switch (state) {
          case "init": {
            if (queryParam == null || queryParam.length === 0) {
              return <div>{t("searchPage.input")}</div>;
            }
            break;
          }
          case "loading": {
            return <div>{t("searchPage.loading")}</div>;
          }
          case "network-error": {
            return <div className="text-danger">{t("error.network")}</div>;
          }
          case "error": {
            return <div className="text-danger">{t("error.unknown")}</div>;
          }
          default: {
            if (state.length === 0) {
              return <div>{t("searchPage.noResult")}</div>;
            }
            return state.map((t) => (
              <TimelineSearchResultItemView key={t.name} timeline={t} />
            ));
          }
        }
      })()}
    </Container>
  );
};

export default SearchPage;
