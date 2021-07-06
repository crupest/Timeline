import React from "react";
import { useHistory } from "react-router";

import { useUserLoggedIn } from "@/services/user";

import SearchInput from "../common/SearchInput";
import Button from "../common/button/Button";
import CenterBoards from "./CenterBoards";
import TimelineCreateDialog from "./TimelineCreateDialog";

import "./index.css";

const HomePage: React.FC = () => {
  const history = useHistory();

  const user = useUserLoggedIn();

  const [navText, setNavText] = React.useState<string>("");

  const [dialog, setDialog] = React.useState<"create" | null>(null);

  return (
    <>
      <div className="container">
        <div className="row my-3 justify-content-center">
          <div className="col col-12 col-md-8">
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
                    text="home.createButton"
                    color="success"
                    onClick={() => {
                      setDialog("create");
                    }}
                  />
                )
              }
            />
          </div>
        </div>
        <CenterBoards />
      </div>
      <TimelineCreateDialog
        open={dialog === "create"}
        close={() => {
          setDialog(null);
        }}
      />
    </>
  );
};

export default HomePage;
