import { TimelineInfo } from "@/services/timeline";
import React from "react";
import { Container, Row } from "react-bootstrap";
import { useHistory, useLocation } from "react-router";
import { Link } from "react-router-dom";

import { getHttpSearchClient } from "@/http/search";

import SearchInput from "../common/SearchInput";
import { HttpNetworkError } from "@/http/common";
import { useAvatar } from "@/services/user";
import BlobImage from "../common/BlobImage";

const TimelineSearchResultItemView: React.FC<{ timeline: TimelineInfo }> = ({
  timeline,
}) => {
  const link = timeline.name.startsWith("@")
    ? `users/${timeline.owner.username}`
    : `timelines/${timeline.name}`;

  const avatar = useAvatar(timeline.owner.username);

  return (
    <div className="timeline-search-result-item my-2 p-3">
      <h4>
        <Link to={link} className="mb-2 text-primary">
          {timeline.title}
          <small className="ml-3 text-secondary">{timeline.name}</small>
        </Link>
      </h4>
      <div>
        <BlobImage
          blob={avatar}
          className="timeline-search-result-item-avatar mr-2"
        />
        {timeline.owner.nickname}
        <small className="ml-3 text-secondary">
          @{timeline.owner.username}
        </small>
      </div>
    </div>
  );
};

const SearchPage: React.FC = () => {
  const history = useHistory();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryParam = searchParams.get("q");

  const [searchText, setSearchText] = React.useState<string>("");
  const [state, setState] = React.useState<
    TimelineInfo[] | "init" | "loading" | "network-error" | "error"
  >("init");

  React.useEffect(() => {
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
  }, [queryParam]);

  return (
    <Container className="my-3">
      <Row className="justify-content-center">
        <SearchInput
          className="col-12 col-sm-9 col-md-6"
          value={searchText}
          onChange={setSearchText}
          loading={state === "loading"}
          onButtonClick={() => {
            if (searchText.length > 0) {
              history.push(`/search?q=${searchText}`);
            }
          }}
        />
      </Row>
      {(() => {
        switch (state) {
          case "init": {
            return "Input something and search!";
          }
          case "loading": {
            return "Loading!";
          }
          case "network-error": {
            return "Network error!";
          }
          case "error": {
            return "Unknown error!";
          }
          default: {
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
