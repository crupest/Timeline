import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Container, Button, Row, Col } from "react-bootstrap";

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
      <Container fluid className="px-0">
        <Row className="my-3 px-2 justify-content-end">
          <Col xs="auto">
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
        <TimelineListView headerText="home.loadingHighlightTimelines" />
      </Container>
      {dialog === "create" && (
        <TimelineCreateDialog open close={() => setDialog(null)} />
      )}
    </>
  );
};

export default HomeV2;
