import React, { Fragment } from "react";
import { Redirect, Route, Switch, useRouteMatch, match } from "react-router";
import { Container } from "react-bootstrap";

import { AuthUser } from "@/services/user";

import i18n from "@/i18n";

import AdminNav from "./AdminNav";
import UserAdmin from "./UserAdmin";
import HighlightTimelineAdmin from "./HighlightTimelineAdmin";

void i18n.loadNamespaces("admin");

interface AdminProps {
  user: AuthUser;
}

const Admin: React.FC<AdminProps> = ({ user }) => {
  const match = useRouteMatch();

  return (
    <Fragment>
      <Switch>
        <Redirect from={match.path} to={`${match.path}/users`} exact />
        <Route path={`${match.path}/:name`}>
          {(p) => {
            const match = p.match as match<{ name: string }>;
            const name = match.params["name"];
            return (
              <Container>
                <AdminNav />
                {(() => {
                  if (name === "users") {
                    return <UserAdmin user={user} />;
                  } else if (name === "highlighttimelines") {
                    return <HighlightTimelineAdmin user={user} />;
                  }
                })()}
              </Container>
            );
          }}
        </Route>
      </Switch>
    </Fragment>
  );
};

export default Admin;
