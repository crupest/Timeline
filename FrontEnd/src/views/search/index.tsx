import * as React from "react";
import { useTranslation } from "react-i18next";
import { useNavigate, useLocation } from "react-router-dom";
import { Link } from "react-router-dom";

import { HttpNetworkError } from "@/http/common";
import { getHttpSearchClient } from "@/http/search";
import { HttpTimelineInfo } from "@/http/timeline";

import SearchInput from "../common/SearchInput";
import UserAvatar from "../common/user/UserAvatar";

import "./index.css";

const TimelineSearchResultItemView: React.FC<{
  timeline: HttpTimelineInfo;
}> = ({ timeline }) => {
  return (
    <div className="timeline-search-result-item my-2 p-3">
      <h4>
        <Link
          to={`/${timeline.owner.username}/${timeline.nameV2}`}
          className="mb-2 text-primary"
        >
          {timeline.title}
          <small className="ms-3 text-secondary">{timeline.nameV2}</small>
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

  const navigate = useNavigate();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryParam = searchParams.get("q");

  const [searchText, setSearchText] = React.useState<string>("");
  const [state, setState] = React.useState<
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
    <div className="container my-3">
      <div className="row justify-content-center">
        <SearchInput
          className="col-12 col-sm-9 col-md-6"
          value={searchText}
          onChange={setSearchText}
          loading={state === "loading"}
          onButtonClick={() => {
            if (queryParam === searchText) {
              setForceResearchKey((old) => old + 1);
            } else {
              navigate(`/search?q=${searchText}`);
            }
          }}
        />
      </div>
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
              <TimelineSearchResultItemView
                key={`${t.owner.username}/${t.nameV2}`}
                timeline={t}
              />
            ));
          }
        }
      })()}
    </div>
  );
};

export default SearchPage;
