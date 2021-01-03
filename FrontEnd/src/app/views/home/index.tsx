import React from "react";
import { useHistory } from "react-router";
import { useTranslation } from "react-i18next";
import { Row, Container, Button, Col } from "react-bootstrap";

import { useUser } from "@/services/user";
import SearchInput from "../common/SearchInput";

import BoardWithoutUser from "./BoardWithoutUser";
import BoardWithUser from "./BoardWithUser";
import TimelineCreateDialog from "./TimelineCreateDialog";

const HomePage: React.FC = () => {
  const history = useHistory();

  const { t } = useTranslation();

  const user = useUser();

  const [navText, setNavText] = React.useState<string>("");

  const [dialog, setDialog] = React.useState<"create" | null>(null);

  const goto = React.useCallback((): void => {
    if (navText === "") {
      history.push("users/crupest");
    } else if (navText.startsWith("@")) {
      history.push(`users/${navText.slice(1)}`);
    } else {
      history.push(`timelines/${navText}`);
    }
  }, [navText, history]);

  return (
    <>
      <Container>
        <Row className="my-3 justify-content-center">
          <Col xs={12} sm={8} lg={6}>
            <SearchInput
              className="justify-content-center"
              value={navText}
              onChange={setNavText}
              onButtonClick={goto}
              buttonText={t("home.go")}
              placeholder="@crupest"
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
        {(() => {
          if (user == null) {
            return <BoardWithoutUser />;
          } else {
            return <BoardWithUser user={user} />;
          }
        })()}
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
