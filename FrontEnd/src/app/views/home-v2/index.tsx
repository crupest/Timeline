import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Container, Button } from "react-bootstrap";

import { useUser } from "@/services/user";
import SearchInput from "../common/SearchInput";

import TimelineListView from "./TimelineListView";
import TimelineCreateDialog from "../home/TimelineCreateDialog";

const HomeV2: React.FC = () => {
  const history = useHistory();

  const { t } = useTranslation();

  const user = useUser();

  const [navText, setNavText] = React.useState<string>("");

  const [dialog, setDialog] = React.useState<"create" | null>(null);

  return (
    <>
      <Container className="px-0">
        <SearchInput
          className="my-3 mx-3"
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
        <TimelineListView headerText="home.loadingHighlightTimelines" />
      </Container>
      {dialog === "create" && (
        <TimelineCreateDialog open close={() => setDialog(null)} />
      )}
    </>
  );
};

export default HomeV2;
