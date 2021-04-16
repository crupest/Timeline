import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Row, Container, Button, Col } from "react-bootstrap";

import { useUserLoggedIn } from "@/services/user";

import SearchInput from "../common/SearchInput";
import CenterBoards from "./CenterBoards";
import TimelineCreateDialog from "./TimelineCreateDialog";

const HomePage: React.FC = () => {
  const history = useHistory();

  const { t } = useTranslation();

  const user = useUserLoggedIn();

  const [navText, setNavText] = React.useState<string>("");

  const [dialog, setDialog] = React.useState<"create" | null>(null);

  return (
    <>
      <Container>
        <Row className="my-3 justify-content-center">
          <Col xs={12} sm={8} lg={6}>
            <SearchInput
              className="justify-content-center"
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
        <CenterBoards />
      </Container>
      {dialog === "create" && (
        <TimelineCreateDialog
          open
          close={() => {
            setDialog(null);
          }}
        />
      )}
    </>
  );
};

export default HomePage;
