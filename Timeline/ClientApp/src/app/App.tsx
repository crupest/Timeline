import React from "react";
import { BrowserRouter as Router, Route, Switch } from "react-router-dom";
import { hot } from "react-hot-loader/root";

import AppBar from "./common/AppBar";
import LoadingPage from "./common/LoadingPage";
import Home from "./home/Home";
import Login from "./user/Login";
import Settings from "./settings/Settings";
import About from "./about/About";
import User from "./user/User";
import TimelinePage from "./timeline/TimelinePage";
import AlertHost from "./common/AlertHost";
import { dataStorage } from "./data/common";
import { userService, useRawUser } from "./data/user";

const NoMatch: React.FC = () => {
  return (
    <>
      <AppBar />
      <div style={{ height: 56 }} />
      <div>Ah-oh, 404!</div>
    </>
  );
};

const LazyAdmin = React.lazy(
  () => import(/* webpackChunkName: "admin" */ "./admin/Admin")
);

const App: React.FC = () => {
  const [loading, setLoading] = React.useState<boolean>(true);

  const user = useRawUser();

  React.useEffect(() => {
    void userService.checkLoginState();
    void dataStorage.ready().then(() => setLoading(false));
  }, []);

  let body;
  if (user === undefined || loading) {
    body = <LoadingPage />;
  } else {
    body = (
      <Router>
        <Switch>
          <Route exact path="/">
            <Home />
          </Route>
          <Route exact path="/login">
            <Login />
          </Route>
          <Route path="/settings">
            <Settings />
          </Route>
          <Route path="/about">
            <About />
          </Route>
          <Route path="/timelines/:name">
            <TimelinePage />
          </Route>
          <Route path="/users/:username">
            <User />
          </Route>
          {user && user.administrator && (
            <Route path="/admin">
              <LazyAdmin user={user} />
            </Route>
          )}
          <Route>
            <NoMatch />
          </Route>
        </Switch>
      </Router>
    );
  }

  return (
    <React.Suspense fallback={<LoadingPage />}>
      {body}
      <AlertHost />
    </React.Suspense>
  );
};

export default hot(App);
